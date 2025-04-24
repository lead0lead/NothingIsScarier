using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{


    public event EventHandler OnJumpAction;
    public event Action<bool> OnCrouchAction;
    private PlayerInputActions playerInputActions;
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Crouch.started += ctx => OnCrouchAction?.Invoke(true);
        playerInputActions.Player.Crouch.canceled += ctx => OnCrouchAction?.Invoke(false);
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnJumpAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized() {

        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;

        return inputVector;
    }
}
