using StateController;
using UnityEngine;
using System.Collections.Generic;

public class MeleeState : BaseState
{
    float timer;
    bool attacked;

    float angle = 60;
    float distance = 5f;
    int rayCount = 10;
    LayerMask hitLayer = LayerMask.GetMask("Enemy");

    const float MELEE_DURATION = 0.8f;
    const float HIT_TIME = 0.1f;
    HashSet<GameObject> targets = new();
    DamageSystem dms;

    public MeleeState(PlayerController controller) : base(controller) { }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if(dms == null)
        {
            dms = StaticRegistry.Find<DamageSystem>();
        }

        timer = 0f;
        attacked = false;

        //Controller.playerCtx.Anim.Play("Melee");
    }

    public override void OnUpdateState()
    {
        timer += Time.deltaTime;

        if (!attacked && timer >= HIT_TIME)
        {
            MeleeHit();
            attacked = true;
        }

        if (timer >= MELEE_DURATION)
        {
            Controller.playerCtx.ActionSM.ChangeState(StateName.ActionIdle);
        }

    }

    void MeleeHit()
    {
        targets.Clear();
        float half = angle * 0.5f;
        var camForward = Camera.main.transform.forward;

        for (int i = 0; i < rayCount; ++i)
        {
            float currentAngle = -half + (angle / rayCount) * i;
            var dir = Quaternion.AngleAxis(currentAngle, Vector3.up) * camForward;

            if (!Physics.Raycast(Controller.transform.position, dir, out var hit, distance, hitLayer))
            {
                Debug.DrawRay(Controller.transform.position, dir * distance, Color.red);
                continue;
            }

            Debug.DrawLine(Controller.transform.position, hit.point, Color.green);

            var target = hit.collider.gameObject;

            // 부채꼴 레이가 같은 대상을 여러 번 맞히므로 1회 공격당 1회만 적용한다
            if (!targets.Add(target)) continue;
            if (!target.TryGetComponent<IDamageable>(out var dmg)) continue;

            DamageContext context = new()
            {
                attacker = Controller.gameObject,
                target = target,
                hitPoint = hit.point,
                hitNormal = hit.normal,
                damage = 100,
                distance = hit.distance,
                damageType = DamageType.Melee,
                hitZone = dms.ResolveHitZone(hit.collider)
            };

            DamageResult res = dms.Pipeline.Calculate(context);
            dmg.ApplyDamage(res);
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();
    }
}