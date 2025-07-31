using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif


[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class TwinStickController : MonoBehaviour
{
    

    [Header("Player")]
    public bool moveEnabled = true;
    public bool inDodge = false;
    public bool isAttacking = false;
    public bool lookAtTarget = true;
    public Vector3 target;

    [Tooltip("Move speed of the character in m/s")]
    public float moveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float sprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 1f)]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float speedChangeRate = 10.0f;

    [Space(10)]
    public float twinStickAimSensitivity = 1000f;

    [Space(10)]
    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Space(10)]
    public bool gravityOn = true;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float fallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool grounded = true;

    [Tooltip("Useful for rough ground")]
    public float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject cinemachineCameraTarget;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool lockCameraPosition = true;

    // cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // player
    private float speed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 530.0f;
    private Vector3 targetDirection;
    private Vector3 dodgeDirection;

    // timeout deltatime
    private float fallTimeoutDelta;

    // animation IDs
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDFreeFall;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput playerInput;
#endif
    private Animator animator;
    private CharacterController controller;
    private StarterAssetsInputs input;
    private GameObject mainCamera;

    // aiming
    private const float threshold = 0.01f;
    //public bool Aim { get; private set; }
    public float VerticalVelocity { get { return verticalVelocity; } }

    private const string GAMEPAD_SCHEME = "Gamepad";
    private const string MOUSE_SCHEME = "KeyboardMouse";

    private bool hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }


    private void Awake()
    {
        // get a reference to our main camera
        if(mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
        playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        fallTimeoutDelta = fallTimeout;
    }

    private void Update()
    {
        hasAnimator = TryGetComponent(out animator);
    }

    private void FixedUpdate()
    {
        GravityAndFall();
        GroundedCheck();
        Move();
    }

    void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDFreeFall = Animator.StringToHash("FreeFall");
    }

    void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if(hasAnimator)
        {
            animator.SetBool(animIDGrounded, grounded);
        }
    }

    void GravityAndFall()
    {
        if(grounded)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = fallTimeout;

            // update animator if using character
            if(hasAnimator)
            {
                animator.SetBool(animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if(verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

        } 
        else
        {
            // fall timeout
            if(fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            } 
            else
            {
                // update animator if using character
                if(hasAnimator)
                {
                    animator.SetBool(animIDFreeFall, true);
                }
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if(verticalVelocity < terminalVelocity && gravityOn)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    void SpeedChange()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = moveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if(input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = input.analogMovement ? input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if(currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * speedChangeRate);

            // round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;
        } 
        else
        {
            speed = targetSpeed;
        }
    }

    void Move()
    {
        SpeedChange();

        // normalize input direction
        Vector3 inputDirection = new Vector3(input.move.x, 0.0f, input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        //float rotation;
        if(moveEnabled && input.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        if(moveEnabled && input.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        if(!inDodge)
        {
            dodgeDirection = -transform.forward;
            if(input.move != Vector2.zero)
            {
                dodgeDirection = targetDirection;
            }
        }

        targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        Vector3 moveDirection = targetDirection;
        float moveSpeed = moveEnabled ? speed : 0f;

        if(moveEnabled || inDodge)
        {
            if(isAttacking && input.move != Vector2.zero)
                moveSpeed = 0;

            if(inDodge)
            {
                moveSpeed = sprintSpeed;
                moveDirection = dodgeDirection;
            }

            float moveAngle = CalculateMoveAngle(moveDirection);

            if(!gravityOn)
                verticalVelocity = 0f;

            if(hasAnimator)
                animator.SetFloat("MoveAngle", moveAngle);

            controller.Move(moveDirection.normalized * (moveSpeed * Time.deltaTime) +
                                new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
        }

        if(!moveEnabled)
            controller.Move(new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        animationBlend = Mathf.Lerp(animationBlend, moveSpeed, Time.deltaTime * speedChangeRate);
        if(animationBlend < 0.01f) 
            animationBlend = 0f;

        if(hasAnimator)
            animator.SetFloat(animIDSpeed, animationBlend);
    }

    float CalculateMoveAngle(Vector3 moveDirection)
    {
        float moveAngle = Vector3.Angle(moveDirection.normalized, transform.forward);
        Vector3 crossProd = Vector3.Cross(moveDirection.normalized, transform.forward);
        if(crossProd.y < 0f)
        {
            return moveAngle;
        }
        return -moveAngle;
    }

    public void OnFootstep()
    {
        if(footstepAudioClips.Length > 0 && moveEnabled && speed > threshold)
        {
            var index = Random.Range(0, footstepAudioClips.Length);
            if(TryGetComponent(out AudioSource audioSource))
            {
                audioSource.volume = footstepAudioVolume;
                audioSource.clip = footstepAudioClips[index];
                audioSource.Play();
            }
        }
    }
}
