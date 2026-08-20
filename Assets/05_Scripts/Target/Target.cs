using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private int hp = 100;
    [SerializeField] private Transform pivot;
    public event Action OnTargetDown;

    public void ApplyDamage(DamageResult res)
    {
        if (hp <= 0) return;

        hp -= (int)res.finalDamage;
        if(hp <= 0)
        {
            OnObjectDown();
            OnTargetDown?.Invoke();
            hp = 0;

            return;
        }

    }

    private void OnObjectDown()
    {
        StartCoroutine(Co_ObjectDown());
    }

    IEnumerator Co_ObjectDown()
    {
        var targetRot = Quaternion.Euler(90, 0, 0);

        while (Quaternion.Angle(pivot.localRotation, targetRot) > 0.1f)
        {
            pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, targetRot, 200f * Time.deltaTime);
            yield return null;
        }

        pivot.localRotation = targetRot;
        ObjectPoolManager.Instance.Despawn(gameObject);
    }

    public void OnSpawned()
    {
        hp = 100;

        if(pivot) pivot.localRotation = Quaternion.identity;
    }

    public void OnDespawned()
    {
    }
}
