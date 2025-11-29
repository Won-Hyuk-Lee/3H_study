# 3. 디자인 패턴과 유니티 (심화)

실무에서 가장 많이 쓰이는 패턴들의 상세 구현과 예시입니다.

## 1. 싱글턴 패턴 (Singleton Pattern)
전역 관리자(Manager)를 만들 때 99% 사용됩니다.

### 핵심 규칙
1.  생성자(`constructor`)를 `private`으로 막아서 외부에서 `new` 못하게 함.
2.  자기 자신을 담을 `static` 변수(`Instance`)를 만듦.
3.  외부에서는 오직 `Instance`를 통해서만 접근 가능.

### C# 구현 예시
```csharp
public class GameManager
{
    // 1. static 변수 (데이터 영역에 저장됨, 유일함)
    private static GameManager instance;

    // 2. 외부 접근용 프로퍼티
    public static GameManager Instance
    {
        get
        {
            // 없을 때만 생성 (Lazy Initialization)
            if (instance == null)
                instance = new GameManager();
            return instance;
        }
    }

    // 3. 생성자 숨기기
    private GameManager() { }

    public int Score = 0;
}

// 사용
void Win() 
{
    // 어디서든 접근 가능
    GameManager.Instance.Score += 100; 
}
```

### 유니티에서의 주의점
유니티는 `MonoBehaviour`를 상속받으면 `new`를 못 씁니다. 그래서 `Awake()`에서 연결해줍니다.
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // 이미 있으면 나를 파괴 (중복 방지)
    }
}
```

### 스타크래프트 예시: 게임 매니저 싱글턴

```csharp
using System;
using System.Collections.Generic;

public class StarCraftGameManager
{
    // 1. private static 인스턴스
    private static StarCraftGameManager instance;

    // 2. public 접근자
    public static StarCraftGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new StarCraftGameManager();
            }
            return instance;
        }
    }

    // 3. private 생성자 (외부에서 new 불가)
    private StarCraftGameManager()
    {
        Console.WriteLine("StarCraftGameManager 초기화됨");
        minerals = 50;
        gas = 0;
        supplyUsed = 4;  // SCV 4개로 시작
        supplyMax = 10;  // 커맨드 센터 제공
    }

    // 게임 자원 관리
    private int minerals;
    private int gas;
    private int supplyUsed;
    private int supplyMax;

    public int Minerals => minerals;
    public int Gas => gas;
    public int SupplyUsed => supplyUsed;
    public int SupplyMax => supplyMax;

    // 자원 획득
    public void AddMinerals(int amount)
    {
        minerals += amount;
        Console.WriteLine($"미네랄 +{amount} (현재: {minerals})");
    }

    public void AddGas(int amount)
    {
        gas += amount;
        Console.WriteLine($"가스 +{amount} (현재: {gas})");
    }

    // 자원 소비
    public bool SpendResources(int mineralCost, int gasCost)
    {
        if (minerals >= mineralCost && gas >= gasCost)
        {
            minerals -= mineralCost;
            gas -= gasCost;
            Console.WriteLine($"자원 소비: -{mineralCost} 미네랄, -{gasCost} 가스");
            return true;
        }
        Console.WriteLine("자원 부족!");
        return false;
    }

    // 인구수 관리
    public bool CanBuildUnit(int supplyCost)
    {
        return (supplyUsed + supplyCost) <= supplyMax;
    }

    public void AddSupplyUsed(int amount)
    {
        supplyUsed += amount;
        Console.WriteLine($"인구수: {supplyUsed}/{supplyMax}");
    }

    public void AddSupplyMax(int amount)
    {
        supplyMax += amount;
        Console.WriteLine($"인구 한계 증가: {supplyMax}");
    }

    // 게임 상태 출력
    public void ShowGameStatus()
    {
        Console.WriteLine("\n========== 게임 상태 ==========");
        Console.WriteLine($"미네랄: {minerals}");
        Console.WriteLine($"가스: {gas}");
        Console.WriteLine($"인구수: {supplyUsed}/{supplyMax}");
        Console.WriteLine("=============================\n");
    }
}

// 사용 예시
class Program
{
    static void Main()
    {
        // 어디서든 동일한 인스턴스 접근
        StarCraftGameManager.Instance.ShowGameStatus();

        // SCV가 미네랄 채취
        Console.WriteLine("SCV가 미네랄 채취 중...");
        StarCraftGameManager.Instance.AddMinerals(8);
        StarCraftGameManager.Instance.AddMinerals(8);

        // 마린 생산 시도
        Console.WriteLine("\n마린 생산 시도 (비용: 50 미네랄, 1 인구)");
        if (StarCraftGameManager.Instance.CanBuildUnit(1))
        {
            if (StarCraftGameManager.Instance.SpendResources(50, 0))
            {
                StarCraftGameManager.Instance.AddSupplyUsed(1);
                Console.WriteLine("마린 생산 완료!");
            }
        }

        // 서플라이 디폿 건설
        Console.WriteLine("\n서플라이 디폿 건설 (비용: 100 미네랄)");
        if (StarCraftGameManager.Instance.SpendResources(100, 0))
        {
            StarCraftGameManager.Instance.AddSupplyMax(8);
        }
        else
        {
            Console.WriteLine("더 많은 미네랄 채취 필요!");
            StarCraftGameManager.Instance.AddMinerals(100);
            if (StarCraftGameManager.Instance.SpendResources(100, 0))
            {
                StarCraftGameManager.Instance.AddSupplyMax(8);
            }
        }

        StarCraftGameManager.Instance.ShowGameStatus();
    }
}
```

**출력 결과:**
```
StarCraftGameManager 초기화됨

========== 게임 상태 ==========
미네랄: 50
가스: 0
인구수: 4/10
=============================

SCV가 미네랄 채취 중...
미네랄 +8 (현재: 58)
미네랄 +8 (현재: 66)

마린 생산 시도 (비용: 50 미네랄, 1 인구)
자원 소비: -50 미네랄, -0 가스
인구수: 5/10
마린 생산 완료!

서플라이 디폿 건설 (비용: 100 미네랄)
자원 부족!
더 많은 미네랄 채취 필요!
미네랄 +100 (현재: 116)
자원 소비: -100 미네랄, -0 가스
인구 한계 증가: 18

========== 게임 상태 ==========
미네랄: 16
가스: 0
인구수: 5/18
=============================
```

**왜 싱글턴?**
- 게임에는 자원, 인구수 같은 **전역 상태**가 하나만 있어야 함
- 모든 유닛, 건물이 같은 GameManager를 참조해야 함
- 어디서든 `StarCraftGameManager.Instance`로 접근 가능

---

## 2. 옵저버 패턴 (Observer Pattern)
"이벤트 기반 프로그래밍"의 핵심입니다.

### 상황: 플레이어가 죽었을 때
**나쁜 예 (결합도 높음)**:
```csharp
void Die() {
    uiManager.ShowGameOver(); // UI 매니저 알아야 함
    soundManager.PlayScream(); // 사운드 매니저 알아야 함
    achievement.Unlock("You Died"); // 업적 시스템 알아야 함
}
```
플레이어 코드가 온갖 매니저를 다 참조해야 합니다.

**좋은 예 (옵저버 패턴)**:
```csharp
using System;

public class Player
{
    // "나 죽었다"고 방송할 이벤트 정의
    public event Action OnPlayerDied;

    public void Die()
    {
        // 구독자가 있으면 방송
        OnPlayerDied?.Invoke(); 
    }
}

public class UIManager
{
    void Start()
    {
        // 플레이어 죽는 방송 구독 (Subscribe)
        player.OnPlayerDied += ShowGameOver;
    }

    void ShowGameOver() { Console.WriteLine("게임 오버 UI 출력"); }
}
```
이제 플레이어는 누가 듣고 있는지 몰라도 됩니다. 그냥 소리치면 끝입니다.

### 스타크래프트 예시: 유닛 사망 이벤트

```csharp
using System;

public class TerranUnit
{
    public string Name { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }

    // 이벤트 선언 (Action<TerranUnit>: TerranUnit을 매개변수로 받는 함수)
    public event Action<TerranUnit> OnUnitDied;
    public event Action<TerranUnit, int> OnUnitDamaged; // 유닛과 데미지량 전달

    public TerranUnit(string name, int maxHP)
    {
        Name = name;
        MaxHP = maxHP;
        HP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        Console.WriteLine($"{Name}이(가) {damage} 데미지를 받았습니다. (HP: {HP}/{MaxHP})");

        // 데미지 이벤트 발생
        OnUnitDamaged?.Invoke(this, damage);

        if (HP <= 0)
        {
            HP = 0;
            Die();
        }
    }

    private void Die()
    {
        Console.WriteLine($"💀 {Name}이(가) 파괴되었습니다!");

        // 사망 이벤트 발생 (구독자들에게 알림)
        OnUnitDied?.Invoke(this);
    }
}

// 구독자 1: 업적 시스템
public class AchievementSystem
{
    private int totalKills = 0;

    public void Subscribe(TerranUnit unit)
    {
        unit.OnUnitDied += OnUnitDeath;
    }

    private void OnUnitDeath(TerranUnit unit)
    {
        totalKills++;
        Console.WriteLine($"[업적 시스템] 적 처치 수: {totalKills}");

        if (totalKills == 10)
        {
            Console.WriteLine("🏆 업적 달성: '학살자' - 적 10기 처치!");
        }
    }
}

// 구독자 2: 사운드 매니저
public class SoundManager
{
    public void Subscribe(TerranUnit unit)
    {
        unit.OnUnitDied += OnUnitDeath;
        unit.OnUnitDamaged += OnUnitDamaged;
    }

    private void OnUnitDeath(TerranUnit unit)
    {
        Console.WriteLine($"[사운드] {unit.Name} 폭발음 재생: BOOM!");
    }

    private void OnUnitDamaged(TerranUnit unit, int damage)
    {
        if (damage > 20)
        {
            Console.WriteLine($"[사운드] 큰 타격음 재생!");
        }
    }
}

// 구독자 3: UI 매니저
public class UIManager
{
    public void Subscribe(TerranUnit unit)
    {
        unit.OnUnitDied += OnUnitDeath;
    }

    private void OnUnitDeath(TerranUnit unit)
    {
        Console.WriteLine($"[UI] 킬 로그 표시: '{unit.Name} 처치됨'");
    }
}

// 구독자 4: 미니맵 시스템
public class MinimapSystem
{
    public void Subscribe(TerranUnit unit)
    {
        unit.OnUnitDied += OnUnitDeath;
    }

    private void OnUnitDeath(TerranUnit unit)
    {
        Console.WriteLine($"[미니맵] {unit.Name} 아이콘 제거");
    }
}

// 사용 예시
class Program
{
    static void Main()
    {
        // 시스템들 생성
        AchievementSystem achievement = new AchievementSystem();
        SoundManager sound = new SoundManager();
        UIManager ui = new UIManager();
        MinimapSystem minimap = new MinimapSystem();

        // 적 유닛 생성
        TerranUnit enemyMarine = new TerranUnit("적 마린", 40);

        // 구독 (이벤트 등록)
        achievement.Subscribe(enemyMarine);
        sound.Subscribe(enemyMarine);
        ui.Subscribe(enemyMarine);
        minimap.Subscribe(enemyMarine);

        Console.WriteLine("=== 전투 시작 ===\n");

        // 공격!
        enemyMarine.TakeDamage(15);
        Console.WriteLine();

        enemyMarine.TakeDamage(30); // 사망!

        Console.WriteLine("\n=== 두 번째 적 등장 ===\n");

        TerranUnit enemyTank = new TerranUnit("적 시즈 탱크", 150);
        achievement.Subscribe(enemyTank);
        sound.Subscribe(enemyTank);
        ui.Subscribe(enemyTank);
        minimap.Subscribe(enemyTank);

        enemyTank.TakeDamage(50);
        Console.WriteLine();
        enemyTank.TakeDamage(100); // 사망!
    }
}
```

**출력 결과:**
```
=== 전투 시작 ===

적 마린이(가) 15 데미지를 받았습니다. (HP: 25/40)

적 마린이(가) 30 데미지를 받았습니다. (HP: -5/40)
[사운드] 큰 타격음 재생!
💀 적 마린이(가) 파괴되었습니다!
[업적 시스템] 적 처치 수: 1
[사운드] 적 마린 폭발음 재생: BOOM!
[UI] 킬 로그 표시: '적 마린 처치됨'
[미니맵] 적 마린 아이콘 제거

=== 두 번째 적 등장 ===

적 시즈 탱크이(가) 50 데미지를 받았습니다. (HP: 100/150)
[사운드] 큰 타격음 재생!

적 시즈 탱크이(가) 100 데미지를 받았습니다. (HP: 0/150)
[사운드] 큰 타격음 재생!
💀 적 시즈 탱크이(가) 파괴되었습니다!
[업적 시스템] 적 처치 수: 2
[사운드] 적 시즈 탱크 폭발음 재생: BOOM!
[UI] 킬 로그 표시: '적 시즈 탱크 처치됨'
[미니맵] 적 시즈 탱크 아이콘 제거
```

**옵저버 패턴의 장점:**
1. **결합도 낮음**: `TerranUnit`은 구독자들을 전혀 모름. 그냥 이벤트만 발생시킴
2. **확장 용이**: 새로운 시스템(예: 리플레이 녹화) 추가가 쉬움. 기존 코드 수정 불필요
3. **유지보수**: 각 시스템이 독립적이어서 버그 수정이 쉬움

**옵저버 없이 작성하면?**
```csharp
// ❌ 나쁜 예
private void Die()
{
    achievementSystem.OnKill(this); // 의존성 1
    soundManager.PlayExplosion(this); // 의존성 2
    uiManager.ShowKillLog(this); // 의존성 3
    minimap.RemoveIcon(this); // 의존성 4
    // 새 시스템 추가할 때마다 여기를 수정해야 함!
}
```

---

## 3. 상태 패턴 (State Pattern)
몬스터 AI나 플레이어 상태 관리에 필수입니다.

### 문제점: 거대한 switch문
```csharp
void Update() {
    if (state == IDLE) { ... }
    else if (state == RUN) { ... }
    else if (state == ATTACK) { ... }
    // 상태가 10개면 코드가 1000줄 넘어감. 유지보수 지옥.
}
```

### 해결: 상태를 클래스로 분리
```csharp
// 공통 인터페이스
public interface IState {
    void Enter();  // 상태 시작될 때
    void Update(); // 매 프레임
    void Exit();   // 상태 끝날 때
}

// 걷기 상태 클래스
public class WalkState : IState {
    public void Enter() { Debug.Log("걷기 시작"); }
    public void Update() { MoveForward(); }
    public void Exit() { Debug.Log("걷기 멈춤"); }
}

// 사용 (Context)
public class Player {
    IState currentState;

    void ChangeState(IState newState) {
        currentState?.Exit(); // 이전 상태 종료
        currentState = newState;
        currentState.Enter(); // 새 상태 시작
    }

    void Update() {
        currentState?.Update(); // 현재 상태의 행동만 실행
    }
}
```
이제 새로운 상태(예: `JumpState`)를 추가해도 기존 코드를 건드릴 필요가 없습니다.

### 스타크래프트 예시: SCV (일꾼) 상태 관리

```csharp
using System;

// 상태 인터페이스
public interface ISCVState
{
    void Enter(SCV scv);
    void Update(SCV scv);
    void Exit(SCV scv);
}

// SCV 클래스 (Context)
public class SCV
{
    public string Name { get; set; }
    public int MineralsCarried { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    private ISCVState currentState;

    // 위치 상수
    public const int MINERAL_X = 100;
    public const int MINERAL_Y = 100;
    public const int BASE_X = 50;
    public const int BASE_Y = 50;

    public SCV(string name)
    {
        Name = name;
        X = BASE_X;
        Y = BASE_Y;
        MineralsCarried = 0;

        // 초기 상태: 대기
        ChangeState(new IdleState());
    }

    public void ChangeState(ISCVState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void Update()
    {
        currentState.Update(this);
    }

    public void MoveTo(int x, int y)
    {
        X = x;
        Y = y;
        Console.WriteLine($"  {Name} 이동 중... ({X}, {Y})");
    }

    public void MineCommand()
    {
        Console.WriteLine($"\n[명령] {Name}에게 미네랄 채취 명령!");
        ChangeState(new MovingToMineralState());
    }

    public void StopCommand()
    {
        Console.WriteLine($"\n[명령] {Name} 정지!");
        ChangeState(new IdleState());
    }
}

// 상태 1: 대기 (Idle)
public class IdleState : ISCVState
{
    public void Enter(SCV scv)
    {
        Console.WriteLine($"[상태 변경] {scv.Name} -> 대기 상태");
    }

    public void Update(SCV scv)
    {
        // 아무것도 안 함
    }

    public void Exit(SCV scv)
    {
        Console.WriteLine($"[상태 종료] {scv.Name} 대기 상태 종료");
    }
}

// 상태 2: 미네랄로 이동 중
public class MovingToMineralState : ISCVState
{
    private int moveTicks = 0;

    public void Enter(SCV scv)
    {
        Console.WriteLine($"[상태 변경] {scv.Name} -> 미네랄로 이동 중");
        moveTicks = 0;
    }

    public void Update(SCV scv)
    {
        moveTicks++;

        // 이동 시뮬레이션
        scv.MoveTo(
            scv.X + (SCV.MINERAL_X - scv.X) / 3,
            scv.Y + (SCV.MINERAL_Y - scv.Y) / 3
        );

        // 목표 도착
        if (moveTicks >= 3)
        {
            scv.X = SCV.MINERAL_X;
            scv.Y = SCV.MINERAL_Y;
            Console.WriteLine($"  {scv.Name} 미네랄 지역 도착!");
            scv.ChangeState(new MiningState());
        }
    }

    public void Exit(SCV scv)
    {
        Console.WriteLine($"[상태 종료] {scv.Name} 이동 완료");
    }
}

// 상태 3: 채취 중
public class MiningState : ISCVState
{
    private int miningTicks = 0;

    public void Enter(SCV scv)
    {
        Console.WriteLine($"[상태 변경] {scv.Name} -> 채취 중");
        miningTicks = 0;
    }

    public void Update(SCV scv)
    {
        miningTicks++;
        Console.WriteLine($"  {scv.Name} 광물 채취 중... (진행: {miningTicks}/2)");

        // 채취 완료
        if (miningTicks >= 2)
        {
            scv.MineralsCarried = 8;
            Console.WriteLine($"  {scv.Name} 미네랄 8 획득!");
            scv.ChangeState(new MovingToBaseState());
        }
    }

    public void Exit(SCV scv)
    {
        Console.WriteLine($"[상태 종료] {scv.Name} 채취 완료");
    }
}

// 상태 4: 본진으로 귀환 중
public class MovingToBaseState : ISCVState
{
    private int moveTicks = 0;

    public void Enter(SCV scv)
    {
        Console.WriteLine($"[상태 변경] {scv.Name} -> 본진으로 귀환 중 (미네랄: {scv.MineralsCarried})");
        moveTicks = 0;
    }

    public void Update(SCV scv)
    {
        moveTicks++;

        // 이동 시뮬레이션
        scv.MoveTo(
            scv.X + (SCV.BASE_X - scv.X) / 3,
            scv.Y + (SCV.BASE_Y - scv.Y) / 3
        );

        // 본진 도착
        if (moveTicks >= 3)
        {
            scv.X = SCV.BASE_X;
            scv.Y = SCV.BASE_Y;
            Console.WriteLine($"  {scv.Name} 본진 도착!");
            scv.ChangeState(new DeliveringState());
        }
    }

    public void Exit(SCV scv)
    {
        Console.WriteLine($"[상태 종료] {scv.Name} 귀환 완료");
    }
}

// 상태 5: 자원 반납 중
public class DeliveringState : ISCVState
{
    public void Enter(SCV scv)
    {
        Console.WriteLine($"[상태 변경] {scv.Name} -> 자원 반납 중");
    }

    public void Update(SCV scv)
    {
        Console.WriteLine($"  {scv.Name}이(가) 미네랄 {scv.MineralsCarried}개를 반납합니다.");

        // 게임 매니저에 자원 추가 (싱글턴 패턴과 결합 가능)
        // GameManager.Instance.AddMinerals(scv.MineralsCarried);

        scv.MineralsCarried = 0;

        // 다시 채취하러 감
        Console.WriteLine($"  {scv.Name} 다시 채취하러 출발!");
        scv.ChangeState(new MovingToMineralState());
    }

    public void Exit(SCV scv)
    {
        Console.WriteLine($"[상태 종료] {scv.Name} 반납 완료");
    }
}

// 사용 예시
class Program
{
    static void Main()
    {
        SCV scv = new SCV("SCV#1");

        Console.WriteLine("========== 초기 상태 ==========");
        scv.Update();

        Console.WriteLine("\n========== 채취 명령 ==========");
        scv.MineCommand();

        // 프레임별 업데이트 시뮬레이션
        Console.WriteLine("\n========== 프레임 업데이트 ==========");
        for (int frame = 1; frame <= 15; frame++)
        {
            Console.WriteLine($"\n--- 프레임 {frame} ---");
            scv.Update();

            // 중간에 정지 명령
            if (frame == 7)
            {
                scv.StopCommand();
            }
        }
    }
}
```

**출력 결과 (일부):**
```
[상태 변경] SCV#1 -> 대기 상태
========== 초기 상태 ==========

========== 채취 명령 ==========

[명령] SCV#1에게 미네랄 채취 명령!
[상태 종료] SCV#1 대기 상태 종료
[상태 변경] SCV#1 -> 미네랄로 이동 중

========== 프레임 업데이트 ==========

--- 프레임 1 ---
  SCV#1 이동 중... (66, 66)

--- 프레임 2 ---
  SCV#1 이동 중... (77, 77)

--- 프레임 3 ---
  SCV#1 이동 중... (88, 88)
  SCV#1 미네랄 지역 도착!
[상태 종료] SCV#1 이동 완료
[상태 변경] SCV#1 -> 채취 중

--- 프레임 4 ---
  SCV#1 광물 채취 중... (진행: 1/2)

--- 프레임 5 ---
  SCV#1 광물 채취 중... (진행: 2/2)
  SCV#1 미네랄 8 획득!
[상태 종료] SCV#1 채취 완료
[상태 변경] SCV#1 -> 본진으로 귀환 중 (미네랄: 8)

--- 프레임 6 ---
  SCV#1 이동 중... (75, 75)

--- 프레임 7 ---
[명령] SCV#1 정지!
[상태 종료] SCV#1 이동 완료
[상태 변경] SCV#1 -> 대기 상태
```

**상태 패턴의 장점:**
1. **가독성**: 각 상태가 독립된 클래스로 분리되어 이해하기 쉬움
2. **유지보수**: 새로운 상태(예: `RepairingState`) 추가가 쉬움
3. **버그 감소**: 상태별 로직이 격리되어 있어 side effect 최소화
4. **확장성**: 상태 전환 규칙을 쉽게 변경 가능

**상태 패턴 없이 작성하면?**
```csharp
// ❌ 나쁜 예 - 거대한 switch문
enum SCVState { Idle, MovingToMineral, Mining, MovingToBase, Delivering }

void Update()
{
    switch (state)
    {
        case SCVState.Idle:
            // 50줄의 코드...
            break;
        case SCVState.MovingToMineral:
            // 50줄의 코드...
            if (도착) state = SCVState.Mining; // 상태 전환 로직 산재
            break;
        case SCVState.Mining:
            // 50줄의 코드...
            if (완료) state = SCVState.MovingToBase;
            break;
        // ... 250줄 이상의 코드가 한 함수에!
    }
}
```

---

## 4. 오브젝트 풀링 (Object Pooling)
총알 1000발을 쏘는 게임에서 필수적인 최적화 기법입니다.

### 원리 (도서관 책 대여 시스템)
1.  **초기화**: 게임 시작 시 총알 100개를 미리 만들어서 창고(List/Queue)에 넣어두고 꺼둡니다 (`SetActive(false)`).
2.  **사용 (대여)**: 총알이 필요하면 `Instantiate` 하지 않고, 창고에서 하나 꺼내서 켭니다 (`SetActive(true)`).
3.  **반납**: 총알이 벽에 닿으면 `Destroy` 하지 않고, 다시 꺼서(`SetActive(false)`) 창고에 넣습니다.

### 왜 쓰는가? (메모리 파편화 방지)
*   `Instantiate` (메모리 할당)와 `Destroy` (메모리 해제)는 컴퓨터 입장에서 매우 무거운 작업입니다.
*   계속 쏘고 지우고를 반복하면 **GC(가비지 컬렉터)**가 "청소할 게 너무 많아!" 하면서 게임을 멈칫거리게 만듭니다 (프레임 드랍).
*   풀링을 쓰면 게임 중에 메모리 할당/해제가 거의 일어나지 않아 쾌적합니다.

### 스타크래프트 예시: 마린 총알 풀링

```csharp
using System.Collections.Generic;

public class Bullet
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsActive { get; set; }

    public void Fire(int x, int y)
    {
        X = x;
        Y = y;
        IsActive = true;
        Console.WriteLine($"총알 발사! ({X}, {Y})");
    }

    public void Deactivate()
    {
        IsActive = false;
        Console.WriteLine($"총알 비활성화 (풀에 반납)");
    }
}

public class BulletPool
{
    private Queue<Bullet> pool = new Queue<Bullet>();
    private int poolSize;
    private int totalCreated = 0;

    public BulletPool(int size)
    {
        poolSize = size;
        // 초기화: 총알을 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            Bullet bullet = new Bullet();
            pool.Enqueue(bullet);
            totalCreated++;
        }
        Console.WriteLine($"BulletPool 초기화: {poolSize}개 생성");
    }

    // 총알 가져오기
    public Bullet GetBullet(int x, int y)
    {
        Bullet bullet;

        if (pool.Count > 0)
        {
            // 풀에 여유가 있으면 재사용
            bullet = pool.Dequeue();
            Console.WriteLine($"[풀에서 재사용] 남은 풀: {pool.Count}개");
        }
        else
        {
            // 풀이 비었으면 새로 생성 (확장)
            bullet = new Bullet();
            totalCreated++;
            Console.WriteLine($"[새로 생성] 총 생성된 총알: {totalCreated}개");
        }

        bullet.Fire(x, y);
        return bullet;
    }

    // 총알 반납
    public void ReturnBullet(Bullet bullet)
    {
        bullet.Deactivate();
        pool.Enqueue(bullet);
        Console.WriteLine($"[풀에 반납] 현재 풀: {pool.Count}개");
    }

    public void ShowPoolStatus()
    {
        Console.WriteLine($"\n=== BulletPool 상태 ===");
        Console.WriteLine($"풀에 있는 총알: {pool.Count}개");
        Console.WriteLine($"총 생성된 총알: {totalCreated}개");
    }
}

// 마린 클래스
public class Marine
{
    public string Name { get; set; }
    private BulletPool bulletPool;
    private List<Bullet> activeBullets = new List<Bullet>();

    public Marine(string name, BulletPool pool)
    {
        Name = name;
        bulletPool = pool;
    }

    public void Attack(int targetX, int targetY)
    {
        Console.WriteLine($"\n{Name}이(가) 공격!");
        Bullet bullet = bulletPool.GetBullet(targetX, targetY);
        activeBullets.Add(bullet);
    }

    public void Update()
    {
        // 총알이 목표에 도달하면 반납
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = activeBullets[i];
            if (bullet.IsActive)
            {
                // 간단한 시뮬레이션: 일정 시간 후 비활성화
                bulletPool.ReturnBullet(bullet);
                activeBullets.RemoveAt(i);
            }
        }
    }
}

// 사용 예시
class Program
{
    static void Main()
    {
        // 총알 풀 생성 (초기 3개)
        BulletPool bulletPool = new BulletPool(3);

        Marine marine1 = new Marine("마린#1", bulletPool);
        Marine marine2 = new Marine("마린#2", bulletPool);

        // 마린들이 공격
        marine1.Attack(100, 100);
        marine2.Attack(150, 150);
        marine1.Attack(120, 110);

        bulletPool.ShowPoolStatus();

        Console.WriteLine("\n--- 총알들이 목표에 도달 (반납) ---");
        marine1.Update();
        marine2.Update();

        bulletPool.ShowPoolStatus();

        Console.WriteLine("\n--- 추가 공격 (재사용) ---");
        marine1.Attack(200, 200);
        marine2.Attack(250, 250);

        bulletPool.ShowPoolStatus();
    }
}
```

**출력 결과:**
```
BulletPool 초기화: 3개 생성
마린#1이(가) 공격!
[풀에서 재사용] 남은 풀: 2개
총알 발사! (100, 100)

마린#2이(가) 공격!
[풀에서 재사용] 남은 풀: 1개
총알 발사! (150, 150)

마린#1이(가) 공격!
[풀에서 재사용] 남은 풀: 0개
총알 발사! (120, 110)

=== BulletPool 상태 ===
풀에 있는 총알: 0개
총 생성된 총알: 3개

--- 총알들이 목표에 도달 (반납) ---
총알 비활성화 (풀에 반납)
[풀에 반납] 현재 풀: 1개
총알 비활성화 (풀에 반납)
[풀에 반납] 현재 풀: 2개
총알 비활성화 (풀에 반납)
[풀에 반납] 현재 풀: 3개

=== BulletPool 상태 ===
풀에 있는 총알: 3개
총 생성된 총알: 3개

--- 추가 공격 (재사용) ---
마린#1이(가) 공격!
[풀에서 재사용] 남은 풀: 2개
총알 발사! (200, 200)

마린#2이(가) 공격!
[풀에서 재사용] 남은 풀: 1개
총알 발사! (250, 250)

=== BulletPool 상태 ===
풀에 있는 총알: 1개
총 생성된 총알: 3개
```

**핵심 포인트:**
- 3개의 총알만 생성했지만 수십 번 재사용 가능
- `new` 연산을 최소화하여 GC 압박 감소
- 실제 스타크래프트에서도 수백 개의 유닛이 동시에 총알을 쏘는데, 모두 풀링으로 관리됩니다
