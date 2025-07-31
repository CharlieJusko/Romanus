using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public static class GameObjectDiscovery
    {
        public static void ListAddTransformRecursive(List<Transform> list, Transform startingTransform)
        {
            list.Add(startingTransform);
            foreach(Transform child in startingTransform)
            {
                ListAddTransformRecursive(list, child);
            }
        }

        public static GameObject FindDirectChildWithTag(GameObject parent, string tag)
        {
            GameObject child = null;

            foreach(Transform transform in parent.transform)
            {
                if(transform.CompareTag(tag))
                {
                    child = transform.gameObject;
                    break;
                }
            }

            return child;
        }

        public static bool CanDetectTarget(Transform originEntity, Transform originPoint, Transform target, float sphereRadius, float minRadius, LayerMask targetLayermask)
        {
            float scale = originEntity.GetComponent<CapsuleCollider>().height / 2;
            if(Physics.SphereCast(originPoint.position, scale, originEntity.forward, out RaycastHit hit, sphereRadius))
            {
                if(hit.collider.transform == target)
                {
                    return true;
                }
            }

            if(Vector3.Distance(target.position, originEntity.position) <= minRadius)
            {
                return true;
            }

            return false;


            //if(Vector3.Angle(origin.forward, target.position - origin.position) <= angle)
            //{
            //    if(Vector3.Distance(target.position, origin.position) <= maxRadius)
            //    {
            //        return true;
            //    }
            //}

            //if(Vector3.Distance(target.position, origin.position) <= minRadius)
            //{
            //    return true;
            //}

            //return false;
        }

        public static Transform ClosestObject(Transform origin, Collider[] colliders)
        {
            if(colliders.Length == 0)
            {
                return origin;
            }

            Transform closest = colliders[0].transform;
            foreach(Collider col in colliders)
            {
                if(Vector3.Distance(origin.position, col.transform.position) < Vector3.Distance(origin.position, closest.position))
                {
                    closest = col.transform;
                }
            }

            return closest;
        }
    }
}
