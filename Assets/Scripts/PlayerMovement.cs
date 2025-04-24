using System.IO.Compression;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController controller;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float crouchSpeed = 6f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float gravity = -9.81f;

    private float standingHeight;
    private Vector3 standingCameraPosition;
    private float speed;
    private MovementState movementState;
    private MovementState previousMovementState;
    private enum MovementState {
        Idle,
        Walking,
        Crouching,
        Air,
    }

    private float jumpHeight = 3f;
    Vector3 velocity;

    bool isGrounded;

    private void Awake()
    {
        SetMovementState(MovementState.Idle);
        standingHeight = controller.height;
        standingCameraPosition = cameraTransform.localPosition;
    }

    private void Start()
    {
        gameInput.OnJumpAction += GameInput_OnJumpAction;
    }

    private void GameInput_OnJumpAction(object sender, System.EventArgs e) {
        if (isGrounded) {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void GameInput_OnCrouchStarted(object sender, System.EventArgs e) {
        SetMovementState(MovementState.Crouching);
    }

    private void GameInput_OnCrouchEnded(object sender, System.EventArgs e) {
        SetMovementState(MovementState.Idle);
    }

    private void Update()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        isGrounded = controller.isGrounded;

        

        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
        HandleGravity();
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleGravity() {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void SetMovementState(MovementState newMovementState) {
        previousMovementState = movementState;
        movementState = newMovementState;

        switch (movementState) {
            case MovementState.Idle:
                speed = walkSpeed;
                HandleCrouch(false);
                break;

            case MovementState.Walking:
                speed = walkSpeed;
                HandleCrouch(false);
                break;

            case MovementState.Crouching:
                speed = crouchSpeed;
                HandleCrouch(true);
            break;
                
            case MovementState.Air:
                speed = walkSpeed;
                HandleCrouch(false);
                break;
        }
    }

        private void HandleCrouch(bool isCrouching)
        {
            Vector3 cameraPos = standingCameraPosition;

            if (isCrouching) {
                speed = crouchSpeed;
                controller.height = crouchHeight;
                cameraPos.y = cameraTransform.localPosition.y * 0.5f;
            } else {
                speed = walkSpeed;
                controller.height = standingHeight;    
            }
            cameraTransform.localPosition = cameraPos;
        }
}

