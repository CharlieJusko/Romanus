using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Unity.MLAgents;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Strafe Around Target", story: "[Agent] strafes around [Target]", category: "Action/Navigation", id: "2ed21097eeef81746014471e342613ea")]
public partial class StrafeAroundTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam;
    [SerializeReference] public BlackboardVariable<string> AnimatorAngleParam;
    [SerializeReference] public BlackboardVariable<float> DistanceMin;
    [SerializeReference] public BlackboardVariable<float> DistanceMax;

    Vector3 targetPoint;

    protected override Status OnStart()
    {
        Agent.Value.GetComponent<NavMeshAgent>().updateRotation = false;

        targetPoint = CalculateNextPosition();
        Agent.Value.GetComponent<NavMeshAgent>().SetDestination(targetPoint);
        Agent.Value.GetComponent<NavMeshAgent>().speed = 0;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        NavMeshAgent navMeshAgent = Agent.Value.GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Speed.Value;
        if(ReachedDestination())
        {
            return Status.Success;
        }

        //var nextPos = navMeshAgent.nextPosition;
        var nextPos = navMeshAgent.path.corners[navMeshAgent.path.corners.Length - 1];
        float moveAngle = Utility.Trigonometry.CalculateMoveAngle(nextPos, Agent.Value.transform);
        Agent.Value.GetComponent<Animator>().SetFloat(AnimatorSpeedParam, navMeshAgent.velocity.magnitude);
        Agent.Value.GetComponent<Animator>().SetFloat(AnimatorAngleParam, moveAngle);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        //Agent.Value.GetComponent<NavMeshAgent>().updateRotation = true;
        Agent.Value.GetComponent<Animator>().SetFloat(AnimatorAngleParam, 0f);
        Agent.Value.GetComponent<Animator>().SetFloat(AnimatorSpeedParam, 0f);
    }

    Vector3 CalculateNextPosition()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        float distance = UnityEngine.Random.Range(DistanceMin.Value, DistanceMax.Value);
        Transform transform = Agent.Value.transform;
        Transform targetTransform = Target.Value.transform;

        Vector3 randomPoint = (UnityEngine.Random.insideUnitSphere * distance) + transform.position;
        Vector3 directionFromAgent = (randomPoint - transform.position).normalized;
        Vector3 targetPoint = transform.position + directionFromAgent * (distance + DistanceMin.Value);

        Vector3 directionFromTarget = (targetPoint - targetTransform.position).normalized;
        targetPoint = targetTransform.position + directionFromTarget * (distance + DistanceMin.Value);

        Vector3 pointOnNavMesh = AI.Utility.EnsurePositionOnNavMesh(targetPoint, distance);
        return pointOnNavMesh;
    }

    bool ReachedDestination()
    {
        NavMeshAgent agent = Agent.Value.GetComponent<NavMeshAgent>();
        return agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.magnitude == 0f);
    }
}

