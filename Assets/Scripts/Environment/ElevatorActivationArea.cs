using UnityEngine;


public enum ElevatorArea 
{ 
    Platform,
    Top,
    Bottom
}

public class ElevatorActivationArea : MonoBehaviour
{
    public Elevator elevator;
    public ElevatorArea area;

    public bool IsPlatform { get { return area == ElevatorArea.Platform; } }
    public bool IsTop { get { return area == ElevatorArea.Top; } }
    public bool IsBottom{ get { return area == ElevatorArea.Bottom; } }


    private void Awake()
    {
        transform.parent.TryGetComponent(out elevator);
    }

    private void Start()
    {
        if(elevator == null)
            transform.parent.TryGetComponent(out elevator);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(elevator.moving)
        {
            SetActivatable(other, false);
            return;
        }
        
        if(other.CompareTag("Player"))
        {
            if(IsPlatform || (IsTop && !elevator.atTop) || (IsBottom && elevator.atTop))
                SetActivatable(other, true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(elevator.moving)
            SetActivatable(other, false);
    }

    private void OnTriggerExit(Collider other)
    {
        SetActivatable(other, false);
    }

    void SetActivatable(Collider other, bool value)
    {
        if(other.CompareTag("Player"))
        {
            elevator.canActivate = value;
            other.GetComponent<Player>().activateAction.activatable = value ? elevator : null;
        }
    }
}
