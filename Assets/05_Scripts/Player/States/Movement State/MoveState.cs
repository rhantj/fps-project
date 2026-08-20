using StateController;

public class MoveState : BasePlayerState
{
    public MoveState(PlayerController controller) : base(controller) { }

    public override void OnUpdateState()
    {
        if (Controller.isSprinting)
        {
            playerCtx.MovementSM.ChangeState(StateName.Sprint);
            return;
        }

        if (Controller.isJump && playerCtx.CharacterController.isGrounded)
        {
            Controller.isJump = false;
            Controller.PrevMovementState = StateName.Move;
            playerCtx.MovementSM.ChangeState(StateName.Jump);
            return;
        }

        if (Controller.isCrouching)
        {
            playerCtx.MovementSM.ChangeState(StateName.Crouch);
            return;
        }

        if (Controller.inputDir.sqrMagnitude <= 0.01f)
        {
            playerCtx.MovementSM.ChangeState(StateName.Idle);
        }

        base.OnUpdateState();
    }
}