using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MathRandomRange", story: "Set [Value] to random value from [Min] to [Max]", category: "Action", id: "79ba0db5a70165528b6121e10c53edd5")]
public partial class MathRandomRangeAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Value;
    [SerializeReference] public BlackboardVariable<float> Min;
    [SerializeReference] public BlackboardVariable<float> Max;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Value.Value = UnityEngine.Random.Range(Min.Value, Max.Value);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

