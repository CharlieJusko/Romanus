using UnityEngine;
using UnityEngine.AI;

namespace AI.BehaviorTree
{
    public abstract class CombatActions
    {
        public const int IDLE = 0;
        public const int ATTACK_MELEE = 1;
        public const int ATTACK_RANGED = 2;
        public const int WANDER = 3;
        public const int PURSUE = 4;
    }

    public class BasicCombatBehaviorTree : BehaviorTree
    {
        EnemyBehavior enemy;

        public int CurrentAction { get; set; }
        
        public BasicCombatBehaviorTree(string name, int priority, EnemyBehavior _enemy) : base(name, priority)
        {
            enemy = _enemy;

            CurrentAction = CombatActions.ATTACK_MELEE;

            PrioritySelector combatSelection = new PrioritySelector("Combat Selection");
            Sequence attackingSequence = new Sequence("Attacking", 30);
            PrioritySelector attackPursueSelection = new PrioritySelector("Attack/Pursue Selection");
            Sequence attackSubSequence = new Sequence("Attack", 10);

            bool InAttackRange()
            {
                if(enemy.PlayerInRange(enemy.attackDistance))
                {
                    attackSubSequence.Reset();
                    return true;
                }
                return false;
            }
            attackSubSequence.AddChild(new Leaf("In Attack Distance?", new ConditionStrategy(InAttackRange)));
            attackSubSequence.AddChild(new Leaf("Attack!", new AttackStrategy(enemy.transform, enemy.GetComponent<Animator>())));

            attackPursueSelection.AddChild(attackSubSequence);
            attackPursueSelection.AddChild(enemy.ChasePlayerLeafNode());

            bool TriggerAttack()
            {
                if(enemy.triggerAttack || CurrentAction == CombatActions.ATTACK_MELEE || CurrentAction == CombatActions.ATTACK_RANGED)
                {
                    CurrentAction = CombatActions.ATTACK_MELEE;
                    return true;
                }
                return false;
            }
            attackingSequence.AddChild(new Leaf("Attack Triggered?", new ConditionStrategy(TriggerAttack)));
            attackingSequence.AddChild(attackPursueSelection);
            //attackingSequence.AddChild(new Leaf("Calculate Next Action", new ActionStrategy(DetermineNextAction)));

            Sequence strafeSequence = new Sequence("Combat Wander", 20);
            strafeSequence.AddChild(new Leaf("Strafe?", new ConditionStrategy(() => CurrentAction == CombatActions.WANDER)));
            strafeSequence.AddChild(new Leaf("Move", new CombatStrafeStrategy(enemy.transform, enemy.GetComponent<NavMeshAgent>(), 45f, enemy.wanderDistance/2)));
            //strafeSequence.AddChild(new Leaf("Calculate Next Action", new ActionStrategy(DetermineNextAction)));

            Sequence idleSequence = new Sequence("Combat Idle", 10);
            bool InCombatIdleRange()
            {
                if(enemy.PlayerInRange(enemy.maxCombatIdleDistance))
                {
                    idleSequence.Reset();
                    return true;
                }
                return false;
            }
            idleSequence.AddChild(new Leaf("In Idle Range?", new ConditionStrategy(InCombatIdleRange)));
            idleSequence.AddChild(new Leaf("Clear Path", new ActionStrategy(enemy.ClearPath)));
            //idleSequence.AddChild(new Leaf("Look At Player", new LookAtStrategy(enemy.agent, enemy.player, 50f)));

            combatSelection.AddChild(attackingSequence);
            combatSelection.AddChild(strafeSequence);
            combatSelection.AddChild(idleSequence);
            combatSelection.AddChild(enemy.ChasePlayerLeafNode());

            AddChild(combatSelection);
        }

        public override void Reset()
        {
            base.Reset();
            DetermineNextAction();
        }

        public void DetermineNextAction()
        {
            int targetAction = CombatActions.IDLE;

            // Adjust action probabilities based on parameters
            float idleChance = 1f; // Base chance to idle
            float moveChance = 0.15f + enemy.agility; // Base chance to move
            float attackChance = 2f + enemy.aggression; // Base chance to attack, Always needs some that's why we add not multiply
            float rangedAttackChance = enemy.aggression + enemy.agility;

            // Calculate total probability
            float totalChance = idleChance + moveChance + attackChance + rangedAttackChance;

            // Generate a random value to determine the action
            Random.InitState(System.DateTime.Now.Millisecond);
            float randomValue = Random.value * totalChance;

            // Determine the action based on the random value
            if(randomValue < idleChance)
            {
                targetAction = CombatActions.IDLE;
            } 
            else if(randomValue < idleChance + moveChance)
            {
                targetAction = CombatActions.WANDER;
            } 
            else if(ShouldAttack())
            {
                if(randomValue < idleChance + moveChance + attackChance)
                {
                    targetAction = CombatActions.ATTACK_MELEE;
                } 
                else
                {
                    targetAction = CombatActions.ATTACK_RANGED;
                }
            }

            if(targetAction == CombatActions.IDLE && CurrentAction == CombatActions.IDLE)
            {
                if(ShouldAttack())
                {
                    targetAction = CombatActions.ATTACK_MELEE;
                }
            }

            if(!enemy.PlayerInRange(enemy.maxCombatIdleDistance))
            {
                targetAction = CombatActions.PURSUE;
            }

            CurrentAction = targetAction;
            Debug.Log(CurrentAction);
        }

        bool ShouldAttack()
        {
            if(enemy.currentStamina < enemy.attackStaminaCost)
            {
                return false;
            }

            return true;
        }
    }
}
