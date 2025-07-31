using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour, IEquipment
{
    public Player player;
    [SerializeField] private GameObject equipment;
    [SerializeField] private Sprite icon;
    [SerializeField] private EquipType equipType;
    [SerializeField] private EquipTransformTarget[] transformTargets;

    public GameObject Prefab { get { return equipment; } private set { equipment = value; } }
    public Sprite Icon { get { return icon; } private set { icon = value; } }
    public EquipType EquipType { get { return equipType; } private set { equipType = value; } }
    public EquipTransformTarget[] TransformTargets { get { return transformTargets; } private set { transformTargets = value; } }

    public void Equip(GameObject _equipment, int index=0)
    {
        _equipment.transform.parent = transformTargets[index].equippedParent;
        _equipment.transform.localPosition = transformTargets[index].equippedLocalPos;
        _equipment.transform.localEulerAngles = transformTargets[index].equippedLocalEuler;
        _equipment.transform.localScale = transformTargets[index].equippedLocalScale;
    }

    public void Holster(GameObject _equipment, int index=0)
    {
        _equipment.transform.parent = transformTargets[index].holsterParent;
        _equipment.transform.localPosition = transformTargets[index].holsterLocalPos;
        _equipment.transform.localEulerAngles = transformTargets[index].holsterLocalEuler;
        _equipment.transform.localScale = transformTargets[index].holsterLocalScale;
    }

    GameObject[] SpawnEquipment()
    {
        List<GameObject> spawnedEquipment = new List<GameObject>();
        if(equipType == EquipType.SINGLE)
            spawnedEquipment.Add(Instantiate(Prefab));
        else
        {
            spawnedEquipment.Add(Instantiate(Prefab));
            spawnedEquipment.Add(Instantiate(Prefab));
        }

        return spawnedEquipment.ToArray();
    }
}
