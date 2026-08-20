using StateController;
using UnityEngine;
public abstract class BasePlayerState : BaseState
{
    protected PlayerContext playerCtx;
    protected float yVelocity;
    protected float gravity;
    protected float minG = -25;
    protected float maxG = -60;

    protected bool groundedRay;
    protected Vector3 groundNormal = Vector3.up;

    protected float groundRayDist = .8f;
    protected float slopeMaxAngle = 40f;
    protected float stickToGroundVelocity = -4f;

    protected Vector3 newMovementVelocity;
    protected Vector3 newMovementVelocityRef;

    const float SMOOTH_TIME = 0.2f;

    public BasePlayerState(PlayerController controller) : base(controller) { }

    public override void OnEnterState()
    {
        playerCtx = Controller.playerCtx;

        // 진입할 때마다 기준 속도를 다시 읽는다. 배수를 쓰는 상태(Sprint, Crouch)는
        // base 호출 뒤에 자기 값으로 덮어쓴다.
        playerCtx.MoveSpeed = playerCtx.BaseMoveSpeed;

        // inputDir은 (x, 0, z)로 채워지므로 전진 성분은 z다. y를 쓰면 항상 0이다.
        Vector3 moveDir = Controller.transform.right * Controller.inputDir.x +
                          Controller.transform.forward * Controller.inputDir.z;

        if (moveDir.sqrMagnitude > 0.0001f)
            moveDir.Normalize();

        newMovementVelocity = moveDir * playerCtx.MoveSpeed;
        newMovementVelocityRef = Vector3.zero;
    }

    public override void OnUpdateState()
    {
        CommonMovement();
    }

    void CalculateSlope()
    {
        var origin = playerCtx.GroundPivot.position;

        Debug.DrawRay(origin, Vector3.down * 0.5f, Color.red);
        if (Physics.Raycast(origin, Vector3.down, out var hit, groundRayDist, playerCtx.groundLayer))
        {
            groundedRay = true;
            groundNormal = hit.normal;

            float cos = groundNormal.y;
            float sin = Mathf.Sqrt(groundNormal.x * groundNormal.x + groundNormal.z * groundNormal.z);          
            float deg = Mathf.Atan2(sin, cos) * Mathf.Rad2Deg;
            float t = Mathf.Clamp01(deg / slopeMaxAngle);
            gravity = Mathf.Lerp(minG, maxG, t);
        }
        else
        {
            groundedRay = false;
            groundNormal = Vector3.up;
            gravity = minG;
        }

        Controller.playerCtx.currentGravity = gravity;
    }

    public void ApplyGravity()
    {
        if (playerCtx.CharacterController.isGrounded && yVelocity < 0f)
        {
            yVelocity = stickToGroundVelocity;
            return;
        }

        yVelocity += gravity * Time.deltaTime;
    }

    public void CommonMovement()
    {
        CalculateSlope();

        Vector3 inputDir = new Vector3(Controller.inputDir.x, 0f, Controller.inputDir.z);

        Vector3 moveDir =
            Controller.transform.right * inputDir.x +
            Controller.transform.forward * inputDir.z;

        if (moveDir.sqrMagnitude > 0.0001f)
            moveDir.Normalize();

        if (groundedRay && moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;
        }

        Vector3 targetVelocity = moveDir * playerCtx.MoveSpeed;

        if (moveDir.sqrMagnitude < 0.01f)
        {
            newMovementVelocity = Vector3.zero;
            newMovementVelocityRef = Vector3.zero;
        }
        else
        {
            newMovementVelocity = Vector3.SmoothDamp(
                newMovementVelocity,
                targetVelocity,
                ref newMovementVelocityRef,
                SMOOTH_TIME);
        }

        ApplyGravity();

        Vector3 velocity = newMovementVelocity;
        velocity.y = yVelocity;

        playerCtx.CharacterController.Move(velocity * Time.deltaTime);
    }
}