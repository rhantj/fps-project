using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeDamage : MonoBehaviour
{
    private float damage;
    private float maxRange;             // 30% damage
    private WaitForSeconds waitExplosion = new(3f);

    private WeaponContext ctx;
    public LayerMask enemyLayer;

    [SerializeField] int maxTargets = 8;
    Collider[] targets;
    SoundManager soundManager;
    AudioClip explosionClip;

    Rigidbody rb;
    Coroutine explosionRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        targets = new Collider[maxTargets];
    }

    public void Init(WeaponContext ctx)
    {
        damage = ctx.damage;
        maxRange = ctx.maxRange;
        this.ctx = ctx;

        // 풀에서 재사용되면 이전 투척의 속도가 남아 투척력이 누적된다
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // StopCoroutine(Co_Explosion())은 새 이터레이터를 넘기므로 아무것도 멈추지 않는다
        if (explosionRoutine != null) StopCoroutine(explosionRoutine);
        explosionRoutine = StartCoroutine(Co_Explosion());
    }

    private void Start()
    {
        soundManager = StaticRegistry.Find<SoundManager>();
        explosionClip = soundManager.GetClip("Grenade");
    }

    IEnumerator Co_Explosion()
    {
        yield return waitExplosion;
        ObjectPoolManager.Instance.Spawn(PoolId.GrenadeExplosionVFX, transform.position, transform.rotation);
        int enemyCnt = Physics.OverlapSphereNonAlloc(transform.position, maxRange, targets, enemyLayer);

        for (int i = 0; i < enemyCnt; ++i)
        {
            float dist = Vector3.Distance(transform.position, targets[i].transform.position);

            if (targets[i].gameObject.TryGetComponent<IDamageable>(out var dmg))
            {
                float t = Mathf.Clamp01(dist/ maxRange);
                float finalDmg = Mathf.Lerp(damage, 1f, t);
                finalDmg = Mathf.Max(finalDmg, 1f);

                DamageContext context = new()
                {
                    attacker = ctx.owner,
                    target = targets[i].gameObject,
                    hitPoint = targets[i].transform.position,
                    hitNormal = targets[i].transform.position.normalized,
                    damage = finalDmg,
                    damageType = DamageType.Explosion,
                    hitZone = ctx.dms.ResolveHitZone(targets[i])
                };

                DamageResult res = ctx.dms.Pipeline.Calculate(context);
                dmg.ApplyDamage(res);

                Debug.Log($"Final Damage = {res.finalDamage}");
            }
        }

        soundManager.PlaySound(explosionClip, transform.position, transform.rotation);

        explosionRoutine = null;
        ObjectPoolManager.Instance.Despawn(gameObject);
    }
}