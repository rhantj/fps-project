using StateController;

public class IdleState : BasePlayerState
{
    public IdleState(PlayerController controller) : base(controller) { }

    public override void OnUpdateState()
    {
        if (Controller.isSprinting && Controller.inputDir.sqrMagnitude > 0.01f)
        {
            playerCtx.MovementSM.ChangeState(StateName.Sprint);
            return;
        }

        if (Controller.isJump)
        {
            Controller.isJump = false;
            Controller.PrevMovementState = StateName.Idle;
            playerCtx.MovementSM.ChangeState(StateName.Jump);
            return;
        }

        if (Controller.isCrouching)
        {
            playerCtx.MovementSM.ChangeState(StateName.Crouch);
            return;
        }

        if(Controller.inputDir.sqrMagnitude > 0.01f)
        {
            playerCtx.MovementSM.ChangeState(StateName.Move);
        }

        base.OnUpdateState();
    }
}