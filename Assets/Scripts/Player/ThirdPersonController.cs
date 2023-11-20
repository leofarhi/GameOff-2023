 using System;
 using UnityEngine;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : TemperatureBehaviour
    {
        [Header("Temperature Player")]
        public GameObject playerBodyNormal;
        public GameObject playerBodyCold;
        public GameObject playerRainParticles;
        public GameObject ParticleColdTransitionPrefab;
        public GameObject ParticleHotTransitionPrefab;
        public GameObject ParticleNormalTransitionPrefab;
        public Vector3 offsetParticleTransition;
        private Vector3 originalCharacterControllerCenter;
        private float originalCharacterControllerHeight;
        
        [Header("General Settings")]
        public InputPreset inputPreset;
        public static ThirdPersonController instance;
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        
        
        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        
        public Animator _animator;
        private CharacterController _controller;

        private GameObject _mainCamera
        {
            get
            {
                return CameraMain.gameObject;
            }
        }
        [HideInInspector]
        public ThirdPersonCamera CameraMain;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;
        
        //lock input
        private uint _lockInputCount = 0;
        public bool lockInput
        {
            get
            {
                return _lockInputCount > 0;
            }
        }
        public void LockInput()
        {
            _lockInputCount++;
        }
        public void UnlockInput()
        {
            _lockInputCount--;
            if (_lockInputCount < 0)
                _lockInputCount = 0;
        }

        //Functions
        private void Awake()
        {
            instance = this;
            FindCamera();
            
        }
        
        public void SetupOriginalBodyScale()
        {
            originalCharacterControllerCenter = _controller.center;
            originalCharacterControllerHeight = _controller.height;
        }
        
        public void FindCamera()
        {
            if (Camera.main != null)
            {
                GameObject _mainCamera = Camera.main.gameObject;
                CameraMain = _mainCamera.GetComponent<ThirdPersonCamera>();
                if (CameraMain != null)
                {
                    CameraMain.player = this;
                }
            }
        }

        private void Start()
        { _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            SetupOriginalBodyScale();
        }

        public override void Update()
        {
            base.Update();
            _hasAnimator = _animator != null;

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        public override void OnTemperatureStateChangeCallback(TemperatureState old,TemperatureState state)
        {
            //resetting Collision
            int playerLayer = LayerMask.NameToLayer("Player");
            int manholeCoverLayer = LayerMask.NameToLayer("ManholeCover");
            Physics.IgnoreLayerCollision(playerLayer, manholeCoverLayer, false);
            if ((state==TemperatureState.Cold || state==TemperatureState.Hot)&&old!=TemperatureState.Normal)
            {
                OnTemperatureStateChangeCallback(old,TemperatureState.Normal);
                old = TemperatureState.Normal;
            }
            switch (state)
            {
                case TemperatureState.Cold:
                    //dividing scale by 2
                    //changing character controller height
                    _controller.height = originalCharacterControllerHeight / 2;
                    _controller.center = originalCharacterControllerCenter - new Vector3(0, 0.5f, 0);
                    //changing player body
                    playerBodyNormal.SetActive(false);
                    playerBodyCold.SetActive(true);
                    //change animator
                    _animator = playerBodyCold.GetComponent<Animator>();
                    //Spawn cold transition particles
                    Instantiate(ParticleColdTransitionPrefab, transform.position+offsetParticleTransition, Quaternion.identity);
                    break;
                case TemperatureState.Normal:
                    //resetting scale
                    //resetting character controller height
                    _controller.height = originalCharacterControllerHeight;
                    _controller.center = originalCharacterControllerCenter;
                    //resetting player body
                    playerBodyNormal.SetActive(true);
                    playerBodyCold.SetActive(false);
                    playerRainParticles.SetActive(false);
                    //resetting animator
                    _animator = playerBodyNormal.GetComponent<Animator>();
                    //Spawn normal transition particles
                    Instantiate(ParticleNormalTransitionPrefab, transform.position+offsetParticleTransition, Quaternion.identity);
                    /*if (old == TemperatureState.Cold)
                    {
                        //Spawn cold transition particles
                        Instantiate(ParticleColdTransitionPrefab, transform.position+offsetParticleTransition, Quaternion.identity);
                    }
                    else if (old == TemperatureState.Hot)
                    {
                        //Spawn hot transition particles
                        Instantiate(ParticleHotTransitionPrefab, transform.position+offsetParticleTransition, Quaternion.identity);
                    }*/
                    break;
                case TemperatureState.Hot:
                    //ingnore layer collision between player and manholeCover
                    Physics.IgnoreLayerCollision(playerLayer, manholeCoverLayer);
                    //rain particles
                    playerRainParticles.SetActive(true);
                    //Spawn hot transition particles
                    Instantiate(ParticleHotTransitionPrefab, transform.position+offsetParticleTransition, Quaternion.identity);
                    break;
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void Move()
        {
            Vector2 moveInput = inputPreset.moveInput;
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = inputPreset.sprintInput.GetButton() ? SprintSpeed : MoveSpeed;
            
            if (lockInput)
            {
                moveInput = Vector2.zero;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = moveInput.magnitude;//_input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (moveInput != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + CameraMain.GetCameraRotationY();
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }
                
                if (!lockInput) 
                    // Jump
                if (inputPreset.jumpInput.GetButtonDown() && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                //_input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        public void OnGameStateChanged(GameState newGameState)
        {
            if (newGameState == GameState.Gameplay)
            {
                UnlockInput();
            }
            else
            {
                LockInput();
            }
        }

        private void OnEnable()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
        
        private void OnDisable()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }