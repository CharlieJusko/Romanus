using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public enum BillboardType { HardLookAtCamera, YLookAtCamera, CameraForward, LocalYToGlobalY  };

    [SerializeField] BillboardType billboardType;

    private void LateUpdate()
    {
        switch (billboardType)
        {
            case BillboardType.HardLookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case BillboardType.YLookAtCamera:
                var lookPos = Camera.main.transform.position - transform.position;
                lookPos.y = 0f;
                var targetRotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 25f * Time.deltaTime);
                break;
            case BillboardType.LocalYToGlobalY:
                var globalFwd = Quaternion.LookRotation(Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, globalFwd, 15f * Time.deltaTime);
                break;
            default:
                break;
        }
    }
}
