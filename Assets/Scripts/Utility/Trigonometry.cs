using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows;

namespace Utility
{
    public static class Trigonometry 
    {
        public static float CalculateMoveAngle(Vector3 targetPos, Transform transform)
        {
            Vector3 targetDirection = targetPos - transform.position;
            float moveAngle = (Vector3.Angle(targetDirection.normalized, transform.forward));

            if(45f <= transform.eulerAngles.y && transform.eulerAngles.y < 135f)
            {
                if(Mathf.Abs(targetDirection.x) > Mathf.Abs(targetDirection.z))
                {
                    moveAngle = targetDirection.x > 0 ? (moveAngle * -1) : moveAngle;
                } 
                else
                {
                    moveAngle = targetDirection.z < 0 ? moveAngle : (moveAngle * -1);
                }
            } 
            else if(135f <= transform.eulerAngles.y && transform.eulerAngles.y < 225f)
            {
                if(Mathf.Abs(targetDirection.x) > Mathf.Abs(targetDirection.z))
                {
                    moveAngle = targetDirection.x > 0 ? (moveAngle * -1) : moveAngle;
                } 
                else
                {
                    moveAngle = targetDirection.z < 0 ? (moveAngle * -1) : moveAngle;
                }
            } 
            else if(225f <= transform.eulerAngles.y && transform.eulerAngles.y < 315f)
            {
                if(Mathf.Abs(targetDirection.x) > Mathf.Abs(targetDirection.z))
                {
                    moveAngle = targetDirection.x > 0 ? (moveAngle * -1) : moveAngle;
                } 
                else
                {
                    moveAngle = targetDirection.z < 0 ? (moveAngle * -1) : moveAngle;
                }
            } 
            else
            {
                if(Mathf.Abs(targetDirection.x) > Mathf.Abs(targetDirection.z))
                {
                    moveAngle = targetDirection.x < 0 ? (moveAngle * -1) : moveAngle;
                } 
                else
                {
                    moveAngle = targetDirection.z > 0 ? (moveAngle * -1) : moveAngle;
                }
            }

            return moveAngle;
        }

        public static Vector3 CalculateTargetPointWithinRadius(float minorRadius, float majorRadius, Vector3 center, Vector3 target)
        {
            float distance = Vector3.Distance(target, center);

            // Too close
            if(distance < minorRadius)
            {
                Vector3 directionToCenter = (center - target).normalized;
                float maxNeeded = majorRadius - distance;
                float minNeeded = minorRadius - distance;
                Vector3 correctedPos = target + (directionToCenter * Random.Range(minNeeded, maxNeeded));
                return correctedPos;
            } 
            // Too far
            else if(distance > majorRadius)
            {
                Vector3 directionToCenter = (center - target).normalized;
                float maxNeeded = distance - minorRadius;
                float minNeeded = distance - majorRadius;
                Vector3 correctedPos = target + (directionToCenter * Random.Range(minNeeded, maxNeeded));
                return correctedPos;
            }

            // Already within radius
            return target;
        }
    }
}
