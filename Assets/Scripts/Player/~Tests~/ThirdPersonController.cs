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
    
    #region VariablesGround
    [Header("Ground & Collision")]
    protected float heightReached;
    public bool isGrounded;
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
    protected bool applyingStepOffset;
    [Tooltip("Snaps the capsule collider to the ground surface, recommend when using complex terrains or inclined ramps")]
    public bool useSnapGround = true;
    [Tooltip("Apply extra gravity when the character is not grounded")]
    [SerializeField] protected float extraGravity = -10f;
    #endregion
    
    [Header("Movement")]
    public float rotationSpeed = 10f;
    public float movementSmooth = 0.15f;
    public float walkSpeed = 1f;
    public float runningSpeed = 1.5f;
    public float stopSpeed = 0.2f;
    public float acceleration = 0.2f;
    [Header("Jump")]
    internal bool isJumping;
    protected float jumpForce = 4f;
    
    [Header("General Settings")]
    public InputPreset inputPreset;
    public bool lockMovement;
    
    
    private Rigidbody _rigidbody;
    private CapsuleCollider _collider;
    private Animator _animator;
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
        CheckGround();
        MoveInput(new Vector2(inputPreset.horizontalInput.GetAxisRaw(), inputPreset.verticalInput.GetAxisRaw()));
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
    
    #region Ground Check

    protected virtual void CheckGround()
    {
        CheckGroundDistance();
        if (isDead)
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
            Ray ray2 = new Ray(transform.position + new Vector3(0, _collider.height / 2, 0), Vector3.down);
            // raycast for check the ground distance
            if (Physics.Raycast(ray2, out groundHit, (_collider.height / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
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

    #endregion
    
    #region Movement

    public void MoveInput(Vector2 input)
    {
        if (lockMovement)return;
        if (input.magnitude > 1f)
        {
            input.Normalize();
        }
        if (cameraMain != null)
        {
            Vector3 camForward = cameraMain.transform.forward;
            Vector3 camRight = cameraMain.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            Vector3 desiredMoveDirection = (camForward * input.y + camRight * input.x).normalized;
            Move(desiredMoveDirection);
        }
    }
    
    public virtual void Move(Vector3 desiredMoveDirection)
    {
        if (lockMovement)return;
        if (isDead)return;
        if (desiredMoveDirection.magnitude > 1f)
        {
            desiredMoveDirection.Normalize();
        }
        desiredMoveDirection = transform.InverseTransformDirection(desiredMoveDirection);
        desiredMoveDirection = Vector3.ProjectOnPlane(desiredMoveDirection, groundHit.normal);
        float turnAmount = Mathf.Atan2(desiredMoveDirection.x, desiredMoveDirection.z);
        float forwardAmount = desiredMoveDirection.z;
        if (isGrounded)
        {
            ApplyExtraTurnRotation();
        }
        if (isGrounded && !isJumping)
        {
            HandleGroundedMovement(forwardAmount, turnAmount);
        }
        else
        {
            HandleAirborneMovement();
        }
    }
    
    protected virtual void HandleGroundedMovement(float forwardAmount, float turnAmount)
    {
        if (lockMovement)return;
        if (isDead)return;
        if (isJumping)return;
        if (isGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (_rigidbody.velocity);
            v.y = 0;
            _rigidbody.velocity = v;
        }
        if (isGrounded && forwardAmount > 0)
        {
            _rigidbody.drag = 0f;
        }
        else
        {
            _rigidbody.drag = stopSpeed;
        }
        if (isGrounded && !isJumping)
        {
            Vector3 targetVelocity = new Vector3(0, 0, forwardAmount);
            targetVelocity = transform.TransformDirection(targetVelocity);
            targetVelocity *= walkSpeed;
            Vector3 velocity = _rigidbody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -acceleration, acceleration);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -acceleration, acceleration);
            velocityChange.y = 0;
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        if (isGrounded)
        {
            _rigidbody.rotation = Quaternion.Euler(0f, _rigidbody.rotation.eulerAngles.y + turnAmount * rotationSpeed, 0f);
        }
        if (isGrounded && isJumping)
        {
            isJumping = false;
        }
    }
    
    protected virtual void HandleAirborneMovement()
    {
        if (lockMovement)return;
        if (isDead)return;
        Vector3 extraGravityForce = (Physics.gravity * extraGravity) - Physics.gravity;
        _rigidbody.AddForce(extraGravityForce);
        groundDistance = _rigidbody.velocity.y < 0 ? groundDistance : 0.01f;
    }
    
    protected virtual void ApplyExtraTurnRotation()
    {
        if (lockMovement)return;
        if (isDead)return;
        float turnSpeed = Mathf.Lerp(movementSmooth, rotationSpeed, _rigidbody.velocity.magnitude / walkSpeed);
        transform.Rotate(0, _rigidbody.angularVelocity.y * turnSpeed * Time.deltaTime, 0);
    }
    
    public virtual void Jump()
    {
        if (lockMovement)return;
        if (isDead)return;
        if (isGrounded && !isJumping)
        {
            isJumping = true;
            _rigidbody.drag = 0f;
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            _rigidbody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
        }
    }
    #endregion
}
