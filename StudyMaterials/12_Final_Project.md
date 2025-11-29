# 12강: 실전 프로젝트 - 미니 RTS 게임 (Final Project - Mini RTS Game)

## 목차
1. [프로젝트 개요](#프로젝트-개요)
2. [프로젝트 구조 설계](#프로젝트-구조-설계)
3. [게임 매니저 시스템](#게임-매니저-시스템)
4. [유닛 시스템](#유닛-시스템)
5. [자원 시스템](#자원-시스템)
6. [건물 시스템](#건물-시스템)
7. [전투 시스템](#전투-시스템)
8. [UI 시스템](#ui-시스템)
9. [사운드 시스템](#사운드-시스템)
10. [카메라 컨트롤러](#카메라-컨트롤러)
11. [전체 통합](#전체-통합)
12. [최적화 및 팁](#최적화-및-팁)

---

## 프로젝트 개요

### 게임 컨셉: 미니 스타크래프트

1강부터 11강까지 배운 모든 내용을 활용하여 **미니 RTS 게임**을 만듭니다.

### 게임 기능

```
핵심 기능:
✅ 유닛 선택 및 이동 (마우스 클릭)
✅ 유닛 생산 (배럭스에서 마린 생산)
✅ 자원 수집 (SCV가 미네랄 채굴)
✅ 전투 시스템 (마린 공격)
✅ UI (자원 표시, 커맨드 패널)
✅ 사운드 (배경음악, 효과음)
✅ 세이브/로드

구현할 유닛:
- SCV (일꾼, 미네랄 채굴)
- 마린 (전투 유닛)

구현할 건물:
- 커맨드 센터 (본진)
- 배럭스 (마린 생산)

자원:
- 미네랄
- 인구수
```

### 사용할 기술

| 강의 | 사용 기술 |
|------|----------|
| 1강 | Git 버전 관리 |
| 2강 | C# 기초 (델리게이트, 프로퍼티, LINQ) |
| 3강 | 싱글턴, 옵저버, 상태 패턴, 오브젝트 풀링 |
| 4강 | List, Dictionary, Queue |
| 5강 | GameObject, Prefab, Tag/Layer |
| 6강 | MonoBehaviour, GetComponent, Coroutine |
| 7강 | Rigidbody, Collider, Raycast |
| 8강 | Canvas, UI 요소, Layout |
| 9강 | Animator, Animation Clip |
| 10강 | AudioSource, AudioMixer |
| 11강 | SceneManager, JSON 저장 |

---

## 프로젝트 구조 설계

### 폴더 구조

```
Assets/
├── Scripts/
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── ResourceManager.cs
│   │   ├── UnitManager.cs
│   │   ├── SoundManager.cs
│   │   └── SaveLoadManager.cs
│   ├── Units/
│   │   ├── Unit.cs (기본 클래스)
│   │   ├── SCV.cs
│   │   ├── Marine.cs
│   │   └── UnitComponents/
│   │       ├── UnitHealth.cs
│   │       ├── UnitMovement.cs
│   │       ├── UnitCombat.cs
│   │       └── UnitAnimator.cs
│   ├── Buildings/
│   │   ├── Building.cs (기본 클래스)
│   │   ├── CommandCenter.cs
│   │   └── Barracks.cs
│   ├── Resources/
│   │   └── Mineral.cs
│   ├── UI/
│   │   ├── GameUI.cs
│   │   ├── ResourcePanel.cs
│   │   ├── CommandPanel.cs
│   │   └── SelectionPanel.cs
│   └── Camera/
│       └── RTSCameraController.cs
├── Prefabs/
│   ├── Units/
│   ├── Buildings/
│   └── Resources/
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameScene.unity
│   └── LoadingScene.unity
├── Audio/
│   ├── BGM/
│   ├── SFX/
│   └── Voice/
└── Data/
    └── UnitData/ (ScriptableObjects)
```

### 아키텍처 패턴

```
Architecture:
- Singleton: GameManager, ResourceManager, SoundManager
- Observer: 유닛 사망 이벤트, 자원 변경 이벤트
- State Pattern: 유닛 AI 상태
- Object Pooling: 총알, 효과음
- Component Pattern: Unit = Health + Movement + Combat
```

---

## 게임 매니저 시스템

### GameManager (핵심 매니저)

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 게임 전체를 관리하는 핵심 매니저
/// - 싱글턴 패턴
/// - 게임 상태 관리
/// - 다른 매니저들 초기화
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentGameState = GameState.Playing;

    [Header("Managers")]
    public ResourceManager resourceManager;
    public UnitManager unitManager;
    public SoundManager soundManager;

    [Header("Settings")]
    public float gameSpeed = 1f;

    // 이벤트
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameOver;
    public event Action OnVictory;

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    void Awake()
    {
        // 싱글턴 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        Debug.Log("[GameManager] 게임 매니저 초기화");

        // 매니저 참조 설정
        if (resourceManager == null)
            resourceManager = GetComponent<ResourceManager>();

        if (unitManager == null)
            unitManager = GetComponent<UnitManager>();

        if (soundManager == null)
            soundManager = GetComponent<SoundManager>();

        // 초기 게임 속도
        Time.timeScale = gameSpeed;

        Debug.Log("[GameManager] 초기화 완료");
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // ESC: 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        // F1: 게임 속도 조절
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetGameSpeed(1f);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetGameSpeed(2f);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetGameSpeed(0.5f);
        }
    }

    // === 게임 상태 ===

    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;

        Debug.Log($"[GameManager] 게임 상태 변경: {currentGameState} → {newState}");

        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = gameSpeed;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                Debug.Log("[GameManager] 게임 오버!");
                break;

            case GameState.Victory:
                Time.timeScale = 0f;
                OnVictory?.Invoke();
                Debug.Log("[GameManager] 승리!");
                break;
        }
    }

    public void PauseGame()
    {
        ChangeGameState(GameState.Paused);
        Debug.Log("[GameManager] 게임 일시정지");
    }

    public void ResumeGame()
    {
        ChangeGameState(GameState.Playing);
        Debug.Log("[GameManager] 게임 재개");
    }

    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Clamp(speed, 0.5f, 3f);
        Time.timeScale = gameSpeed;
        Debug.Log($"[GameManager] 게임 속도: {gameSpeed}x");
    }

    // === 씬 관리 ===

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[GameManager] 씬 로딩: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] 게임 재시작");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] 게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === 승리/패배 조건 ===

    public void CheckVictoryCondition()
    {
        // 예시: 모든 적 유닛 파괴
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            ChangeGameState(GameState.Victory);
        }
    }

    public void CheckDefeatCondition()
    {
        // 예시: 커맨드 센터 파괴
        GameObject commandCenter = GameObject.FindGameObjectWithTag("CommandCenter");
        if (commandCenter == null)
        {
            ChangeGameState(GameState.GameOver);
        }
    }
}
```

### ResourceManager (자원 관리)

```csharp
using UnityEngine;
using System;

/// <summary>
/// 자원 관리 매니저
/// - 미네랄, 가스, 인구수 관리
/// - 옵저버 패턴 (자원 변경 이벤트)
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private int minerals = 50;
    [SerializeField] private int gas = 0;
    [SerializeField] private int supplyUsed = 4;
    [SerializeField] private int supplyMax = 10;

    // 프로퍼티 (읽기 전용)
    public int Minerals => minerals;
    public int Gas => gas;
    public int SupplyUsed => supplyUsed;
    public int SupplyMax => supplyMax;

    // 이벤트 (옵저버 패턴)
    public event Action<int, int> OnMineralsChanged;  // (oldValue, newValue)
    public event Action<int, int> OnGasChanged;
    public event Action<int, int> OnSupplyChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log($"[Resource] 초기 자원 - 미네랄: {minerals}, 가스: {gas}, 인구수: {supplyUsed}/{supplyMax}");
    }

    // === 미네랄 ===

    public void AddMinerals(int amount)
    {
        int oldValue = minerals;
        minerals += amount;
        OnMineralsChanged?.Invoke(oldValue, minerals);

        Debug.Log($"[Resource] 미네랄 +{amount} (총: {minerals})");
    }

    public bool SpendMinerals(int amount)
    {
        if (minerals >= amount)
        {
            int oldValue = minerals;
            minerals -= amount;
            OnMineralsChanged?.Invoke(oldValue, minerals);

            Debug.Log($"[Resource] 미네랄 -{amount} (남은: {minerals})");
            return true;
        }
        else
        {
            Debug.LogWarning($"[Resource] 미네랄 부족! (필요: {amount}, 보유: {minerals})");
            return false;
        }
    }

    // === 가스 ===

    public void AddGas(int amount)
    {
        int oldValue = gas;
        gas += amount;
        OnGasChanged?.Invoke(oldValue, gas);

        Debug.Log($"[Resource] 가스 +{amount} (총: {gas})");
    }

    public bool SpendGas(int amount)
    {
        if (gas >= amount)
        {
            int oldValue = gas;
            gas -= amount;
            OnGasChanged?.Invoke(oldValue, gas);

            Debug.Log($"[Resource] 가스 -{amount} (남은: {gas})");
            return true;
        }
        else
        {
            Debug.LogWarning($"[Resource] 가스 부족! (필요: {amount}, 보유: {gas})");
            return false;
        }
    }

    // === 인구수 ===

    public void AddSupply(int amount)
    {
        int oldUsed = supplyUsed;
        supplyUsed += amount;
        OnSupplyChanged?.Invoke(oldUsed, supplyUsed);

        Debug.Log($"[Resource] 인구수 사용 +{amount} ({supplyUsed}/{supplyMax})");

        if (supplyUsed >= supplyMax)
        {
            Debug.LogWarning("[Resource] 인구수 한계! 서플라이 디폿을 건설하세요.");
        }
    }

    public void RemoveSupply(int amount)
    {
        int oldUsed = supplyUsed;
        supplyUsed = Mathf.Max(0, supplyUsed - amount);
        OnSupplyChanged?.Invoke(oldUsed, supplyUsed);

        Debug.Log($"[Resource] 인구수 사용 -{amount} ({supplyUsed}/{supplyMax})");
    }

    public void IncreaseSupplyMax(int amount)
    {
        supplyMax += amount;
        Debug.Log($"[Resource] 최대 인구수 증가: {supplyMax}");
    }

    public bool HasEnoughSupply(int amount)
    {
        return (supplyUsed + amount) <= supplyMax;
    }

    // === 자원 소비 (통합) ===

    public bool CanAfford(int mineralCost, int gasCost, int supplyCost)
    {
        bool hasResources = (minerals >= mineralCost) && (gas >= gasCost);
        bool hasSupply = HasEnoughSupply(supplyCost);

        if (!hasResources)
        {
            Debug.LogWarning($"[Resource] 자원 부족! (필요: {mineralCost}M / {gasCost}G, 보유: {minerals}M / {gas}G)");
        }

        if (!hasSupply)
        {
            Debug.LogWarning($"[Resource] 인구수 부족! (필요: {supplyCost}, 여유: {supplyMax - supplyUsed})");
        }

        return hasResources && hasSupply;
    }

    public bool SpendResources(int mineralCost, int gasCost, int supplyCost)
    {
        if (!CanAfford(mineralCost, gasCost, supplyCost))
        {
            return false;
        }

        SpendMinerals(mineralCost);
        SpendGas(gasCost);
        AddSupply(supplyCost);

        Debug.Log($"[Resource] 자원 소비 완료: {mineralCost}M / {gasCost}G / {supplyCost} 인구수");
        return true;
    }

    // === 치트 (테스트용) ===

    void Update()
    {
        // Ctrl+M: 미네랄 +500
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            AddMinerals(500);
            Debug.Log("[Cheat] 미네랄 +500");
        }

        // Ctrl+G: 가스 +500
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.G))
        {
            AddGas(500);
            Debug.Log("[Cheat] 가스 +500");
        }
    }
}
```

---

## 유닛 시스템

### Unit (기본 클래스)

```csharp
using UnityEngine;

/// <summary>
/// 모든 유닛의 기본 클래스
/// - Component 패턴 사용
/// </summary>
public abstract class Unit : MonoBehaviour
{
    [Header("Unit Info")]
    public string unitName = "Unit";
    public int mineralCost = 50;
    public int gasCost = 0;
    public int supplyCost = 1;

    [Header("Components")]
    protected UnitHealth health;
    protected UnitMovement movement;
    protected UnitCombat combat;
    protected UnitAnimator unitAnimator;
    protected Renderer meshRenderer;

    [Header("State")]
    public UnitState currentState = UnitState.Idle;
    public bool isSelected = false;

    public enum UnitState
    {
        Idle,
        Moving,
        Gathering,
        Attacking,
        Dead
    }

    protected virtual void Awake()
    {
        // 컴포넌트 캐싱
        health = GetComponent<UnitHealth>();
        movement = GetComponent<UnitMovement>();
        combat = GetComponent<UnitCombat>();
        unitAnimator = GetComponent<UnitAnimator>();
        meshRenderer = GetComponentInChildren<Renderer>();

        Debug.Log($"[Unit] {unitName} 생성");
    }

    protected virtual void Start()
    {
        // 이벤트 구독
        if (health != null)
        {
            health.OnDeath += OnUnitDeath;
        }
    }

    protected virtual void OnDestroy()
    {
        // 이벤트 구독 해제
        if (health != null)
        {
            health.OnDeath -= OnUnitDeath;
        }
    }

    // === 유닛 제어 ===

    public virtual void Select()
    {
        isSelected = true;

        // 선택 표시 (색상 변경)
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.green;
        }

        Debug.Log($"[Unit] {unitName} 선택됨");
    }

    public virtual void Deselect()
    {
        isSelected = false;

        // 선택 해제
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.white;
        }
    }

    public virtual void MoveTo(Vector3 targetPosition)
    {
        if (movement != null)
        {
            movement.SetDestination(targetPosition);
            ChangeState(UnitState.Moving);
        }
    }

    public virtual void AttackTarget(GameObject target)
    {
        if (combat != null)
        {
            combat.SetTarget(target);
            ChangeState(UnitState.Attacking);
        }
    }

    public virtual void Stop()
    {
        if (movement != null)
        {
            movement.Stop();
        }

        if (combat != null)
        {
            combat.ClearTarget();
        }

        ChangeState(UnitState.Idle);
        Debug.Log($"[Unit] {unitName} 정지");
    }

    // === 상태 변경 ===

    protected void ChangeState(UnitState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"[Unit] {unitName} 상태 변경: {currentState} → {newState}");
        currentState = newState;

        // 애니메이션 업데이트
        if (unitAnimator != null)
        {
            unitAnimator.UpdateAnimation(newState);
        }
    }

    // === 이벤트 핸들러 ===

    protected virtual void OnUnitDeath()
    {
        ChangeState(UnitState.Dead);

        // 인구수 반환
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.RemoveSupply(supplyCost);
        }

        Debug.Log($"[Unit] {unitName} 사망");

        // 2초 후 제거
        Destroy(gameObject, 2f);
    }
}
```

### UnitHealth (체력 컴포넌트)

```csharp
using UnityEngine;
using System;

/// <summary>
/// 유닛 체력 관리 컴포넌트
/// </summary>
public class UnitHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 40;
    public int currentHP;
    public int armor = 0;

    // 이벤트
    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged; // (damage, currentHP)

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        // 방어력 적용
        int actualDamage = Mathf.Max(1, damage - armor);
        currentHP -= actualDamage;

        OnHealthChanged?.Invoke(actualDamage, currentHP);

        Debug.Log($"[Health] {gameObject.name} 피해: {actualDamage} (HP: {currentHP}/{maxHP})");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (currentHP <= 0) return;

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHealthChanged?.Invoke(-amount, currentHP);

        Debug.Log($"[Health] {gameObject.name} 회복: {amount} (HP: {currentHP}/{maxHP})");
    }

    void Die()
    {
        Debug.Log($"[Health] {gameObject.name} 사망!");
        OnDeath?.Invoke();
    }

    public float GetHealthPercent()
    {
        return (float)currentHP / maxHP;
    }
}
```

### UnitMovement (이동 컴포넌트)

```csharp
using UnityEngine;

/// <summary>
/// 유닛 이동 컴포넌트
/// </summary>
public class UnitMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.75f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.5f;

    private Vector3 destination;
    private bool isMoving = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            MoveToDestination();
        }
    }

    public void SetDestination(Vector3 target)
    {
        destination = target;
        destination.y = transform.position.y; // Y축 고정
        isMoving = true;

        Debug.Log($"[Movement] {gameObject.name} 이동 시작: {destination}");
    }

    public void Stop()
    {
        isMoving = false;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }

    void MoveToDestination()
    {
        float distance = Vector3.Distance(transform.position, destination);

        if (distance <= stoppingDistance)
        {
            // 도착
            Stop();
            Debug.Log($"[Movement] {gameObject.name} 목적지 도착");
            return;
        }

        // 이동
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0;

        if (rb != null)
        {
            Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
        }

        // 회전
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, destination);
            Gizmos.DrawWireSphere(destination, 0.5f);
        }
    }
}
```

### UnitCombat (전투 컴포넌트)

```csharp
using UnityEngine;

/// <summary>
/// 유닛 전투 컴포넌트
/// </summary>
public class UnitCombat : MonoBehaviour
{
    [Header("Combat")]
    public int attackDamage = 6;
    public float attackRange = 10f;
    public float attackSpeed = 1.5f; // 초당 공격 횟수
    public GameObject projectilePrefab;
    public Transform firePoint;

    private GameObject target;
    private float lastAttackTime = 0f;
    private UnitMovement movement;

    void Awake()
    {
        movement = GetComponent<UnitMovement>();

        // FirePoint 자동 생성
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0, 1f, 0.5f);
            firePoint = firePointObj.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            HandleCombat();
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
        Debug.Log($"[Combat] {gameObject.name} 타겟 설정: {target.name}");
    }

    public void ClearTarget()
    {
        target = null;
    }

    void HandleCombat()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            // 사거리 밖: 접근
            if (movement != null)
            {
                movement.SetDestination(target.transform.position);
            }
        }
        else
        {
            // 사거리 안: 정지 및 공격
            if (movement != null && movement.IsMoving())
            {
                movement.Stop();
            }

            // 타겟 바라보기
            Vector3 direction = (target.transform.position - transform.position);
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

            // 공격
            if (Time.time >= lastAttackTime + (1f / attackSpeed))
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        if (target == null) return;

        Debug.Log($"[Combat] {gameObject.name} 공격!");

        // 투사체 발사
        if (projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Vector3 targetPos = target.transform.position + Vector3.up;
            Vector3 direction = (targetPos - firePoint.position).normalized;

            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, attackDamage, gameObject.tag);
            }
        }
        else
        {
            // 즉시 데미지
            UnitHealth targetHealth = target.GetComponent<UnitHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
            }
        }

        // 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("GunShot", transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 타겟 라인
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}
```

### Projectile (투사체)

```csharp
using UnityEngine;

/// <summary>
/// 투사체 (총알, 미사일 등)
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 20f;
    public float lifetime = 3f;

    private Vector3 direction;
    private int damage;
    private string shooterTag;

    public void Initialize(Vector3 dir, int dmg, string tag)
    {
        direction = dir.normalized;
        damage = dmg;
        shooterTag = tag;

        transform.forward = direction;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // 자신이 발사한 팀은 무시
        if (other.CompareTag(shooterTag))
        {
            return;
        }

        // 적에게 데미지
        UnitHealth health = other.GetComponent<UnitHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"[Projectile] {other.name}에게 {damage} 데미지");
        }

        Destroy(gameObject);
    }
}
```

### SCV (일꾼 유닛)

```csharp
using UnityEngine;

/// <summary>
/// SCV - 자원 채굴 유닛
/// </summary>
public class SCV : Unit
{
    [Header("Mining")]
    public int miningAmount = 5; // 한 번에 채굴량
    public float miningInterval = 1f; // 채굴 간격
    public float miningRange = 2f;

    private Mineral targetMineral;
    private float lastMiningTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        unitName = "SCV";
        mineralCost = 50;
        supplyCost = 1;
    }

    void Update()
    {
        if (currentState == UnitState.Gathering)
        {
            HandleMining();
        }
    }

    public void StartMining(Mineral mineral)
    {
        targetMineral = mineral;
        ChangeState(UnitState.Gathering);

        Debug.Log($"[SCV] 채굴 시작: {mineral.name}");
    }

    public void StopMining()
    {
        targetMineral = null;
        ChangeState(UnitState.Idle);

        Debug.Log("[SCV] 채굴 중단");
    }

    void HandleMining()
    {
        if (targetMineral == null)
        {
            StopMining();
            return;
        }

        float distance = Vector3.Distance(transform.position, targetMineral.transform.position);

        if (distance > miningRange)
        {
            // 미네랄로 이동
            if (movement != null)
            {
                movement.SetDestination(targetMineral.transform.position);
            }
        }
        else
        {
            // 채굴
            if (movement != null && movement.IsMoving())
            {
                movement.Stop();
            }

            if (Time.time >= lastMiningTime + miningInterval)
            {
                Mine();
                lastMiningTime = Time.time;
            }
        }
    }

    void Mine()
    {
        if (targetMineral != null)
        {
            int minedAmount = targetMineral.Mine(miningAmount);

            if (minedAmount > 0)
            {
                // 자원 추가
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddMinerals(minedAmount);
                }

                Debug.Log($"[SCV] 미네랄 {minedAmount} 채굴");

                // 사운드
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX("Mining", transform.position);
                }
            }
            else
            {
                // 자원 고갈
                StopMining();
            }
        }
    }
}
```

### Marine (전투 유닛)

```csharp
using UnityEngine;

/// <summary>
/// 마린 - 기본 전투 유닛
/// </summary>
public class Marine : Unit
{
    [Header("Marine Specific")]
    public bool stimpackActive = false;
    public float stimpackDuration = 10f;
    public float stimpackSpeedMultiplier = 1.5f;

    protected override void Awake()
    {
        base.Awake();
        unitName = "마린";
        mineralCost = 50;
        supplyCost = 1;
    }

    void Update()
    {
        // T키: 스팀팩
        if (isSelected && Input.GetKeyDown(KeyCode.T))
        {
            UseStimpack();
        }
    }

    public void UseStimpack()
    {
        if (stimpackActive)
        {
            Debug.Log("[Marine] 이미 스팀팩 활성화 중");
            return;
        }

        if (health != null && health.currentHP <= 10)
        {
            Debug.Log("[Marine] HP가 부족하여 스팀팩 사용 불가");
            return;
        }

        // HP 소모
        if (health != null)
        {
            health.TakeDamage(10);
        }

        // 스팀팩 효과 시작
        StartCoroutine(StimpackEffect());

        Debug.Log("[Marine] 스팀팩 사용!");
    }

    System.Collections.IEnumerator StimpackEffect()
    {
        stimpackActive = true;

        // 속도 증가
        if (movement != null)
        {
            movement.moveSpeed *= stimpackSpeedMultiplier;
        }

        // 공격 속도 증가
        if (combat != null)
        {
            combat.attackSpeed *= stimpackSpeedMultiplier;
        }

        // 색상 변경
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
        }

        Debug.Log($"[Marine] 스팀팩 활성화 ({stimpackDuration}초)");

        // 대기
        yield return new WaitForSeconds(stimpackDuration);

        // 효과 종료
        if (movement != null)
        {
            movement.moveSpeed /= stimpackSpeedMultiplier;
        }

        if (combat != null)
        {
            combat.attackSpeed /= stimpackSpeedMultiplier;
        }

        if (meshRenderer != null)
        {
            meshRenderer.material.color = isSelected ? Color.green : Color.white;
        }

        stimpackActive = false;

        Debug.Log("[Marine] 스팀팩 효과 종료");
    }
}
```

---

## 자원 시스템

### Mineral (미네랄)

```csharp
using UnityEngine;

/// <summary>
/// 미네랄 자원
/// </summary>
public class Mineral : MonoBehaviour
{
    [Header("Resource")]
    public int totalAmount = 500;
    private int currentAmount;

    void Awake()
    {
        currentAmount = totalAmount;

        // Trigger Collider 설정
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.isTrigger = true;
        collider.radius = 2f;

        gameObject.tag = "Mineral";
    }

    public int Mine(int amount)
    {
        if (currentAmount <= 0)
        {
            return 0;
        }

        int minedAmount = Mathf.Min(amount, currentAmount);
        currentAmount -= minedAmount;

        Debug.Log($"[Mineral] 채굴: {minedAmount}, 남은 양: {currentAmount}/{totalAmount}");

        if (currentAmount <= 0)
        {
            Deplete();
        }

        return minedAmount;
    }

    void Deplete()
    {
        Debug.Log("[Mineral] 자원 고갈!");

        // 시각적 효과 (색상 변경)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }

        // 2초 후 파괴
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        // SCV가 접근하면
        SCV scv = other.GetComponent<SCV>();
        if (scv != null && currentAmount > 0)
        {
            scv.StartMining(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // SCV가 멀어지면
        SCV scv = other.GetComponent<SCV>();
        if (scv != null)
        {
            scv.StopMining();
        }
    }
}
```

---

## 건물 시스템

### Building (건물 기본 클래스)

```csharp
using UnityEngine;

/// <summary>
/// 건물 기본 클래스
/// </summary>
public abstract class Building : MonoBehaviour
{
    [Header("Building Info")]
    public string buildingName = "Building";
    public int mineralCost = 100;
    public int gasCost = 0;
    public float buildTime = 5f;

    [Header("Health")]
    public int maxHP = 500;
    protected int currentHP;

    protected bool isConstructed = false;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void StartConstruction()
    {
        Debug.Log($"[Building] {buildingName} 건설 시작 ({buildTime}초)");
        StartCoroutine(ConstructionRoutine());
    }

    protected virtual System.Collections.IEnumerator ConstructionRoutine()
    {
        float elapsed = 0f;

        while (elapsed < buildTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / buildTime;

            // 건설 진행도 표시
            Debug.Log($"[Building] {buildingName} 건설 진행: {progress * 100:F0}%");

            yield return null;
        }

        OnConstructionComplete();
    }

    protected virtual void OnConstructionComplete()
    {
        isConstructed = true;
        Debug.Log($"[Building] {buildingName} 건설 완료!");
    }

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"[Building] {buildingName} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Destroy();
        }
    }

    protected virtual void Destroy()
    {
        Debug.Log($"[Building] {buildingName} 파괴됨!");
        Destroy(gameObject);
    }
}
```

### Barracks (배럭스)

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 배럭스 - 마린 생산 건물
/// </summary>
public class Barracks : Building
{
    [Header("Production")]
    public GameObject marinePrefab;
    public Transform spawnPoint;
    public Transform rallyPoint;

    private Queue<UnitProductionOrder> productionQueue = new Queue<UnitProductionOrder>();
    private bool isProducing = false;

    [System.Serializable]
    public class UnitProductionOrder
    {
        public GameObject prefab;
        public string unitName;
        public float productionTime;

        public UnitProductionOrder(GameObject prefab, string name, float time)
        {
            this.prefab = prefab;
            this.unitName = name;
            this.productionTime = time;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        buildingName = "배럭스";
        mineralCost = 150;
        buildTime = 10f;

        // SpawnPoint 자동 생성
        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.SetParent(transform);
            spawnObj.transform.localPosition = new Vector3(3, 0, 0);
            spawnPoint = spawnObj.transform;
        }

        // RallyPoint 자동 생성
        if (rallyPoint == null)
        {
            GameObject rallyObj = new GameObject("RallyPoint");
            rallyObj.transform.SetParent(transform);
            rallyObj.transform.localPosition = new Vector3(5, 0, 0);
            rallyPoint = rallyObj.transform;
        }
    }

    void Update()
    {
        if (!isConstructed) return;

        // 생산 처리
        if (!isProducing && productionQueue.Count > 0)
        {
            StartCoroutine(ProduceUnit());
        }
    }

    public void ProduceMarine()
    {
        if (!isConstructed)
        {
            Debug.Log("[Barracks] 건설 중에는 생산할 수 없습니다");
            return;
        }

        // 자원 체크
        if (ResourceManager.Instance != null)
        {
            if (ResourceManager.Instance.SpendResources(50, 0, 1))
            {
                UnitProductionOrder order = new UnitProductionOrder(marinePrefab, "마린", 3f);
                productionQueue.Enqueue(order);

                Debug.Log($"[Barracks] 마린 생산 대기열 추가 (대기: {productionQueue.Count})");
            }
        }
    }

    IEnumerator ProduceUnit()
    {
        isProducing = true;

        UnitProductionOrder order = productionQueue.Dequeue();
        Debug.Log($"[Barracks] {order.unitName} 생산 중... ({order.productionTime}초)");

        float elapsed = 0f;
        while (elapsed < order.productionTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 유닛 생성
        GameObject unit = Instantiate(order.prefab, spawnPoint.position, Quaternion.identity);
        unit.name = order.unitName;

        Debug.Log($"[Barracks] {order.unitName} 생산 완료!");

        // 집결 지점으로 이동
        if (rallyPoint != null)
        {
            UnitMovement movement = unit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.SetDestination(rallyPoint.position);
            }
        }

        isProducing = false;
    }

    public void SetRallyPoint(Vector3 position)
    {
        if (rallyPoint != null)
        {
            rallyPoint.position = position;
            Debug.Log($"[Barracks] 집결 지점 설정: {position}");
        }
    }

    void OnDrawGizmos()
    {
        // SpawnPoint
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
        }

        // RallyPoint
        if (rallyPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rallyPoint.position, 0.5f);

            if (spawnPoint != null)
            {
                Gizmos.DrawLine(spawnPoint.position, rallyPoint.position);
            }
        }
    }
}
```

---

## UI 시스템

### GameUI (통합 UI)

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 UI 통합 관리
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Resource Panel")]
    public TextMeshProUGUI mineralText;
    public TextMeshProUGUI gasText;
    public TextMeshProUGUI supplyText;

    [Header("Selection Panel")]
    public GameObject selectionPanel;
    public TextMeshProUGUI selectedUnitName;
    public Image selectedUnitHPBar;

    [Header("Command Panel")]
    public GameObject commandPanel;
    public Button[] commandButtons;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 이벤트 구독
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnMineralsChanged += UpdateMinerals;
            ResourceManager.Instance.OnGasChanged += UpdateGas;
            ResourceManager.Instance.OnSupplyChanged += UpdateSupply;
        }

        // 초기 UI 업데이트
        UpdateResourceUI();
        HideSelectionPanel();
        HideCommandPanel();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnMineralsChanged -= UpdateMinerals;
            ResourceManager.Instance.OnGasChanged -= UpdateGas;
            ResourceManager.Instance.OnSupplyChanged -= UpdateSupply;
        }
    }

    // === 자원 UI ===

    void UpdateMinerals(int oldValue, int newValue)
    {
        if (mineralText != null)
        {
            mineralText.text = $"미네랄: {newValue}";
        }
    }

    void UpdateGas(int oldValue, int newValue)
    {
        if (gasText != null)
        {
            gasText.text = $"가스: {newValue}";
        }
    }

    void UpdateSupply(int oldUsed, int newUsed)
    {
        if (supplyText != null)
        {
            int max = ResourceManager.Instance.SupplyMax;
            supplyText.text = $"인구수: {newUsed}/{max}";

            // 색상 변경
            if (newUsed >= max)
            {
                supplyText.color = Color.red;
            }
            else
            {
                supplyText.color = Color.white;
            }
        }
    }

    void UpdateResourceUI()
    {
        if (ResourceManager.Instance != null)
        {
            UpdateMinerals(0, ResourceManager.Instance.Minerals);
            UpdateGas(0, ResourceManager.Instance.Gas);
            UpdateSupply(0, ResourceManager.Instance.SupplyUsed);
        }
    }

    // === 선택 UI ===

    public void ShowSelectionPanel(Unit unit)
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);

            if (selectedUnitName != null)
            {
                selectedUnitName.text = unit.unitName;
            }

            UpdateHealthBar(unit);
        }
    }

    public void HideSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    public void UpdateHealthBar(Unit unit)
    {
        if (selectedUnitHPBar != null && unit != null)
        {
            UnitHealth health = unit.GetComponent<UnitHealth>();
            if (health != null)
            {
                selectedUnitHPBar.fillAmount = health.GetHealthPercent();
            }
        }
    }

    // === 커맨드 패널 ===

    public void ShowCommandPanel(Unit unit)
    {
        if (commandPanel != null)
        {
            commandPanel.SetActive(true);

            // 버튼 설정 (유닛 타입에 따라)
            SetupCommandButtons(unit);
        }
    }

    public void HideCommandPanel()
    {
        if (commandPanel != null)
        {
            commandPanel.SetActive(false);
        }
    }

    void SetupCommandButtons(Unit unit)
    {
        // 모든 버튼 비활성화
        foreach (Button btn in commandButtons)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }

        // 마린인 경우
        if (unit is Marine)
        {
            if (commandButtons.Length > 0 && commandButtons[0] != null)
            {
                commandButtons[0].gameObject.SetActive(true);
                commandButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "스팀팩 (T)";
                commandButtons[0].onClick.RemoveAllListeners();
                commandButtons[0].onClick.AddListener(() =>
                {
                    Marine marine = unit as Marine;
                    marine.UseStimpack();
                });
            }
        }

        // SCV인 경우
        if (unit is SCV)
        {
            // 특별한 명령어 없음
        }
    }
}
```

---

## 사운드 시스템

### SoundManager (사운드 관리)

```csharp
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// 사운드 관리 싱글턴
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("BGM")]
    public AudioClip[] bgmClips;
    private AudioSource bgmSource;

    [Header("SFX")]
    [System.Serializable]
    public class SFX
    {
        public string name;
        public AudioClip clip;
    }
    public SFX[] sfxList;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    public int poolSize = 20;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        // BGM Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // SFX Dictionary
        foreach (SFX sfx in sfxList)
        {
            sfxDictionary[sfx.name] = sfx.clip;
        }

        // SFX Pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateSFXSource();
        }

        Debug.Log("[SoundManager] 초기화 완료");
    }

    AudioSource CreateSFXSource()
    {
        GameObject obj = new GameObject("SFX_Source");
        obj.transform.SetParent(transform);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;

        sfxPool.Enqueue(source);
        return source;
    }

    // === BGM ===

    public void PlayBGM(int index)
    {
        if (index >= 0 && index < bgmClips.Length)
        {
            bgmSource.clip = bgmClips[index];
            bgmSource.Play();
            Debug.Log($"[Sound] BGM 재생: {bgmClips[index].name}");
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // === SFX ===

    public void PlaySFX(string name, Vector3 position = default, float volume = 1f)
    {
        if (!sfxDictionary.ContainsKey(name))
        {
            Debug.LogWarning($"[Sound] SFX 없음: {name}");
            return;
        }

        AudioSource source = GetSFXSource();
        source.transform.position = position;
        source.clip = sfxDictionary[name];
        source.volume = volume;
        source.Play();

        StartCoroutine(ReturnSFXSource(source));
    }

    AudioSource GetSFXSource()
    {
        if (sfxPool.Count > 0)
        {
            return sfxPool.Dequeue();
        }
        else
        {
            return CreateSFXSource();
        }
    }

    System.Collections.IEnumerator ReturnSFXSource(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        sfxPool.Enqueue(source);
    }
}
```

---

## 카메라 컨트롤러

### RTSCameraController

```csharp
using UnityEngine;

/// <summary>
/// RTS 카메라 컨트롤러
/// - WASD 이동
/// - 마우스 스크롤 줌
/// </summary>
public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float edgeScrollSpeed = 30f;
    public float edgeScrollMargin = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    [Header("Bounds")]
    public Vector2 minBounds = new Vector2(-50, -50);
    public Vector2 maxBounds = new Vector2(50, 50);

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 movement = Vector3.zero;

        // WASD 이동
        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }

        // 마우스 엣지 스크롤
        if (Input.mousePosition.x < edgeScrollMargin)
        {
            movement += Vector3.left;
        }
        if (Input.mousePosition.x > Screen.width - edgeScrollMargin)
        {
            movement += Vector3.right;
        }
        if (Input.mousePosition.y < edgeScrollMargin)
        {
            movement += Vector3.back;
        }
        if (Input.mousePosition.y > Screen.height - edgeScrollMargin)
        {
            movement += Vector3.forward;
        }

        // 이동 적용
        if (movement != Vector3.zero)
        {
            Vector3 newPos = transform.position + movement.normalized * moveSpeed * Time.deltaTime;

            // 경계 제한
            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            newPos.z = Mathf.Clamp(newPos.z, minBounds.y, maxBounds.y);

            transform.position = newPos;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            Vector3 newPos = transform.position;
            newPos.y -= scroll * zoomSpeed;
            newPos.y = Mathf.Clamp(newPos.y, minZoom, maxZoom);

            transform.position = newPos;
        }
    }
}
```

---

## 전체 통합

### UnitSelectionController (유닛 선택)

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 유닛 선택 및 제어
/// </summary>
public class UnitSelectionController : MonoBehaviour
{
    [Header("Selection")]
    public List<Unit> selectedUnits = new List<Unit>();
    public LayerMask unitLayer;
    public LayerMask groundLayer;

    void Update()
    {
        HandleSelection();
        HandleCommands();
    }

    // === 선택 ===

    void HandleSelection()
    {
        // 좌클릭: 유닛 선택
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, unitLayer))
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null && unit.CompareTag("Player"))
                {
                    // Shift: 추가 선택
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        DeselectAll();
                    }

                    SelectUnit(unit);
                }
            }
            else
            {
                // 빈 곳 클릭: 선택 해제
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }
            }
        }
    }

    void SelectUnit(Unit unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.Select();

            Debug.Log($"[Selection] {unit.unitName} 선택 (총 {selectedUnits.Count}개)");

            // UI 업데이트
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowSelectionPanel(unit);
                GameUI.Instance.ShowCommandPanel(unit);
            }
        }
    }

    void DeselectAll()
    {
        foreach (Unit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.Deselect();
            }
        }

        selectedUnits.Clear();

        // UI 숨김
        if (GameUI.Instance != null)
        {
            GameUI.Instance.HideSelectionPanel();
            GameUI.Instance.HideCommandPanel();
        }

        Debug.Log("[Selection] 모든 유닛 선택 해제");
    }

    // === 명령 ===

    void HandleCommands()
    {
        if (selectedUnits.Count == 0) return;

        // 우클릭: 이동 또는 공격
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 적 유닛 클릭: 공격
            if (Physics.Raycast(ray, out hit, 100f, unitLayer))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    AttackTarget(hit.collider.gameObject);
                    return;
                }
            }

            // 지면 클릭: 이동
            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                MoveToPosition(hit.point);
            }
        }

        // S키: 정지
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopUnits();
        }
    }

    void MoveToPosition(Vector3 position)
    {
        Debug.Log($"[Command] 이동 명령: {position} ({selectedUnits.Count}개 유닛)");

        foreach (Unit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.MoveTo(position);
            }
        }
    }

    void AttackTarget(GameObject target)
    {
        Debug.Log($"[Command] 공격 명령: {target.name} ({selectedUnits.Count}개 유닛)");

        foreach (Unit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.AttackTarget(target);
            }
        }
    }

    void StopUnits()
    {
        Debug.Log($"[Command] 정지 ({selectedUnits.Count}개 유닛)");

        foreach (Unit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.Stop();
            }
        }
    }
}
```

---

## 최적화 및 팁

### 최적화 기법

```csharp
/// <summary>
/// 최적화 팁 모음
/// </summary>
public class OptimizationTips : MonoBehaviour
{
    // 1. Object Pooling
    Queue<GameObject> bulletPool = new Queue<GameObject>();

    // 2. GetComponent 캐싱
    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // 한 번만
    }

    // 3. Update 최소화
    void Update()
    {
        // 필요한 것만 Update에
    }

    // 4. Coroutine 활용
    void Start()
    {
        StartCoroutine(UpdateEverySecond());
    }

    System.Collections.IEnumerator UpdateEverySecond()
    {
        while (true)
        {
            // 1초마다 실행
            yield return new WaitForSeconds(1f);
        }
    }

    // 5. Layer와 LayerMask 활용
    void RaycastOptimized()
    {
        int layerMask = LayerMask.GetMask("Enemy");
        Physics.Raycast(transform.position, Vector3.forward, 10f, layerMask);
    }

    // 6. String 대신 Hash 사용
    int attackTrigger = Animator.StringToHash("Attack");

    // 7. LINQ 사용 최소화 (성능 중요 시)
    // List.Find 대신 foreach 사용

    // 8. GameObject.Find 사용 금지
    // Start나 Awake에서 한 번만 찾기

    // 9. null 체크 최소화
    void OptimizedUpdate()
    {
        if (rb == null) return; // 초반에 체크

        rb.AddForce(Vector3.up);
    }

    // 10. Physics 설정 최적화
    void PhysicsOptimization()
    {
        // Fixed Timestep: 0.02초 (50fps)
        // Physics.autoSimulation = true;
    }
}
```

### 디버깅 팁

```csharp
/// <summary>
/// 디버깅 도구
/// </summary>
public class DebugTools : MonoBehaviour
{
    // 1. Debug.Log 카테고리화
    void LogWithCategory(string message)
    {
        Debug.Log($"[{GetType().Name}] {message}");
    }

    // 2. Gizmos로 시각화
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }

    // 3. Debug.DrawRay
    void DrawDebugRay()
    {
        Debug.DrawRay(transform.position, Vector3.forward * 10f, Color.green, 1f);
    }

    // 4. 조건부 컴파일
#if UNITY_EDITOR
    void EditorOnlyCode()
    {
        Debug.Log("에디터에서만 실행");
    }
#endif

    // 5. Assert
    void ValidateData()
    {
        Debug.Assert(gameObject != null, "GameObject is null!");
    }
}
```

---

## 프로젝트 완성 체크리스트

### 필수 구현 항목

```
✅ GameManager - 게임 상태 관리
✅ ResourceManager - 자원 관리
✅ UnitManager - 유닛 관리
✅ SoundManager - 사운드 관리

✅ Unit 시스템
  ✅ SCV (미네랄 채굴)
  ✅ Marine (전투)

✅ Building 시스템
  ✅ Barracks (마린 생산)

✅ Resource
  ✅ Mineral

✅ UI
  ✅ 자원 표시
  ✅ 선택 UI
  ✅ 커맨드 패널

✅ 카메라
  ✅ RTS 카메라 (WASD 이동, 줌)

✅ 전투 시스템
  ✅ 유닛 공격
  ✅ 투사체
  ✅ HP 시스템

✅ 사운드
  ✅ BGM
  ✅ 효과음 (총소리, 채굴)

✅ 저장/불러오기
  ✅ JSON 저장
  ✅ 세이브 슬롯
```

### 추가 개선 아이디어

```
선택 사항:
□ 더 많은 유닛 (파이어뱃, 메딕)
□ 더 많은 건물 (벙커, 터렛)
□ 업그레이드 시스템
□ 미니맵
□ 안개 (Fog of War)
□ AI 적 유닛
□ 멀티플레이어 (Photon)
□ 난이도 설정
□ 성취 시스템
```

---

## 정리 및 마무리

### 12강에서 배운 것

✅ **프로젝트 구조 설계** - 폴더 구조, 아키텍처 패턴
✅ **매니저 시스템** - GameManager, ResourceManager 등
✅ **컴포넌트 패턴** - Unit = Health + Movement + Combat
✅ **이벤트 시스템** - 옵저버 패턴으로 UI 업데이트
✅ **오브젝트 풀링** - 투사체, 효과음 최적화
✅ **통합 시스템** - 모든 강의 내용 활용

### 학습 경로 정리

```
1강: Git & Tools
  → 버전 관리 기초

2강: C# 기초
  → 델리게이트, 이벤트, LINQ

3강: 디자인 패턴
  → 싱글턴, 옵저버, 상태 패턴

4강: 자료구조
  → List, Dictionary, Queue

5강: Unity 기초
  → GameObject, Prefab, Tag/Layer

6강: Unity 스크립팅
  → MonoBehaviour, Coroutine

7강: 물리와 충돌
  → Rigidbody, Collider, Raycast

8강: UI 시스템
  → Canvas, UI 요소, Layout

9강: 애니메이션
  → Animator, State Machine

10강: 오디오
  → AudioSource, AudioMixer

11강: 씬과 데이터
  → SceneManager, JSON 저장

12강: 실전 프로젝트
  → 모든 내용 통합!
```

### 다음 단계

이제 여러분은:
- ✅ C#과 유니티의 핵심 개념 이해
- ✅ 실전 게임 프로젝트 구조 설계 가능
- ✅ RTS 게임 핵심 기능 구현 가능

**추천 학습 경로:**
1. 이 프로젝트를 확장 (더 많은 유닛, 건물)
2. 새로운 장르 도전 (RPG, FPS, 퍼즐)
3. 고급 주제 학습 (셰이더, 네트워킹, AI)
4. 포트폴리오 프로젝트 완성

---

## 축하합니다! 🎉

**C# & Unity 게임개발 강의 1~12강 완료!**

이제 여러분은 유니티 게임 개발자입니다!

계속 코딩하고, 계속 만들고, 계속 배우세요! 💪🎮
