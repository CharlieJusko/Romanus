using StarterAssets;
using System;
using UnityEngine;

[System.Serializable]
public class AttackPlayerAction : IPlayerAction
{
    public Transform player;
    private TwinStickController twinStickController;
    private Animator animator;
    private StarterAssetsInputs input;

    //public Attack currentAttack;
    public bool getInRange = false;
    private bool canAttack = true;
    [SerializeField] private bool inMeleeAtk = false;
    [SerializeField] private bool inFirearmAtk = false;
    private bool leftDefaultState = false;
    private int currentAttackType;
    [SerializeField] private float visceralRange = 10f;

    int firearmIndex = 0;
    float forceAimTimer = 0f;
    float maxForceAimTime = 3f;
    bool visceral = false;


    public bool LookAtTargetDuringAttack { get; set; }

    public int AttackType { get; set; }
    public Enemy Enemy { get; set; }
    public float DamageMultiplier { get; set; }
    public bool CanCommit { get { return canAttack; } set { canAttack = value; } }
    public bool InAction { get { return inMeleeAtk || inFirearmAtk; } set { inMeleeAtk = value; } }
    public bool InVisceral { get => visceral; }
    public bool Lunge { get; set; }

    public void Initialize()
    {
        animator = player.GetComponent<Animator>();
        twinStickController = player.GetComponent<TwinStickController>();
        input = player.GetComponent<StarterAssetsInputs>();
    }

    public void Reset()
    {
        twinStickController.isAttacking = false;
        twinStickController.moveEnabled = true;
        inMeleeAtk = false;
        inFirearmAtk = false;
        animator.SetBool("IsAttacking", false);
        leftDefaultState = false;
        visceral = false;

        input.lightAttack = false;
        input.heavyAttack = false;

        LookAtTargetDuringAttack = false;
    }

    public void Scan()
    {
        AnimatorStateInfo baseLayerState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo lowerBodyLayerState = animator.GetCurrentAnimatorStateInfo(1);
        if(baseLayerState.IsTag("Attack") || lowerBodyLayerState.IsTag("Attack"))
        {
            if(baseLayerState.IsTag("Attack"))
            {
                twinStickController.moveEnabled = false;
                twinStickController.isAttacking = true;
            }
            if(lowerBodyLayerState.IsTag("Attack"))
            {
                twinStickController.moveEnabled = true;
                twinStickController.isAttacking = false;
            }

            if(!leftDefaultState && visceral)
            {
                Enemy.TakeDamage(player.GetComponent<Player>().meleeWeapons[0].Damage * 1.75f, playerAttackType: AttackType);
                ForceVisceralDistance();
            }

            leftDefaultState = true;

            if(Enemy != null && !Enemy.isDead && LookAtTargetDuringAttack)
                ForceLookAtEnemy();

            if(input.move == Vector2.zero && Enemy != null && !Enemy.isDead && getInRange && Lunge)
                ForceRange();
        } 

        else
        {
            if(leftDefaultState)
            {
                Reset();
            }
        }
    }

    public void Update()
    {
        Scan();

        if(Enemy && Enemy.TryGetComponent(out Targetable t))
        {
            t.Target();
        }

        if(canAttack)
        {
            if(!twinStickController.grounded)
            {
                input.lightAttack = false;
                animator.SetBool("MeleeAttack", false);
                animator.SetBool("FirearmAttack", false);
                animator.SetBool("IsAttacking", false);
                return;
            }

            if(input.lightAttack)
            {
                if(!inFirearmAtk)
                {
                    CommitMeleeAttack();
                    input.lightAttack = false;
                    input.heavyAttack = false;
                }
            }
            else if(input.heavyAttack)
            {
                if(!inMeleeAtk)
                {
                    CommitRangedAttack();
                    input.lightAttack = false;
                    input.heavyAttack = false;
                }
            }
            else
            {
                animator.SetBool("IsAttacking", false);
            }
        } 
        else
        {
            input.lightAttack = false;
            input.heavyAttack = false;
            animator.SetBool("IsAttacking", false);
        }
    }

    public void TriggerEnemyDeath()
    {
        if(Enemy != null)
        {
            ForceLookAtEnemy();
            Enemy.TriggerDeath(currentAttackType, false, Vector3.zero);
        }
    }

    void ForceLookAtEnemy()
    {
        var targetRotation = GetEnemyDirection();
        player.rotation = Quaternion.Slerp(player.rotation, targetRotation, 100f * Time.deltaTime);
    }

    void ForceRange()
    {
        float distance = Vector3.Distance(Enemy.transform.position, player.position);
        float range = player.GetComponent<Player>().blessing.attackRange;
        if(distance >= range)
        {
            ForceLookAtEnemy();
            player.GetComponent<CharacterController>().Move((Enemy.transform.position - player.position).normalized * (twinStickController.sprintSpeed * 2 * Time.deltaTime));
        }
    }

    void ForceVisceralDistance()
    {
        Vector3 target = Enemy.transform.position;
        target.y = player.position.y;
        player.LookAt(target);

        //Vector3 targetDir = player.transform.position + player.transform.forward * visceralRange;
        //player.transform.position = targetDir;
    }

    Quaternion GetEnemyDirection()
    {
        var lookPos = Enemy.transform.position - player.position;
        lookPos.y = 0f;
        return Quaternion.LookRotation(lookPos);
    }

    void CommitMeleeAttack()
    {
        DamageMultiplier = 1;
        currentAttackType = AttackType = player.GetComponent<Player>().meleeWeapons[0].weaponType;

        visceral = false;

        // If Enemy is parried and we're in range, visceral attack
        if(Enemy != null && Enemy.parried)
        {
            float distance = Vector3.Distance(Enemy.transform.position, player.position);
            float range = player.GetComponent<Player>().blessing.attackRange;

            if(distance <= range)
            {
                currentAttackType = AttackType = player.GetComponent<Player>().meleeWeapons[0].weaponType + 1;
                visceral = true;
            }

        }

        animator.SetInteger("AttackType", AttackType);
        animator.SetBool("IsAttacking", true);
        animator.SetTrigger("MeleeAttack");
        if(!inMeleeAtk)
        {
            inMeleeAtk = true;
            twinStickController.isAttacking = true;
        }

        input.heavyAttack = false;
    }

    void CommitRangedAttack()
    {
        int length = player.GetComponent<Player>().firearms.Length;
        if(length == 1)
        {
            Firearm firearm = player.GetComponent<Player>().firearms[0];
            if(firearm.CanFire)
                FirearmAttack(firearm, 0);
        } 
        else if(length > 1)
        {
            Firearm firearm = player.GetComponent<Player>().firearms[firearmIndex];
            if(firearm.CanFire)
            {
                FirearmAttack(firearm, firearmIndex);
                IncrementFirearmIndex(length);
            }
            else
            {
                int nextIndex = firearmIndex + 1;
                if(nextIndex >= length)
                    nextIndex = 0;

                if(player.GetComponent<Player>().firearms[nextIndex].CanFire)
                    firearmIndex = nextIndex;
            }
        }
    }

    void FirearmAttack(Firearm firearm, int index=0)
    {
        animator.SetInteger("GunIndex", index);
        animator.SetTrigger("FirearmAttack");
        //firearm.Shoot();
    }

    void IncrementFirearmIndex(int length)
    {
        firearmIndex++;
        if(firearmIndex >= length)
            firearmIndex = 0;

        player.GetComponent<Player>().firearms[firearmIndex].fireDelayTimer = 0f;
    }
}
