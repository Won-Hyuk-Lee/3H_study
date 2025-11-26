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
