using UnityEngine;


public enum EquipType
{
    SINGLE,
    DUAL
}

[System.Serializable]
public struct EquipTransformTarget
{
    [Header("Holstered")]
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

public interface IEquipment
{
    public GameObject Prefab { get; }
    public Sprite Icon { get; }
    public EquipType EquipType { get; }
    public EquipTransformTarget[] TransformTargets { get; }
}
