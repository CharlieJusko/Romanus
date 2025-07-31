using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour, IWeapon
{
    [SerializeField] protected float damage = 1;
    [SerializeField] protected bool equipped;
    [SerializeField] protected WieldingType wieldingType;
    [SerializeField] protected WeaponTransformTarget[] transformTargets;
    public List<WeaponEffect> effects = new List<WeaponEffect>();
    public int weaponType;

    [Space(5)]
    [Header("Animation")]
    public RuntimeAnimatorController overrideController;

    [Space(5)]
    [Header("SFX")]
    public AudioClip soundEffect;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;
    public AudioClip hitSFX;
    public AudioClip visceralSFX;

    [Space(5)]
    [Header("UI")]
    [SerializeField] private string weaponName;
    [SerializeField][TextArea] private string description;
    [SerializeField] private Sprite icon;

    public float Damage { get { return damage; } set { damage = value; } }
    public bool Equipped { get { return equipped; } set { equipped = value; } }
    public string Name { get { return weaponName; } set { weaponName = value; } }
    public string Description { get { return description; } set { description = value; } }
    public Sprite Icon { get { return icon; } set { icon = value; } }
    public WieldingType WieldingType { get { return wieldingType; } set { wieldingType = value; } }
    public WeaponTransformTarget[] TransformTargets { get { return transformTargets; } }


    private void Update()
    {
        if(!Equipped)
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy") && transform.root.CompareTag("Player"))
        {
            GameEvents.current.EnemyHit(hitSFX);
            other.GetComponent<Enemy>().TakeDamage(Damage, playerAttackType:0);
            foreach(var effect in effects)
            {
                if(effect.Active && effect.Condition == WeaponEffect.EffectCondition.HIT)
                {
                    StartCoroutine(effect.Apply());
                }
            }
        }

        else if(other.CompareTag("Player") && transform.root.CompareTag("Enemy"))
        {
            other.GetComponent<Player>().TakeDamage(Damage);
        }
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
        PlaySFX(soundEffect);
    }

    public void PlaySFX(AudioClip clip)
    {
        if(TryGetComponent(out AudioSource audioSource))
        {
            audioSource.clip = clip;
            audioSource.volume = sfxVolume;
            audioSource.Play();
        }
    }
}

[System.Serializable]
public class WeaponEffect
{
    public enum EffectCondition 
    {
        ATTACK,
        HIT
    }

    public enum EffectStrategy
    {
        SLOW_TIME
    }

    public bool active = true;
    [SerializeField] EffectCondition condition;
    [SerializeField] EffectStrategy strategy;
    public SlowTimeEffect slowTimeEffect;

    public bool Active { get { return active; } set { active = value; } }
    public EffectCondition Condition { get { return condition; } set { condition = value; } }
    public EffectStrategy Strategy { get { return strategy; } set { strategy = value; } }

    public IEnumerator Apply()
    {
        if(active && strategy == EffectStrategy.SLOW_TIME)
        {
            slowTimeEffect.Apply();
            yield return new WaitForSeconds(slowTimeEffect.Duration);
            slowTimeEffect.Stop();
        }
    }
}

public interface IWeaponEffect 
{
    public float Duration { get; set; }
    public void Apply();
    public void Stop();
}


[System.Serializable]
public struct SlowTimeEffect : IWeaponEffect
{
    [Range(0f, 1f)]
    [SerializeField] float factor;
    [SerializeField] float duration;

    public float Duration { get { return duration; } set { duration = value; } }


    public void Apply()
    {
        Time.timeScale = factor;
    }

    public void Stop()
    {
        Time.timeScale = 1f;
    }
}

