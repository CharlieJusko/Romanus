using System.Collections;
using System.Collections.Generic;
using AI.BehaviorTree;
using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyActionType
{
    public const int Idle = 0;
    public const int Wander = 1;
    public const int Attack_Melee = 2;
    public const int Attack_Ranged = 3;
    public const int Chase = 4;
    public const int Stagger = 5;
    public const int Die = 6;
    public const int PoiseBreak = 7;
}

public class EnemyBehavior : MonoBehaviour
{
    [Header("Attributes")]
    [Range(0, 1f)]
    public float agility = 0.5f;
    [Range(0f, 1f)]
    public float aggression = 0.5f;
    [Range(0f, 1f)]
    public float toughness = 0.5f;

    [Space(5)]
    [Header("Idle & Wander")]
    public bool canMove = true;
    public float waitTime = 2f;
    public float currentWaitTime = 2f;
    public float wanderDistance = 2f;
    public bool limitToStartPoint = true;

    [Space(5)]
    [Header("Health & Poise")]
    public float maxHealth = 10f;
    private float currentHealth = 10f;
    // death
    public bool isDead = false;
    public LayerMask deathExcludeLayers;
    public GameObject deathVFXPrefab;
    // poise
    public int poiseMax = 3;
    public int currentPoiseDamage = 0;
    public float poiseRestTime = 2f;
    public float currentPoiseTimer = 0f;

    [Space(5)]
    [Header("Player Discovery")]
    public Transform player;
    public bool playerDetected = false;
    public LayerMask playerLayerMask;
    public float detectionRadius = 10f;
    [Range(0f, 180f)]
    public float detectionAngle = 90f;

    [Space(5)]
    [Header("Attacking")]
    //public Attack currentAttack;
    public bool triggerAttack = false; 
    public bool canAttack = false;
    public float attackDistance = 6f;
    public float maxCombatIdleDistance = 15f;
    public bool lookAtPlayerDuringAttack = false;
    public bool inLungeAttack = false;
    public bool canBeCountered = false;

    [Space(5)]
    [Header("Stamina")]
    public float maxStamina = 25f;
    public float currentStamina = 25f;
    public float staminaRegenFactor = 1.5f;
    public float attackStaminaCost = 7f;

    [Space(5)]
    [Header("Stagger")]
    public bool canStagger = true;
    public int staggerType = 0;
    //public HitType currentHitType;
    public Vector3 hitPosition;

    [Space(5)]
    [Header("Quick Time Events")]
    public bool stunned = false;

    [Space(5)]
    [Header("Outline")]
    public Transform model;
    public List<Material> attackOutlineMaterials = new List<Material>();
    public List<Material> stunOutlineMaterials = new List<Material>();
    private List<Material> defaultMaterials = new List<Material>();

    public float Health { get { return currentHealth; } }
    public int CurrentAction { get; set; }

    protected Animator animator;
    protected NavMeshAgent agent;
    protected BehaviorTree tree;


    protected void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        tree = new BehaviorTree("Enemy");

        Sequence idleWanderSequence = new Sequence("Idle/Wander", 0);
        idleWanderSequence.AddChild(new Leaf("Wander?", new ConditionStrategy(WaitTimeReached)));
        idleWanderSequence.AddChild(new Leaf("Reset Timer", new ActionStrategy(() => currentWaitTime = 0f)));
        idleWanderSequence.AddChild(new Leaf("Wander", new WanderStrategy(transform, agent, transform.position, wanderDistance, limitToStartPoint)));

        Sequence enterCombatSequence = new Sequence("Enter Combat", 10);
        enterCombatSequence.AddChild(new Leaf("In Combat?", new ConditionStrategy(() => playerDetected)));
        enterCombatSequence.AddChild(new Leaf("Reset Wander Path", new ActionStrategy(ClearPath)));

        BasicCombatBehaviorTree combatTree = new BasicCombatBehaviorTree("Combat Tree", 0, this);
        enterCombatSequence.AddChild(combatTree);

        Sequence hitReactionSequence = new Sequence("Hit Reaction", 20);
        hitReactionSequence.AddChild(new Leaf("Hit?", new ConditionStrategy(() => CurrentAction == EnemyActionType.Stagger)));
        hitReactionSequence.AddChild(new Leaf("Reset VFX", new ActionStrategy(ResetAllMeleeStrikes)));
        hitReactionSequence.AddChild(new Leaf("Reaction", new HitReactStrategy(transform, animator)));
        hitReactionSequence.AddChild(new Leaf("Reset Action", new ActionStrategy(() => CurrentAction = EnemyActionType.Idle)));

        Sequence poiseBreakSequence = new Sequence("Poise Break", 30);
        PrioritySelector stunSelector = new PrioritySelector("Poise Break Stun");
        poiseBreakSequence.AddChild(new Leaf("Poise Broken?", new ConditionStrategy(() => currentPoiseDamage >= poiseMax)));
        poiseBreakSequence.AddChild(new Leaf("Stun", new StunStrategy(transform, animator)));

        //stunSelector

        Sequence deathSequence = new Sequence("Death", 40);
        deathSequence.AddChild(new Leaf("Is Dead?", new ConditionStrategy(() => isDead)));
        deathSequence.AddChild(new Leaf("Disable Agent", new ActionStrategy(Die)));


        PrioritySelector actionSelection = new PrioritySelector("Action Selection");
        actionSelection.AddChild(idleWanderSequence);
        actionSelection.AddChild(enterCombatSequence);
        actionSelection.AddChild(hitReactionSequence);
        actionSelection.AddChild(poiseBreakSequence);
        actionSelection.AddChild(deathSequence);

        tree.AddChild(actionSelection);
    }

    public void ClearPath()
    {
        agent.SetDestination(transform.position);
        agent.ResetPath();
    }

    public Leaf ChasePlayerLeafNode() => new Leaf("Chase Player", new ChaseStrategy(transform, agent, player.gameObject, attackDistance), 0);

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        foreach(Material mat in model.GetComponent<Renderer>().sharedMaterials)
        {
            if(!defaultMaterials.Contains(mat))
            {
                defaultMaterials.Add(mat);
            }
        }
        DisableOutline();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        Node.Status status = tree.Process();
        if(!isDead)
        {
            UpdateWaitTimer();
            DetectPlayer();
            StaminaRegen();
            PoiseTimer();

            if(status == Node.Status.Success)
            {
                tree.Reset();
            }
        }
        else
        {
            if(status == Node.Status.Running && agent.enabled)
            {
                tree.Reset();
            }
        }
        
    }

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

    public bool WaitTimeReached()
    {
        if(currentWaitTime == waitTime)
        {
            //idleWanderSequence.Reset();
            return true;
        }

        return false;
    }

    #region Range and Detection
    void DetectPlayer()
    {
        if(playerDetected)
        {
            if(!CanDetectPlayer())
            {
                playerDetected = false;
            }
        }

        if(CanDetectPlayer() && !playerDetected)
        {
            playerDetected = true;
        }
    }

    bool CanDetectPlayer()
    {
        if(Vector3.Angle(transform.forward, player.position - transform.position) <= detectionAngle)
        {
            if(Vector3.Distance(player.position, transform.position) <= detectionRadius)
            {
                return true;
            }
        }

        if(Vector3.Distance(player.position, transform.position) <= wanderDistance)
        {
            return true;
        }

        if(currentHealth < maxHealth)
        {
            return true;
        }

        return false;
    }

    public void ForceLookAtPoint(Vector3 point)
    {
        var lookPos = point - transform.position;
        lookPos.y = 0f;
        var targetRotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100f * Time.deltaTime);
    }

    public void ForceLookAtPlayer()
    {
        ForceLookAtPoint(player.transform.position);
    }

    public bool PlayerInRange(float range) => Vector3.Distance(player.position, transform.position) <= range;
    #endregion

    #region Outlines and Materials
    public void EnableOutline(bool stun=false)
    {
        List<Material> outlineMaterials = attackOutlineMaterials;
        if(stun)
        {
            outlineMaterials = stunOutlineMaterials;
        }


        Renderer renderer = model.GetComponent<Renderer>();
        Material[] materials = renderer.sharedMaterials;
        for(int index = 0; index < materials.Length; index++)
        {
            Material currentMat = materials[index];
            foreach(Material oMat in outlineMaterials)
            {
                if(currentMat.name == oMat.name)
                {
                    materials[index] = oMat;
                }
            }
        }

        renderer.sharedMaterials = materials;
    }

    public void DisableOutline()
    {
        Renderer renderer = model.GetComponent<Renderer>();
        Material[] materials = renderer.sharedMaterials;
        for(int index = 0; index < materials.Length; index++)
        {
            Material currentMat = materials[index];
            foreach(Material dMat in defaultMaterials)
            {
                if(currentMat.name == dMat.name)
                {
                    materials[index] = dMat;
                }
            }
        }

        renderer.sharedMaterials = materials;
    }
    #endregion

    #region Health and Poise
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        DamagePoise();

        if(CurrentAction == EnemyActionType.Stagger && currentHealth > 0)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(state.fullPathHash, 0, 0);
        }
        
        if(canStagger && CurrentAction != EnemyActionType.Stagger && currentHealth > 0)
        {
            CurrentAction = EnemyActionType.Stagger;
            tree.Reset();
        } 
        else if(currentHealth <= 0)
        {
            tree.Reset();
            ResetAllMeleeStrikes();
            isDead = true;
            Die();
        }
    }

    public void Die()
    {
        DisableOutline();
        Destroy(gameObject, 50f);
        agent.enabled = false;

        animator.SetInteger("Reaction", staggerType);
        animator.SetBool("Is Dead", true);

        GetComponent<Collider>().excludeLayers = deathExcludeLayers;
        foreach(Collider col in GetComponentsInChildren<Collider>())
        {
            col.excludeLayers = deathExcludeLayers;
        }
        player.GetComponent<LevelController>().AddKill();

        SpawnDeathFX();
    }

    void StaminaRegen()
    {
        if((CurrentAction == EnemyActionType.Idle || CurrentAction == EnemyActionType.Wander))
        {
            if(currentStamina < maxStamina)
            {
                currentStamina += Time.deltaTime * staminaRegenFactor;
            } 
            else
            {
                currentStamina = maxStamina;
            }
        }
    }

    public void StaminaHit()
    {
        currentStamina -= attackStaminaCost;
    }

    void PoiseTimer()
    {
        if(currentPoiseTimer < poiseRestTime)
        {
            currentPoiseTimer += Time.deltaTime;
        } else
        {
            currentPoiseTimer = poiseRestTime;
            currentPoiseDamage = 0;
        }
    }

    void DamagePoise()
    {
        currentPoiseDamage++;
        currentPoiseTimer = 0f;
        if(currentPoiseDamage >= poiseMax)
        {
            tree.Reset();
            //animator.SetTrigger("PoiseBreak");
        }
    }
    #endregion

    #region VFX
    public void SpawnDeathFX()
    {
        //Vector3 fxSpawnPoint = player.position + player.forward * 5f;
        Vector3 fxSpawnPoint = transform.position;
        fxSpawnPoint.y += 5f;
        Vector3 adjustedSpawnPoint = fxSpawnPoint + (-Camera.main.transform.forward * 20f);
        var deathFX = Instantiate(deathVFXPrefab, adjustedSpawnPoint, Quaternion.identity);
        Destroy(deathFX, 1.5f);
    }

    public void ResetAllMeleeStrikes()
    {
    }
    #endregion
}
