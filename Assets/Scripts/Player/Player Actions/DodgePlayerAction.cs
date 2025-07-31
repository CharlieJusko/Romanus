using StarterAssets;
using UnityEngine;


[System.Serializable]
public class DodgePlayerAction : IPlayerAction
{
    public Transform player;
    private TwinStickController twinStickController;
    private Animator animator;
    private StarterAssetsInputs input;

    [SerializeField] private bool canDodge = true;
    [SerializeField] private bool inDodge = false;
    private bool leftDefaultState = false;

    public bool CanCommit { get => canDodge; set => canDodge = value; }
    public bool InAction { get => inDodge; set => inDodge = value; }

    public void Initialize()
    {
        animator = player.GetComponent<Animator>();
        twinStickController = player.GetComponent<TwinStickController>();
        input = player.GetComponent<StarterAssetsInputs>();
    }

    public void Reset()
    {
        twinStickController.inDodge = false;
        twinStickController.moveEnabled = true;

        input.dodge = false;

        animator.SetBool("Dodge", false);

        inDodge = false;
        leftDefaultState = false;
    }

    public void Scan()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag("Dodge"))
        {
            twinStickController.moveEnabled = false;
            twinStickController.inDodge = true;
            leftDefaultState = true;
        } 
        else
        {
            if(leftDefaultState)
            {
                Reset();
            }
        }
    }

    public void Update()
    {
        Scan();
        if(canDodge)
        {
            if(!twinStickController.grounded)
            {
                input.dodge = false;
                animator.SetBool("Dodge", false);
                return;
            }

            if(input.dodge)
            {
                if(!inDodge)
                {
                    inDodge = true;
                    //animator.SetTrigger("Dodge");
                    animator.SetBool("Dodge", true);
                    twinStickController.inDodge = true;
                }
            }
        }
        input.dodge = false;
    }
}
