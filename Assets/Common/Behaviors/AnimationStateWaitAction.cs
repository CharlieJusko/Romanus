using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AnimationStateWait", story: "Wait until [Agent] animaiton state does not have [tag]", category: "Action/Animation", id: "2b5f656ac395c041cb6cf600d5f4dcc2")]
public partial class AnimationStateWaitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<string> Tag;
    [SerializeReference] public BlackboardVariable<int> LayerIndex;

    bool animationStarted = false;
    bool animationComplete = false;

    protected override Status OnStart()
    {
        animationStarted = false;
        animationComplete = false;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Animator animator = Agent.Value.GetComponent<Animator>();
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag(Tag.Value))
        {
            animationStarted = true;
        } 
        else
        {
            if(animationStarted)
            {
                animationComplete = true;
            }
        }

        if(animationComplete) return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

