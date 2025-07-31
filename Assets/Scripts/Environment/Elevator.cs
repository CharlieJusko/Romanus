using StarterAssets;
using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour, IActivatable
{
    public Player player;
    public float speed = 1.5f;

    [Header("Doors and Platform")]
    public Door doorTop;
    public Door doorBottom;
    public Transform platform;
    public Vector3 localPositionTop;
    public Vector3 localPositionBottom;

    [Space(5)]
    [Header("Triggers")]
    [SerializeField] private bool trigger = false;
    public bool canActivate = false;

    [Space(5)]
    [Header("Turner")]
    public Transform elevatorTurner;
    public float turnSpeed = 10f;
    public AnimationCurve turnSmoothingCurve;

    [Space(5)]
    [Header("Utility")]
    public bool moving = false;
    public bool atTop = true;

    public bool CanActivate { get { return canActivate; } set { canActivate = value; } }


    private void FixedUpdate()
    {
        if(trigger)
            Activate();
    }

    public void Activate()
    {
        CanActivate = false;
        moving = false;
        trigger = false;
        StartCoroutine(RideElevator());
    }

    private IEnumerator RideElevator()
    {
        yield return StartCoroutine(ToggleDoor());
        Door door = atTop ? doorTop : doorBottom;
        door.Open = !door.Open;

        yield return StartCoroutine(Move());

        yield return StartCoroutine(ToggleDoor());
        door = atTop ? doorTop : doorBottom;
        door.Open = !door.Open;
        moving = false;
    }

    private IEnumerator Move(bool toggleTrigger=false)
    {
        Vector3 startPosition = !atTop ? localPositionBottom : localPositionTop;
        Vector3 targetPosition = atTop ? localPositionBottom : localPositionTop;
        float t = 0f;

        while(t <= 1f)
        {
            t += Time.deltaTime / speed;
            float x = Mathf.SmoothStep(0f, 1f, t);
            platform.localPosition = Vector3.Lerp(startPosition, targetPosition, x);

            //float fx = ((-0.5f) * (x*x)) + (0.5f * x);
            Vector3 localEuler = elevatorTurner.localEulerAngles;
            //localEuler.y += (fx * turnSpeed);
            localEuler.y += turnSmoothingCurve.Evaluate(x);
            elevatorTurner.localEulerAngles = localEuler;

            yield return null;
        }

        platform.localPosition = targetPosition;

        atTop = !atTop;

        if(toggleTrigger)
            trigger = false;
    }

    private IEnumerator ToggleDoor()
    {
        Door door = atTop ? doorTop : doorBottom;
        float t = 0f;

        while(t <= 1f)
        {
            t += Time.deltaTime / (speed / 3f);
            door.Lerp(Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}
