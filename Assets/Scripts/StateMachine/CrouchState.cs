using UnityEngine;

public class CrouchState : BaseState {
    public CrouchState (PlayerController player, Animator animator) : base(player, animator) { }
    public override void OnEnter()
    {
        Debug.Log("CrouchState.OnEnter");
        // animator.CrossFade(CrouchHash, crossFadeDuration);
        player.moveSpeed = player.crouchSpeed;
        player.rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public override void FixedUpdate()
    {
        if (player.GetColliderHeight() != player.crouchHeight) {
                player.SetColliderHeight(Mathf.Lerp(player.GetColliderHeight(), player.crouchHeight, player.crouchTransitionSpeed));
            }
        player.cameraHandler.HandleCameraPosition(new Vector3(player.cameraTransform.localPosition.y, player.crouchCameraPositionY, player.cameraTransform.localPosition.z));
        player.groundChecker.HandleGroundCheckerPosition(new Vector3(player.groundChecker.transform.localPosition.y, player.crouchGroundCheckerPositionY, player.groundChecker.transform.localPosition.z));
        player.HandleMovement();
    }
}
