using StateController;
using UnityEngine;

public class CrouchState : BasePlayerState
{
    const float CROUCH_SPEED_MULTIPLIER = 0.7f;
    const float CROUCH_CAMERA_RATIO = 0.7f;

    public CrouchState(PlayerController controller) : base(controller) { }

    Vector3 standingCamPos;

    public override void OnEnterState()
    {
        base.OnEnterState();

        // 선 자세의 카메라 위치를 그대로 보관했다가 이탈할 때 되돌린다.
        // 상대 연산으로 되돌리면 이탈이 한 번이라도 누락됐을 때 높이가 계속 내려간다.
        standingCamPos = playerCtx.PlayerCamera.localPosition;

        var crouchedCamPos = standingCamPos;
        crouchedCamPos.y *= CROUCH_CAMERA_RATIO;
        playerCtx.PlayerCamera.localPosition = crouchedCamPos;

        playerCtx.MoveSpeed = playerCtx.BaseMoveSpeed * CROUCH_SPEED_MULTIPLIER;
    }

    public override void OnUpdateState()
    {
        if (Controller.isJump)
        {
            // 앉은 채로 점프하면 일어서면서 점프한다. 기존에는 Move/Idle로 보내며
            // isJump를 지워버려 입력이 삼켜졌다.
            Controller.isCrouching = false;
            Controller.PrevMovementState =
                Controller.inputDir.sqrMagnitude > 0.01f ? StateName.Move : StateName.Idle;
            playerCtx.MovementSM.ChangeState(StateName.Jump);
            return;
        }

        if (!Controller.isCrouching)
        {
            var state = Controller.inputDir.sqrMagnitude > 0.01f ? StateName.Move : StateName.Idle;
            playerCtx.MovementSM.ChangeState(state);
            return;
        }

        base.OnUpdateState();
    }

    public override void OnExitState()
    {
        playerCtx.PlayerCamera.localPosition = standingCamPos;
        playerCtx.MoveSpeed = playerCtx.BaseMoveSpeed;
    }
}
