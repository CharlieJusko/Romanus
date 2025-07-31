using UnityEngine;

public class CullArea : MonoBehaviour
{
    public GameObject[] objectsToCull;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ToggleRenders(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ToggleRenders(true);
        }
    }

    void ToggleRenders(bool enabled)
    {
        foreach(GameObject obj in objectsToCull)
        {
            if(obj.TryGetComponent(out MeshRenderer mr))
                mr.enabled = enabled;
        }
    }
}
