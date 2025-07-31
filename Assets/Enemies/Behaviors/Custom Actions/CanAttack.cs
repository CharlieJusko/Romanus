using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/CanAttack")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "CanAttack", message: "[Agent] can attack [Target]", category: "Events", id: "9144741201ae2897cc84e63f47dc3b74")]
public partial class CanAttack : EventChannelBase
{
    public delegate void CanAttackEventHandler(GameObject Agent, GameObject Target);
    public event CanAttackEventHandler Event; 

    public void SendEventMessage(GameObject Agent, GameObject Target)
    {
        Event?.Invoke(Agent, Target);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<GameObject> AgentBlackboardVariable = messageData[0] as BlackboardVariable<GameObject>;
        var Agent = AgentBlackboardVariable != null ? AgentBlackboardVariable.Value : default(GameObject);

        BlackboardVariable<GameObject> TargetBlackboardVariable = messageData[1] as BlackboardVariable<GameObject>;
        var Target = TargetBlackboardVariable != null ? TargetBlackboardVariable.Value : default(GameObject);

        Event?.Invoke(Agent, Target);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        CanAttackEventHandler del = (Agent, Target) =>
        {
            BlackboardVariable<GameObject> var0 = vars[0] as BlackboardVariable<GameObject>;
            if(var0 != null)
                var0.Value = Agent;

            BlackboardVariable<GameObject> var1 = vars[1] as BlackboardVariable<GameObject>;
            if(var1 != null)
                var1.Value = Target;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as CanAttackEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as CanAttackEventHandler;
    }
}

