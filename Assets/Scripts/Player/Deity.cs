using UnityEngine;

public class Deity : MonoBehaviour
{
    //public AnimatorController controller;
    public RuntimeAnimatorController controller;
    public float moveSpeed;
    public float aimSpeed;
    public Avatar avatar;
    public bool active;
    public bool controlable = true;
    public int attackTypeOffset = 0;
    public float detectionRadius = 10f;
    public float attackRange = 10f;
    public GameObject spawnVFX;

    [Space(5)]
    [Header("Instinct Loadout")]
    public Loadout loadout;
    // should be either 1 or 2
    //public LoadoutFirearm[] firearms;
    //public LoadoutMelee[] meleeWeapons;

    TwinStickController playerController;


    private void Start()
    {
        playerController = transform.root.GetComponent<TwinStickController>();
    }


    private void LateUpdate()
    {
        //if(playerController.Aim == AimMode.ENABLED)
        //{
        //    if(loadout.reticle != null)
        //    {
        //        loadout.reticle.SetActive(true);
        //        loadout.UpdateReticlePosition(transform.root, 6.75f, loadout.firearms[0].firearm.range);
        //    }
        //}
        //else
        //{
        //    loadout.reticle.SetActive(false);
        //}
        
    }

    public void Activate(Animator animator, TwinStickController tsc)
    {
        animator.runtimeAnimatorController = controller;
        animator.avatar = avatar;
        tsc.moveSpeed = moveSpeed;
        //tsc.aimSpeed = aimSpeed;
        gameObject.SetActive(true);
        active = true;
    }
}


[System.Serializable]
public struct Loadout
{
    public LoadoutFirearm[] firearms;
    public LoadoutMelee[] meleeWeapons;
    [Space(5)]
    [Header("UI")]
    public GameObject reticle;
    
    public void UpdateReticlePosition(Transform target, float height, float range)
    {
        Vector3 targetPos = target.position + target.forward * (range / 2);
        targetPos.y = target.position.y + height;
        reticle.transform.position = Camera.main.WorldToScreenPoint(targetPos);

        Vector3 targetEuler = reticle.transform.localEulerAngles;
        targetEuler.z = (360f - Mathf.Abs(target.eulerAngles.y)) - 45f;
        reticle.GetComponent<RectTransform>().localEulerAngles = targetEuler;
    }
}

[System.Serializable]
public struct LoadoutFirearm
{
    public Firearm firearm;
    [Header("Holster")]
    public Transform holsterParent;
    public Vector3 holsterLocalPos;
    public Vector3 holsterLocalEuler;
    public Vector3 holsterLocalScale;
    [Header("Equipped")]
    public Transform equippedParent;
    public Vector3 equippedLocalPos;
    public Vector3 equippedLocalEuler;
    public Vector3 equippedLocalScale;
}

[System.Serializable]
public struct LoadoutMelee
{
    public Weapon weapon;
    [Header("Holster")]
    public Transform holsterParent;
    public Vector3 holsterLocalPos;
    public Vector3 holsterLocalEuler;
    public Vector3 holsterLocalScale;
    [Header("Equipped")]
    public Transform equippedParent;
    public Vector3 equippedLocalPos;
    public Vector3 equippedLocalEuler;
    public Vector3 equippedLocalScale;
}
