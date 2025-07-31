using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(VisionController))]
public class FieldOfViewDrawer : Editor
{
    private void OnSceneGUI()
    {
        VisionController vision = (VisionController)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(vision.transform.position, Vector3.up, vision.transform.forward, 360f, vision.viewRadius);

        Vector3 viewA = vision.DirectionFromAngle(-vision.viewAngle / 2);
        Vector3 viewB = vision.DirectionFromAngle(vision.viewAngle / 2);
        Handles.DrawLine(vision.transform.position, vision.transform.position + viewA * vision.viewRadius);
        Handles.DrawLine(vision.transform.position, vision.transform.position + viewB * vision.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visibleTarget in vision.visibleTargets)
        {
            Handles.DrawLine(vision.transform.position, visibleTarget.position);
        }
    }
}
