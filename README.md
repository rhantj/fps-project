# FPS Project

FPS 게임의 기초 시스템(이동·사격·데미지·UI)을 직접 설계해 구현한 Unity 개인 프로젝트입니다.
게임의 완성도보다 **각 시스템을 확장 가능한 구조로 분리하는 것**에 초점을 맞췄습니다.

> 오즈코딩스쿨 게임개발 1기 2차 프로젝트 (Personal 01)

---

## 목차

- [개발 환경](#개발-환경)
- [조작](#조작)
- [폴더 구조](#폴더-구조)
- [적용한 디자인 패턴](#적용한-디자인-패턴)
  - [1. State Pattern — 플레이어 상태 관리](#1-state-pattern--플레이어-상태-관리)
  - [2. Strategy Pattern — 사격 로직 분리](#2-strategy-pattern--사격-로직-분리)
  - [3. Pipeline (Chain of Responsibility) — 데미지 계산](#3-pipeline-chain-of-responsibility--데미지-계산)
  - [4. Object Pool Pattern — 인스턴스 재사용](#4-object-pool-pattern--인스턴스-재사용)
  - [5. MVVM — UI 계층 분리](#5-mvvm--ui-계층-분리)
  - [6. Service Locator — 매니저 참조](#6-service-locator--매니저-참조)
  - [7. Observer Pattern — C# event 기반 통신](#7-observer-pattern--c-event-기반-통신)
  - [8. Template Method — 공통 흐름 고정](#8-template-method--공통-흐름-고정)
  - [9. Context Object (DTO) — 파라미터 묶음](#9-context-object-dto--파라미터-묶음)
- [주요 시스템](#주요-시스템)
  - [이동 시스템](#이동-시스템)
  - [사격 시스템 — 하이브리드 탄도](#사격-시스템--하이브리드-탄도)
  - [데미지 시스템](#데미지-시스템)
  - [게임 상태 관리](#게임-상태-관리)
  - [사운드 시스템](#사운드-시스템)
- [사용 에셋](#사용-에셋)
- [개선 예정](#개선-예정)

---

## 개발 환경

| 항목 | 버전 |
|------|------|
| Unity | 6000.3.2f1 |
| 렌더 파이프라인 | Universal RP 17.3.0 |
| 입력 | Input System 1.17.0 |
| 언어 | C# |

---

## 조작

| 키 | 동작 |
|----|------|
| `W` `A` `S` `D` | 이동 |
| `Shift` | 달리기 |
| `Space` | 점프 |
| `Ctrl` / `C` | 앉기 (Hold / Toggle) |
| `마우스 좌클릭` | 사격 |
| `R` | 재장전 |
| `B` | 발사 모드 전환 (단발 / 점사 / 연사) |
| `V` | 근접 공격 |
| `1` / `2` / `G` | 주무기 / 보조무기 / 수류탄 |
| `Esc` | 일시정지 · 이전 UI로 복귀 |

---

## 폴더 구조

```
Assets/05_Scripts/
├── DTO/          데이터 전달 객체 (DamageContext, WeaponContext, FireInputContext)
├── Interface/    IDamageable, IWeaponFireStrategy, IFireModeStrategy, IPoolable ...
├── Managers/     GameManager, SoundManager, ObjectPoolManager, StaticRegistry
│   └── DamageSystem/   DamagePipeline + Modifier 구현체
├── Player/       PlayerController, PlayerContext, StateMachine
│   └── States/   Movement State / Action State
├── Target/       피격 대상
├── UI/           View(UIPanel) + ViewModels
└── Weapon/       Weapon 베이스 + 발사 전략 / 발사 모드
```

---

# 적용한 디자인 패턴

## 1. State Pattern — 플레이어 상태 관리

`Player/StateMachine.cs`, `Player/States/`

상태별 로직을 클래스로 분리하고, `StateMachine`이 `Dictionary<StateName, BaseState>`로 보관하며 전이를 담당합니다.

```csharp
public abstract class BaseState
{
    protected PlayerController Controller { get; private set; }

    public virtual void OnEnterState() { }
    public virtual void OnUpdateState() { }
    public virtual void OnFixedUpdateState() { }
    public virtual void OnExitState() { }
}
```

**이중 FSM 구조**
`PlayerContext`가 이동용·행동용 상태 머신을 각각 하나씩 소유합니다.

| FSM | 상태 |
|-----|------|
| `MovementSM` | Idle · Move · Sprint · Jump · Crouch |
| `ActionSM` | ActionIdle · Fire · Reload · Melee · Throw |

두 축을 하나의 FSM에 넣으면 `달리며 재장전`, `앉아서 사격` 같은 조합마다 상태가 필요해 전이 수가 곱으로 늘어납니다.
축을 분리해 상태 수를 **곱이 아닌 합**으로 유지했습니다.

상태 진입/이탈 시점에 값을 되돌리는 책임도 상태 본인이 가집니다.

```csharp
// SprintState — 진입 시 올린 속도를 이탈 시 스스로 되돌린다
public override void OnEnterState() { speed = playerCtx.MoveSpeed * 0.5f; playerCtx.MoveSpeed += speed; }
public override void OnExitState()  { playerCtx.MoveSpeed -= speed; }
```

---

## 2. Strategy Pattern — 사격 로직 분리

`Weapon/`, `Interface/IWeaponFireStrategy.cs`, `Interface/IFireModeStrategy.cs`

발사를 **두 개의 독립된 축**으로 나눴습니다.

| 인터페이스 | 책임 | 구현체 |
|-----------|------|--------|
| `IWeaponFireStrategy` | **어떻게** 나가는가 | `HybridFireStrategy`, `GrenadeStrategy` |
| `IFireModeStrategy` | **언제** 나가는가 | `SingleFireMode`, `SemiAutoFireMode`, `FullAutoFireMode` |

```csharp
public virtual void Fire(FireInputContext input)
{
    fireModes[currentMode].Tick(this, context, input);   // 언제
}

public virtual void DoFire(WeaponContext context)
{
    if (CurrentMag <= 0) { AmmoEmptyInvoke(); return; }
    if (!fireStrategy.Fire(context)) return;             // 어떻게
    CurrentMag--;
    ...
}
```

`Weapon`은 두 전략을 조합만 하므로, 새 무기는 **전략 선택 + 스탯 설정**만으로 추가됩니다.

```csharp
// GrenadeWeapon — 전략만 갈아끼운다
fireModes.Add(FireMode.Single, new SingleFireMode());
fireStrategy = new GrenadeStrategy(throwPower);
```

발사 모드 순환(`B` 키)도 무기가 **실제로 등록한 모드만** 건너뛰며 도는 방식이라,
단발만 가진 보조무기와 3모드를 가진 주무기가 같은 코드를 씁니다.

---

## 3. Pipeline (Chain of Responsibility) — 데미지 계산

`Managers/DamageSystem/`

데미지 규칙 하나 = `IDamageModifier` 하나. 파이프라인이 등록 순서대로 통과시킵니다.

```csharp
public interface IDamageModifier
{
    void Modify(ref DamageContext context, ref DamageResult result);
}

public DamageResult Calculate(DamageContext context)
{
    DamageResult res = new() { finalDamage = context.damage };
    for (int i = 0; i < modifiers.Count; ++i)
        modifiers[i].Modify(ref context, ref res);
    return res;
}
```

현재 등록된 Modifier

| Modifier | 역할 |
|----------|------|
| `DistanceFalloffModifier` | 기준 거리(30m) 초과분에 비례해 데미지 감쇠 |
| `HeadShotModifier` | `HitZone.Head` 피격 시 2배 + 크리티컬 플래그 |
| `ArmorModifier` | 방어구 수치만큼 감산 (구조만 준비, 미적용) |

**얻은 것**
- 방어구·속성 저항·버프 같은 규칙 추가가 `Pipeline.Add(new XxxModifier())` 한 줄
- 총알 / 근접 / 폭발이 모두 **같은 계산 경로**를 통과 → 규칙이 한 군데서만 관리됨
- 순서가 곧 계산 순서라 우선순위를 코드 위치로 표현

---

## 4. Object Pool Pattern — 인스턴스 재사용

`Managers/ObjectPoolManager.cs`, `Interface/IPoolable.cs`

`PoolId` enum 기준으로 프리팹별 풀을 관리하고, 인스펙터에서 프리웜 개수를 지정합니다.

```csharp
public enum PoolId { Target, SoundPlayer, Grenade, GrenadeExplosionVFX, HitVFX, MuzzleVFX }

ObjectPoolManager.Instance.Spawn(PoolId.MuzzleVFX, muzzleVFX.position, rot);
ObjectPoolManager.Instance.Despawn(gameObject);
```

- 인스턴스에 `PoolMember`를 자동 부착해 **반환 시 자기 풀을 스스로 찾아감** (호출부가 PoolId를 몰라도 됨)
- `IPoolable.OnSpawned` / `OnDespawned` 로 재사용 시 상태 초기화 (예: 타겟 HP·회전 복구)
- 풀이 비면 즉석 생성 후 확장

사격마다 발생하는 총구 VFX·사운드·피격 이펙트가 주 대상이며, 런타임 `Instantiate`/`Destroy` 를 제거했습니다.

---

## 5. MVVM — UI 계층 분리

`UI/`, `UI/ViewModels/`, `Interface/IBindable.cs`

| 계층 | 역할 | 클래스 |
|------|------|--------|
| View | 표시·입력 전달만 | `IngameUI`, `MenuUI`, `SettingUI` (모두 `UIPanel` 상속) |
| ViewModel | 상태 보관·게임 로직 호출 | `HUDViewModel`, `MenuViewModel`, `SettingViewModel` |
| Model | 게임 시스템 | `PlayerContext`, `WeaponManager`, `GameManager`, `SoundManager` |

```csharp
public interface IBindable<T>
{
    void Bind(T vm);
    void Unbind();
}
```

**바인딩 규칙**
- View는 `vm.OnChanged` 하나만 구독하고, 콜백에서 텍스트를 통째로 갱신
- `Bind()` 첫 줄이 `Unbind()` → 중복 구독 원천 차단
- `UIManager`가 패널을 닫을 때 `UnbindVM()` 을 호출해 **이벤트 누수 방지**

**패널 스택**
`UIManager`가 열린 순서를 리스트로 유지하고, `Esc` 는 최상단부터 역순으로 닫습니다.
설정 → 메뉴 → 게임으로 자연스럽게 되돌아가며, HUD는 스택에서 닫히지 않도록 예외 처리했습니다.

---

## 6. Service Locator — 매니저 참조

`Managers/StaticRegistry.cs`

매니저마다 싱글톤을 만드는 대신, 타입을 키로 쓰는 정적 딕셔너리 하나로 통일했습니다.

```csharp
public static class StaticRegistry
{
    static Dictionary<Type, UnityEngine.Object> _register = new();

    public static void Add<T>(T obj) where T : UnityEngine.Object { ... }
    public static T Find<T>() where T : UnityEngine.Object { ... }
}
```

```csharp
// 등록 (Awake)
StaticRegistry.Add(this);

// 조회 (Start)
soundManager = StaticRegistry.Find<SoundManager>();
```

- `IRegistryAdder` 를 구현한 클래스가 `Awake`에서 자기 자신을 등록
- 씬 안에서 매니저끼리 직렬화 참조를 물고 있을 필요가 없어짐
- 등록 순서 의존은 `PlayerInputActions` 처럼 **코루틴으로 대기**해 해결

```csharp
while ((gm = StaticRegistry.Find<GameManager>()) == null)
    yield return null;
```

---

## 7. Observer Pattern — C# event 기반 통신

게임 로직 → UI 방향은 전부 이벤트입니다. UI가 매 프레임 폴링하지 않습니다.

| 발신 | 이벤트 | 수신 |
|------|--------|------|
| `Weapon` | `OnAmmoChanged` `OnFireModeChanged` `OnAmmoEmpty` | HUD, `PlayerController`(자동 재장전) |
| `PlayerContext` | `OnHPChanged` | HUD |
| `WeaponManager` | `OnWeaponChanged` | HUD, `PlayerController` |
| `GameManager` | `OnPlayStateChanged` `OnTimeScaleChanged` | 입력 활성화 제어 |

```csharp
// 탄약이 비면 상태 머신이 스스로 재장전으로 전이
weapon.OnAmmoEmpty += AmmoEmpty;

private void AmmoEmpty()
{
    if (playerCtx.ActionSM.CurrentState is ReloadState) return;
    playerCtx.ActionSM.ChangeState(StateName.Reload);
}
```

---

## 8. Template Method — 공통 흐름 고정

상위 클래스가 흐름을 정하고, 하위 클래스는 훅만 채웁니다.

```csharp
// UIPanel — 열고 닫는 절차는 고정, 부가 동작만 하위에서
public virtual void Open()
{
    IsOpen = true;
    cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true;
    OnOpened();          // ← 하위 클래스 훅
}

protected virtual void OnOpened() { }
protected virtual void OnClosed() { }
```

`Weapon.Awake()/Start()`, `BaseState`, `BasePlayerState.CommonMovement()` 도 같은 방식입니다.
특히 `BasePlayerState`는 중력·경사·가감속 처리를 전부 가지고 있어,
각 이동 상태는 **전이 조건만 쓰고 `base.OnUpdateState()` 를 호출**하면 됩니다.

---

## 9. Context Object (DTO) — 파라미터 묶음

`DTO/`

인자 개수가 많아지는 것을 막기 위해 문맥 객체로 묶어 전달합니다.

| DTO | 용도 |
|-----|------|
| `WeaponContext` | 총구·연사속도·데미지·사거리·탄속·퍼짐 등 발사에 필요한 전부 |
| `DamageContext` / `DamageResult` | 데미지 계산 입력 / 출력 |
| `FireInputContext` | `isPressed`, `wasPressedThisFrame` |

`WeaponContext`는 **의도적으로 class**입니다.

```csharp
// struct는 값 타입이라 복사본 전달 시 lastFireTime 갱신이 원본에 반영되지 않아
// 연사 제한이 걸리지 않는 문제가 있었음 → class로 변경
public float lastFireTime;
```

---

# 주요 시스템

## 이동 시스템

`Player/States/BasePlayerState.cs`

- `CharacterController` 기반
- **경사 대응**: 발밑으로 Ray를 쏴 법선을 구하고, 경사각(0°~40°)에 따라 중력을 `-25 ~ -60` 사이로 보간
  → 완만한 곳에선 가볍게, 급경사에선 강하게 눌러 미끄러짐 억제
- 이동 방향을 `Vector3.ProjectOnPlane` 으로 지면에 투영해 경사면에서 속도 손실 방지
- `Vector3.SmoothDamp` 로 가감속, 접지 시 `-4f` 를 유지해 계단·요철에서 뜨는 현상 방지

## 사격 시스템 — 하이브리드 탄도

`Weapon/HybridFireStrategy.cs`, `Managers/DamageSystem/BulletManager.cs`

거리에 따라 두 방식을 자동 전환합니다.

| 구간 | 방식 | 이유 |
|------|------|------|
| ~30m (`hitscanRange`) | **히트스캔** — 단일 Raycast로 즉시 판정 | 근거리 반응성, 연산 비용 최소 |
| 30m~150m (`maxRange`) | **투사체** — 코루틴으로 탄속(800m/s)만큼 전진하며 스텝 단위 Raycast | 탄속·비행 시간 표현 |

퍼짐은 `spreadAngle` 범위 내 랜덤 회전을 총구 방향에 적용해 계산합니다.

## 데미지 시스템

- 피격 부위는 **태그**로 판정 (`Head` / `Limb` / 그 외 `Body`)
- 총알·근접·폭발 모두 `DamageContext`를 만들어 동일 파이프라인 통과
- 대상은 `IDamageable.ApplyDamage(DamageResult)` 만 구현하면 피격 대상이 됨
- 근접 공격은 시야 60°를 10개 Ray로 부채꼴 스캔, `HashSet` 으로 중복 타격 제거
- 수류탄은 3초 후 `OverlapSphereNonAlloc` 로 범위 내 대상을 찾아 거리 비례 감쇠 적용

## 게임 상태 관리

`Managers/GameManager.cs`

```csharp
public enum PlayState { None, Playing, Pause }
```

`TimeScale` 프로퍼티가 시간 정지와 **커서 잠금/표시를 함께** 처리해, 일시정지 시 마우스 조작이 UI로 자연스럽게 넘어갑니다.
`OnPlayStateChanged` 를 구독하는 `PlayerInputActions` 가 **플레이 중일 때만 Player 액션맵을 활성화**하므로,
메뉴가 떠 있는 동안 사격·이동 입력이 들어오지 않습니다.

## 사운드 시스템

`Managers/SoundManager.cs`

- 클립을 이름 키 딕셔너리로 프리로드 (`GetClip("SFX_Single Shot")`)
- SFX는 풀에서 꺼낸 `SoundPlayer` 에 3D(`spatialBlend = 1`), BGM은 2D(`0`) + 루프로 재생
- 재생이 끝나면 코루틴이 감지해 자동으로 풀 반환
- 마스터/SFX/BGM 볼륨을 설정 UI와 연동

---

## 사용 에셋

- Low Poly FPS Lite
- Stylized Weapon Pack (M4 Scoped Assault Rifle)
- Free Gun
- Prototype Map
- OccaSoft Crosshairs
- Unity Technologies — Effect Examples
- TextMesh Pro

---

## 개선 예정

- [ ] 적 AI (현재 타겟은 고정 배치)
- [ ] 탄흔 트레이서(LineRenderer) 연출 보강
- [ ] 방어구 Modifier(`ArmorModifier`) 실제 적용
- [ ] 설정값 저장/불러오기
