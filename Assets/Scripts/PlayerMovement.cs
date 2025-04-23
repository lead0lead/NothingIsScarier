using System.IO.Compression;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private CharacterController controller;
    [SerializeField] private GameInput gameInput;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float gravity = -9.81f;

    private float jumpHeight = 3f;
    Vector3 velocity;

    bool isGrounded;

    private void Start()
    {
        gameInput.OnJumpAction += GameInput_OnJumpAction;
    }

    private void GameInput_OnJumpAction(object sender, System.EventArgs e) {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
}
