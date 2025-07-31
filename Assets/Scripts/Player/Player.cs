using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Character")]
    public Deity blessing;
    public Inventory inventory;

    [Space(5)]
    [Header("Health")]
    public float maxHealth;
    public float currentHealth;
    public bool isDead = false;
    public HealthBar healthBar;

    [Space(5)]
    [Header("Enemy Detection & Utility")]
    public LayerMask enemyLayerMask;
    public float enemyAttackRadius = 7f;
    public Enemy closestEnemy;

    [Space(5)]
    [Header("Actions")]
    public AttackPlayerAction attack;
    public DodgePlayerAction dodge;
    public ReloadPlayerAction reload;
    public StaggerPlayerAction stagger;
    public ActivateAction activateAction;

    [Space(5)]
    [Header("Loadout")]
    public Weapon[] meleeWeapons;
    public Firearm[] firearms;

    [Space(5)]
    [Header("Armature")]
    public Transform hips;
    public Transform spineTop;
    public Transform leftHand;
    public Transform rightHand;


    StarterAssetsInputs input;
    TwinStickController twinStickController;
    Animator animator;

    public int CurrentClipCount 
    { 
        get 
        {
            int count = 0;
            Array.ForEach(firearms, x => count += x.currentAmmoCount);
            return count;
        } 
    }

    //public int TotalClipCount
    //{
    //    get
    //    {
    //        int count = 0;
    //        Array.ForEach(firearms, x => count += x.clipSize);
    //        return count;
    //    }
    //}


    private void Start()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
        twinStickController = GetComponent<TwinStickController>();

        InitPlayerActions();

        blessing.Activate(animator, twinStickController);
        inventory.SelectMelee(meleeWeapons[0]);
        inventory.SelectFirearm(firearms[0]);

        GameEvents.current.onStart += inventory.Start;

        GameEvents.current.onUpdate += inventory.Update;
        GameEvents.current.onUpdate += attack.Update;
        GameEvents.current.onUpdate += dodge.Update;
        GameEvents.current.onUpdate += stagger.Update;
        GameEvents.current.onUpdate += activateAction.Update;

        GameEvents.current.onDisablePlayerMovement += DisableMovement;
        GameEvents.current.onEnablePlayerMovement += EnableMovement;

        GameEvents.current.onFirearmShoot += inventory.UseAmmo;
        GameEvents.current.onEnemyDeath += AddFavor;

        GameEvents.current.OnStart();
    }

    private void Update()
    {
        ClosestEnemy();
        UpdatePlayerActions();
        GameEvents.current.OnUpdate();

        Array.ForEach(firearms, x => x.currentAmmoCount = inventory.ammunition);

        if(!blessing.controlable)
            twinStickController.moveEnabled = false;
    }

    void InitPlayerActions()
    {
        attack.player = transform;
        attack.Initialize();

        dodge.player = transform;
        dodge.Initialize();

        reload.player = transform;
        reload.Initialize();

        stagger.player = transform;
        stagger.Initialize();

        activateAction.player = transform;
        activateAction.Initialize();
    }

    void UpdatePlayerActions()
    {
        attack.CanCommit = !dodge.InAction && !stagger.InAction && !activateAction.InAction;
        dodge.CanCommit = !attack.InAction && !stagger.InAction && !activateAction.InAction;
        stagger.CanCommit = !dodge.InAction && !attack.InAction && !activateAction.InAction;
        activateAction.CanCommit = !dodge.InAction && !attack.InAction && !stagger.InAction;
    }

    public void TakeDamage(float damage)
    {
        if(currentHealth > 0)
        {
            currentHealth -= damage;
        }
        healthBar.UpdateFillAmount(currentHealth, maxHealth);
        if(stagger.CanCommit)
            animator.SetTrigger("Hit");
        animator.SetFloat("Health", currentHealth);
    }

    void ClosestEnemy()
    {
        Vector3 center = transform.position;
        center.y += 1.5f;

        List<Enemy> closeEnemies = GetCloseEnemies();
        bool setClosestEnemy = false;
        foreach(Enemy c in closeEnemies)
        {
            if(Vector3.Angle(transform.forward, c.transform.position - transform.position) < 90f && !c.isDead)
            {
                if(closestEnemy == null || Vector3.Distance(closestEnemy.transform.position, transform.position) > Vector3.Distance(c.transform.position, transform.position))
                {
                    setClosestEnemy = true;
                    closestEnemy = c;
                } 
                else if(closestEnemy == c && !setClosestEnemy)
                {
                    setClosestEnemy = true;
                }
            }
        }

        if(!setClosestEnemy && closestEnemy != null && !closestEnemy.isDead && attack.InVisceral)
            setClosestEnemy = true;

        if(!setClosestEnemy)
        {
            if(closestEnemy && closestEnemy.TryGetComponent(out Targetable t))
                t.Untarget();

            attack.Enemy = closestEnemy = null;
        }
        else
        {
            if(attack.Enemy != closestEnemy)
            {
                if(attack.Enemy && attack.Enemy.TryGetComponent(out Targetable t))
                    t.Untarget();
            }
            attack.Enemy = closestEnemy;

            if(closestEnemy.TryGetComponent(out Targetable _t))
                _t.Target();
        }
    }

    List<Enemy> GetCloseEnemies(float radius = -1f)
    {
        if(radius == -1f)
            radius = blessing.detectionRadius;

        Vector3 center = transform.position;
        center.y += 1.5f;
        Collider[] overlappedEnemies = Physics.OverlapSphere(center, radius, enemyLayerMask);
        List<Enemy> closeEnemies = new List<Enemy>();
        foreach(Collider c in overlappedEnemies)
        {
            var root = c.transform.root;
            if(root.TryGetComponent(out Enemy e))
            {
                closeEnemies.Add(e);
            }
        }

        return closeEnemies;
    }

    public void PlayMeleeWeaponSFX(bool left)
    {
        var targetWeapon = meleeWeapons[0];
        foreach(Weapon melee in meleeWeapons)
        {
            if(left)
                if(melee.transform.parent.name.ToLower().Contains("left"))
                {
                    targetWeapon = melee;
                    break;
                }
            else
                if(melee.transform.parent.name.ToLower().Contains("right"))
                {
                    targetWeapon = melee;
                    break;
                }
        }

        targetWeapon.PlaySFX();
    }

    public void PlayVisceralSFX(bool left)
    {
        var targetWeapon = meleeWeapons[0];
        foreach(Weapon melee in meleeWeapons)
        {
            if(left)
                if(melee.transform.parent.name.ToLower().Contains("left"))
                {
                    targetWeapon = melee;
                    break;
                } 
            else
                if(melee.transform.parent.name.ToLower().Contains("right"))
                {
                    targetWeapon = melee;
                    break;
                }
        }

        targetWeapon.PlaySFX(targetWeapon.visceralSFX);
    }

    public void PlaySFX(AudioClip clip)
    {
        if(TryGetComponent(out AudioSource audioSource))
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    //public bool InEnemyAttackRange()
    //{
    //    if(closestEnemy != null)
    //    {
    //        return Vector3.Distance(closestEnemy.transform.position, transform.position) <= closestEnemy.attackRange;
    //    }

    //    return false;
    //}

    //public bool CanReloadFirearms()
    //{
    //    foreach(Firearm firearm in firearms)
    //    {
    //        if(firearm.CanReload && inventory.ammunition > 0)
    //            return true;
    //    }

    //    return false;
    //}

    //public void ReloadFirearms()
    //{
    //    if(firearms.Length > 1)
    //    {
    //        while(inventory.ammunition > 0)
    //        {
    //            if(firearms[0].CanReload)
    //                firearms[0].Reload(ref inventory.ammunition, 1);

    //            if(firearms[1].CanReload)
    //                firearms[1].Reload(ref inventory.ammunition, 1);

    //            if(!firearms[0].CanReload && !firearms[1].CanReload)
    //                break;
    //        }

    //    } 
    //    else
    //    {
    //        while(inventory.ammunition > 0 && firearms[0].CanReload)
    //        {
    //            firearms[0].Reload(ref inventory.ammunition, 1);
    //        }
    //    }
    //}

    void EnableMovement()
    {
        twinStickController.moveEnabled = true;
    }

    void DisableMovement()
    {
        twinStickController.moveEnabled = false;
    }

    public void Shoot()
    {
        firearms[0].Shoot();
    }

    public void AddFavor(int amount)
    {
        GameEvents.current.FavorGain(amount);
        StartCoroutine(inventory.AddFavorUI(amount));
    }

    public void HolsterFirearms()
    {
        for(int i = 0; i < firearms.Length; i++)
        {
            Transform parent = null;
            switch(firearms[i].TransformTargets[i]._holsterParent)
            {
                case ArmatureParent.HIPS:
                    parent = hips;
                    break;
                case ArmatureParent.SPINE_TOP:
                    parent = spineTop;
                    break;
                case ArmatureParent.LEFT_HAND:
                    parent = leftHand;
                    break;
                case ArmatureParent.RIGHT_HAND:
                    parent = rightHand;
                    break;
                default:
                    break;

            }
            Firearm firearm = firearms[i];
            firearm.Holster(
                parent,
                firearm.TransformTargets[i].holsterLocalPos,
                firearm.TransformTargets[i].holsterLocalEuler,
                firearm.TransformTargets[i].holsterLocalScale
            );
        }
    }

    public void EquipFirearms()
    {
        for(int i = 0; i < firearms.Length; i++)
        {
            Transform parent = null;
            switch(firearms[i].TransformTargets[i]._equippedParent)
            {
                case ArmatureParent.HIPS:
                    parent = hips;
                    break;
                case ArmatureParent.SPINE_TOP:
                    parent = spineTop; 
                    break;
                case ArmatureParent.LEFT_HAND: 
                    parent = leftHand; 
                    break;
                case ArmatureParent.RIGHT_HAND:
                    parent = rightHand;
                    break;
                default:
                    break;

            }
            Firearm firearm = firearms[i];
            firearm.Equip(
                parent,
                firearm.TransformTargets[i].equippedLocalPos,
                firearm.TransformTargets[i].equippedLocalEuler,
                firearm.TransformTargets[i].equippedLocalScale
            );
            //firearm.currentAmmoCount = inventory.ammunition;
        }
    }

    public void HolsterMelee()
    {
        for(int i = 0; i < meleeWeapons.Length; i++)
        {
            Transform parent = null;
            switch(meleeWeapons[i].TransformTargets[i]._holsterParent)
            {
                case ArmatureParent.HIPS:
                    parent = hips;
                    break;
                case ArmatureParent.SPINE_TOP:
                    parent = spineTop;
                    break;
                case ArmatureParent.LEFT_HAND:
                    parent = leftHand;
                    break;
                case ArmatureParent.RIGHT_HAND:
                    parent = rightHand;
                    break;
                default:
                    break;

            }
            Weapon weapon = meleeWeapons[i];
            weapon.Holster(
                parent,
                weapon.TransformTargets[i].holsterLocalPos,
                weapon.TransformTargets[i].holsterLocalEuler,
                weapon.TransformTargets[i].holsterLocalScale
            );
        }
    }

    public void EquipMelee()
    {
        for(int i = 0; i < meleeWeapons.Length; i++)
        {
            Transform parent = null;
            switch(meleeWeapons[i].TransformTargets[i]._equippedParent)
            {
                case ArmatureParent.HIPS:
                    parent = hips;
                    break;
                case ArmatureParent.SPINE_TOP:
                    parent = spineTop;
                    break;
                case ArmatureParent.LEFT_HAND:
                    parent = leftHand;
                    break;
                case ArmatureParent.RIGHT_HAND:
                    parent = rightHand;
                    break;
                default:
                    break;

            }
            Weapon weapon = meleeWeapons[i];
            weapon.Equip(
                parent,
                weapon.TransformTargets[i].equippedLocalPos,
                weapon.TransformTargets[i].equippedLocalEuler,
                weapon.TransformTargets[i].equippedLocalScale
            );
        }
    }
}
