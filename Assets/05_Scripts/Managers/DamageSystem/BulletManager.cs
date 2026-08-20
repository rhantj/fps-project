using System.Collections;
using UnityEngine;

// 탄도 계산 및 탄환 스폰 관리
public class BulletManager : MonoBehaviour
{
    private void Awake()
    {
        StaticRegistry.Add(this);
    }

    // 탄환 소환 및 초기화
    public void SpawnBullet(Vector3 pos, Vector3 dir, float speed, float damage, DamageSystem dms, float maxRange)
    {
        StartCoroutine(Co_SpawnBullet(pos, dir, speed, damage, dms, maxRange));
    }

    IEnumerator Co_SpawnBullet(Vector3 pos, Vector3 dir, float speed, float damage, DamageSystem dms, float maxRange)
    {
        float traveled = 0f;

        while(traveled < maxRange)
        {
            float step = speed * Time.deltaTime;
            Vector3 next = pos + dir * step;

            if (Physics.Raycast(pos, dir, out var hit, step))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
                {
                    HandleHit(hit, traveled, damage, dms);
                    yield break;
                }
            }

            pos = next;
            traveled += step;
            yield return null;
        }
    }

    // 데미지 적용 메서드
    void HandleHit(RaycastHit hit, float traveled, float damage, DamageSystem dms)
    {
        if (!hit.collider.TryGetComponent<IDamageable>(out var dmg)) return;
        DamageContext ctx = new()
        {
            attacker = gameObject,
            target = hit.collider.gameObject,
            hitPoint = hit.point,
            hitNormal = hit.normal,
            damage = damage,
            distance = traveled,
            damageType = DamageType.Bullet,
            hitZone = dms.ResolveHitZone(hit.collider)
        };

        DamageResult res = dms.Pipeline.Calculate(ctx);
        dmg.ApplyDamage(res);
    }
}