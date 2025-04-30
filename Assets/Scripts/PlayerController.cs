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
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GroundChecker groundChecker;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float jumpCooldown = 0f;
    [SerializeField] float jumpDuration = 0.5f;
    [SerializeField] float gravityMultiplier = 3f;

    private float moveSpeed;

    List<Timer> timers;
    CountdownTimer jumpTimer;
    CountdownTimer jumpCooldownTimer;
    private float currentSpeed;
    private float velocity;
    private float jumpVelocity;

    private Vector3 movement;

    StateMachine stateMachine;

    const float ZeroF = 0f;

    private void Awake() {
        moveSpeed = walkSpeed;
        
        rb.freezeRotation = true;

        // Setup Timers
        jumpTimer = new CountdownTimer(jumpDuration);
        jumpCooldownTimer = new CountdownTimer(jumpCooldown);
        timers = new List<Timer>(capacity:2) { jumpTimer, jumpCooldownTimer };

        jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
        jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

        // State machine
        stateMachine = new StateMachine();

        // Declare states
        var locomotionState = new LocomotionState(player:this, animator);
        var jumpState = new JumpState(player:this, animator);

        // Define transitions
        At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
        At(jumpState, locomotionState, new FuncPredicate(() => groundChecker.IsGrounded && !jumpTimer.IsRunning));

        // Set initial state
        stateMachine.SetState(locomotionState);

    }

    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    private void Start() {
    }

    private void OnEnable() {
        gameInput.Jump += OnJump;
    }

    private void OnDisable() {
        gameInput.Jump -= OnJump;
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

    private void Update() {
        HandleTimers();
        movement = new Vector3(gameInput.Direction.x, 0f, gameInput.Direction.y);
    }

    void FixedUpdate() {
        stateMachine.FixedUpdate();
    }

    public void HandleJump() {
        // if not jumping and grounded, keep jumping velocity at 0
        if (!jumpTimer.IsRunning && groundChecker.IsGrounded) {
            jumpVelocity = ZeroF;
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
            HandleHorizontalMovement(movementDirectionAdjusted);

            SmoothSpeed(movementDirectionAdjusted.magnitude);
        }
        else {
            SmoothSpeed(ZeroF);
            rb.linearVelocity = new Vector3(ZeroF, rb.linearVelocity.y, ZeroF);
        }

    }

    private void HandleHorizontalMovement(Vector3 movementDirectionAdjusted)
    {
        Vector3 velocity = movementDirectionAdjusted * moveSpeed * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
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

    private void SetColliderHeight(float newHeight) {
        capsuleCollider.height = newHeight;
    }

}

