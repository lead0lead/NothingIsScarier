using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour, PlayerInputActions.IPlayerActions
{

    public event UnityAction<Vector2> Move = delegate { };
    public event UnityAction<Vector2> Look = delegate { };
    public event UnityAction<bool> Jump = delegate {};
    public event UnityAction<bool> Crouch = delegate { };

    public event UnityAction Interact = delegate { };
    private PlayerInputActions inputActions;
    public Vector3 Direction => (Vector3)inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookVector => (Vector2)inputActions.Player.Look.ReadValue<Vector2>();

    private void OnEnable()
    {
        if (inputActions == null) {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(instance: this);
        }
        inputActions.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Move.Invoke(arg0: context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase) {
            case InputActionPhase.Started:
                Jump.Invoke(arg0:true);
                break;
            case InputActionPhase.Canceled:
                Jump.Invoke(arg0:false);
                break;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (context.phase) {
            case InputActionPhase.Started:
                Crouch.Invoke(arg0:true);
                break;
            case InputActionPhase.Canceled:
                Crouch.Invoke(arg0:false);
                break;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) {
            Interact.Invoke();
        }
    }
}