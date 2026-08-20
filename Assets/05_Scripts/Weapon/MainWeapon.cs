using UnityEngine;

public class MainWeapon : Weapon
{
    protected override void Awake()
    {
        base.Awake();
        fireStrategy = new HybridFireStrategy();

        fireModes.Add(FireMode.Single, new SingleFireMode());
        fireModes.Add(FireMode.SemiAuto, new SemiAutoFireMode());
        fireModes.Add(FireMode.Auto, new FullAutoFireMode());
        SetFireMode(FireMode.Auto);

        context.muzzle = muzzle;
        context.fireRate = fireRate;
        context.damage = hitDamage;
        context.hitscanRange = hitscanRange;
        context.bulletSpeed = bulletSpeed;
        context.maxRange = maxRange;
        context.spreadAngle = spreadAngle;

        CurrentMag = MaxMag;


    }

    protected override void Start()
    {
        base.Start();
        fireSound = soundManager.GetClip("SFX_Single Shot");
    }
}
