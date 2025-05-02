using System;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    
    public float mouseSensitivity = 100f;

    [SerializeField] private Transform playerBody;
    [SerializeField] public float crouchTransitionSpeed = 32f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
    

    public void HandleCameraPosition(Vector3 newTargetPosition) {
        if (transform.localPosition.y != newTargetPosition.y) {
            var currentCameraTargetPositionY = Mathf.Lerp(transform.localPosition.y, newTargetPosition.y, crouchTransitionSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, currentCameraTargetPositionY, transform.localPosition.z);
        } 
    }
}
