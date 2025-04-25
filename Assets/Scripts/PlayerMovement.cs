using System.IO.Compression;
using System.Runtime.CompilerServices;
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
    [SerializeField] private float crouchTransitionSpeed;

    private float standingHeight;
    private float currentHeight;
    private float targetHeight;
    private Vector3 standingCameraPosition;
    private float speed;

    private float jumpHeight = 3f;
    Vector3 velocity;

    bool isGrounded;

    private void Awake()
    {
        standingHeight = controller.height;
        standingCameraPosition = cameraTransform.localPosition;
        speed = walkSpeed;
        currentHeight = standingHeight;
    }

    private void Start()
    {
        gameInput.OnJumpAction += GameInput_OnJumpAction;
        gameInput.OnCrouchAction += GameInput_OnCrouchAction;
    }

    private void GameInput_OnJumpAction(object sender, System.EventArgs e) {
        if (isGrounded) {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void GameInput_OnCrouchAction(bool isCrouching) {
        HandleCrouch(isCrouching);
    }

    private void Update()
    {
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);

        
        controller.height = targetHeight;

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


        private void HandleCrouch(bool isCrouching)
        {
            Vector3 cameraPos = standingCameraPosition;

            if (isCrouching) {
                speed = crouchSpeed;
                targetHeight = crouchHeight;
                cameraPos.y = cameraTransform.localPosition.y - ((standingHeight - targetHeight) * 0.5f);
            } else {
                speed = walkSpeed;
                targetHeight = standingHeight;
            }
            cameraTransform.localPosition = cameraPos;
        }
}

