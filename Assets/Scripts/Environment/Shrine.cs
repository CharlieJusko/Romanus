using UnityEngine;

public class Shrine : MonoBehaviour, IActivatable
{
    public Player player;
    public float facingAngle = 15f;
    
    [SerializeField] private bool canActivate = false;
    [SerializeField] private bool active = false;

    public bool CanActivate { get { return canActivate; } set { canActivate = value; } }


    public void Activate()
    {
        // TODO
        print("Activated");
        active = true;
        GameEvents.current.TriggerPlayerAnimation("ShrineActivate");
    }

    public void Deactivate()
    {
        active = false;
        CanActivate = true;
        GameEvents.current.TriggerPlayerAnimation("ShrineActivate");
    }

    bool IsPlayerFacing(Transform player)
    {
        Vector3 correctedPosition = transform.position;
        correctedPosition.y = player.position.y;

        return Vector3.Angle(player.transform.forward, correctedPosition - player.transform.position) <= facingAngle;
    }

    void SetActivatable(bool value)
    {
        CanActivate = value;
        player.activateAction.activatable = value ? this : null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            if(IsPlayerFacing(player.transform) && !active)
                SetActivatable(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(IsPlayerFacing(player.transform))
                if(!active)
                    SetActivatable(true);
            else
                SetActivatable(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            SetActivatable(false);
            active = false;
        }
    }
}

public static class Shrines 
{
    public static float interactionDistance = 14f;
    public static bool respawnEnemies = true;
    public static Shrine currentShrine;
}