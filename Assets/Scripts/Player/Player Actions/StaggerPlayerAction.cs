using UnityEngine;

[System.Serializable]
public class StaggerPlayerAction : IPlayerAction
{
    public Transform player;
    private TwinStickController twinStickController;
    private Animator animator;

    private bool leftDefaultState = false;

    public bool CanCommit { get; set; }
    public bool InAction { get; set; }
    public bool Trigger { get; set; }

    public void Initialize()
    {
        animator = player.GetComponent<Animator>();
        twinStickController = player.GetComponent<TwinStickController>();
    }

    public void Reset()
    {
        twinStickController.moveEnabled = true;
        leftDefaultState = false;
    }

    public void Scan()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag("Hit"))
        {
            twinStickController.moveEnabled = false;
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
        //throw new System.NotImplementedException();
    }
}
