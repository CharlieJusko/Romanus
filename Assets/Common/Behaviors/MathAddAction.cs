using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MathAdd", story: "Add [amount] to [variable]", category: "Action", id: "3543a8e9205ed5ee3bbf0d8a5bb30d1a")]
public partial class MathAddAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Amount;
    [SerializeReference] public BlackboardVariable<float> Variable;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Variable.Value += Amount.Value;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

