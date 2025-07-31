using UnityEngine;

public class Targetable : MonoBehaviour
{
    public Renderer mr;
    public Material[] baseMaterials;
    public Material[] targetMaterials;


    private void Start()
    {
        if(mr == null)
            TryGetComponent(out mr);
    }

    public void Target()
    {
        print("Target");
        UpdateMatierals(targetMaterials);
    }

    public void Untarget()
    {
        print("UNtarget");
        UpdateMatierals(baseMaterials);
    }

    void UpdateMatierals(Material[] updateMaterials)
    {
        Material[] materials = mr.sharedMaterials;
        for(int i = 0; i < mr.sharedMaterials.Length; i++)
        {
            materials[i] = updateMaterials[i];
        }

        mr.sharedMaterials = materials;
    }
}
