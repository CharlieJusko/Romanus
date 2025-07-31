using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform spawnPoint;
    [Range(0f, 1f)]
    public float forwardLerp = 0.5f;

    GameObject spawned;

    public GameObject Spawn(Vector3? direction=null)
    {
        spawned = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        if(spawned.TryGetComponent(out Projectile proj))
        {
            if(!proj.enabled)
            {
                spawned.transform.parent = spawnPoint.transform;
            }
            
            if(TryGetComponent(out Firearm firearm))
            {
                proj.FiredFrom = firearm;
            }
        }

        Vector3 forward;
        if(direction != null)
            forward = Vector3.Lerp((Vector3)direction, transform.root.forward, forwardLerp);
        else
            forward = Vector3.Lerp(spawnPoint.forward, transform.root.forward, forwardLerp);

        spawned.transform.forward = forward;
        Destroy(spawned, 5f);
        return spawned;
    }

    public void Release()
    {
        spawned.transform.parent = null;
        if(spawned.TryGetComponent(out Projectile proj))
        {
            Vector3 forward = Vector3.Lerp(spawnPoint.forward, transform.root.forward, forwardLerp);
            spawned.transform.forward = forward;
            proj.enabled = true;
        }
    }

    public void SetLerpValue(float val)
    {
        forwardLerp = val;
    }
}
