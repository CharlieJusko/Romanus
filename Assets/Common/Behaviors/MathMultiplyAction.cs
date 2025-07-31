using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MathMultiply", story: "Multiply [Amount] and [Variable]", category: "Action", id: "b9191ae471b8a97453d22c62fb51ec24")]
public partial class MathMultiplyAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Amount;
    [SerializeReference] public BlackboardVariable<float> Variable;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Variable.Value *= Amount.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

