using AI.BehaviorTree;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class SmartEnemy : MonoBehaviour
{
    [Space(5)]
    [Header("Health & Death")]
    public int deathType = 1;
    public bool isDead = false;
    public LayerMask deathExcludeLayers;
    //public GameObject deathVFXPrefab;

    [Space(5)]
    [Header("Idle & Wander")]
    public bool wander = true;
    public float runSpeed = 35f;
    public float walkSpeed = 10f;
    public bool canMove = true;
    public float rotationalSpeed = 10f;
    public float waitTime = 2f;
    public float currentWaitTime = 2f;
    public float wanderDistance = 2f;
    public bool limitToStartPoint = true;

    [Space(5)]
    [Header("Player Discovery")]
    public bool playerDetected = false;
    public Transform detectionCastOrigin;
    public LayerMask playerLayerMask;
    public float detectionRadius = 10f;
    [Range(0f, 180f)]
    public float detectionAngle = 90f;

    [Space(5)]
    [Header("Attack")]
    [Range(0f, 100f)]
    public float attackRange = 15f;
    public float attackAngleSpread = 10f;
    public Vector2 atkWaitTimeMinMax = new Vector2(3f, 5f);
    private float atkWaitTime;
    public float currentAtkWaitTime = 2f;

    [Space(5)]
    [Header("VFX")]
    public GameObject[] models;
    public VFXSpawner hitFX;
    public VFXSpawner deathFX;


    [SerializeField] protected Transform player;
    [SerializeField] protected Animator animator;
    protected NavMeshAgent agent;
    protected BehaviorTree tree;

    [Space(5)]
    [Header("Armature")]
    [SerializeField] private Transform rootBone;
    public Transform midSpine;
    private List<Transform> fullArmature = new List<Transform>();

    BehaviorTree BuildCombatTree(int priority)
    {
        BehaviorTree combatTree = new BehaviorTree("Combat Tree", priority);

        Sequence engageAttackSequence = new Sequence("Engage Attack", 9);

        PrioritySelector attackOrChaseSelector = new PrioritySelector("Attack or Chase?");
        Sequence meleeAttackSequence = new Sequence("Melee Attack", 19);
        bool InAttackRange()
        {
            if(PlayerInRange(attackRange))
            {
                meleeAttackSequence.Reset();
                return true;
            }
            return false;
        }

        // If we are in attack range, attacking is our priority
        meleeAttackSequence.AddChild(new Leaf("In Attack Range?", new ConditionStrategy(InAttackRange)));
        meleeAttackSequence.AddChild(new Leaf("Clear Path", new ActionStrategy(ClearPath)));
        meleeAttackSequence.AddChild(new Leaf("Attack", new AttackStrategy(transform, animator)));
        meleeAttackSequence.AddChild(new Leaf("Reset Timer", new ActionStrategy(() => currentAtkWaitTime = 0f)));
        meleeAttackSequence.AddChild(new Leaf("New Wait Time", new ActionStrategy(() => atkWaitTime = Random.Range(atkWaitTimeMinMax.x, atkWaitTimeMinMax.y))));

        attackOrChaseSelector.AddChild(meleeAttackSequence);
        // If we aren't in attack range, we need to get into attack range
        Sequence chasePlayerSequence = new Sequence("Pursue Player", 18);
        chasePlayerSequence.AddChild(new Leaf("Update Speed", new ActionStrategy(() => agent.speed = runSpeed)));
        chasePlayerSequence.AddChild(new Leaf("Chase Player", new ChaseStrategy(transform, agent, player.gameObject, attackRange)));
        attackOrChaseSelector.AddChild(chasePlayerSequence);
        bool CanAttack()
        {
            if(Vector3.Angle(transform.forward, player.position - transform.position) <= attackAngleSpread)
            {
                if(currentAtkWaitTime >= atkWaitTime)
                {
                    engageAttackSequence.Reset();
                    return true;
                }
            }

            return false;
        }
        engageAttackSequence.AddChild(new Leaf("Can Attack?", new ConditionStrategy(CanAttack)));
        engageAttackSequence.AddChild(attackOrChaseSelector);

        // If we can't attack yet, we should try to retreat
        Sequence retreatSequence = new Sequence("Retreat", 2);
        bool InRetreatRange()
        {
            if(PlayerInRange(attackRange * 0.75f))
            {
                retreatSequence.Reset();
                return true;
            }
            return false;
        }
        retreatSequence.AddChild(new Leaf("In Range to Retreat?", new ConditionStrategy(InRetreatRange)));
        retreatSequence.AddChild(new Leaf("Update Speed", new ActionStrategy(() => agent.speed = walkSpeed)));
        retreatSequence.AddChild(new Leaf("Retreat", new CombatStrafeStrategy(transform, agent, 180f, AdjustedCombatRange * 0.95f)));

        Sequence combatIdleRangeSequence = new Sequence("Range for Comabt Idle", 1);
        bool OutCombatIdleRange()
        {
            if(!PlayerInRange(AdjustedCombatRange))
            {
                combatIdleRangeSequence.Reset();
                return true;
            }
            return false;
        }
        combatIdleRangeSequence.AddChild(new Leaf("Out of Combat Idle Range?", new ConditionStrategy(OutCombatIdleRange)));
        combatIdleRangeSequence.AddChild(new Leaf("Update Speed", new ActionStrategy(() => agent.speed = runSpeed)));
        combatIdleRangeSequence.AddChild(new Leaf("Chase Player", new ChaseStrategy(transform, agent, player.gameObject, AdjustedCombatRange * 0.95f)));

        Sequence combatIdleSequence = new Sequence("Combat Idle", 0);
        combatIdleSequence.AddChild(new Leaf("Clear Path", new ActionStrategy(ClearPath)));
        combatIdleSequence.AddChild(new Leaf("Look At Player", new LookAtStrategy(agent, player, rotationalSpeed)));

        PrioritySelector combatSelection = new PrioritySelector("Engage Melee Attack");
        combatSelection.AddChild(engageAttackSequence);
        combatSelection.AddChild(retreatSequence);
        combatSelection.AddChild(combatIdleRangeSequence);
        combatSelection.AddChild(combatIdleSequence);

        combatTree.AddChild(combatSelection);
        return combatTree;
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviorTree("Enemy Behavior Tree");

        PrioritySelector idleSelector = new PrioritySelector("Idle");

        Sequence deathSequence = new Sequence("Death", 100);
        deathSequence.AddChild(new Leaf("Is Dead?", new ConditionStrategy(() => isDead)));
        deathSequence.AddChild(new Leaf("Disable Agent", new ActionStrategy(Die)));

        Sequence patrolSequence = new Sequence("Patrol", 0);
        patrolSequence.AddChild(new Leaf("Patrol?", new ConditionStrategy(() => wander && WaitTimeReached())));
        patrolSequence.AddChild(new Leaf("Reset Timer", new ActionStrategy(() => currentWaitTime = 0f)));
        patrolSequence.AddChild(new Leaf("Update Speed", new ActionStrategy(() => agent.speed = walkSpeed)));
        patrolSequence.AddChild(new Leaf("Wander", new WanderStrategy(transform, agent, transform.position, wanderDistance, limitToStartPoint)));

        Sequence engageCombatSequence = new Sequence("Engage Combat", 8);
        engageCombatSequence.AddChild(new Leaf("Player Detected?", new ConditionStrategy(() => playerDetected)));
        var meleeCombatTree = BuildCombatTree(8);
        engageCombatSequence.AddChild(meleeCombatTree);

        idleSelector.AddChild(deathSequence);
        idleSelector.AddChild(engageCombatSequence);
        idleSelector.AddChild(patrolSequence);
        
        tree.AddChild(idleSelector);
    }

    public void ClearPath()
    {
        agent.SetDestination(transform.position);
        agent.ResetPath();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        if(rootBone != null)
            Utility.GameObjectDiscovery.ListAddTransformRecursive(fullArmature, rootBone);
        atkWaitTime = Random.Range(atkWaitTimeMinMax.x, atkWaitTimeMinMax.y);
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        Node.Status status = tree.Process();
        if(!isDead)
        {
            UpdateWaitTimer();
            DetectPlayer();
            CalculateMoveAngle();
            if(playerDetected)
            {
                UpdateAtkWaitTimer();
            }
            if(status == Node.Status.Success)
            {
                tree.Reset();
            }
        }
        if(animator.GetCurrentAnimatorStateInfo(0).IsTag("Fixed") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.35f)
        {
            ForceLookAtPlayer();
            SyncAnimation("Fixed", "Attack");
        }

        if(isDead && TryGetComponent(out ImpactDestructible ides) && ides.Destructed)
        {
            PlayDeathFX();
        }
    }

    void SyncAnimation(string tag, string targetTag)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if(state.IsTag(tag))
        {
            AnimatorStateInfo playerAnimStateInfo = player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0);
            if(playerAnimStateInfo.IsTag(targetTag))
            {
                float targetAnimRemaining = playerAnimStateInfo.normalizedTime;
                animator.Play(state.fullPathHash, 0, targetAnimRemaining);
            }
        }
    }

    public void TriggerDeath(int playerAttackType, bool _knockback, Vector3 _knockbackDirection)
    {
        deathType = playerAttackType;
        animator.SetInteger("DeathType", playerAttackType);
        isDead = true;
        animator.applyRootMotion = false;
        tree.Reset();
    }

    public void Die()
    {
        Destroy(gameObject, 25f);
        agent.enabled = false;

        animator.SetInteger("DeathType", deathType);
        animator.SetTrigger("Die");

        GetComponent<Collider>().excludeLayers = deathExcludeLayers;
        //GetComponent<Collider>().isTrigger = true;
        foreach(Collider col in GetComponentsInChildren<Collider>())
        {
            col.excludeLayers = deathExcludeLayers;
        }

        // TODO
        //player.GetComponent<LevelController>().AddKill();

        //SpawnDeathFX();
    }

    #region Common Update Functions
    private float AdjustedCombatRange { get { return attackRange * ((-0.003f * Mathf.Pow(attackRange, 2)) + 3); } }

    void UpdateWaitTimer()
    {
        if(currentWaitTime < waitTime)
        {
            currentWaitTime += Time.deltaTime;
        } 
        else
        {
            currentWaitTime = waitTime;
        }
    }

    void UpdateAtkWaitTimer()
    {
        if(currentAtkWaitTime < atkWaitTime)
        {
            currentAtkWaitTime += Time.deltaTime;
        }
        else
        {
            currentAtkWaitTime = atkWaitTime;
        }
    }

    public bool WaitTimeReached() => currentWaitTime == waitTime;

    void CalculateMoveAngle()
    {
        if(agent.velocity.magnitude > 0.01f)
        {
            //var path = agent.path.status;
            //agent.nex
            var nextPos = agent.path.corners[agent.path.corners.Length - 1];
            float moveAngle = Utility.Trigonometry.CalculateMoveAngle(nextPos, transform);
            animator.SetFloat("MoveAngle", moveAngle);
        }
    }
    #endregion

    #region Range and Detection
    void DetectPlayer()
    {
        if(playerDetected)
        {
            //if(!CanDetectPlayer())
            //{
            //    playerDetected = false;
            //}
        }

        if(CanDetectPlayer() && !playerDetected)
        {
            playerDetected = true;
        }
    }

    public void ForceLookAtPoint(Vector3 point, float speed=100f)
    {
        var lookPos = point - transform.position;
        lookPos.y = 0f;
        var targetRotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
    }

    bool CanDetectPlayer() => Utility.GameObjectDiscovery.CanDetectTarget(transform, detectionCastOrigin, player, detectionRadius, wanderDistance, playerLayerMask) && player.GetComponent<Player>().blessing.controlable;
    public void ForceLookAtPlayer(float speed=100f) => ForceLookAtPoint(player.transform.position, speed: speed);
    public bool PlayerInRange(float range) => Vector3.Distance(player.position, transform.position) <= range;

    #endregion

    #region VFX
    public void SpawnDeathFX()
    {

    }

    public void SpawnFX(GameObject fxPrefab, float height=5f, float destroyTime=2.5f)
    {
        Vector3 fxSpawnPoint = transform.position;
        //fxSpawnPoint.y += height;
        Vector3 adjustedSpawnPoint = fxSpawnPoint + (-Camera.main.transform.forward * 20f);
        var deathFX = Instantiate(fxPrefab, adjustedSpawnPoint, Quaternion.identity);
        Destroy(deathFX, destroyTime);
    }

    public void PlayDeathFX()
    {
        if(!deathFX.spawned)
        {
            var spawned = deathFX.Spawn();
            Destroy(spawned, 10f);
        }
        foreach(GameObject model in models)
        {
            model.SetActive(false);
        }
    }

    public void PlayHitFX()
    {
        var spawned = hitFX.Spawn();
        Destroy(spawned, 5f);
    }
    #endregion

    #region Collisions
    private void OnParticleCollision(GameObject particle)
    {
        bool hold = true; // TODO
        if(!isDead)
        {
            TriggerDeath(player.GetComponent<Player>().attack.AttackType, false, Vector3.zero);
        }
        else
        {
            if(hold)
            {
                var currentAnimState = animator.GetCurrentAnimatorStateInfo(0);
                if(currentAnimState.IsTag("Death") && currentAnimState.normalizedTime >= 0.52f)
                {
                    animator.Play(currentAnimState.fullPathHash, 0, 0.47f);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider;
        WeaponCollision(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        WeaponCollision(other);
    }

    private void WeaponCollision(Collider other)
    {
        if(other.CompareTag("Weapon"))
        {
            if(other.transform.TryGetComponent<Throwable>(out Throwable throwable))
            {
                deathFX.vfxPrefab = throwable.killFX;
            } 
            else if(other.transform.parent != null && other.transform.parent.TryGetComponent<Throwable>(out Throwable pThrowable))
            {
                deathFX.vfxPrefab = pThrowable.killFX;
            }

            if(hitFX.vfxPrefab != null)
            {
                PlayHitFX();
            }

            if(TryGetComponent<ImpactDestructible>(out ImpactDestructible impactable)) {
                impactable.CanDestruct = true;
            }

            if(!isDead)
            {
                TriggerDeath(player.GetComponent<Player>().attack.AttackType, true, -transform.forward);
            }
        }
    }

    public Transform GetClosestBone(Vector3 point)
    {
        Transform closestBone = rootBone;
        float distance = float.MaxValue;
        foreach(Transform bone in fullArmature)
        {
            var currentDistance = Vector3.Distance(bone.position, point);
            if(currentDistance < distance)
            {
                closestBone = bone;
                distance = currentDistance;
            }
        }

        return closestBone;
    }

    #endregion
}

[System.Serializable]
public struct VFXSpawner
{
    public GameObject vfxPrefab;
    public Transform spawnPoint;
    public bool spawned;

    public GameObject Spawn()
    {
        spawned = true;
        return GameObject.Instantiate(vfxPrefab, spawnPoint.position, Quaternion.identity);
    }
}