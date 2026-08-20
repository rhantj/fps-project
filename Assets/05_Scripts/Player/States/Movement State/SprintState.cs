public class SprintState : BasePlayerState
{
    const float SPRINT_MULTIPLIER = 1.5f;

    public SprintState(PlayerController controller) : base(controller) { }
    public override void OnEnterState()
    {
        base.OnEnterState();

        playerCtx.MoveSpeed = playerCtx.BaseMoveSpeed * SPRINT_MULTIPLIER;
    }

    public override void OnUpdateState()
    {
        if (Controller.isJump && playerCtx.CharacterController.isGrounded)
        {
            Controller.isJump = false;
            Controller.PrevMovementState = StateController.StateName.Sprint;
            playerCtx.MovementSM.ChangeState(StateController.StateName.Jump);
            return;
        }

        if (!Controller.isSprinting || Controller.isCrouching || Controller.inputDir.sqrMagnitude <= 0.01f)
        {
            playerCtx.MovementSM.ChangeState(StateController.StateName.Move);
            return;
        }

        base.OnUpdateState();
    }

    public override void OnExitState()
    {
        playerCtx.MoveSpeed = playerCtx.BaseMoveSpeed;
    }
}