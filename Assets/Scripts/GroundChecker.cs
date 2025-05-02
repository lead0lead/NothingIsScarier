using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] public float crouchTransitionSpeed = 16f;

    public bool IsGrounded { get; private set; }

    private Vector3 groundCheckerOrigin;

    void Start()
    {
        groundCheckerOrigin = new Vector3(transform.position.x, transform.position.y - (playerController.GetColliderHeight() / 2), transform.position.z);    
    }

    void Update()
    {
        IsGrounded = Physics.CheckSphere(transform.position, groundDistance, groundLayer);
    }


    public void HandleGroundCheckerPosition(Vector3 newTargetPosition) {
        if (transform.localPosition.y != newTargetPosition.y) {
            var currentCameraTargetPositionY = Mathf.Lerp(transform.localPosition.y, newTargetPosition.y, crouchTransitionSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, currentCameraTargetPositionY, transform.localPosition.z);
        } 
    }
}
