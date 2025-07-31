using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MathSubtract", story: "Subtract [Amount] From [Variable]", category: "Action", id: "2bcd5fec1e8000f750c76a4ac1d26001")]
public partial class MathSubtractAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Amount;
    [SerializeReference] public BlackboardVariable<float> Variable;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Variable.Value -= Amount.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

