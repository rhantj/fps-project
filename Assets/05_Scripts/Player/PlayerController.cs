using StateController;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[RequireComponent(typeof(PlayerContext), typeof(PlayerInputActions))]
public class PlayerController : MonoBehaviour, IDamageable, IRegistryAdder
{
    [Header("Player ref")]
    public PlayerContext playerCtx;
    public PlayerInputActions inputAction;
    public WeaponManager weaponManager;
    public FireInputContext fireInput;
    public StateName PrevMovementState { get; set; } = StateName.Move;

    public Vector3 inputDir;
    public Vector3 mouseDelta;
    float pitch = 0;
    public int weaponIdx = 0;
    public bool isSprinting = false;
    public bool ISGROUNDED;
    public bool isJump = false;
    public bool isCrouching = false;
    public bool prevFirePressed;
    public bool isMelee;
    public bool isReload;

    Weapon currentWeapon;
    UIManager um;

    private void Awake()
    {
        playerCtx = GetComponent<PlayerContext>();
        inputAction = GetComponent<PlayerInputActions>();
        weaponManager = GetComponent<WeaponManager>();
        fireInput = new FireInputContext();

        AddRegistry();
    }

    void Start()
    {
        um = StaticRegistry.Find<UIManager>();
    }

    private void Update()
    {
        OnMouseInput();
        ISGROUNDED = playerCtx.CharacterController.isGrounded;
    }

    private void OnEnable()
    {
        weaponManager.OnWeaponChanged += OnWeaponChanged;

        // 재활성화 시 Equip()이 다시 불리지 않으므로 현재 무기와 직접 동기화한다
        OnWeaponChanged(weaponManager.GetCurrentWeapon());
    }

    private void OnDisable()
    {
        weaponManager.OnWeaponChanged -= OnWeaponChanged;
        UnBindWeapon();
    }

    void OnWeaponChanged(Weapon weapon)
    {
        UnBindWeapon();

        currentWeapon = weapon;
        if (currentWeapon != null)
            currentWeapon.OnAmmoEmpty += AmmoEmpty;
    }

    void UnBindWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon.OnAmmoEmpty -= AmmoEmpty;
        currentWeapon = null;
    }

    public void OnMouseInput()
    {
        if (Time.timeScale <= 0.5f) return;
        mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * um.settingVM.Sensitivity;
        float mouseY = mouseDelta.y * um.settingVM.Sensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        playerCtx.PlayerCamera.localRotation = Quaternion.Euler(pitch, 0, 0);
        playerCtx.CharacterController.transform.Rotate(Vector3.up * mouseX);
    }

    private void AmmoEmpty()
    {
        if (playerCtx.ActionSM.CurrentState is ReloadState)
            return;

        playerCtx.ActionSM.ChangeState(StateName.Reload);
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        inputDir = new Vector3(input.x, 0, input.y);
    }

    public void OnMoveInputCanceled(InputAction.CallbackContext _)
    {
        inputDir = Vector3.zero;
    }

    public void OnSprintInput(InputAction.CallbackContext _)
    {
        if (inputDir != Vector3.zero)
            isSprinting = true;
    }

    public void OnSprintInputCanceled(InputAction.CallbackContext _)
    {
        isSprinting = false;
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (!context.started || !playerCtx.CharacterController.isGrounded) return;
        isJump = true;
    }

    public void OnCrouchInput(InputAction.CallbackContext context)
    {
        if(context.interaction is PressInteraction)
        {
            isCrouching = !isCrouching;
            return;
        }

        isCrouching = true;
    }

    public void OnCrouchInputCanceled(InputAction.CallbackContext context)
    {
        if(context.interaction is HoldInteraction)
        {
            isCrouching = false;
        }
    }

    public void OnFireInput(InputAction.CallbackContext context)
    {
        fireInput.isPressed = context.ReadValueAsButton();
    }

    public void OnFireInputCanceled(InputAction.CallbackContext context)
    {
        fireInput.isPressed = context.ReadValueAsButton();
    }

    public void OnModeInput(InputAction.CallbackContext _)
    {
        weaponManager.GetCurrentWeapon().NextFireMode();
    }

    public void OnMainWeaponInput(InputAction.CallbackContext _)
    {
        if (isReload) return;
        weaponManager.Equip(0);
    }
    public void OnSubWeaponInput(InputAction.CallbackContext _)
    {
        if (isReload) return;
        weaponManager.Equip(1);
    }

    public void OnGrenadeInput(InputAction.CallbackContext _)
    {
        if (isReload) return;
        weaponManager.Equip(2);
    }

    public void OnMeleeInput(InputAction.CallbackContext context)
    {
        isMelee = context.ReadValueAsButton();
    }

    public void OnMeleeInputCanceled(InputAction.CallbackContext context)
    {
        isMelee = context.ReadValueAsButton();
    }

    public void OnReloadInput(InputAction.CallbackContext context)
    {
        if (isReload && currentWeapon.CurrentMag == currentWeapon.MaxMag) 
            return;
        isReload = context.ReadValueAsButton();
    }

    public void ApplyDamage(DamageResult res)
    {
        playerCtx.CurrentHP = Mathf.Max(playerCtx.CurrentHP - res.finalDamage, 0f);
    }

    public void AddRegistry()
    {
        StaticRegistry.Add(this);
    }
}