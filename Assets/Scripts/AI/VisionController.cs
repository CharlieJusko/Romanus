using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionController : MonoBehaviour
{
    public float viewRadius = 30f;
    [Range(0f, 360f)]
    public float viewAngle = 90f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();


    private void Start()
    {
        StartCoroutine(ViewWithDelay(0.1f));
    }

    IEnumerator ViewWithDelay(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (Collider target in targets)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, direction) < viewAngle / 2)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if(!Physics.Raycast(transform.position, direction, distance, obstacleMask))
                    visibleTargets.Add(target.transform);
            }
        }
    }

    public Vector3 DirectionFromAngle(float angleDegree)
    {
        float directionAngle = angleDegree + transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(directionAngle * Mathf.Deg2Rad), 0, Mathf.Cos(directionAngle * Mathf.Deg2Rad));
    }

    public bool HasTarget(Transform target) => visibleTargets.Contains(target);
}
