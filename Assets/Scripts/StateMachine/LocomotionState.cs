using UnityEngine;

public class LocomotionState : BaseState {
    public LocomotionState(PlayerController player, Animator animator) : base(player, animator) { }

    public override void OnEnter()
    {
        Debug.Log("LocomotionState.OnEnter");
        // animator.CrossFade(LocomotionHash, crossFadeDuration);
       player.moveSpeed = player.walkSpeed;
    }

    public override void FixedUpdate()
    {
        if (player.GetColliderHeight() != player.standingHeight) {
            player.SetColliderHeight(Mathf.Lerp(player.GetColliderHeight(), player.standingHeight, player.crouchTransitionSpeed));
        }
        player.cameraHandler.HandleCameraPosition(new Vector3(player.cameraTransform.position.y, player.standingCameraPositionY, player.cameraTransform.position.z));
        player.groundChecker.HandleGroundCheckerPosition(new Vector3(player.groundChecker.transform.localPosition.y, player.standingGroundCheckerPositionY, player.groundChecker.transform.localPosition.z));
        player.HandleMovement();
    }
}
