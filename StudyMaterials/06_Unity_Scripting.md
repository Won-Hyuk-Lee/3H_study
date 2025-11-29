# 6강: 유니티 스크립팅 기초 (Unity Scripting Fundamentals)

## 목차
1. [MonoBehaviour란?](#monobehaviour란)
2. [생명주기 (Lifecycle) 함수](#생명주기-lifecycle-함수)
3. [GetComponent - 컴포넌트 통신](#getcomponent---컴포넌트-통신)
4. [Input 시스템](#input-시스템)
5. [Instantiate와 Destroy](#instantiate와-destroy)
6. [Coroutine - 코루틴](#coroutine---코루틴)
7. [Time.deltaTime의 중요성](#timedeltatime의-중요성)
8. [ScriptableObject](#scriptableobject)
9. [실전 프로젝트: 스타크래프트 유닛 제어 시스템](#실전-프로젝트-스타크래프트-유닛-제어-시스템)

---

## MonoBehaviour란?

**MonoBehaviour**는 유니티 스크립트의 기반 클래스입니다. 모든 유니티 스크립트는 `MonoBehaviour`를 상속받습니다.

### MonoBehaviour의 역할

```csharp
using UnityEngine;

// MonoBehaviour를 상속받아야 GameObject에 부착 가능
public class MyScript : MonoBehaviour
{
    // 유니티 엔진이 자동으로 호출하는 함수들
    void Awake() { }
    void Start() { }
    void Update() { }
}
```

**MonoBehaviour가 제공하는 것:**
1. **생명주기 함수**: Awake, Start, Update 등
2. **GameObject 참조**: `gameObject`, `transform`
3. **Coroutine 기능**: `StartCoroutine`, `StopCoroutine`
4. **Invoke 기능**: 지연 실행, 반복 실행
5. **유니티 이벤트**: OnCollision, OnTrigger 등

### MonoBehaviour 규칙

```csharp
// ✅ 올바른 방법
public class UnitController : MonoBehaviour
{
    // MonoBehaviour는 new로 생성하면 안 됨!
}

// GameObject에 AddComponent로 추가
GameObject unit = new GameObject("Marine");
UnitController controller = unit.AddComponent<UnitController>();

// ❌ 잘못된 방법
UnitController controller = new UnitController(); // 에러!
```

**핵심 규칙:**
- `new`로 생성 불가
- `AddComponent<T>()`로만 추가 가능
- GameObject에 부착되어야만 작동

---

## 생명주기 (Lifecycle) 함수

유니티는 스크립트의 함수를 **특정 시점에 자동으로 호출**합니다.

### 생명주기 순서

```
게임 시작
    ↓
Awake() ────────→ 초기화 (생성 직후, 1회)
    ↓
OnEnable() ─────→ 활성화될 때마다
    ↓
Start() ────────→ 첫 프레임 전 (1회)
    ↓
    ┌─────────────┐
    │ Update()    │ ← 매 프레임 (불규칙)
    │ FixedUpdate()│ ← 고정 시간마다 (0.02초)
    │ LateUpdate()│ ← Update 후
    └─────────────┘
    ↓
OnDisable() ────→ 비활성화될 때마다
    ↓
OnDestroy() ────→ 파괴될 때 (1회)
```

### 1. Awake()

**가장 먼저 호출**되는 함수. 초기화 작업에 사용합니다.

```csharp
public class Marine : MonoBehaviour
{
    private int hp;
    private Rigidbody rb;

    void Awake()
    {
        // ✅ Awake에서 할 것:
        // 1. 변수 초기화
        hp = 40;

        // 2. 컴포넌트 참조 (자기 자신)
        rb = GetComponent<Rigidbody>();

        // 3. 싱글턴 설정
        if (GameManager.Instance == null)
            GameManager.Instance = this;

        Debug.Log("Awake 호출됨");
    }
}
```

**Awake vs Start:**
- **Awake**: GameObject가 비활성화되어 있어도 호출됨
- **Start**: GameObject가 활성화되어 있을 때만 호출됨

### 2. Start()

**첫 프레임이 시작되기 전**에 호출됩니다. Awake보다 나중입니다.

```csharp
public class Marine : MonoBehaviour
{
    private GameObject target;

    void Start()
    {
        // ✅ Start에서 할 것:
        // 1. 다른 GameObject 참조
        target = GameObject.FindGameObjectWithTag("Enemy");

        // 2. 초기 설정
        transform.position = new Vector3(10, 0, 10);

        // 3. 코루틴 시작
        StartCoroutine(AttackRoutine());

        Debug.Log("Start 호출됨");
    }
}
```

**언제 Awake, 언제 Start?**
```csharp
void Awake()
{
    // 자기 자신의 초기화
    hp = 100;
    rb = GetComponent<Rigidbody>();
}

void Start()
{
    // 다른 오브젝트와의 상호작용
    target = GameObject.Find("Player");
    gameManager = GameManager.Instance;
}
```

### 3. Update()

**매 프레임마다 호출**됩니다. 가장 많이 사용하는 함수입니다.

```csharp
public class UnitMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // ✅ Update에서 할 것:
        // 1. 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // 2. 이동 (비물리)
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // 3. 회전
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0, -100 * Time.deltaTime, 0);
        }

        // 4. 시간 체크
        cooldownTimer -= Time.deltaTime;
    }
}
```

**주의사항:**
```csharp
// ❌ 나쁜 예 - 매 프레임 검색
void Update()
{
    GameObject enemy = GameObject.Find("Enemy"); // 느림!
    enemy.transform.position = Vector3.zero;
}

// ✅ 좋은 예 - 한 번만 검색
private GameObject enemy;

void Start()
{
    enemy = GameObject.Find("Enemy"); // 한 번만
}

void Update()
{
    if (enemy != null)
    {
        enemy.transform.position = Vector3.zero;
    }
}
```

### 4. FixedUpdate()

**고정된 시간 간격**으로 호출됩니다. 물리 연산에 사용합니다.

```csharp
public class PhysicsMovement : MonoBehaviour
{
    private Rigidbody rb;
    public float moveForce = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // ✅ FixedUpdate에서 할 것:
        // 1. Rigidbody 조작
        rb.AddForce(Vector3.forward * moveForce);

        // 2. 물리 기반 이동
        rb.velocity = new Vector3(5, rb.velocity.y, 0);

        // 3. 레이캐스트 (물리 관련)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            Debug.Log($"바닥 감지: {hit.distance}m");
        }
    }
}
```

**Update vs FixedUpdate:**
```
Update:
- 프레임마다 (60fps라면 1초에 60번)
- 불규칙한 시간 간격
- 입력 처리, 카메라, UI

FixedUpdate:
- 고정 간격 (기본 0.02초 = 1초에 50번)
- 규칙적인 시간 간격
- 물리 연산 (Rigidbody, Collider)
```

### 5. LateUpdate()

**모든 Update()가 끝난 후** 호출됩니다. 주로 카메라에 사용합니다.

```csharp
public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public Vector3 offset = new Vector3(0, 5, -10);

    void LateUpdate()
    {
        // ✅ LateUpdate에서 할 것:
        // 1. 카메라 추적
        // target이 Update에서 이동한 후, 카메라가 따라감
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target);
        }
    }
}
```

**왜 LateUpdate를 쓰나?**
```
Frame 1:
  Update() - Player 이동 (10, 0, 0) → (11, 0, 0)
  Update() - Camera가 Player 따라감 (10, 5, -10) → (11, 5, -10)
  ❌ 문제: Camera가 Player의 이전 위치를 따라감 (떨림)

Frame 1:
  Update() - Player 이동 (10, 0, 0) → (11, 0, 0)
  LateUpdate() - Camera가 Player 따라감 (11, 5, -10)
  ✅ 해결: Camera가 Player의 현재 위치를 따라감 (부드러움)
```

### 6. OnEnable / OnDisable

GameObject가 **활성화/비활성화**될 때 호출됩니다.

```csharp
public class EnemySpawner : MonoBehaviour
{
    void OnEnable()
    {
        // 활성화될 때
        Debug.Log("스포너 활성화!");
        StartCoroutine(SpawnRoutine());
    }

    void OnDisable()
    {
        // 비활성화될 때
        Debug.Log("스포너 비활성화!");
        StopAllCoroutines();
    }
}

// 사용
spawner.SetActive(false); // OnDisable 호출
spawner.SetActive(true);  // OnEnable 호출
```

### 7. OnDestroy()

GameObject가 **파괴될 때** 호출됩니다. 정리 작업에 사용합니다.

```csharp
public class Unit : MonoBehaviour
{
    void OnDestroy()
    {
        // ✅ OnDestroy에서 할 것:
        // 1. 이벤트 구독 해제
        GameManager.OnGameOver -= HandleGameOver;

        // 2. 리소스 정리
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 3. 로그
        Debug.Log($"{gameObject.name} 파괴됨");
    }
}
```

### 스타크래프트 예시: 마린 생명주기

```csharp
using UnityEngine;

public class Marine : MonoBehaviour
{
    private int hp = 40;
    private Rigidbody rb;
    private Renderer meshRenderer;

    void Awake()
    {
        Debug.Log("[Awake] 마린 생성 - 컴포넌트 초기화");
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        Debug.Log("[OnEnable] 마린 활성화 - 이벤트 구독");
        GameManager.OnGameOver += HandleGameOver;
    }

    void Start()
    {
        Debug.Log("[Start] 마린 배치 완료 - 초기 명령 수행");
        transform.position = new Vector3(10, 0, 10);
    }

    void Update()
    {
        Debug.Log($"[Update] 프레임 업데이트 - HP: {hp}");

        // 입력 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        Debug.Log("[FixedUpdate] 물리 업데이트");
        // 물리 기반 이동
    }

    void LateUpdate()
    {
        Debug.Log("[LateUpdate] 후처리");
        // 카메라 추적 등
    }

    void OnDisable()
    {
        Debug.Log("[OnDisable] 마린 비활성화 - 이벤트 구독 해제");
        GameManager.OnGameOver -= HandleGameOver;
    }

    void OnDestroy()
    {
        Debug.Log("[OnDestroy] 마린 파괴 - 최종 정리");
        // 리소스 정리
    }

    void Attack()
    {
        Debug.Log("마린 공격!");
    }

    void HandleGameOver()
    {
        Debug.Log("게임 종료 - 마린 정지");
        enabled = false; // 스크립트 비활성화
    }
}
```

---

## GetComponent - 컴포넌트 통신

GameObject의 **다른 Component에 접근**하는 방법입니다.

### 기본 사용법

```csharp
public class ComponentAccess : MonoBehaviour
{
    void Start()
    {
        // 1. 자기 자신의 컴포넌트
        Rigidbody rb = GetComponent<Rigidbody>();

        // 2. 자식의 컴포넌트
        Renderer childRenderer = GetComponentInChildren<Renderer>();

        // 3. 부모의 컴포넌트
        Transform parentTransform = GetComponentInParent<Transform>();

        // 4. 모든 컴포넌트 (배열)
        Collider[] colliders = GetComponents<Collider>();
    }
}
```

### 성능 최적화: 캐싱

```csharp
// ❌ 나쁜 예 - 매번 검색
public class BadExample : MonoBehaviour
{
    void Update()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up); // 매 프레임 검색!
    }
}

// ✅ 좋은 예 - 한 번만 검색
public class GoodExample : MonoBehaviour
{
    private Rigidbody rb; // 캐싱

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // 한 번만
    }

    void Update()
    {
        rb.AddForce(Vector3.up); // 빠름!
    }
}
```

### SerializeField로 Inspector 할당

```csharp
public class UnitController : MonoBehaviour
{
    // 방법 1: GetComponent (코드로 찾기)
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 방법 2: SerializeField (Inspector에서 할당)
    [SerializeField] private Rigidbody rbManual;

    // 방법 3: public (Inspector에서 할당, 외부 접근 가능)
    public Animator animator;
}
```

**언제 뭘 쓸까?**
```csharp
// 같은 GameObject의 컴포넌트 → GetComponent
private Rigidbody rb;
void Awake() { rb = GetComponent<Rigidbody>(); }

// 다른 GameObject의 컴포넌트 → SerializeField
[SerializeField] private GameObject target;

// 자주 바뀌는 참조 → public
public Transform waypoint;
```

### 다른 GameObject의 Component 접근

```csharp
public class TargetFinder : MonoBehaviour
{
    void Start()
    {
        // 1. Tag로 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
        }

        // 2. 이름으로 찾기 (느림, 비추천)
        GameObject enemy = GameObject.Find("Enemy");

        // 3. 타입으로 찾기 (느림, 비추천)
        PlayerController controller = FindObjectOfType<PlayerController>();

        // 4. SerializeField (추천!)
        [SerializeField] private GameObject targetObject;
        if (targetObject != null)
        {
            Health health = targetObject.GetComponent<Health>();
        }
    }
}
```

### 스타크래프트 예시: 유닛 컴포넌트 구조

```csharp
using UnityEngine;

// HP 관리 컴포넌트
public class UnitHealth : MonoBehaviour
{
    public int maxHP = 40;
    private int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 파괴됨!");
        Destroy(gameObject);
    }
}

// 이동 컴포넌트
public class UnitMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }
}

// 공격 컴포넌트
public class UnitWeapon : MonoBehaviour
{
    public int attackDamage = 6;
    public float attackRange = 10f;
    public float attackCooldown = 1f;

    private float lastAttackTime = 0f;

    public void Attack(GameObject target)
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("재장전 중...");
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > attackRange)
        {
            Debug.Log("사거리 밖!");
            return;
        }

        // 타겟의 Health 컴포넌트에 접근
        UnitHealth targetHealth = target.GetComponent<UnitHealth>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name}이(가) {target.name}을(를) 공격!");
            lastAttackTime = Time.time;
        }
    }
}

// 통합 컨트롤러
public class MarineController : MonoBehaviour
{
    // 컴포넌트 참조 (캐싱)
    private UnitHealth health;
    private UnitMovement movement;
    private UnitWeapon weapon;

    void Awake()
    {
        // 같은 GameObject의 모든 컴포넌트 가져오기
        health = GetComponent<UnitHealth>();
        movement = GetComponent<UnitMovement>();
        weapon = GetComponent<UnitWeapon>();
    }

    void Update()
    {
        // 우클릭: 이동
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    movement.MoveTo(hit.point);
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    weapon.Attack(hit.collider.gameObject);
                }
            }
        }
    }
}
```

---

## Input 시스템

플레이어의 **키보드, 마우스 입력**을 받는 방법입니다.

### 키보드 입력

```csharp
public class KeyboardInput : MonoBehaviour
{
    void Update()
    {
        // 1. GetKey - 키를 누르고 있는 동안 (매 프레임 true)
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime);
        }

        // 2. GetKeyDown - 키를 누르는 순간 (1회만 true)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // 3. GetKeyUp - 키를 떼는 순간 (1회만 true)
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StopRunning();
        }
    }

    void Jump() { Debug.Log("점프!"); }
    void StopRunning() { Debug.Log("달리기 중단"); }
}
```

### 마우스 입력

```csharp
public class MouseInput : MonoBehaviour
{
    void Update()
    {
        // 1. 마우스 버튼
        if (Input.GetMouseButtonDown(0))  // 좌클릭
        {
            Debug.Log("좌클릭!");
        }

        if (Input.GetMouseButtonDown(1))  // 우클릭
        {
            Debug.Log("우클릭!");
        }

        if (Input.GetMouseButtonDown(2))  // 휠 클릭
        {
            Debug.Log("휠 클릭!");
        }

        // 2. 마우스 위치 (스크린 좌표)
        Vector3 mousePos = Input.mousePosition;
        Debug.Log($"마우스 위치: {mousePos}");

        // 3. 마우스 휠
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            Debug.Log("휠 위로");
        }
        else if (scroll < 0f)
        {
            Debug.Log("휠 아래로");
        }
    }
}
```

### Input Axis (축 입력)

```csharp
public class AxisInput : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Horizontal: A/D 또는 좌/우 화살표
        float horizontal = Input.GetAxis("Horizontal"); // -1.0 ~ 1.0

        // Vertical: W/S 또는 상/하 화살표
        float vertical = Input.GetAxis("Vertical"); // -1.0 ~ 1.0

        // 이동
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        Debug.Log($"Horizontal: {horizontal}, Vertical: {vertical}");
    }
}
```

**GetAxis vs GetAxisRaw:**
```csharp
// GetAxis - 부드러운 가속/감속
float smooth = Input.GetAxis("Horizontal"); // 0 → 0.3 → 0.7 → 1.0

// GetAxisRaw - 즉시 반응 (0, -1, 1만)
float raw = Input.GetAxisRaw("Horizontal"); // 0 → 1.0 (즉시)
```

### 마우스로 월드 클릭 (Raycast)

```csharp
public class WorldClick : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 마우스 위치에서 Ray 생성
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 2. Raycast로 충돌 체크
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                Debug.Log($"클릭한 오브젝트: {hit.collider.name}");
                Debug.Log($"클릭한 위치: {hit.point}");

                // 3. 클릭한 위치로 이동
                transform.position = hit.point;
            }
        }
    }

    // Scene View에서 Ray 시각화
    void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * 100f);
    }
}
```

### 스타크래프트 예시: RTS 조작

```csharp
using UnityEngine;
using System.Collections.Generic;

public class RTSController : MonoBehaviour
{
    private List<GameObject> selectedUnits = new List<GameObject>();
    public LayerMask groundLayer;
    public LayerMask unitLayer;

    void Update()
    {
        HandleUnitSelection();
        HandleUnitCommands();
    }

    // 유닛 선택 (좌클릭)
    void HandleUnitSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, unitLayer))
            {
                GameObject unit = hit.collider.gameObject;

                // Shift 누르면 추가 선택
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }

                SelectUnit(unit);
            }
            else
            {
                // 빈 곳 클릭하면 선택 해제
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }
            }
        }
    }

    // 유닛 명령 (우클릭)
    void HandleUnitCommands()
    {
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // 적 유닛 클릭: 공격
                if (hit.collider.CompareTag("Enemy"))
                {
                    AttackTarget(hit.collider.gameObject);
                }
                // 지면 클릭: 이동
                else if (hit.collider.CompareTag("Ground"))
                {
                    MoveToPosition(hit.point);
                }
            }
        }
    }

    void SelectUnit(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);

            // 선택 표시 (링 등)
            unit.GetComponent<Renderer>().material.color = Color.green;

            Debug.Log($"{unit.name} 선택됨 (총 {selectedUnits.Count}개)");
        }
    }

    void DeselectAll()
    {
        foreach (GameObject unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        selectedUnits.Clear();
        Debug.Log("모든 유닛 선택 해제");
    }

    void MoveToPosition(Vector3 position)
    {
        Debug.Log($"선택된 {selectedUnits.Count}개 유닛 이동 명령: {position}");

        foreach (GameObject unit in selectedUnits)
        {
            if (unit != null)
            {
                UnitMovement movement = unit.GetComponent<UnitMovement>();
                if (movement != null)
                {
                    movement.MoveTo(position);
                }
            }
        }
    }

    void AttackTarget(GameObject target)
    {
        Debug.Log($"선택된 {selectedUnits.Count}개 유닛 공격 명령: {target.name}");

        foreach (GameObject unit in selectedUnits)
        {
            if (unit != null)
            {
                UnitWeapon weapon = unit.GetComponent<UnitWeapon>();
                if (weapon != null)
                {
                    weapon.Attack(target);
                }
            }
        }
    }
}
```

---

## Instantiate와 Destroy

GameObject를 **생성하고 파괴**하는 방법입니다.

### Instantiate (생성)

```csharp
public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefab; // Inspector에서 할당

    void Start()
    {
        // 1. 기본 생성 (위치: 0,0,0, 회전: 없음)
        GameObject obj1 = Instantiate(prefab);

        // 2. 위치와 회전 지정
        Vector3 position = new Vector3(10, 0, 5);
        Quaternion rotation = Quaternion.identity; // 회전 없음
        GameObject obj2 = Instantiate(prefab, position, rotation);

        // 3. 부모 설정
        GameObject obj3 = Instantiate(prefab, transform); // this의 자식으로

        // 4. 생성 후 수정
        GameObject obj4 = Instantiate(prefab);
        obj4.name = "Spawned_Marine"; // 이름 변경
        obj4.transform.position = new Vector3(5, 0, 0);
    }
}
```

### Destroy (파괴)

```csharp
public class ObjectDestroyer : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            // 1. 즉시 파괴
            Destroy(gameObject);

            // 2. 지연 파괴 (3초 후)
            Destroy(gameObject, 3f);

            // 3. 컴포넌트만 파괴
            Destroy(GetComponent<Rigidbody>());

            // 4. 자식들 파괴
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void OnDestroy()
    {
        Debug.Log("파괴되기 직전!");
    }
}
```

### 스타크래프트 예시: 유닛 생산

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barracks : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject marinePrefab;
    public GameObject firebatPrefab;
    public GameObject medicPrefab;

    [Header("Spawn Settings")]
    public Transform spawnPoint; // 유닛 생성 위치
    public Transform rallyPoint;  // 집결 지점

    private Queue<GameObject> productionQueue = new Queue<GameObject>();
    private bool isProducing = false;

    // 리소스
    private int minerals = 500;
    private int gas = 200;

    void Update()
    {
        // 키보드로 생산
        if (Input.GetKeyDown(KeyCode.M))
        {
            TryProduceUnit(marinePrefab, 50, 0, "마린");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            TryProduceUnit(firebatPrefab, 50, 25, "파이어뱃");
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            TryProduceUnit(medicPrefab, 50, 25, "메딕");
        }

        // 생산 처리
        if (!isProducing && productionQueue.Count > 0)
        {
            StartCoroutine(ProduceUnit());
        }
    }

    void TryProduceUnit(GameObject prefab, int mineralCost, int gasCost, string unitName)
    {
        // 자원 체크
        if (minerals >= mineralCost && gas >= gasCost)
        {
            minerals -= mineralCost;
            gas -= gasCost;

            productionQueue.Enqueue(prefab);
            Debug.Log($"{unitName} 생산 대기열에 추가 (대기: {productionQueue.Count})");
            Debug.Log($"남은 자원 - 미네랄: {minerals}, 가스: {gas}");
        }
        else
        {
            Debug.Log($"자원 부족! (필요: {mineralCost} 미네랄, {gasCost} 가스)");
        }
    }

    IEnumerator ProduceUnit()
    {
        isProducing = true;

        GameObject prefab = productionQueue.Dequeue();
        Debug.Log($"{prefab.name} 생산 중...");

        // 생산 시간 대기
        yield return new WaitForSeconds(3f);

        // 유닛 생성
        GameObject unit = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        unit.name = $"{prefab.name}_{Time.time}";

        Debug.Log($"{unit.name} 생산 완료!");

        // 집결 지점으로 이동
        if (rallyPoint != null)
        {
            UnitMovement movement = unit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.MoveTo(rallyPoint.position);
            }
        }

        isProducing = false;
    }

    // Gizmos로 시각화
    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
        }

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

## Coroutine - 코루틴

**시간에 걸쳐 실행**되는 함수입니다. 대기, 반복 작업에 사용합니다.

### 기본 문법

```csharp
public class CoroutineExample : MonoBehaviour
{
    void Start()
    {
        // 코루틴 시작
        StartCoroutine(MyCoroutine());

        // 문자열로도 시작 가능 (정지할 때 편함)
        StartCoroutine("MyCoroutine");
    }

    IEnumerator MyCoroutine()
    {
        Debug.Log("코루틴 시작");

        // 1초 대기
        yield return new WaitForSeconds(1f);

        Debug.Log("1초 경과");

        // 다음 프레임까지 대기
        yield return null;

        Debug.Log("코루틴 끝");
    }

    void StopAllRoutines()
    {
        // 모든 코루틴 정지
        StopAllCoroutines();

        // 특정 코루틴 정지
        StopCoroutine("MyCoroutine");
    }
}
```

### yield return 종류

```csharp
IEnumerator YieldExamples()
{
    // 1. 다음 프레임까지 대기
    yield return null;

    // 2. 지정 시간 대기 (초)
    yield return new WaitForSeconds(2f);

    // 3. 실제 시간 대기 (Time.timeScale 무시)
    yield return new WaitForSecondsRealtime(2f);

    // 4. 다음 FixedUpdate까지 대기
    yield return new WaitForFixedUpdate();

    // 5. 프레임 끝까지 대기
    yield return new WaitForEndOfFrame();

    // 6. 조건이 true될 때까지 대기
    yield return new WaitUntil(() => playerHP <= 0);

    // 7. 조건이 false될 때까지 대기
    yield return new WaitWhile(() => isLoading);

    // 8. 다른 코루틴이 끝날 때까지 대기
    yield return StartCoroutine(OtherCoroutine());
}

IEnumerator OtherCoroutine()
{
    yield return new WaitForSeconds(1f);
}
```

### 코루틴 vs 일반 함수

```csharp
// ❌ 일반 함수 - 멈출 수 없음
void NormalFunction()
{
    Debug.Log("시작");
    // Thread.Sleep(3000); // 게임 멈춤!
    Debug.Log("3초 후"); // 위 줄 때문에 실행 안 됨
}

// ✅ 코루틴 - 멈출 수 있음
IEnumerator CoroutineFunction()
{
    Debug.Log("시작");
    yield return new WaitForSeconds(3f); // 게임 안 멈춤!
    Debug.Log("3초 후"); // 3초 후에 실행됨
}
```

### 스타크래프트 예시: 스팀팩

```csharp
using UnityEngine;
using System.Collections;

public class MarineStimpack : MonoBehaviour
{
    [Header("Stimpack Settings")]
    public int hpCost = 10;          // HP 소모
    public float speedBoost = 2f;    // 속도 증가 배수
    public float duration = 10f;     // 지속 시간

    private UnitHealth health;
    private UnitMovement movement;
    private bool isStimActive = false;
    private Coroutine stimCoroutine;

    void Awake()
    {
        health = GetComponent<UnitHealth>();
        movement = GetComponent<UnitMovement>();
    }

    void Update()
    {
        // T키로 스팀팩 사용
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseStimpack();
        }
    }

    void UseStimpack()
    {
        if (isStimActive)
        {
            Debug.Log("이미 스팀팩이 활성화되어 있습니다!");
            return;
        }

        if (health.currentHP <= hpCost)
        {
            Debug.Log("HP가 부족합니다!");
            return;
        }

        // HP 소모
        health.TakeDamage(hpCost);
        Debug.Log($"스팀팩 사용! (HP -{hpCost})");

        // 코루틴 시작
        stimCoroutine = StartCoroutine(StimpackEffect());
    }

    IEnumerator StimpackEffect()
    {
        isStimActive = true;

        // 속도 증가
        float originalSpeed = movement.moveSpeed;
        movement.moveSpeed *= speedBoost;

        Debug.Log($"이동 속도 증가! {originalSpeed} → {movement.moveSpeed}");

        // 시각 효과 (빨간색으로 변경)
        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        renderer.material.color = Color.red;

        // 지속 시간 대기
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 남은 시간 표시 (매 초마다)
            if (elapsed % 1f < 0.1f)
            {
                Debug.Log($"스팀팩 남은 시간: {duration - elapsed:F1}초");
            }

            yield return null;
        }

        // 효과 종료
        movement.moveSpeed = originalSpeed;
        renderer.material.color = originalColor;
        isStimActive = false;

        Debug.Log("스팀팩 효과 종료!");
    }

    // 캔슬 기능
    public void CancelStimpack()
    {
        if (stimCoroutine != null)
        {
            StopCoroutine(stimCoroutine);
            isStimActive = false;
            Debug.Log("스팀팩 강제 종료!");
        }
    }
}
```

### 코루틴 활용: 반복 작업

```csharp
using UnityEngine;
using System.Collections;

public class AutoAttack : MonoBehaviour
{
    public GameObject target;
    public float attackInterval = 1f; // 공격 간격
    public int damage = 10;

    private bool isAttacking = false;

    void Start()
    {
        // 자동 공격 시작
        StartCoroutine(AutoAttackRoutine());
    }

    IEnumerator AutoAttackRoutine()
    {
        isAttacking = true;

        while (isAttacking)
        {
            if (target != null)
            {
                // 공격
                UnitHealth targetHealth = target.GetComponent<UnitHealth>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);
                    Debug.Log($"{gameObject.name}이(가) {target.name}을(를) 공격!");
                }
            }
            else
            {
                Debug.Log("타겟 없음!");
            }

            // 다음 공격까지 대기
            yield return new WaitForSeconds(attackInterval);
        }
    }

    public void StopAttacking()
    {
        isAttacking = false;
        StopAllCoroutines();
    }

    void OnDestroy()
    {
        StopAttacking();
    }
}
```

---

## Time.deltaTime의 중요성

**프레임 독립적인 이동**을 위해 필수적입니다.

### 문제 상황

```csharp
// ❌ 나쁜 예 - 프레임 의존적
void Update()
{
    transform.position += Vector3.forward * 5f;
    // 60fps: 1초에 60번 = 300 이동
    // 30fps: 1초에 30번 = 150 이동
    // → 컴퓨터 성능에 따라 속도가 달라짐!
}

// ✅ 좋은 예 - 프레임 독립적
void Update()
{
    transform.position += Vector3.forward * 5f * Time.deltaTime;
    // 60fps: 5 * (1/60) * 60번 = 5m/s
    // 30fps: 5 * (1/30) * 30번 = 5m/s
    // → 항상 초당 5m 이동!
}
```

### Time.deltaTime이란?

```csharp
void Update()
{
    // 이전 프레임에서 현재 프레임까지 걸린 시간 (초)
    float dt = Time.deltaTime;

    // 60fps: dt ≈ 0.0167초 (1/60)
    // 30fps: dt ≈ 0.0333초 (1/30)

    Debug.Log($"Delta Time: {dt}");
    Debug.Log($"FPS: {1f / dt}");
}
```

### 실전 예시

```csharp
public class ProperMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 180f;
    public float jumpForce = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ✅ 이동 (Time.deltaTime 사용)
        float horizontal = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontal * moveSpeed * Time.deltaTime);

        // ✅ 회전 (Time.deltaTime 사용)
        float rotation = Input.GetAxis("Vertical");
        transform.Rotate(Vector3.up, rotation * rotateSpeed * Time.deltaTime);

        // ✅ 타이머 (Time.deltaTime 사용)
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = 3f; // 3초마다
            FireWeapon();
        }
    }

    void FixedUpdate()
    {
        // ❌ FixedUpdate에서는 Time.fixedDeltaTime 사용
        // (하지만 보통 그냥 Time.deltaTime 써도 됨, 자동으로 fixedDeltaTime 값을 가짐)
        rb.velocity = new Vector3(5f, rb.velocity.y, 0);
    }

    private float cooldownTimer = 0f;
    void FireWeapon() { Debug.Log("발사!"); }
}
```

### Time 클래스의 다른 속성들

```csharp
void Update()
{
    // 게임 시작 후 경과 시간
    float elapsed = Time.time;

    // 타임 스케일 (게임 속도)
    Time.timeScale = 0.5f; // 50% 속도 (슬로우 모션)
    Time.timeScale = 0f;   // 정지 (일시정지)
    Time.timeScale = 2f;   // 200% 속도 (빠르게)

    // 실제 시간 (timeScale 무시)
    float realTime = Time.unscaledTime;
    float realDelta = Time.unscaledDeltaTime;

    // FixedUpdate 간격
    float fixedDelta = Time.fixedDeltaTime; // 기본 0.02초
}
```

---

## ScriptableObject

**데이터를 저장하는 에셋**입니다. Prefab처럼 재사용 가능합니다.

### ScriptableObject 생성

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "StarCraft/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName = "마린";
    public int maxHP = 40;
    public int attackDamage = 6;
    public float moveSpeed = 5f;
    public int mineralCost = 50;
    public int gasCost = 0;
    public int supplyCost = 1;

    public Sprite icon;
    public GameObject prefab;
}
```

**생성 방법:**
1. `Assets > Create > StarCraft > Unit Data`
2. Inspector에서 값 설정
3. 스크립트에서 참조

### ScriptableObject 사용

```csharp
public class UnitSpawner : MonoBehaviour
{
    public UnitData unitData; // Inspector에서 할당

    void Start()
    {
        Debug.Log($"유닛: {unitData.unitName}");
        Debug.Log($"HP: {unitData.maxHP}");
        Debug.Log($"비용: {unitData.mineralCost} 미네랄");

        // Prefab 생성
        GameObject unit = Instantiate(unitData.prefab);

        // 데이터 적용
        UnitHealth health = unit.GetComponent<UnitHealth>();
        health.maxHP = unitData.maxHP;
    }
}
```

### ScriptableObject의 장점

```
1. **재사용성**: 여러 Prefab이 같은 데이터 공유
2. **수정 편의성**: 데이터 변경 시 모든 유닛 자동 업데이트
3. **메모리 효율**: 데이터를 한 번만 로드
4. **디자이너 친화적**: 코드 수정 없이 밸런스 조정
```

### 스타크래프트 예시: 유닛 데이터베이스

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "StarCraft/Unit Data")]
public class StarCraftUnitData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitName = "마린";
    public string description = "테란의 기본 보병 유닛";
    public Sprite portrait;

    [Header("스탯")]
    public int maxHP = 40;
    public int maxShield = 0;
    public int attackDamage = 6;
    public float attackRange = 4f;
    public float attackSpeed = 0.86f;
    public float moveSpeed = 3.75f;
    public int armor = 0;

    [Header("비용")]
    public int mineralCost = 50;
    public int gasCost = 0;
    public int supplyCost = 1;
    public float buildTime = 24f;

    [Header("Prefab")]
    public GameObject prefab;
}

// Marine.asset
// Firebat.asset
// Medic.asset
```

```csharp
using UnityEngine;

public class DataDrivenUnit : MonoBehaviour
{
    public StarCraftUnitData data; // Inspector에서 할당

    private int currentHP;
    private int currentShield;

    void Awake()
    {
        // 데이터로부터 초기화
        currentHP = data.maxHP;
        currentShield = data.maxShield;

        gameObject.name = data.unitName;

        // 이동 속도 적용
        UnitMovement movement = GetComponent<UnitMovement>();
        if (movement != null)
        {
            movement.moveSpeed = data.moveSpeed;
        }

        Debug.Log($"{data.unitName} 생성 완료!");
        Debug.Log($"HP: {data.maxHP}, 공격력: {data.attackDamage}");
    }

    public void ShowUnitInfo()
    {
        Debug.Log($"=== {data.unitName} ===");
        Debug.Log($"{data.description}");
        Debug.Log($"HP: {currentHP}/{data.maxHP}");
        Debug.Log($"공격력: {data.attackDamage}");
        Debug.Log($"사거리: {data.attackRange}");
        Debug.Log($"이동 속도: {data.moveSpeed}");
        Debug.Log($"비용: {data.mineralCost} 미네랄, {data.gasCost} 가스");
    }
}
```

---

## 실전 프로젝트: 스타크래프트 유닛 제어 시스템

모든 내용을 종합한 완전한 프로젝트입니다.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 스타크래프트 스타일 유닛 제어 시스템
/// - 생명주기, Input, GetComponent, Coroutine 모두 활용
/// </summary>
public class CompleteUnitSystem : MonoBehaviour
{
    [Header("Unit Data")]
    public StarCraftUnitData unitData;

    [Header("Components")]
    private Rigidbody rb;
    private Renderer meshRenderer;
    private Color originalColor;

    [Header("Stats")]
    private int currentHP;
    private bool isAlive = true;

    [Header("Movement")]
    private Vector3 targetPosition;
    private bool isMoving = false;

    [Header("Combat")]
    private GameObject attackTarget;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Effects")]
    private bool stimpackActive = false;
    private Coroutine stimpackCoroutine;

    // ==================== 생명주기 ====================

    void Awake()
    {
        Debug.Log("[Awake] 유닛 초기화 시작");

        // 컴포넌트 캐싱
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<Renderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }

        // 데이터 초기화
        if (unitData != null)
        {
            currentHP = unitData.maxHP;
            gameObject.name = unitData.unitName;
        }
        else
        {
            Debug.LogError("UnitData가 할당되지 않았습니다!");
        }
    }

    void Start()
    {
        Debug.Log($"[Start] {unitData.unitName} 배치 완료");

        // 초기 위치 설정 등
        transform.position = new Vector3(
            Random.Range(-10f, 10f),
            0.5f,
            Random.Range(-10f, 10f)
        );
    }

    void Update()
    {
        if (!isAlive) return;

        HandleInput();
        HandleMovement();
        HandleCombat();
    }

    void OnDestroy()
    {
        Debug.Log($"[OnDestroy] {unitData.unitName} 파괴됨");

        // 코루틴 정리
        if (stimpackCoroutine != null)
        {
            StopCoroutine(stimpackCoroutine);
        }
    }

    // ==================== 입력 처리 ====================

    void HandleInput()
    {
        // 우클릭: 이동 또는 공격
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    // 적 클릭: 공격
                    AttackTarget(hit.collider.gameObject);
                }
                else
                {
                    // 지면 클릭: 이동
                    MoveToPosition(hit.point);
                }
            }
        }

        // T키: 스팀팩
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseStimpack();
        }

        // S키: 정지
        if (Input.GetKeyDown(KeyCode.S))
        {
            Stop();
        }

        // I키: 정보 표시
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowInfo();
        }
    }

    // ==================== 이동 ====================

    void HandleMovement()
    {
        if (!isMoving) return;

        // 목표 지점으로 이동
        Vector3 direction = (targetPosition - transform.position).normalized;
        float speed = unitData.moveSpeed;

        if (stimpackActive)
        {
            speed *= 2f; // 스팀팩 시 2배 속도
        }

        transform.position += direction * speed * Time.deltaTime;

        // 회전
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 10f
            );
        }

        // 도착 체크
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            isMoving = false;
            Debug.Log($"{unitData.unitName} 목표 지점 도착");
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
        isAttacking = false;
        attackTarget = null;

        Debug.Log($"{unitData.unitName} 이동 명령: {position}");
    }

    public void Stop()
    {
        isMoving = false;
        isAttacking = false;
        attackTarget = null;

        Debug.Log($"{unitData.unitName} 정지");
    }

    // ==================== 전투 ====================

    void HandleCombat()
    {
        if (!isAttacking || attackTarget == null) return;

        // 타겟까지의 거리
        float distance = Vector3.Distance(transform.position, attackTarget.transform.position);

        if (distance > unitData.attackRange)
        {
            // 사거리 밖: 접근
            MoveToPosition(attackTarget.transform.position);
        }
        else
        {
            // 사거리 안: 공격
            isMoving = false;

            // 타겟 바라보기
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // 공격 쿨다운 체크
            if (Time.time >= lastAttackTime + (1f / unitData.attackSpeed))
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    public void AttackTarget(GameObject target)
    {
        attackTarget = target;
        isAttacking = true;

        Debug.Log($"{unitData.unitName} 공격 명령: {target.name}");
    }

    void PerformAttack()
    {
        if (attackTarget == null) return;

        Debug.Log($"{unitData.unitName}이(가) {attackTarget.name}을(를) 공격!");

        // 타겟에게 데미지
        CompleteUnitSystem targetUnit = attackTarget.GetComponent<CompleteUnitSystem>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(unitData.attackDamage);
        }

        // 공격 이펙트 (코루틴)
        StartCoroutine(AttackFlash());
    }

    IEnumerator AttackFlash()
    {
        meshRenderer.material.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = stimpackActive ? Color.red : originalColor;
    }

    // ==================== HP 시스템 ====================

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHP -= damage;
        Debug.Log($"{unitData.unitName} 피해: -{damage} (HP: {currentHP}/{unitData.maxHP})");

        // 피격 이펙트
        StartCoroutine(DamageFlash());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        meshRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        meshRenderer.material.color = stimpackActive ? Color.red : originalColor;
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{unitData.unitName} 파괴됨!");

        // 죽는 애니메이션 (코루틴)
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // 1. 회전하며 가라앉기
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * 2f;

            transform.position = Vector3.Lerp(startPos, startPos + Vector3.down * 2f, elapsed);
            transform.Rotate(Vector3.up, 360f * Time.deltaTime);

            yield return null;
        }

        // 2. 파괴
        Destroy(gameObject);
    }

    // ==================== 스팀팩 ====================

    void UseStimpack()
    {
        if (stimpackActive)
        {
            Debug.Log("이미 스팀팩 활성화 중!");
            return;
        }

        if (currentHP <= 10)
        {
            Debug.Log("HP가 부족합니다!");
            return;
        }

        // HP 소모
        TakeDamage(10);

        // 스팀팩 효과 시작
        stimpackCoroutine = StartCoroutine(StimpackEffect());
    }

    IEnumerator StimpackEffect()
    {
        stimpackActive = true;
        meshRenderer.material.color = Color.red;

        Debug.Log($"{unitData.unitName} 스팀팩 사용! (지속: 10초)");

        yield return new WaitForSeconds(10f);

        stimpackActive = false;
        meshRenderer.material.color = originalColor;

        Debug.Log($"{unitData.unitName} 스팀팩 종료");
    }

    // ==================== 정보 ====================

    void ShowInfo()
    {
        Debug.Log($"\n========== {unitData.unitName} ==========");
        Debug.Log($"HP: {currentHP}/{unitData.maxHP}");
        Debug.Log($"공격력: {unitData.attackDamage}");
        Debug.Log($"사거리: {unitData.attackRange}");
        Debug.Log($"공격 속도: {unitData.attackSpeed}");
        Debug.Log($"이동 속도: {unitData.moveSpeed}");
        Debug.Log($"스팀팩: {(stimpackActive ? "활성화" : "비활성화")}");
        Debug.Log($"상태: {(isMoving ? "이동 중" : isAttacking ? "공격 중" : "대기")}");
        Debug.Log("===============================\n");
    }

    // ==================== Gizmos ====================

    void OnDrawGizmosSelected()
    {
        if (unitData == null) return;

        // 사거리 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, unitData.attackRange);

        // 이동 목표 표시
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
        }

        // 공격 타겟 표시
        if (attackTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, attackTarget.transform.position);
        }
    }
}
```

---

## 정리 및 다음 단계

### 이번 강의에서 배운 것

✅ **MonoBehaviour** - 유니티 스크립트의 기반
✅ **생명주기 함수** - Awake, Start, Update, FixedUpdate, LateUpdate
✅ **GetComponent** - 컴포넌트 통신과 캐싱
✅ **Input 시스템** - 키보드, 마우스, Raycast
✅ **Instantiate/Destroy** - 오브젝트 생성과 파괴
✅ **Coroutine** - 시간 기반 작업
✅ **Time.deltaTime** - 프레임 독립적 이동
✅ **ScriptableObject** - 데이터 관리

### 핵심 규칙 정리

```csharp
// 1. 컴포넌트는 Awake에서 캐싱
void Awake() { rb = GetComponent<Rigidbody>(); }

// 2. 이동은 Time.deltaTime 필수
transform.position += direction * speed * Time.deltaTime;

// 3. 물리는 FixedUpdate
void FixedUpdate() { rb.AddForce(Vector3.up * 10f); }

// 4. 시간 작업은 Coroutine
IEnumerator Wait() { yield return new WaitForSeconds(3f); }

// 5. MonoBehaviour는 new 금지
unit.AddComponent<UnitController>(); // ✅
new UnitController(); // ❌
```

### 다음 강의 예고: 7강 - 물리와 충돌

다음 강의에서는 유니티의 물리 시스템을 배웁니다:
- Rigidbody와 Collider
- Trigger vs Collision
- OnCollisionEnter, OnTriggerEnter
- Raycast 심화
- 실전: 총알 발사와 충돌 처리

다음 강의에서 만나요! 🎮
