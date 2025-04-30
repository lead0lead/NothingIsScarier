using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] float groundDistance = 1.99f;
    [SerializeField] LayerMask groundLayer;

    public bool IsGrounded { get; private set; }

    private Vector3 groundCheckerOrigin;

    void Start()
    {
        groundCheckerOrigin = new Vector3(transform.position.x, transform.position.y - (playerController.GetColliderHeight() / 2), transform.position.z);    
    }

    void Update()
    {
        RaycastHit hit;
        IsGrounded = Physics.SphereCast(transform.position, groundDistance, Vector3.down, out hit, groundDistance, groundLayer);
    }
}
