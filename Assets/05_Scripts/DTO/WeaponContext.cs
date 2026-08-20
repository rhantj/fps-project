using UnityEngine;

public class WeaponContext
{
    public GameObject owner;
    public Transform muzzle;

    public float fireRate;
    public float lastFireTime;
    public float spreadAngle;

    public float damage;
    public float hitscanRange;
    public float maxRange;
    public float bulletSpeed;

    public DamageSystem dms;
}

// struct는 값 타입이기 떄문에 복사본 전달 시 필드의 값이 무시된다.
// 따라서 연사에 제한이 걸리지 않고 메서드가 사용되기 때문에
// struct가 아닌 class를 사용한다.