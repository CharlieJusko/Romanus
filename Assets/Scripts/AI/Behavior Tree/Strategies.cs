using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public interface IStrategy
    {
        public Node.Status Process();
        public void Reset()
        {

        }
    }

    public class ConditionStrategy : IStrategy
    {
        readonly Func<bool> predicate;
        public ConditionStrategy(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class ActionStrategy : IStrategy 
    { 
        readonly Action action;
        public ActionStrategy(Action action) 
        {
            this.action = action;
        }

        public Node.Status Process()
        {
            action();
            return Node.Status.Success;
        }
    }

    public class MoveStrategy : IStrategy
    {
        protected readonly Transform entity;
        protected readonly NavMeshAgent agent;

        protected Vector3 target;
        protected bool atDestination = false;

        public MoveStrategy(Transform entity, NavMeshAgent agent)
        {
            this.entity = entity;
            this.agent = agent;
            this.agent.updatePosition = true;
            this.agent.updateRotation = true;
        }

        public virtual Node.Status Process()
        {
            if(atDestination) return Node.Status.Success;

            agent.SetDestination(target);
            if(ReachedDestination())
            {
                atDestination = true;
            }

            return Node.Status.Running;
        }

        public virtual void Reset()
        {
            atDestination = false;
            CalculateTargetPosition();
        }

        protected bool ReachedDestination() => agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.magnitude == 0f);

        protected virtual void CalculateTargetPosition() { throw new NotImplementedException(); }
    }

    public class WanderStrategy : MoveStrategy
    {
        readonly Vector3 startingPostion;
        readonly float range;
        readonly bool limitToStartPos;

        public WanderStrategy(Transform entity, NavMeshAgent agent, Vector3 startingPostion, float range, bool limitToStartPos) : base(entity, agent)
        {
            this.startingPostion = startingPostion;
            this.range = range;
            this.limitToStartPos = limitToStartPos;
            CalculateTargetPosition();
        }

        protected override void CalculateTargetPosition()
        {
            Vector3 randomPoint = UnityEngine.Random.insideUnitSphere * (range + agent.stoppingDistance);
            if(limitToStartPos)
            {
                randomPoint += startingPostion;
            } else
            {
                randomPoint += entity.position;
            }
            target = Utility.EnsurePositionOnNavMesh(randomPoint, range + agent.stoppingDistance);
        }
    }

    public class CombatStrafeStrategy : MoveStrategy
    {
        readonly float angle;
        readonly float range;

        public CombatStrafeStrategy(Transform entity, NavMeshAgent agent, float angle, float range) : base(entity, agent)
        {
            this.angle = angle;
            this.range = range;
            CalculateTargetPosition();
        }

        public override Node.Status Process()
        {
            agent.updateRotation = false;
            if(atDestination)
            {
                agent.updateRotation = true;
                return Node.Status.Success;
            }

            entity.GetComponent<SmartEnemy>().ForceLookAtPlayer();
            agent.SetDestination(target);
            if(ReachedDestination())
            {
                //agent.updateRotation = true;
                atDestination = true;
            }

            return Node.Status.Running;
        }

        protected override void CalculateTargetPosition()
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            //float targetAngle = UnityEngine.Random.Range(-angle, angle);
            //if(targetAngle < 0f)
            //{
            //    targetAngle = UnityEngine.Random.Range(-angle * 1.5f, -angle / 2);
            //} 
            //else
            //{
            //    targetAngle = UnityEngine.Random.Range(angle / 2, angle * 1.5f);
            //}
            float distance = UnityEngine.Random.Range(range * 0.75f, range) + (agent.stoppingDistance);

            Vector3 targetDirection = Quaternion.Euler(0, angle, 0) * entity.forward;
            Vector3 targetPoint = entity.position + targetDirection * distance;
            target = Utility.EnsurePositionOnNavMesh(targetPoint, range);
        }
    }

    public class ChaseStrategy : MoveStrategy
    {
        public GameObject chaseTarget;
        float range;

        public ChaseStrategy(Transform entity, NavMeshAgent agent, GameObject chaseTarget, float range) : base(entity, agent)
        {
            this.chaseTarget = chaseTarget;
            CalculateTargetPosition();
            this.range = range;
        }

        public override Node.Status Process()
        {
            if(atDestination) return Node.Status.Success;

            CalculateTargetPosition();
            agent.updateRotation = true;
            agent.SetDestination(target);
            if(ReachedDestination())
            {
                atDestination = true;
            }

            return Node.Status.Running;
        }

        protected override void CalculateTargetPosition() { 
            Vector3 direction = (entity.position - chaseTarget.transform.position).normalized;
            target = chaseTarget.transform.position + direction * range;
        }
    }

    public class LookAtStrategy : IStrategy 
    {
        Transform target;
        float rotationalSpeed = 5f;
        NavMeshAgent agent;

        public LookAtStrategy(NavMeshAgent agent, Transform target, float rotationalSpeed)
        {
            this.target = target;
            this.rotationalSpeed = rotationalSpeed;
            this.agent = agent;
        }

        public Node.Status Process()
        {
            if(Vector3.Angle(agent.transform.forward, target.position - agent.transform.position) <= 1.5f)
            {
                agent.updatePosition = true;
                return Node.Status.Success;
            }
            agent.GetComponent<SmartEnemy>().ForceLookAtPlayer(rotationalSpeed);
            //agent.updatePosition = false;
            //agent.SetDestination(target.position);

            return Node.Status.Running;
        }
    }

    public class AnimateStrategy : IStrategy
    {
        protected Transform entity;
        protected Animator animator;

        protected bool sentTrigger = false;
        protected bool animationStarted = false;
        protected bool animationComplete = false;

        public AnimateStrategy(Transform entity, Animator animator)
        {
            this.entity = entity;
            this.animator = animator;
        }

        public virtual Node.Status Process() { return Node.Status.Failure; }

        public virtual void Reset()
        {
            sentTrigger = false;
            animationStarted = false;
            animationComplete = false;
        }
    }

    public class AttackStrategy : AnimateStrategy
    {
        int comboCount = 2;
        bool comboCounted = false;

        public AttackStrategy(Transform entity, Animator animator) : base(entity, animator) 
        {
        }

        public override Node.Status Process()
        {
            //enemy.triggerAttack = false;
            if(!sentTrigger)
            {
                animator.SetInteger("Attack Type", 1);
                animator.SetInteger("ComboCount", comboCount);
                animator.SetTrigger("Attack");

                sentTrigger = true;
            }

            // Look at player during attack
            entity.GetComponent<SmartEnemy>().ForceLookAtPlayer(5f);

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if(state.IsTag("Attack") || state.IsTag("Attack_Windup"))
            {
                //enemy.canStagger = false;
                animationStarted = true;
                comboCounted = false;

            } 
            else if(state.IsTag("Attack_Swing") && !comboCounted)
            {
                //enemy.canStagger = false;
                int count = animator.GetInteger("ComboCount") - 1;
                animator.SetInteger("ComboCount", count);
                comboCounted = true;
            } 
            else if(state.IsTag("Default"))
            {
                if(animationStarted)
                {
                    animationComplete = true;
                }
            }

            if(animationComplete)
            {
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }

        public override void Reset()
        {
            base.Reset();
            comboCounted = false;
            animator.SetInteger("ComboCount", 0);
        }
    }

    public class HitReactStrategy : AnimateStrategy
    {
        public HitReactStrategy(Transform entity, Animator animator) : base(entity, animator) { }

        public override Node.Status Process()
        {
            if(!sentTrigger)
            {
                animator.SetInteger("Reaction", entity.GetComponent<EnemyBehavior>().staggerType);
                animator.SetTrigger("Hit Reaction");
                sentTrigger = true;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if(state.IsTag("Stagger"))
            {
                animationStarted = true;
            } 
            else if(state.IsTag("Default"))
            {
                if(animationStarted)
                {
                    animationComplete = true;
                }
            }

            if(animationComplete)
            {
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }
    }

    public class StunStrategy : AnimateStrategy
    {
        SmartEnemy enemy;
        public StunStrategy(Transform entity, Animator animator) : base(entity, animator) 
        { 
            enemy = entity.GetComponent<SmartEnemy>();
        }

        public override Node.Status Process()
        {
            if(!sentTrigger)
            {
                animator.SetTrigger("PoiseBreak");
                sentTrigger = true;
            }

            //enemy.CurrentAction = EnemyActionType.PoiseBreak;
            //enemy.canStagger = false;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if(state.IsTag("Stun") || state.IsTag("Poise Break"))
            {
                //enemy.stunned = true;
                //enemy.EnableOutline(true);
                animationStarted = true;
            } 
            else
            {
                if(animationStarted)
                {
                    //enemy.DisableOutline();
                    animationComplete = true;
                    //enemy.stunned = false;
                }
            }

            if(animationComplete)
            {
                //enemy.canStagger = true;
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }
    }
}
