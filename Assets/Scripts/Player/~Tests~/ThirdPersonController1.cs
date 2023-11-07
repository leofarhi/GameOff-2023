using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThirdPersonController : TemperatureBehaviour
{
    public static ThirdPersonController instance;
    [Header("Health")]
    public int maxHealth = 100;
    [SerializeField]
    private int _currentHealth = 100;

    public int currentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
            if (_currentHealth <= 0)
            {
                isDead = true;
                onDead.Invoke();
            }
        }
    }

    public bool isDead
    {
        get { return currentHealth <= 0; }
        set { currentHealth = value ? 0 : maxHealth; }
    }
    
    [HideInInspector]
    public UnityEvent onDead;
    
    [Header("Ground & Collision")]
    public bool isGrounded;
    public virtual float colliderHeight { get { return _collider.height; }
        set
        {
            _collider.height = value;
            _collider.center = Vector3.up * value * 0.5f;
        }
    }
    internal float groundDistance;
    public RaycastHit groundHit;
    public LayerMask groundLayer = 1 << 0;
    [Tooltip("The length of the Ray cast to detect ground ")]
    public float groundDetectionDistance = 10f;
    [Tooltip("Distance to became not grounded")]
    [Range(0, 10)]
    public float groundMinDistance = 0.1f;
    [Range(0, 10)]
    public float groundMaxDistance = 0.5f;

    protected float heightReached;
    protected bool applyingStepOffset;
    [Tooltip("Snaps the capsule collider to the ground surface, recommend when using complex terrains or inclined ramps")]
    public bool useSnapGround = true;
    
    protected float inputMagnitude;
    protected float moveSpeed;
    internal bool isJumping;
    
    [Header("Movement")]
    public float rotationSpeed = 10f;
    public float movementSmooth = 0.15f;
    public float walkSpeed = 1f;
    public float runningSpeed = 1.5f;
    public float sprintSpeed = 2f;
    [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, " +
             "or use with root motion as extra speed." +
             "When 'Use RootMotion' is checked, make sure to reset all speeds " +
             "to zero to use the original root motion velocity.")]
    public bool useRootMotion = false;
    
    public LayerMask stopMoveLayer;
    [Tooltip("Character will stop moving, ex: walls - set the layer to nothing to not use")]
    public float stopMoveRayDistance = 1f;
    public float stopMoveMaxHeight = 1.6f;
    public bool lockSetMoveSpeed;
    [HideInInspector]
    public bool keepDirection;
    [HideInInspector]
    public Vector3 oldInput;
    
    public enum StopMoveCheckMethod
    {
        RayCast, SphereCast, CapsuleCast
    }
    
    public StopMoveCheckMethod stopMoveCheckMethod = StopMoveCheckMethod.RayCast;
    
    [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
    public bool useContinuousSprint = true;

    [HideInInspector]
    public bool alwaysWalkByDefault;
    
    
    [Header("Slope Limit")]
    [SerializeField] public bool useSlopeLimit = true;
    [Range(30, 80)]
    [SerializeField] protected float slopeLimit = 75f;
    [SerializeField] protected float stopSlopeMargin = 20f;
    [SerializeField] protected float slopeSidewaysSmooth = 2f;
    [SerializeField] protected float slopeMinDistance = 0f;

    [SerializeField] protected float slopeMaxDistance = 1.5f;
    [SerializeField] protected float slopeLimitHeight = 0.2f;
    public RaycastHit slopeHitInfo;

    [HideInInspector]
    public bool steepSlopeAhead;


    [Range(0, 1)]
    public float snapPower = 0.5f;

    protected Vector3 input;
    protected Vector3 inputSmooth;
    protected Vector3 lastCharacterAngle;
    protected float rotationMagnitude;

    protected float speedMultiplier;
    protected float defaultSpeedMultiplier;

    protected float verticalSpeed;
    protected float horizontalSpeed;
    protected Vector3 moveDirection;
    
    internal bool isSprinting;
    internal bool isInAirborne;
    
    [Header("Step Offset")]
    [SerializeField] protected bool useStepOffset = true;
    [Tooltip("Layers that the character will perform a StepOffset")]
    public LayerMask stepOffsetLayer = 1 << 0;
    [Tooltip("Offset max height to walk on steps - YELLOW Raycast in front of the legs")]
    [Range(0, 1)]
    [SerializeField] protected float stepOffsetMaxHeight = 0.5f;
    [Tooltip("Offset min height to walk on steps. Make sure to keep slight above the floor - YELLOW Raycast in front of the legs")]
    [Range(0, 1)]
    public float stepOffsetMinHeight = 0f;
    [Tooltip("Offset distance to walk on steps - YELLOW Raycast in front of the legs")]
    [Range(0, 1)]
    public float stepOffsetDistance = 0.1f;
    protected float stopMoveWeight;
    internal float sprintWeight;
    public RaycastHit stepOffsetHit;
    
    [Header("Jump")]
    [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
    public bool jumpWithRigidbodyForce = false;
    [Tooltip("Rotate or not while airborne")]
    public bool jumpAndRotate = true;
    [Tooltip("How much time the character will be jumping")]
    public float jumpTimer = 0.3f;
    [Tooltip("Delay to match the animation anticipation")]
    public float jumpStandingDelay = 0.25f;
    internal float jumpCounter;
    internal bool inJumpStarted;
    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    [SerializeField] protected float jumpHeight = 4f;

    protected float jumpMultiplier;
    protected float timeToResetJumpMultiplier;

    [Header("Falling")]
    [Tooltip("Speed that the character will move while airborne")]
    [SerializeField] protected float airSpeed = 5f;

    [Tooltip("Smoothness of the direction while airborne")]
    [SerializeField] protected float airSmooth = 6f;
    [Tooltip("Apply extra gravity when the character is not grounded")]
    [SerializeField] protected float extraGravity = -10f;


    [Header("General Settings")]
    public InputPreset inputPreset;
    public bool lockMovement;
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private Animator _animator;
    internal bool customAction = false;
    [HideInInspector]
    public ThirdPersonCamera cameraMain;
    
    private void Awake()
    {
        instance = this;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        FindCamera();
    }

    public override void Update()
    {
        base.Update();
        if (lockMovement)return;

        MoveInput();
        SprintInput();
        JumpInput();
    }

    private void FixedUpdate()
    {
        CheckGround();
        
        ControlJumpBehaviour();
        AirControl();
        CalculateRotationMagnitude();
        ControlLocomotionType();
        ControlRotation();
    }

    public void FindCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraMain = mainCamera.gameObject.GetComponent<ThirdPersonCamera>();
            if (cameraMain != null)
            {
                cameraMain.player = this;
            }
        }
    }

    #region Control

    public virtual void ControlLocomotionType()
        {
            if (lockMovement || customAction)
            {
                return;
            }

            if (!lockSetMoveSpeed)
            {
                SetControllerMoveSpeed();
                //SetAnimatorMoveSpeed(freeSpeed);
            }

            if (!useRootMotion)
            {
                MoveCharacter(moveDirection);
            }
        }

        /// <summary>
        /// Manage the Control Rotation Type of the Player
        /// </summary>
        public virtual void ControlRotationType()
        {
            if (customAction)
            {
                return;
            }
            RotateToDirection(moveDirection);
        }

        /// <summary>
        /// Use it to keep the direction the Player is moving (most used with CCV camera)
        /// </summary>
        public virtual void ControlKeepDirection()
        {
            // update oldInput to compare with current Input if keepDirection is true
            if (!keepDirection)
            {
                oldInput = input;
            }
            else if ((input.magnitude < 0.01f || Vector3.Distance(oldInput, input) > 0.9f) && keepDirection)
            {
                keepDirection = false;
            }
        }

        /// <summary>
        /// Determine the direction the player will face based on input and the referenceTransform
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {

            if (referenceTransform)// && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
                var moveDirectionRaw= (input.x * right) + (input.z * forward);
                SetInputDirection(moveDirectionRaw);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
                var moveDirectionRaw = new Vector3(input.x, 0, input.z);
                SetInputDirection(moveDirectionRaw);
            }
        }

        public virtual void SetInputDirection(Vector3 direction)
        {
            float movementDirection = direction.magnitude >= .1 ? transform.forward.AngleFormOtherDirection(direction).y : 0;
            //if (vAnimatorParameters.InputDirection != -1) animator.SetFloat(vAnimatorParameters.InputDirection, movementDirection);
        }
        
        public virtual void Sprint(bool value)
        {
            var sprintConditions = ( hasMovementInput &&
                !((horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (hasMovementInput)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                        if (isSprinting)
                        {
                            alwaysWalkByDefault = false;
                        }
                    }
                    else if (!isSprinting)
                    {
                        alwaysWalkByDefault = false;
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting && (!useContinuousSprint || !sprintConditions))
            {

                isSprinting = false;
            }
        }
        
        public virtual void Jump()
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
            {
                StartCoroutine(DelayToJump());
                //animator.CrossFadeInFixedTime("Jump", 0.1f);
            }
            else
            {
                isJumping = true;
                //animator.CrossFadeInFixedTime("JumpMove", .2f);
            }
        }

        protected IEnumerator DelayToJump()
        {
            inJumpStarted = true;
            yield return new WaitForSeconds(jumpStandingDelay);
            isJumping = true;
            inJumpStarted = false;
        }

    #endregion
    
    #region Input
    
    public virtual void MoveInput()
    {
        input.x = inputPreset.horizontalInput.GetAxisRaw();
        input.z = inputPreset.verticalInput.GetAxisRaw();

        ControlKeepDirection();
    }
    
    public virtual void ControlRotation()
    {
        if (cameraMain)
        {
            if (!keepDirection)
            {
                UpdateMoveDirection(cameraMain.transform);
            }
        }
        ControlRotationType(); // control character rotation
    }
    
    public virtual void SprintInput()
    {
        if (inputPreset.sprintInput.useInput)
        {
            Sprint(useContinuousSprint ? inputPreset.sprintInput.GetButtonDown() : inputPreset.sprintInput.GetButton());
        }
    }
    
    public virtual bool JumpConditions()
    {
        return !inJumpStarted && !customAction && isGrounded && GroundAngle() < slopeLimit && !isJumping;
    }
    
    public virtual void JumpInput()
    {
        if (inputPreset.jumpInput.GetButtonDown() && JumpConditions())
        {
            Jump();
        }
    }
    
    #endregion
    
    #region Ground Check

    protected virtual void CheckGround()
    {
        CheckGroundDistance();

        if (isDead || customAction)
        {
            if (!isDead || isGrounded)
            {
                isGrounded = true;
                heightReached = transform.position.y;
                return;
            }
        }

        if (groundDistance <= groundMinDistance)
        {
            //CheckFallDamage();
            isGrounded = true;
            if (!useSnapGround && !applyingStepOffset && !isJumping && groundDistance > 0.05f && extraGravity != 0)
            {
                _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
            }

            heightReached = transform.position.y;
        }
        else
        {
            if (groundDistance >= groundMaxDistance)
            {
                isGrounded = false;
                // apply extra gravity when falling
                if (!applyingStepOffset && !isJumping && extraGravity != 0)
                {
                    _rigidbody.AddForce(transform.up * extraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
            }
            else if (!applyingStepOffset && !isJumping && extraGravity != 0)
            {
                _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
            }
        }
    }
    
    protected virtual void CheckGroundDistance()
    {
        if (isDead && isGrounded)
        {
            return;
        }

        if (_collider != null)
        {
            Vector3 bottomCollider = transform.position + _collider.center + Vector3.up * -_collider.height * 0.5F * transform.localScale.y;
            // radius of the SphereCast
            float radius = _collider.radius * 0.9f;
            var dist = groundDetectionDistance;
            // ray for RayCast
            Ray ray2 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
            // raycast for check the ground distance
            if (Physics.Raycast(ray2, out groundHit, (colliderHeight / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
            {
                dist = (bottomCollider.y - groundHit.point.y);
            }
            // sphere cast around the base of the capsule to check the ground distance
            if (dist >= groundMinDistance && dist <= groundMaxDistance)
            {
                Vector3 pos = transform.position + Vector3.up * (_collider.radius);
                Ray ray = new Ray(pos, -Vector3.up);
                if (Physics.SphereCast(ray, radius, out groundHit, _collider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
                {
                    Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                    float newDist = bottomCollider.y - groundHit.point.y;
                    if (dist > newDist)
                    {
                        dist = newDist;
                    }
                }
            }
            groundDistance = (float)System.Math.Round(dist, 2);
        }
    }
    
    public virtual float GroundAngle()
    {
        var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        return groundAngle;
    }
    
    protected virtual bool CheckForSlope(ref Vector3 targetVelocity)
    {

        if (!useSlopeLimit || moveDirection.magnitude == 0f || targetVelocity.magnitude == 0f)
        {
            slopeSidewaysSmooth = 1f;
            return false;
        }

        // DYNAMIC LINE
        if (Physics.Linecast(transform.position + Vector3.up * (_collider.height * slopeLimitHeight), transform.position + moveDirection.normalized *
            (steepSlopeAhead ? _collider.radius + slopeMaxDistance : _collider.radius + slopeMinDistance), out slopeHitInfo, groundLayer))
        {
            var hitAngle = Vector3.Angle(Vector3.up, slopeHitInfo.normal);

            if (hitAngle > slopeLimit && hitAngle < 85f)
            {
                var normal = slopeHitInfo.normal;
                normal.y = 0f;
                var normalAngle = targetVelocity.normalized.AngleFormOtherDirection(-normal.normalized);
                var dir = Quaternion.AngleAxis(normalAngle.y > 0f ? 90f : -90, Vector3.up) * normal.normalized * targetVelocity.magnitude;

                if (Mathf.Abs(normalAngle.y) > stopSlopeMargin)
                {
                    slopeSidewaysSmooth = Mathf.Clamp(slopeSidewaysSmooth - Time.deltaTime * slopeSidewaysSmooth, 0f, 1f);
                }
                else
                {
                    slopeSidewaysSmooth = 1f;
                }

                targetVelocity = Vector3.Lerp(dir, Vector3.zero, slopeSidewaysSmooth);
                return true;
            }
        }

        slopeSidewaysSmooth = 1f;
        return false;
    }

    #endregion
    
    #region Locomotion

        protected virtual void CalculateRotationMagnitude()
        {
            var eulerDifference = this.transform.eulerAngles - lastCharacterAngle;
            if (eulerDifference.sqrMagnitude < 0.01)
            {
                lastCharacterAngle = transform.eulerAngles;
                rotationMagnitude = 0f;
                return;
            }

            var magnitude = (eulerDifference.NormalizeAngle().y / rotationSpeed);
            rotationMagnitude = (float)System.Math.Round(magnitude, 2);
            lastCharacterAngle = transform.eulerAngles;
        }

        public virtual void SetControllerSpeedMultiplier(float speed)
        {
            this.speedMultiplier = speed;
        }

        public virtual void ResetControllerSpeedMultiplier()
        {
            this.speedMultiplier = defaultSpeedMultiplier;
        }

        public virtual void SetControllerMoveSpeed()
        {
            if (alwaysWalkByDefault)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? runningSpeed : walkSpeed, movementSmooth * Time.fixedDeltaTime);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? sprintSpeed : runningSpeed, movementSmooth * Time.fixedDeltaTime);
            }
        }

        public virtual void MoveCharacter(Vector3 direction)
        {
            // calculate input smooth
            inputSmooth = Vector3.Lerp(inputSmooth, input, movementSmooth * (useRootMotion ? Time.deltaTime : Time.fixedDeltaTime));

            if (!isGrounded || isJumping || _rigidbody.isKinematic)
            {
                return;
            }
            var _direction = transform.forward;
            _direction.y = 0;
            _direction = _direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);

            Vector3 targetPosition = (useRootMotion ? _animator.rootPosition : _rigidbody.position) + _direction * (moveSpeed * speedMultiplier) * (useRootMotion ? Time.deltaTime : Time.fixedDeltaTime);
            Vector3 targetVelocity = (targetPosition - transform.position) / (useRootMotion ? Time.deltaTime : Time.fixedDeltaTime);

            bool useVerticalVelocity = true;

            SnapToGround(ref targetVelocity, ref useVerticalVelocity);

            steepSlopeAhead = CheckForSlope(ref targetVelocity);

            if (!steepSlopeAhead)
            {
                CalculateStepOffset(_direction.normalized, ref targetVelocity, ref useVerticalVelocity);
            }

            CheckStopMove(ref targetVelocity);
            if (useVerticalVelocity)
            {
                targetVelocity.y = _rigidbody.velocity.y;
            }
            
            Debug.Log("targetVelocity 2: " + targetVelocity);

            _rigidbody.velocity = targetVelocity;
        }

        protected virtual void CheckStopMove(ref Vector3 targetVelocity)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + transform.up * _collider.radius;
            Vector3 direction = moveDirection.normalized;
            direction = Vector3.ProjectOnPlane(direction, groundHit.normal);
            float distance = _collider.radius + 1;
            float targetStopWeight = 0;
            float smooth = movementSmooth;
            bool checkStopMoveCondition = isGrounded && !isJumping && !isInAirborne && !applyingStepOffset && !customAction;

            if (steepSlopeAhead)
            {
                targetStopWeight = 1f * slopeSidewaysSmooth;
            }
            else if (checkStopMoveCondition && CheckStopMove(direction, out hit))
            {
                var angle = Vector3.Angle(direction, -hit.normal);
                if (angle < slopeLimit)
                {
                    float dst = hit.distance - _collider.radius;
                    targetStopWeight = (1.0f - dst);
                }
                else
                {
                    targetStopWeight = -0.01f;
                }
            }
            else
            {
                targetStopWeight = -0.01f;
            }
            stopMoveWeight = Mathf.Lerp(stopMoveWeight, targetStopWeight, smooth * Time.deltaTime);
            stopMoveWeight = Mathf.Clamp(stopMoveWeight, 0f, 1f);

            targetVelocity = Vector3.LerpUnclamped(targetVelocity, Vector3.zero, stopMoveWeight);
        }

        protected virtual bool CheckStopMove(Vector3 direction, out RaycastHit hit)
        {
            Vector3 origin = transform.position + transform.up * _collider.radius;
            float distance = _collider.radius + stopMoveRayDistance;
            switch (stopMoveCheckMethod)
            {
                case StopMoveCheckMethod.SphereCast:

                case StopMoveCheckMethod.CapsuleCast:
                    Vector3 p1 = origin + transform.up * (slopeLimitHeight);
                    Vector3 p2 = origin + transform.up * (stopMoveMaxHeight - _collider.radius);
                    return Physics.CapsuleCast(p1, p2, _collider.radius, direction, out hit, distance, stopMoveLayer);
                default:
                    return Physics.Raycast(origin, direction, out hit, distance, stopMoveLayer);
            }
        }

        protected virtual void SnapToGround(ref Vector3 targetVelocity, ref bool useVerticalVelocity)
        {
            if (!useSnapGround)
            {
                return;
            }

            if (groundDistance < groundMinDistance * 0.2f || applyingStepOffset)
            {
                return;
            }

            var snapConditions = isGrounded && groundHit.collider != null && GroundAngle() <= slopeLimit && !isJumping && !customAction && input.magnitude > 0.1f && !isInAirborne;

            if (snapConditions)
            {
                var distanceToGround = Mathf.Max(0.0f, groundDistance);
                var snapVelocity = transform.up * (-distanceToGround * snapPower / Time.fixedDeltaTime);
                targetVelocity = (targetVelocity + snapVelocity).normalized * targetVelocity.magnitude;
                useVerticalVelocity = false;
            }
        }

        protected virtual void CalculateStepOffset(Vector3 moveDir, ref Vector3 targetVelocity, ref bool useVerticalVelocity)
        {
            if (useStepOffset && isGrounded && !isJumping && !customAction && !isInAirborne)
            {
                Vector3 dir = Vector3.Lerp(transform.forward, moveDir.normalized, inputSmooth.magnitude);
                float distance = _collider.radius + stepOffsetDistance;
                float height = (stepOffsetMaxHeight + 0.01f + _collider.radius * 0.5f);
                Vector3 pA = transform.position + transform.up * (stepOffsetMinHeight + 0.05f);
                Vector3 pB = pA + dir.normalized * distance;
                if (Physics.Linecast(pA, pB, out stepOffsetHit, stepOffsetLayer))
                {

                    distance = stepOffsetHit.distance + 0.1f;
                }
                Ray ray = new Ray(transform.position + transform.up * height + dir.normalized * distance, Vector3.down);

                if (Physics.SphereCast(ray, _collider.radius * 0.5f, out stepOffsetHit, (stepOffsetMaxHeight - stepOffsetMinHeight), stepOffsetLayer) && stepOffsetHit.point.y > transform.position.y)
                {
                    dir = (stepOffsetHit.point) - transform.position;
                    dir.Normalize();
                    targetVelocity = Vector3.Project(targetVelocity, dir);
                    applyingStepOffset = true;
                    useVerticalVelocity = false;
                    return;
                }
            }

            applyingStepOffset = false;
        }

        public virtual void StopCharacterWithLerp()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            input = Vector3.Lerp(input, Vector3.zero, 2f * Time.fixedDeltaTime);
            inputSmooth = Vector3.Lerp(inputSmooth, Vector3.zero, 2f * Time.fixedDeltaTime);
            if (!_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, 4f * Time.fixedDeltaTime);
            inputMagnitude = Mathf.Lerp(inputMagnitude, 0f, 2f * Time.fixedDeltaTime);
            moveSpeed = Mathf.Lerp(moveSpeed, 0f, 2f * Time.fixedDeltaTime);
            /*_animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
            _animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.2f, Time.fixedDeltaTime);
            _animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.2f, Time.fixedDeltaTime);
            _animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.2f, Time.fixedDeltaTime);*/
        }

        public virtual void StopCharacter()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            input = Vector3.zero;
            inputSmooth = Vector3.zero;
            if (_rigidbody!=null && !_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.zero;
            inputMagnitude = 0f;
            moveSpeed = 0f;
            /*animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.25f, Time.fixedDeltaTime);*/
        }

        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public virtual void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, rotationSpeed);
        }

        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (customAction || (!jumpAndRotate && !isGrounded))
            {
                return;
            }

            direction.y = 0f;
            if (direction.normalized.magnitude == 0)
            {
                direction = transform.forward;
            }

            var euler = transform.rotation.eulerAngles.NormalizeAngle();
            var targetEuler = Quaternion.LookRotation(direction.normalized).eulerAngles.NormalizeAngle();
            euler.y = Mathf.LerpAngle(euler.y, targetEuler.y, rotationSpeed * Time.fixedDeltaTime);
            Quaternion _newRotation = Quaternion.Euler(euler);
            transform.rotation = _newRotation;
        }

        /// <summary>
        /// Check if <see cref="input"/> and <see cref="inputSmooth"/> has some value greater than 0.1f
        /// </summary>
        public virtual bool hasMovementInput
        {
            get => ((inputSmooth.sqrMagnitude + input.sqrMagnitude) > 0.1f || (input - inputSmooth).sqrMagnitude > 0.1f);
        }

        #endregion

    #region Jump Methods

    protected virtual void ControlJumpBehaviour()
    {
        if (!isJumping)
        {
            return;
        }

        jumpCounter -= Time.fixedDeltaTime;
        if (jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        // apply extra force to the jump height   
        var vel = _rigidbody.velocity;
        vel.y = jumpHeight * jumpMultiplier;
        _rigidbody.velocity = vel;
    }

    public virtual void SetJumpMultiplier(float jumpMultiplier)
    {
        this.jumpMultiplier = jumpMultiplier;
    }

    public virtual void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
    {
        this.jumpMultiplier = jumpMultiplier;

        if (timeToResetJumpMultiplier <= 0)
        {
            timeToResetJumpMultiplier = timeToReset;
            StartCoroutine(ResetJumpMultiplierRoutine());
        }
        else
        {
            timeToResetJumpMultiplier = timeToReset;
        }
    }

    public virtual void ResetJumpMultiplier()
    {
        StopCoroutine(ResetJumpMultiplierRoutine());
        timeToResetJumpMultiplier = 0;
        jumpMultiplier = 1;
    }

    protected virtual IEnumerator ResetJumpMultiplierRoutine()
    {

        while (timeToResetJumpMultiplier > 0 && jumpMultiplier != 1 && (isJumping || !isGrounded))
        {
            timeToResetJumpMultiplier -= Time.fixedDeltaTime;
            yield return null;
        }
        timeToResetJumpMultiplier = 0;
        jumpMultiplier = 1;
    }

    public virtual void AirControl()
    {
        if ((isGrounded && !isJumping) || _rigidbody.isKinematic || customAction)
        {
            return;
        }
        if (transform.position.y > heightReached)
        {
            heightReached = transform.position.y;
        }

        inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.fixedDeltaTime);

        if (jumpWithRigidbodyForce && !isGrounded)
        {
            _rigidbody.AddForce(moveDirection * airSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
            return;
        }
        var _moveDirection = moveDirection;
        _moveDirection.y = 0;
        _moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
        _moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);
        moveDirection = _moveDirection;
        Vector3 targetPosition = _rigidbody.position + (moveDirection * airSpeed) * Time.fixedDeltaTime;
        Vector3 targetVelocity = (targetPosition - transform.position) / Time.fixedDeltaTime;

        targetVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, airSmooth * Time.fixedDeltaTime);
    }

    protected virtual bool jumpFwdCondition
    {
        get
        {
            Vector3 p1 = transform.position + _collider.center + Vector3.up * -_collider.height * 0.5F;
            Vector3 p2 = p1 + Vector3.up * _collider.height;
            return Physics.CapsuleCastAll(p1, p2, _collider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
        }
    }

    #endregion


    public  void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }
}
