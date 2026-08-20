using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FireMode
{
    Single,
    SemiAuto,
    Auto
}

public abstract class Weapon : MonoBehaviour
{
    protected WeaponContext context;
    protected IWeaponFireStrategy fireStrategy;
    protected Dictionary<FireMode, IFireModeStrategy> fireModes = new();
    [SerializeField] protected FireMode currentMode;
    FireMode[] modes;
    protected SoundManager soundManager;
    protected AudioClip fireSound;

    [Header("References"), Tooltip("탄환과 이펙트가 나오는 위치")]
    public Transform muzzle;
    public Transform muzzleVFX;

    [Header("Hit-scan"), Tooltip("히트 스캔 방식에 필요한 변수")]
    public float hitscanRange = 30f;
    public int hitDamage = 30;

    [Header("Ballistic"), Tooltip("탄도학 계산에 필요한 변수")]
    public float bulletSpeed = 800f;
    public float fireRate = 0.1f;
    public float maxRange = 150f;

    [Header("Spread"), Tooltip("탄환 퍼짐 정도")]
    public float spreadAngle = 0.6f;
    Quaternion rotOffset = Quaternion.Euler(0, -90f, 0);

    [Header("Mag"), Tooltip("장탄 수 제한")]
    [SerializeField] int maxMag;
    [SerializeField] int currentMag;
    public int MaxMag { get { return maxMag; } set { maxMag = value; } }
    public int CurrentMag 
    { 
        get { return currentMag; } 
        set 
        { 
            currentMag = value;
            OnAmmoChanged?.Invoke(CurrentMag, MaxMag);
        } 
    }
    public FireMode CurrentMode { get { return currentMode; } }
    public virtual bool UseThrowState => false;

    public event Action<int, int> OnWeaponFire;
    public event Action OnReload;
    public event Action OnAmmoEmpty;
    public event Action<int, int> OnAmmoChanged;
    public event Action<string> OnFireModeChanged;

    protected virtual void Awake()
    {
        context = new();
        modes = Enum.GetValues(typeof(FireMode)).Cast<FireMode>().ToArray();

        CurrentMag = MaxMag;
    }

    protected virtual void Start()
    {
        context.dms = StaticRegistry.Find<DamageSystem>();
        soundManager = StaticRegistry.Find<SoundManager>();
    }

    public void SetFireMode(FireMode mode)
    {
        if (!fireModes.ContainsKey(mode)) return;
        currentMode = mode;
        OnFireModeChanged?.Invoke(currentMode.ToString());
    }

    public void NextFireMode()
    {
        int startIdx = Array.IndexOf(modes, currentMode);

        for (int i = 1; i <= modes.Length; ++i)
        {
            FireMode next = modes[(startIdx + i) % modes.Length];
            if (fireModes.ContainsKey(next))
            {
                currentMode = next;
                OnFireModeChanged?.Invoke(currentMode.ToString());
                return;
            }
        }
    }

    public virtual void Fire(FireInputContext input)
    {
        if (!fireModes.TryGetValue(currentMode, out var mode)) return;
        mode.Tick(this, context, input);
    }

    public virtual void DoFire(WeaponContext context)
    {
        if (CurrentMag <= 0)
        {
            AmmoEmptyInvoke();
            return;
        }

        if (!fireStrategy.Fire(context)) return;

        CurrentMag--;
        if(fireSound != null && muzzleVFX)
        {
            soundManager.PlaySound(fireSound, muzzleVFX.position, muzzleVFX.rotation);
            ObjectPoolManager.Instance.Spawn(PoolId.MuzzleVFX, muzzleVFX.position, muzzleVFX.rotation * rotOffset);
        }

        OnWeaponFire?.Invoke(CurrentMag, MaxMag);

        if (CurrentMag <= 0)
        {
            AmmoEmptyInvoke();
        }
    }

    public void ReloadInvoke()
    {
        OnReload?.Invoke();
    }

    public void AmmoEmptyInvoke()
    {
        OnAmmoEmpty?.Invoke();
    }
}