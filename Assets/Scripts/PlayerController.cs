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
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] public float standingHeight;
    [SerializeField] public float crouchHeight = 2f;
    [SerializeField] public float crouchTransitionSpeed = 16f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float gravityMultiplier = 3f;

    [Header("Slope Handling")]
    [SerializeField] float maxSlopeAngle = 45f;
    [SerializeField] RaycastHit slopeHit;
    [SerializeField] float slopeCheckDistance = 0.3f;

    public float moveSpeed;
    public bool IsCrouching { get; private set; }
    public float standingCameraPositionY { get; private set; }
    public float crouchCameraPositionY { get; private set; }
    public float standingGroundCheckerPositionY { get; private set; }
    public float crouchGroundCheckerPositionY { get; private set; }

    List<Timer> timers;
    CountdownTimer jumpTimer;
    CountdownTimer jumpCooldownTimer;
    private float currentSpeed;
    private float velocity;
    private float jumpVelocity;

    private Vector3 movement;

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
 
        // Setup Timers
        jumpTimer = new CountdownTimer(jumpDuration);
        jumpCooldownTimer = new CountdownTimer(jumpCooldown);
        timers = new List<Timer>(capacity:2) { jumpTimer, jumpCooldownTimer };

        jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
        jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

        // State machine
        stateMachine = new StateMachine();

        // Declare states
        var locomotionState = new LocomotionState(this, animator);
        var jumpState = new JumpState(this, animator);
        var crouchState = new CrouchState(this, animator);

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
        At(jumpState, locomotionState, new FuncPredicate(() => groundChecker.IsGrounded && !jumpTimer.IsRunning));

        At(locomotionState, crouchState, new FuncPredicate(() => IsCrouching && groundChecker.IsGrounded && !jumpTimer.IsRunning));
        At(crouchState, locomotionState, new FuncPredicate(() => !IsCrouching));
        At(crouchState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning && IsCrouching));

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
    }

    private void OnDisable() {
        gameInput.Jump -= OnJump;
        gameInput.Crouch -= OnCrouch;
    }

    void OnJump(bool performed) {
        if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded) {
            jumpTimer.Start();
            // Debug.Log("jump timer started");
        } else if (!performed && jumpTimer.IsRunning) { 
            jumpTimer.Stop();
            // Debug.Log("jump timer stopped");
        }
    }

    void OnCrouch(bool IsPressed) {
        if (IsPressed) {
            IsCrouching = true;
        } else {
            IsCrouching = false;
        }
    }

    private void Update() {
        HandleTimers();
        movement = new Vector3(gameInput.Direction.x, 0f, gameInput.Direction.y);

        stateMachine.Update();
    }

    void FixedUpdate() {
        //HandleJump();
        // HandleMovement();
       stateMachine.FixedUpdate();
    }

    public void HandleJump() {
        // if not jumping and grounded, keep jumping velocity at 0
        if (!jumpTimer.IsRunning && groundChecker.IsGrounded) {
            jumpVelocity = ZeroF;
            jumpTimer.Stop();
            return;
        }

        // if jumping or falling calculate velocity
        if (!jumpTimer.IsRunning) {
            // Gravity takes over
            jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        // Apply velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
    }

    private void HandleTimers() {
        foreach (var timer in timers) {
            timer.Tick(Time.deltaTime);
        }
    }

    public void HandleMovement() {
        Vector3 movementDirectionAdjusted = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector2.up) * movement;

        if (movementDirectionAdjusted.magnitude > ZeroF)
        {
            // HandleRotation(movementDirectionAdjusted);
            HandleHorizontalMovement(movementDirectionAdjusted, OnSlope());

            SmoothSpeed(movementDirectionAdjusted.magnitude);
        }
        else {
            SmoothSpeed(ZeroF);
            rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
        }

    }

    private void HandleHorizontalMovement(Vector3 movementDirectionAdjusted, bool OnSlope)
    {
        if (OnSlope) {
            movementDirectionAdjusted = GetSlopeMovementDirection(movementDirectionAdjusted);
            Vector3 velocity = movementDirectionAdjusted * moveSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = velocity;
            } else {
            Vector3 velocity = movementDirectionAdjusted * moveSpeed * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
    }

    /* private void HandleRotation(Vector3 movementDirectionAdjusted)
    {
        var targetRotation = Quaternion.LookRotation(movementDirectionAdjusted);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    } */

    void SmoothSpeed(float value) {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
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
}

