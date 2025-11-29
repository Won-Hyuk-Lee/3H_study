# 9강: 애니메이션 시스템 (Animation System)

## 목차
1. [애니메이션 시스템 개요](#애니메이션-시스템-개요)
2. [Animation Clip](#animation-clip)
3. [Animator Controller](#animator-controller)
4. [State와 Transition](#state와-transition)
5. [Parameter와 제어](#parameter와-제어)
6. [Blend Tree](#blend-tree)
7. [스크립트에서 제어](#스크립트에서-제어)
8. [실전 프로젝트: 유닛 애니메이션](#실전-프로젝트-유닛-애니메이션)

---

## 애니메이션 시스템 개요

유니티의 **Animator 시스템**은 State Machine 기반으로 애니메이션을 제어합니다.

### 애니메이션 구성 요소

```
GameObject
├── Animator (컴포넌트)
│   └── Animator Controller (에셋)
│       ├── Animation Clip 1
│       ├── Animation Clip 2
│       └── State Machine
```

### Animation vs Animator

```csharp
// Legacy Animation (구형)
Animation anim = GetComponent<Animation>();
anim.Play("Walk");

// Animator (신형, 권장)
Animator animator = GetComponent<Animator>();
animator.SetBool("isWalking", true);
```

---

## Animation Clip

**Animation Clip**은 실제 애니메이션 데이터입니다.

### Animation Clip 생성

```
1. Window > Animation > Animation
2. GameObject 선택
3. Create 버튼 클릭
4. 이름: "Marine_Idle"
5. 키프레임 추가
```

### 스크립트에서 Animation Clip 사용

```csharp
using UnityEngine;

public class AnimationClipExample : MonoBehaviour
{
    public AnimationClip idleClip;
    public AnimationClip walkClip;

    void Start()
    {
        Debug.Log($"Idle 길이: {idleClip.length}초");
        Debug.Log($"Walk 길이: {walkClip.length}초");
    }
}
```

---

## Animator Controller

**Animator Controller**는 애니메이션 State Machine입니다.

### Animator Controller 생성

```
1. Project 우클릭 > Create > Animator Controller
2. 이름: "MarineAnimatorController"
3. GameObject의 Animator 컴포넌트에 할당
```

### Animator 컴포넌트 설정

```csharp
using UnityEngine;

public class AnimatorSetup : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();

        // Animator Controller 할당
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("MarineController");

        // Avatar (휴머노이드용)
        // animator.avatar = myAvatar;

        // 설정
        animator.updateMode = AnimatorUpdateMode.Normal; // Normal, AnimatePhysics, UnscaledTime
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms; // 최적화
    }
}
```

---

## State와 Transition

### State (상태)

```
Animator Controller에서:
1. 우클릭 > Create State > Empty
2. 이름: "Idle"
3. Motion에 Animation Clip 할당
```

### Transition (전환)

```
1. State 우클릭 > Make Transition
2. 다른 State로 드래그
3. Transition 설정:
   - Has Exit Time: 애니메이션 끝나면 자동 전환
   - Transition Duration: 전환 시간
   - Conditions: 전환 조건
```

### 스타크래프트 예시: 마린 State Machine

```
States:
- Idle (대기)
- Walk (이동)
- Attack (공격)
- Die (사망)

Transitions:
Idle → Walk: isWalking = true
Walk → Idle: isWalking = false
Any State → Attack: Attack Trigger
Any State → Die: isDead = true
```

---

## Parameter와 제어

**Parameter**는 Transition 조건으로 사용됩니다.

### Parameter 타입

```csharp
using UnityEngine;

public class AnimatorParameters : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Bool
        animator.SetBool("isWalking", true);
        bool isWalking = animator.GetBool("isWalking");

        // 2. Float
        animator.SetFloat("Speed", 5.5f);
        float speed = animator.GetFloat("Speed");

        // 3. Int
        animator.SetInteger("WeaponType", 1);
        int weaponType = animator.GetInteger("WeaponType");

        // 4. Trigger (일회성)
        animator.SetTrigger("Attack");
        animator.ResetTrigger("Attack"); // 취소
    }
}
```

### 스타크래프트 예시: 마린 애니메이션 제어

```csharp
using UnityEngine;

/// <summary>
/// 마린 애니메이션 제어
/// </summary>
public class MarineAnimator : MonoBehaviour
{
    private Animator animator;
    private bool isWalking = false;
    private bool isAttacking = false;
    private bool isDead = false;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Animator Controller 확인
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[Marine] Animator Controller가 없습니다!");
        }
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        // WASD 이동
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool shouldWalk = (horizontal != 0 || vertical != 0);

        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            animator.SetBool("isWalking", isWalking);

            if (isWalking)
            {
                Debug.Log("[Marine] Walk 애니메이션 시작");
            }
            else
            {
                Debug.Log("[Marine] Idle 애니메이션 시작");
            }
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        Debug.Log("[Marine] Attack 애니메이션 Trigger");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("isDead", true);
        Debug.Log("[Marine] Die 애니메이션 시작");
    }

    // Animation Event에서 호출
    public void OnAttackHit()
    {
        Debug.Log("[Marine] 공격 명중 타이밍!");
        // 실제 데미지 처리
    }

    public void OnDieComplete()
    {
        Debug.Log("[Marine] 사망 애니메이션 완료");
        Destroy(gameObject, 2f);
    }
}
```

---

## Blend Tree

**Blend Tree**는 여러 애니메이션을 부드럽게 섞습니다.

### Blend Tree 생성

```
1. Animator Controller에서 우클릭
2. Create State > From New Blend Tree
3. 더블클릭하여 편집
4. Motion 추가 (Idle, Walk, Run)
```

### Blend Tree 타입

```
1D Blend:
- Parameter: Speed (0~10)
- 0: Idle
- 5: Walk
- 10: Run

2D Simple Directional:
- Parameter X: Horizontal (-1~1)
- Parameter Y: Vertical (-1~1)
- 8방향 이동
```

### 스크립트에서 Blend Tree 제어

```csharp
using UnityEngine;

public class BlendTreeController : MonoBehaviour
{
    private Animator animator;
    public float moveSpeed = 5f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 1D Blend Tree (속도)
        float speed = new Vector2(horizontal, vertical).magnitude * moveSpeed;
        animator.SetFloat("Speed", speed);

        // 2D Blend Tree (방향)
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }
}
```

---

## 스크립트에서 제어

### Animator 정보 가져오기

```csharp
using UnityEngine;

public class AnimatorInfo : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 현재 State 정보
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // Layer 0

        // State 이름 확인
        if (stateInfo.IsName("Idle"))
        {
            Debug.Log("Idle 상태");
        }

        // State 해시 확인 (성능 좋음)
        int idleHash = Animator.StringToHash("Idle");
        if (stateInfo.shortNameHash == idleHash)
        {
            Debug.Log("Idle 상태 (해시)");
        }

        // 재생 시간 (0~1)
        float normalizedTime = stateInfo.normalizedTime;
        Debug.Log($"애니메이션 진행도: {normalizedTime * 100}%");

        // Transition 중인지
        bool isTransitioning = animator.IsInTransition(0);
        if (isTransitioning)
        {
            Debug.Log("전환 중...");
        }
    }

    // 특정 애니메이션 재생 (Legacy 방식, 비추천)
    void PlayAnimation()
    {
        animator.Play("Attack", 0); // Layer 0에서 Attack 재생
        animator.Play("Attack", 0, 0.5f); // 50% 지점부터 재생
    }

    // Layer 제어
    void SetLayerWeight()
    {
        // Layer 1의 가중치 설정 (0~1)
        animator.SetLayerWeight(1, 0.5f);
    }
}
```

### Animation Event

```csharp
using UnityEngine;

/// <summary>
/// Animation Event 예시
/// Animation Clip에서 특정 프레임에 이벤트 추가
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    // Animation Event에서 호출됨
    public void OnFootstep()
    {
        Debug.Log("[Animation Event] 발소리 재생");
        // 발소리 AudioSource.Play()
    }

    public void OnAttackStart()
    {
        Debug.Log("[Animation Event] 공격 시작");
    }

    public void OnAttackHit()
    {
        Debug.Log("[Animation Event] 공격 명중 타이밍");
        // 실제 데미지 처리
    }

    public void OnAttackEnd()
    {
        Debug.Log("[Animation Event] 공격 끝");
    }

    // 파라미터 받기
    public void OnEventWithParam(int value)
    {
        Debug.Log($"[Animation Event] 파라미터: {value}");
    }
}
```

---

## 실전 프로젝트: 유닛 애니메이션

완전한 유닛 애니메이션 시스템입니다.

```csharp
using UnityEngine;

/// <summary>
/// 완전한 유닛 애니메이션 시스템
/// - Idle, Walk, Attack, Die
/// - State Machine 기반
/// </summary>
public class CompleteUnitAnimator : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private Rigidbody rb;

    [Header("State")]
    private UnitState currentState = UnitState.Idle;
    private bool isDead = false;

    [Header("Movement")]
    public float moveSpeed = 3.75f;
    private Vector3 moveDirection;

    [Header("Combat")]
    public GameObject target;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    // Animation Hash (성능 최적화)
    private readonly int hashIsWalking = Animator.StringToHash("isWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashIsDead = Animator.StringToHash("isDead");
    private readonly int hashSpeed = Animator.StringToHash("Speed");

    public enum UnitState
    {
        Idle,
        Walking,
        Attacking,
        Dead
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            Debug.LogError("[Unit] Animator 컴포넌트가 없습니다!");
        }
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
        HandleCombat();
        UpdateAnimator();
    }

    void HandleInput()
    {
        // WASD 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // 이동
        if (moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            ChangeState(UnitState.Walking);
        }
        else
        {
            if (currentState == UnitState.Walking)
            {
                ChangeState(UnitState.Idle);
            }
        }

        // 수동 공격 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerAttack();
        }

        // 테스트: 죽음 (K키)
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }

    void HandleCombat()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance <= attackRange)
        {
            // 타겟 바라보기
            Vector3 direction = (target.transform.position - transform.position);
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            // 자동 공격
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                TriggerAttack();
            }
        }
    }

    void UpdateAnimator()
    {
        // Bool: isWalking
        bool isWalking = (currentState == UnitState.Walking);
        animator.SetBool(hashIsWalking, isWalking);

        // Float: Speed (Blend Tree용)
        float speed = moveDirection.magnitude * moveSpeed;
        animator.SetFloat(hashSpeed, speed);
    }

    void ChangeState(UnitState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"[Unit] State 변경: {currentState} → {newState}");
        currentState = newState;
    }

    public void TriggerAttack()
    {
        if (isDead) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        animator.SetTrigger(hashAttack);
        lastAttackTime = Time.time;

        ChangeState(UnitState.Attacking);

        Debug.Log("[Unit] Attack Trigger!");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool(hashIsDead, true);

        ChangeState(UnitState.Dead);

        Debug.Log("[Unit] 사망!");

        // Rigidbody 비활성화
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    // === Animation Event 콜백 ===

    public void OnAttackHit()
    {
        Debug.Log("[AnimEvent] 공격 명중 타이밍!");

        if (target != null)
        {
            // 타겟에게 데미지
            UnitHealth targetHealth = target.GetComponent<UnitHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(10);
            }
        }
    }

    public void OnAttackEnd()
    {
        Debug.Log("[AnimEvent] 공격 애니메이션 종료");

        if (moveDirection == Vector3.zero)
        {
            ChangeState(UnitState.Idle);
        }
    }

    public void OnDieComplete()
    {
        Debug.Log("[AnimEvent] 사망 애니메이션 완료");

        // 2초 후 파괴
        Destroy(gameObject, 2f);
    }

    public void OnFootstep()
    {
        // 발소리 재생
        Debug.Log("[AnimEvent] 발소리");
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

public class UnitHealth : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"[{gameObject.name}] HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            CompleteUnitAnimator animator = GetComponent<CompleteUnitAnimator>();
            if (animator != null)
            {
                animator.Die();
            }
        }
    }
}
```

---

## 정리

### 이번 강의 핵심

✅ **Animator Controller** - State Machine 기반 애니메이션
✅ **Parameter** - Bool, Float, Int, Trigger
✅ **Transition** - 상태 전환 조건
✅ **Blend Tree** - 부드러운 애니메이션 블렌딩
✅ **Animation Event** - 특정 프레임에 함수 호출
✅ **스크립트 제어** - SetBool, SetTrigger, GetCurrentAnimatorStateInfo

### 핵심 코드

```csharp
// Animator 제어
animator.SetBool("isWalking", true);
animator.SetTrigger("Attack");
animator.SetFloat("Speed", 5f);

// State 확인
AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
if (state.IsName("Idle")) { }

// Animation Event
public void OnAttackHit() {
    // 공격 명중 타이밍
}
```

---

## 다음 강의 예고: 10강 - 오디오 시스템

다음 강의에서는 오디오 시스템을 배웁니다! 🔊
