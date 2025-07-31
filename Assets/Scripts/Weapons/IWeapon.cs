using UnityEngine;


public enum WieldingType
{
    SINGLE,
    DUAL
}

public enum ArmatureParent 
{
    NONE,
    HIPS,
    SPINE_TOP,
    LEFT_HAND,
    RIGHT_HAND
}

[System.Serializable]
public struct WeaponTransformTarget
{
    [Header("Holstered")]
    public Transform holsterParent;
    public ArmatureParent _holsterParent;
    public Vector3 holsterLocalPos;
    public Vector3 holsterLocalEuler;
    public Vector3 holsterLocalScale;
    [Header("Equipped")]
    public Transform equippedParent;
    public ArmatureParent _equippedParent;
    public Vector3 equippedLocalPos;
    public Vector3 equippedLocalEuler;
    public Vector3 equippedLocalScale;
}

public interface IWeapon
{
    public float Damage { get; set; }
    public bool Equipped { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon {  get; set; }
    public WieldingType WieldingType { get; set; }
    public WeaponTransformTarget[] TransformTargets { get; }

    public void Holster(Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale);
    public void Equip(Transform parent, Vector3 localPosition, Vector3 localEuler, Vector3 localScale);

    public void PlaySFX();
}
