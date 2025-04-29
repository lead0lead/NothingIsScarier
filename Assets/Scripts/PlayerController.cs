using System.IO.Compression;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;
    [SerializeField] private CharacterController controller;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float smoothTime = 0.2f;

    private float moveSpeed;
    private float currentSpeed;
    private float velocity;

    private float jumpHeight = 3f;


    bool isGrounded;

    StateMachine stateMachine;

    const float ZeroF = 0f;

    private void Awake()
    {
        moveSpeed = walkSpeed;

        // state machine
        stateMachine = new StateMachine();

        var locomotionState = new LocomotionState(player: this, animator);
    }

    private void Start()
    {
    }


    private void Update()
    {
        HandleMovement();
    }


    public void HandleJump() {
        // later
    }

    public void HandleMovement() {
        Vector3 movementDirection = new Vector3(gameInput.Direction.x, 0f, gameInput.Direction.y).normalized;
        Vector3 movementDirectionAdjusted = Quaternion.AngleAxis(cameraTransform.eulerAngles.y, Vector2.up) * movementDirection;
        if (movementDirectionAdjusted.magnitude > ZeroF)
        {
            HandleRotation(movementDirectionAdjusted);

            HandleController(movementDirectionAdjusted);

            SmoothSpeed(movementDirectionAdjusted.magnitude);
        }
        else {
            SmoothSpeed(ZeroF);
        }
    }

    private void HandleController(Vector3 movementDirectionAdjusted)
    {
        var movement = movementDirectionAdjusted * (moveSpeed * Time.deltaTime);

        controller.Move(movement);
    }

    private void HandleRotation(Vector3 movementDirectionAdjusted)
    {
        var targetRotation = Quaternion.LookRotation(movementDirectionAdjusted);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void SmoothSpeed(float value) {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref velocity, smoothTime);
    }

}

