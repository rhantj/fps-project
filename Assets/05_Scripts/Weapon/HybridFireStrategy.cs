using System.Net;
using UnityEngine;

public class HybridFireStrategy : IWeaponFireStrategy
{
    public bool Fire(WeaponContext ctx)
    {
        if (Time.time < ctx.lastFireTime + ctx.fireRate) return false;
        ctx.lastFireTime = Time.time;

        Vector3 dir = GetSpreadDirection(ctx);

        Debug.DrawRay(ctx.muzzle.position, dir * ctx.hitscanRange, Color.red);
        if (Physics.Raycast(ctx.muzzle.position, dir, out var hit, ctx.hitscanRange))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                DamageContext context = new()
                {
                    attacker = ctx.owner,
                    target = hit.collider.gameObject,
                    hitPoint = hit.point,
                    hitNormal = hit.normal,
                    damage = ctx.damage,
                    distance = hit.distance,
                    damageType = DamageType.Bullet,
                    hitZone = ctx.dms.ResolveHitZone(hit.collider)
                };

                DamageResult res = ctx.dms.Pipeline.Calculate(context);
                dmg.ApplyDamage(res);
            }

            SpawnTracer(hit.point);
            return true;
        }

        var bm = StaticRegistry.Find<BulletManager>();
        bm.SpawnBullet(ctx.muzzle.position, dir, ctx.bulletSpeed, ctx.damage, ctx.dms, ctx.maxRange);
        return true;
    }

    Vector3 GetSpreadDirection(WeaponContext ctx)
    {
        Vector3 dir = ctx.muzzle.forward;
        dir = Quaternion.Euler(
            Random.Range(-ctx.spreadAngle, ctx.spreadAngle),
            Random.Range(-ctx.spreadAngle, ctx.spreadAngle),
            0f
        ) * dir;
        return dir.normalized;
    }


    void SpawnTracer(Vector3 endPoint)
    {
        // LineRenderer
    }
}