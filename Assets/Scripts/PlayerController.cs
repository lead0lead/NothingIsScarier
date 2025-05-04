using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class PlayerController : MonoBehaviour
{



    [Header("References")]
    [SerializeField] public Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private GameInput gameInput;
    [SerializeField] public Transform cameraTransform;
    [SerializeField] public CameraHandler cameraHandler;
    [SerializeField] public GroundChecker groundChecker;

    [Header("Settings")]
    [SerializeField] public float walkSpeed = 800f;
    [SerializeField] public float crouchSpeed = 400f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float airAcceleration = 10f;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] public float standingHeight;
    [SerializeField] public float crouchHeight = 2f;
    [SerializeField] public float crouchTransitionSpeed = 16f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 0f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 15f;
    [SerializeField] float jumpCooldown = 0.25f;
    private bool jumpPressed;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle = 45f;
    [SerializeField] RaycastHit slopeHit;
    [SerializeField] float slopeCheckDistance = 0.3f;

    [Header("Interaction Handling")]
    [SerializeField] float interactDistance = 5f;

    public float moveSpeed;
    public bool IsCrouching { get; private set; }
    public float standingCameraPositionY { get; private set; }
    public float crouchCameraPositionY { get; private set; }
    public float standingGroundCheckerPositionY { get; private set; }
    public float crouchGroundCheckerPositionY { get; private set; }

    private float currentSpeed;
    private float velocity;
    private float jumpVelocity;

    private Vector3 movement;
    private Vector3 moveDirection;

    StateMachine stateMachine;

    const float ZeroF = 0f;

    float slopeangle;

    private void Awake() {
 
        standingHeight = GetColliderHeight();
        standingCameraPositionY = cameraTransform.localPosition.y;
        crouchCameraPositionY = standingCameraPositionY - ((standingHeight - crouchHeight) / 2);

        standingGroundCheckerPositionY = groundChecker.transform.localPosition.y;
        crouchGroundCheckerPositionY = standingGroundCheckerPositionY + ((standingHeight - crouchHeight) / 2);

        cameraHandler.crouchTransitionSpeed = crouchTransitionSpeed;
        groundChecker.crouchTransitionSpeed = crouchTransitionSpeed;
 
        // State machine
        stateMachine = new StateMachine();

        // Declare states
        var locomotionState = new LocomotionState(this, animator);
        var jumpState = new JumpState(this, animator);
        var crouchState = new CrouchState(this, animator);

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpPressed));
        At(jumpState, locomotionState, new FuncPredicate(() => groundChecker.IsGrounded && !jumpPressed));

        At(locomotionState, crouchState, new FuncPredicate(() => IsCrouching && groundChecker.IsGrounded && !jumpPressed));
        At(crouchState, locomotionState, new FuncPredicate(() => !IsCrouching));
        At(crouchState, jumpState, new FuncPredicate(() => jumpPressed && IsCrouching));

        // Set initial state
        stateMachine.SetState(locomotionState);

        rb.freezeRotation = true;
    }

    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    private void Start() {
    }

    private void OnEnable() {
        gameInput.Jump += OnJump;
        gameInput.Crouch += OnCrouch;
        gameInput.Interact += OnInteract;
    }

    private void OnDisable() {
        gameInput.Jump -= OnJump;
        gameInput.Crouch -= OnCrouch;
        gameInput.Interact -= OnInteract;
    }

    void OnJump(bool pressed) {
        jumpPressed = pressed;
    }

    void OnCrouch(bool IsPressed) {
        if (IsPressed) {
            IsCrouching = true;
        } else {
            IsCrouching = false;
        }
    }

    void OnInteract() {
        HandleInteractions();
    }
    
    private void Update() {
        movement = new Vector3(gameInput.Direction.x, 0f, gameInput.Direction.y);

        stateMachine.Update();
    }

    void FixedUpdate() {
        //HandleJump();
        // HandleMovement();
       stateMachine.FixedUpdate();

    }

    public void HandleJump() {
        if (groundChecker.IsGrounded) {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, ZeroF, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    public void HandleMovement() {

        float appliedAcceleration = groundChecker.IsGrounded ? acceleration : airAcceleration;
        moveDirection = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector2.up) * movement;

        if (OnSlope()) {
            rb.AddForce(GetSlopeMovementDirection(moveDirection) * moveSpeed * appliedAcceleration, ForceMode.Force);
            if (rb.linearVelocity.y > ZeroF) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        } else {
            rb.AddForce(moveDirection.normalized * moveSpeed * appliedAcceleration, ForceMode.Force);
        }

        rb.linearDamping = groundChecker.IsGrounded ? groundDrag : airDrag;
        rb.useGravity = !OnSlope();
        SpeedControl();
    }

    void SpeedControl() {

        if (OnSlope()) {
            if (rb.linearVelocity.magnitude > moveSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        } else {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, ZeroF, rb.linearVelocity.z);

            if (horizontalVelocity.magnitude > moveSpeed) {
                Vector3 cappedVelocity = horizontalVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
            }
        }
    }

    public float GetColliderHeight() {
        return capsuleCollider.height;
    }

    public void SetColliderHeight(float newHeight) {
        capsuleCollider.height = newHeight;
    }

    private bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, GetColliderHeight() / 2.0f + slopeCheckDistance)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            slopeangle = angle;
            return angle < maxSlopeAngle && angle != ZeroF;
        } 

        return false;
    }

    private Vector3 GetSlopeMovementDirection(Vector3 moveDir) {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    private void HandleInteractions() {
        RaycastHit hitInfo;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, interactDistance)) {
            if (hitInfo.transform.TryGetComponent(out IInteractable interactable)) {
                interactable.Interact(transform);
            } else {
            }
        }
    }
}

