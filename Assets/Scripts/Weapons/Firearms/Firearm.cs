using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public enum FireMode 
{ 
    Automatic,
    SemiAutomatic,
    Burst,
    Charge
}

public enum FirePattern
{
    Single,
    Spread,
    Beam
}

[System.Serializable]
public struct ShotBehaviour
{
    public FireMode fireMode;
    public FirePattern firePattern;
    [Range(1, 24)]
    public int projectilesPerShot;
    [Range(0,1)]
    [Tooltip("Only applied if FirePattern is set to 'Spread'")] public float spread;
}


public class Firearm : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 1;
    public Transform bulletSpawnPos;
    [SerializeField] public float range;
    [SerializeField] protected bool equipped;
    [SerializeField] protected WieldingType wieldingType;
    [SerializeField] protected WeaponTransformTarget[] transformTargets;
    [SerializeField] protected ShotBehaviour shotBehaviour;

    [Space(5)]
    [Header("Ammo Mangement")]
    //public int clipSize;
    public int currentAmmoCount;

    [Space(5)]
    [Header("Fire Rate")]
    public float fireRate;
    public float fireDelay;
    public float fireDelayTimer = 0f;
    public bool simultaneousFire = false;

    [Space(5)]
    [Header("VFX")]
    public VisualEffect muzzleFlashVFX;

    [Space(5)]
    [Header("SFX")]
    public AudioClip shotSFX;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;

    [Space(5)]
    [Header("Bullet Trail Effect")]
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private GameObject impactEffect;

    [Space(5)]
    [Header("UI")]
    [SerializeField] private string weaponName;
    [SerializeField][TextArea] private string description;
    [SerializeField] private Sprite icon;

    [Space(5)]
    [Header("Animation")]
    public RuntimeAnimatorController overrideController;
    [Range(0f, 1f)]
    public float rightArmMaskStrength = 1f;
    [Range(0f, 1f)]
    public float upperBodyMaskStrength = 0.7f;

    //public bool CanReload { get { return currentAmmoCount < clipSize; } }
    public bool CanFire { get { return /*equipped && */fireDelayTimer >= fireDelay; } }
    public float Damage { get { return damage; } set { damage = value; } }
    public bool Equipped { get { return equipped; } set { equipped = value; } }
    public string Name { get { return weaponName; } set { weaponName = value; } }
    public string Description { get { return description; } set { description = value; } }
    public Sprite Icon { get { return icon; } set { icon = value; } }
    public WieldingType WieldingType { get { return wieldingType; } set { wieldingType = value; } }
    public WeaponTransformTarget[] TransformTargets { get { return transformTargets; } }
    public Transform BulletSpawnPos { get { return bulletSpawnPos; } }


    void Start()
    {
        fireDelayTimer = fireDelay;
        //currentAmmoCount = clipSize;
        //GameEvents.current.onFirearmShoot += Shoot;
    }

    void Update()
    {
        if(currentAmmoCount > 0)
            FireRateTimer();
        else
            fireDelayTimer = 0f;
    }

    public void Holster(Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale)
    {
        transform.parent = parent;
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEuler;
        transform.localScale = localScale;
        Equipped = false;
    }

    public void Equip(Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale)
    {
        transform.parent = parent;
        transform.localPosition = localPosition;
        transform.localEulerAngles = localEuler;
        transform.localScale = localScale;
        Equipped = true;
    }

    public void PlaySFX()
    {
        if(TryGetComponent(out AudioSource audioSource))
        {
            audioSource.clip = shotSFX;
            audioSource.volume = sfxVolume;
            audioSource.Play();
        }
    }

    public void Shoot()
    {
        if(currentAmmoCount <= 0)
            return;

        // Play Muzzle Flash
        muzzleFlashVFX.Play();
        fireDelayTimer = 0f;
        //currentAmmoCount--;

        PlaySFX();

        // TODO Have this be an event
        //transform.root.GetComponent<Player>().inventory.ammunition--;

        GameEvents.current.FirearmShoot();

        switch(shotBehaviour.firePattern) 
        {
            case FirePattern.Single:
                {
                    ShootBullet();
                    break;
                }
            case FirePattern.Spread:
                {
                    for(int i = 0; i < shotBehaviour.projectilesPerShot; ++i)
                    {
                        ShootBullet(shotBehaviour.spread);
                    }
                    break;
                }
            default:
                {
                    ShootBullet();
                    break;
                }
        }

        fireDelayTimer = 0f;
    }

    void ShootBullet(float spread=0)
    {
        if(TryGetComponent(out ProjectileSpawner spawner) && spawner.enabled)
        {
            var spawned = spawner.Spawn(GetShotDirection(spread));
            spawned.GetComponent<Projectile>().range = range;
        } 
        else
        {
            // If we hit something
            if(Physics.Raycast(bulletSpawnPos.position, GetShotDirection(spread), out RaycastHit hit, range))
            {
                CreateBulletTrailLaser(hit.point, true);

                // Player hits the enemy
                if(hit.collider.gameObject.CompareTag("Enemy") && hit.collider.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    enemy.TakeDamage(Damage, playerAttackType:0);
                }

                // Enemy hits the player
                if(hit.collider.gameObject.CompareTag("Player") || (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Player")))
                {
                    // TODO
                }

            } 
            else
            {
                CreateBulletTrailLaser(bulletSpawnPos.position + GetShotDirection(spread) * range, false);
            }
        }
    }

    Vector3 GetShotDirection(float spread=0)
    {
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);
        Vector3 direction = transform.root.forward + new Vector3(x, y, z);
        return direction;
    }

    protected void CreateBulletTrailLaser(Vector3 endPos, bool hitSomething)
    {
        TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPos.position, Quaternion.identity);
        StartCoroutine(FadeTrail(trail, endPos, hitSomething));
    }

    protected IEnumerator FadeTrail(TrailRenderer trail, Vector3 endPos, bool hitSomething)
    {
        float timer = 0;
        Vector3 startPos = trail.transform.position;
        while(timer < fadeTime)
        {
            trail.transform.position = Vector3.Lerp(startPos, endPos, timer);
            timer += Time.deltaTime / trail.time;
            yield return null;
        }

        if(hitSomething)
        {
            var impact = Instantiate(impactEffect, endPos, Quaternion.identity);
            Destroy(impact, 1f);
        }

        Destroy(trail.gameObject, trail.time);
    }

    //public void Reload(ref int totalAmmoCount, int amount=-1)
    //{
    //    int targetReloadAmnt = amount;
    //    if(targetReloadAmnt > 0)
    //    {
    //        int reloadAmnt = targetReloadAmnt;
    //        if(totalAmmoCount < targetReloadAmnt)
    //        {
    //            reloadAmnt = totalAmmoCount;
    //        }
    //        totalAmmoCount -= reloadAmnt;
    //        currentAmmoCount += reloadAmnt;
    //    }
    //}

    //public int TargetReloadAmount { get { return clipSize - currentAmmoCount; } }

    protected void FireRateTimer()
    {
        if(fireDelayTimer <= fireDelay)
        {
            fireDelayTimer += Time.deltaTime * fireRate;
        }
    }
}