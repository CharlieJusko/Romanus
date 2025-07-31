using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ReloadPlayerAction : IPlayerAction
{
    public Transform player;
    private TwinStickController twinStickController;
    private Animator animator;
    private StarterAssetsInputs input;
    public GameObject reloadUI;

    private bool canReload = true;
    private bool inReload = false;
    private bool leftDefaultState = false;

    public bool CanCommit { get => canReload; set => canReload = value; }
    public bool InAction { get => inReload; set => inReload = value; }

    public void Initialize()
    {
        animator = player.GetComponent<Animator>();
        twinStickController = player.GetComponent<TwinStickController>();
        input = player.GetComponent<StarterAssetsInputs>();
    }

    public void Reset()
    {
        twinStickController.moveEnabled = true;

        input.reload = false;

        inReload = false;
        leftDefaultState = false;

        animator.SetBool("Reload", false);

        if(reloadUI != null ) 
            reloadUI.SetActive(false);
    }

    public void Scan()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag("Reload"))
        {
            //twinStickController.moveEnabled = false;
            leftDefaultState = true;

            if(reloadUI != null)
            {
                reloadUI.SetActive(true);
                reloadUI.GetComponent<Image>().fillAmount = state.normalizedTime / state.length;
            }
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
        if(canReload)
        {
            if(!twinStickController.grounded)
            {
                input.reload = false;
                animator.SetBool("Reoad", false);
                return;
            }

            //if(input.reload)
            //{
            //    Commit();
            //}
        }

        input.reload = false;
    }

    public void Commit()
    {
        if(!inReload)
        {
            inReload = true;
            animator.SetBool("Reload", true);
            //twinStickController.moveEnabled = false;
        }
    }
}
