public class SemiAutoFireMode : IFireModeStrategy
{
    public void Tick(Weapon weapon, WeaponContext ctx, FireInputContext input)
    {
        // isPressed를 쓰면 FullAuto와 완전히 같아진다. 반자동은 당길 때마다 1발이다.
        if (!input.wasPressedThisFrame) return;
        weapon.DoFire(ctx);
    }
}

