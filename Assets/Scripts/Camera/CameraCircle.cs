using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCircle : MonoBehaviour
{
    public Transform centralPoint;
    public Transform cameraTarget;
    public float smoothing = 3f;

    private float radius;


    private void Start()
    {
        Vector3 center = new Vector3(centralPoint.position.x, 0f, centralPoint.position.z);
        Vector3 camera = new Vector3(transform.position.x, 0f, transform.position.z);
        radius = Vector3.Distance(center, camera);
    }

    private void LateUpdate()
    {
        Circle();
    }

    void Circle()
    {

        // Ignore the height value
        Vector3 center = new Vector3(centralPoint.position.x, transform.position.y, centralPoint.position.z);
        Vector3 target = new Vector3(cameraTarget.position.x, transform.position.y, cameraTarget.position.z);

        Vector3 targetDirection = (target - center).normalized;
        Vector3 targetPosition = cameraTarget.position + targetDirection * radius;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);

    }
}
