using StateController;
using System;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public StateMachine MovementSM { get; private set; }
    public StateMachine ActionSM { get; private set; }
    public Animator Anim { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public Transform PlayerCamera;

    public float MaxHP { get { return maxHp; } }
    public float CurrentHP { get { return currentHp; } set { currentHp = value; OnHPChanged?.Invoke(CurrentHP, MaxHP); } }
    // moveSpeed는 Inspector 설정값으로 고정하고, 상태가 바꾸는 건 MoveSpeed만이다.
    // 상대 연산(+=, -=)으로 되돌리면 OnExitState가 한 번이라도 누락됐을 때 값이 영구 왜곡된다.
    public float BaseMoveSpeed => moveSpeed;
    public float MoveSpeed { get; set; }
    public float JumpForce { get { return jumpForce; } set { jumpForce = value; } }
    public Transform GroundPivot { get { return groundPivot; } }

    [Header("Character Stat")]
    [SerializeField] protected float maxHp;
    protected float currentHp;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float jumpForce;
    [SerializeField] protected Transform groundPivot;
    public LayerMask groundLayer;
    public float currentGravity;
    public string CurrentMoveState;
    public string CurrentActionState;

    private PlayerController player;
    public event Action<float, float> OnHPChanged;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        CharacterController = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>();

        MoveSpeed = moveSpeed;

        // StateMachine은 1회만 생성한다. OnEnable에서 재생성하면 진행 중인 상태와
        // OnExitState가 되돌려야 할 값(MoveSpeed, 카메라 높이)이 함께 유실된다.
        InitMovementStateMachine();
        InitActionStateMachine();
    }

    private void Start()
    {
        MovementSM?.EnterState();
        ActionSM?.EnterState();

        CurrentHP = MaxHP;
    }

    private void Update()
    {
        CurrentMoveState = MovementSM?.CurrentState.ToString();
        CurrentActionState = ActionSM?.CurrentState.ToString();

        UpdateFireInput();
        MovementSM?.UpdateState();
        ActionSM?.UpdateState();
    }

    private void FixedUpdate()
    {
        MovementSM?.FixedUpdateState();
        ActionSM?.FixedUpdateState();
    }

    private void UpdateFireInput()
    {
        var input = player.fireInput;

        input.wasPressedThisFrame = input.isPressed && !player.prevFirePressed;
        player.prevFirePressed = input.isPressed;
    }

    private void InitMovementStateMachine()
    {
        MovementSM = new StateMachine(StateName.Idle, new IdleState(player));
        MovementSM.AddState(StateName.Move, new MoveState(player));
        MovementSM.AddState(StateName.Sprint, new SprintState(player));
        MovementSM.AddState(StateName.Jump, new JumpState(player));
        MovementSM.AddState(StateName.Crouch, new CrouchState(player));
    }

    private void InitActionStateMachine()
    {
        ActionSM = new StateMachine(StateName.ActionIdle, 
                                    new ActionIdleState(player));
        ActionSM.AddState(StateName.Fire, new FireState(player));
        ActionSM.AddState(StateName.Reload, new ReloadState(player));
        ActionSM.AddState(StateName.Melee, new MeleeState(player));
        ActionSM.AddState(StateName.Throw, new ThrowState(player));
    }
}