using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Space(5)]
    [Header("Health & Death")]
    public int deathType = 0;
    public float maxHealth;
    public float currentHealth;
    public bool isDead = false;
    public LayerMask deathExcludeLayers;

    [Space(5)]
    [Header("Health Bar")]
    public GameObject healthBarUIPrefab;
    public Transform healthBarTarget;
    public bool showHealthBar = false;
    [SerializeField] private HealthBar healthBarUI;

    [Space(5)]
    [Header("Movement")]
    public float runSpeed = 35f;
    public float walkSpeed = 10f;
    public bool canMove = true;
    public float rotationalSpeed = 10f;

    [Space(5)]
    [Header("Player Discovery")]
    public bool playerDetected = false;
    public float currentDetectionTimer = 0f;
    [SerializeField] private float maxDetectionTimer = 2f;
    public GameObject detectionUIPrefab;
    public GameObject alertUIPrefeb;
    public Transform discoveryUITarget;
    [SerializeField] private UIGameObject discoveryUI;

    //public Transform detectionCastOrigin;
    //public LayerMask playerLayerMask;
    //public float detectionRadius = 10f;

    [Space(5)]
    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] BlackboardVariable<float> stamina;
    [SerializeField] private float staminaRegenFactor;

    [Space(5)]
    [Header("SFX")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip visceralHitClip;

    [SerializeField] protected Transform player;
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    protected BehaviorGraphAgent behaviorGraphAgent;
    protected VisionController visionController;

    BlackboardVariable<float> blackboardMaxStamina;

    public bool parried = false;

    public bool CanDamage { get; set; }


    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        //if(rootBone != null)
        //    Utility.GameObjectDiscovery.ListAddTransformRecursive(fullArmature, rootBone);
        behaviorGraphAgent.GetVariable("CurrentStamina", out stamina);
        stamina.Value = maxStamina;

        behaviorGraphAgent.GetVariable("MaxStamina", out blackboardMaxStamina);
        blackboardMaxStamina.Value = maxStamina;

        currentHealth = maxHealth;

        GameEvents.current.onEnemyHit += PlayHitSFX;
    }

    private void Update()
    {
        AnimationScan();
        if(!isDead)
        {
            //UpdateAtkWaitTimer();
            //DetectPlayer();
            //CalculateMoveAngle();
            if(playerDetected)
                UpdateStamina();
            else
                PlayerDetection();

            behaviorGraphAgent.SetVariableValue("CurrentStamina", stamina);
        }
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        //navMeshAgent.updateRotation = false;
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        TryGetComponent(out audioSource);
        TryGetComponent(out visionController);
    }

    public void ClearPath()
    {
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.ResetPath();
    }

    public void TriggerDeath(int playerAttackType, bool _knockback, Vector3 _knockbackDirection)
    {
        deathType = playerAttackType;
        animator.SetInteger("DeathType", playerAttackType);
        isDead = true;
        animator.applyRootMotion = false;
    }

    #region Animation
    public void Die()
    {
        Destroy(gameObject, 25f);

        isDead = true;

        navMeshAgent.enabled = false;

        animator.SetInteger("DeathType", deathType);
        animator.SetTrigger("Die");

        GetComponent<Collider>().excludeLayers = deathExcludeLayers;
        foreach(Collider col in GetComponentsInChildren<Collider>())
        {
            col.excludeLayers = deathExcludeLayers;
        }

        behaviorGraphAgent.SetVariableValue("Dead", isDead);
        behaviorGraphAgent.Restart();

        GameEvents.current.EnemyDeath(5);

        // TODO
        //player.GetComponent<LevelController>().AddKill();

        //SpawnDeathFX();
    }

    public void TakeDamage(float damage, int playerAttackType=0, bool parry=false, bool alert=true)
    {
        if(!CanDamage)
            return;

        if(currentHealth > 0)
        {
            currentHealth -= damage;
            if(!showHealthBar)
                SpawnHealthBar();

            if(alert && !playerDetected)
                PlayerDetected();
        }

        if(healthBarUI != null)
            healthBarUI.UpdateFillAmount(currentHealth, maxHealth);

        animator.SetFloat("Health", currentHealth);
        deathType = playerAttackType;
        if(parried)
        {
            if(playerAttackType % 2 == 0)
                animator.SetTrigger("Knockback");
            else
                animator.SetTrigger("Visceral");

            if(currentHealth <= 0)
            {
                animator.SetInteger("DeathType", deathType);
                Die();
            }
        }

        else if(currentHealth > 0)
        {
            animator.SetTrigger("Stagger");
            if(parry)
                animator.SetTrigger("Parry");
        }

        else if(!isDead)
        {
            animator.SetInteger("DeathType", deathType);
            Die();
        }
    }

    void AnimationScan()
    {
        IMoveable move;
        TryGetComponent(out move);

        void CommonFreezeMovement(bool clearPath, bool canDamage)
        {
            FreezeMovement(clearPath);
            if(move != null)
                move.CanMove = false;

            CanDamage = canDamage;
        }

        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        parried = false;
        if(stateInfo.IsTag("Hit"))
        {
            CommonFreezeMovement(true, false);
        }

        else if(stateInfo.IsTag("Stagger"))
        {
            parried = true;
            if(navMeshAgent.isActiveAndEnabled)
                ClearPath();
            behaviorGraphAgent.Restart();

            // TODO: Allow parry
            if(move != null)
                move.CanMove = true;

            CanDamage = true;
            animator.applyRootMotion = false;
        }

        else if(stateInfo.IsTag("StaggerStart"))
        {
            CommonFreezeMovement(true, false);
            parried = false;
        }

        else if(stateInfo.IsTag("StaggerEnd"))
        {
            CommonFreezeMovement(true, false);
            animator.applyRootMotion = true;
            parried = false;
        }

        else if(stateInfo.IsTag("Visceral"))
        {
            CommonFreezeMovement(true, false);
            animator.applyRootMotion = true;
        }

        else if(stateInfo.IsTag("Knockdown"))
        {
            if(move != null)
                transform.forward = -move.Direction;

            float animationTime = stateInfo.normalizedTime;
            float t;
            if(animationTime < 0.1f)
                t = 0f;
            else
                t = 5 * (animationTime * animationTime);

            if(t >= 1f)
                t = 1f;

            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().linearVelocity *= (1 - t);
            CanDamage = false;
            animator.SetBool("Knockback", false);
            animator.applyRootMotion = false;
        }

        else if(stateInfo.IsTag("KnockdownRise"))
        {
            FreezeMovement(clearPath: true);

            if(move != null)
            {
                move.CanMove = false;
                move.Stop();
            }

            CanDamage = false;

            animator.SetBool("Knockback", false);
            animator.applyRootMotion = true;
        }

        else if(stateInfo.IsTag("Attack"))
        {
            CommonFreezeMovement(false, true);
        }

        else
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            behaviorGraphAgent.SetVariableValue("CanMove", true);
            if(move != null)
            {
                move.CanMove = false;
                move.Stop();
            }

            CanDamage = true;

            animator.SetBool("Knockback", false);
        }
    }

    void FreezeMovement(bool clearPath=false)
    {
        if(clearPath && navMeshAgent.isActiveAndEnabled)
            ClearPath();

        behaviorGraphAgent.SetVariableValue("CanMove", false);

        if(clearPath)
            behaviorGraphAgent.Restart();

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
    #endregion

    #region SFX
    public void PlayHitSFX(AudioClip clip)
    {
        if(audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void PlayVisceralSFX()
    {
        PlayHitSFX(visceralHitClip);
    }
    #endregion

    #region Common Update Functions

    void UpdateStamina()
    {
        if(stamina < maxStamina)
            stamina.Value += Time.deltaTime * staminaRegenFactor;
        else
            stamina.Value = maxStamina;
    }

    public void LookAtPlayer()
    {
        Vector3 target = player.position;
        target.y = transform.position.y;
        transform.LookAt(target);
    }
    #endregion

    #region Range and Detection
    void PlayerDetection()
    {
        if(visionController != null && visionController.HasTarget(player))
        {
            if (currentDetectionTimer == 0f || discoveryUI == null)
                SpawnDetectionIndicator();

            float distance = Vector3.Distance(transform.position, player.position);
            float factor = Mathf.Lerp(1f, 0f, distance / visionController.viewRadius);

            Utility.TimerManager.Increment(maxDetectionTimer, ref currentDetectionTimer, Time.deltaTime * 3f * factor);
            discoveryUI.transform.GetChild(0).GetComponent<Image>().fillAmount = currentDetectionTimer / maxDetectionTimer;

            if(currentDetectionTimer >= maxDetectionTimer)
            {
                PlayerDetected();
            }
        } 
        else
        {
            if(currentDetectionTimer > 0f)
            {
                Utility.TimerManager.Decrement(0f, ref currentDetectionTimer, Time.deltaTime / 10f);
                if(discoveryUI != null)
                    discoveryUI.transform.GetChild(0).GetComponent<Image>().fillAmount = currentDetectionTimer / maxDetectionTimer;
            }
            else
            {
                currentDetectionTimer = 0f;
                if(discoveryUI != null)
                    Destroy(discoveryUI.gameObject);
            }

        }
    }

    void PlayerDetected()
    {
        playerDetected = true;
        SpawnAlertedIndicator();
    }

    void SpawnDetectionIndicator()
    {
        if(discoveryUI == null)
        {
            discoveryUI = Instantiate(detectionUIPrefab).GetComponent<UIGameObject>();
            discoveryUI.target = discoveryUITarget;
        }
    }

    void SpawnAlertedIndicator()
    {
        if(discoveryUI != null)
            Destroy(discoveryUI.gameObject);

        discoveryUI = Instantiate(alertUIPrefeb).GetComponent<UIGameObject>();
        discoveryUI.target = discoveryUITarget;

        Destroy(discoveryUI.gameObject, 1f);
    }

    #endregion

    #region Health Bar
    void SpawnHealthBar()
    {
        if(healthBarUI == null)
        {
            healthBarUI = Instantiate(healthBarUIPrefab).GetComponent<HealthBar>();
            healthBarUI.target = healthBarTarget;
            showHealthBar = true;
        }
    }
    #endregion
}
