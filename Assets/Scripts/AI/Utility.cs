using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public static class Utility
    {
        public static Vector3 EnsurePositionOnNavMesh(Vector3 target, float distance)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(target, out hit, distance, NavMesh.AllAreas);
            return hit.position;
        }
    }
}
