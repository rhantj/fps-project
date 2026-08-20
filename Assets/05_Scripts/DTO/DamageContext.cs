using UnityEngine;

// 데미지 계산 DTO

// 데미지의 정보 제공
public struct DamageContext
{
    public GameObject attacker;
    public GameObject target;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public float damage;
    public float distance;

    public DamageType damageType;
    public HitZone hitZone;
}

// 계산된 데미지 구조체
public struct DamageResult
{
    public float finalDamage;
    public bool isCritical;
    public bool isBlocked;
}

// 탄환, 폭발, 근접 판단
public enum DamageType
{
    Bullet,
    Explosion,
    Melee
}

// 히트 부위
public enum HitZone
{
    Body,
    Head,
    Limb
}