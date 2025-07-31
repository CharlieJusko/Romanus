using StarterAssets;
using UnityEngine;


[System.Serializable]
public class ActivateAction : IPlayerAction
{
    public Transform player;
    public IActivatable activatable;
    public MenuManager menuManager;

    private StarterAssetsInputs input;
    private Animator animator;
    private bool leftDefaultState = false;

    [SerializeField] private bool canActivate = false;
    [SerializeField] private bool activating = false;

    public bool CanCommit { get { return activatable != null && activatable.CanActivate && canActivate; } set => canActivate = value; }
    public bool InAction { get => activating; set => activating = value; }

    public void Initialize()
    {
        input = player.GetComponent<StarterAssetsInputs>();
        animator = player.GetComponent<Animator>();

        GameEvents.current.triggerPlayerAnimation += ActivateAnimationTrigger;
    }

    public void Reset()
    {
        leftDefaultState = false;
        activating = false;
        GameEvents.current.OnEnablePlayerMovement();
    }

    public void Scan()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag("ActivateStart"))
        {
            GameEvents.current.OnDisablePlayerMovement();
            leftDefaultState = true;
            activating = true;
        }
        else if(state.IsTag("Activate"))
        {
            activating = true;
            GameEvents.current.OnDisablePlayerMovement();
        }
        else if(state.IsTag("ActivateEnd"))
        {
            activating = true;
            GameEvents.current.OnDisablePlayerMovement();
        } 
        else
        {
            if(leftDefaultState)
                Reset();
        }
    }

    public void Update()
    {
        Scan();
        menuManager.SetActivationIndicator(CanCommit);
        if(input.activate)
        {
            if(CanCommit)
            {
                activatable.Activate();
                activating = true;
            }

            input.activate = false;
        }
    }

    public void ActivateAnimationTrigger(string triggerName=default)
    {
        if(triggerName != default)
            animator.SetTrigger(triggerName);
    }
}
