using StateController;
using UnityEngine;

public class ReloadState : BaseState
{
    float timer;
    bool takeoffMagazine;
    bool completed;
    const float TAKEOFF_MAGAZINE_TIME = 0.2f;
    const float RELOADING_TIME = 0.8f;

    Weapon currentWeapon;

    public ReloadState(PlayerController controller) : base(controller) { }

    public override void OnEnterState()
    {
        base.OnEnterState();
        timer = 0;
        takeoffMagazine = false;
        completed = false;

        currentWeapon = Controller.weaponManager.GetCurrentWeapon();
        currentWeapon.ReloadInvoke();
    }

    public override void OnUpdateState()
    {
        timer += Time.deltaTime;

        if(timer >= TAKEOFF_MAGAZINE_TIME && !takeoffMagazine)
        {
            takeoffMagazine = true;
        }

        if (timer >= RELOADING_TIME)
        {
            completed = true;
            Controller.playerCtx.ActionSM.ChangeState(StateName.ActionIdle);
        }
    }

    public override void OnExitState()
    {
        // 재장전이 끝나기 전에 상태를 벗어나면(무기 교체 등) 탄창을 채우지 않는다
        if (completed && currentWeapon != null)
            currentWeapon.CurrentMag = currentWeapon.MaxMag;

        Controller.isReload = false;
    }
}