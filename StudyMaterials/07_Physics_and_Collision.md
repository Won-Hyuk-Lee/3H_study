# 7강: 물리와 충돌 (Physics & Collision)

## 목차
1. [물리 시스템 개요](#물리-시스템-개요)
2. [Rigidbody - 강체 물리](#rigidbody---강체-물리)
3. [Collider - 충돌체](#collider---충돌체)
4. [Collision vs Trigger](#collision-vs-trigger)
5. [충돌 이벤트 함수](#충돌-이벤트-함수)
6. [Raycast 심화](#raycast-심화)
7. [Physics Material](#physics-material)
8. [Layer Collision Matrix](#layer-collision-matrix)
9. [실전 프로젝트: 스타크래프트 전투 시스템](#실전-프로젝트-스타크래프트-전투-시스템)

---

## 물리 시스템 개요

유니티의 물리 엔진은 **PhysX**를 기반으로 하며, 중력, 충돌, 마찰 등 현실적인 물리 시뮬레이션을 제공합니다.

### 물리 시스템의 핵심 구성 요소

```
GameObject
├── Transform       (위치, 회전, 크기)
├── Rigidbody       (물리 속성: 질량, 중력, 속도)
└── Collider        (충돌 영역: Box, Sphere, Capsule)
```

**물리 시뮬레이션 흐름:**
```
1. FixedUpdate (0.02초마다)
2. Physics 계산 (충돌 감지, 힘 계산)
3. OnCollision/OnTrigger 호출
4. Transform 업데이트
```

### 물리 vs 비물리 움직임

```csharp
using UnityEngine;

public class MovementComparison : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ❌ 비물리 움직임 - Transform 직접 수정
        // 물리 계산 무시, 벽 통과 가능
        transform.position += Vector3.forward * 5f * Time.deltaTime;
    }

    void FixedUpdate()
    {
        // ✅ 물리 움직임 - Rigidbody 사용
        // 충돌 감지, 중력, 마찰 적용
        rb.MovePosition(rb.position + Vector3.forward * 5f * Time.fixedDeltaTime);

        // 또는
        rb.velocity = Vector3.forward * 5f;
    }
}
```

**언제 무엇을 사용할까?**
```
Transform 직접 수정:
- UI 요소
- 배경 오브젝트
- 물리가 필요 없는 단순 이동

Rigidbody 사용:
- 플레이어 캐릭터
- 총알, 미사일
- 중력이 필요한 모든 것
- 충돌 감지가 필요한 것
```

---

## Rigidbody - 강체 물리

**Rigidbody**는 GameObject에 물리 속성을 부여하는 컴포넌트입니다.

### Rigidbody 기본 속성

```csharp
using UnityEngine;

public class RigidbodyBasics : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // === 기본 속성 ===

        // Mass (질량) - 무거울수록 밀리기 어려움
        rb.mass = 10f; // 기본값: 1

        // Drag (저항) - 공기 저항, 클수록 빨리 멈춤
        rb.drag = 0.5f; // 기본값: 0

        // Angular Drag (회전 저항) - 회전 속도 감소
        rb.angularDrag = 0.05f; // 기본값: 0.05

        // Use Gravity (중력 사용)
        rb.useGravity = true; // 기본값: true

        // Is Kinematic (키네마틱 모드)
        // true: 물리 계산 무시, 충돌은 감지
        rb.isKinematic = false; // 기본값: false

        // Interpolate (보간) - 떨림 방지
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // None, Interpolate, Extrapolate

        // Collision Detection (충돌 감지 모드)
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        // Discrete (기본), Continuous, ContinuousDynamic, ContinuousSpeculative
    }
}
```

### Constraints (제약 조건)

```csharp
public class RigidbodyConstraints : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 위치 고정 (Freeze Position)
        rb.constraints = RigidbodyConstraints.FreezePositionY; // Y축 고정
        rb.constraints = RigidbodyConstraints.FreezePosition;  // 모든 축 고정

        // 회전 고정 (Freeze Rotation)
        rb.constraints = RigidbodyConstraints.FreezeRotationX; // X축 회전 고정
        rb.constraints = RigidbodyConstraints.FreezeRotation;  // 모든 회전 고정

        // 조합 (비트 플래그)
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotation;
    }
}
```

**실전 사용 예시:**
```csharp
// 3D 게임의 2D 플레이어 (좌우 이동만)
rb.constraints = RigidbodyConstraints.FreezePositionZ |
                 RigidbodyConstraints.FreezeRotation;

// RTS 유닛 (Y축 고정, 회전 고정)
rb.constraints = RigidbodyConstraints.FreezePositionY |
                 RigidbodyConstraints.FreezeRotation;
```

### Rigidbody 힘 적용 (AddForce)

```csharp
using UnityEngine;

public class ForceExamples : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 1. AddForce - 지속적인 힘 (가속)
            rb.AddForce(Vector3.up * 500f, ForceMode.Force);
            // ForceMode.Force: mass를 고려한 힘 (기본)

            // 2. AddForce (Impulse) - 순간적인 힘 (점프)
            rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            // ForceMode.Impulse: mass를 고려한 순간 힘

            // 3. Acceleration - mass 무시
            rb.AddForce(Vector3.up * 10f, ForceMode.Acceleration);

            // 4. VelocityChange - mass 무시, 즉시 속도 변경
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
        }
    }

    // ForceMode 비교
    void ForceModeComparison()
    {
        // 1. Force (지속적, mass 고려)
        // F = ma → a = F/m
        rb.AddForce(Vector3.forward * 100f, ForceMode.Force);

        // 2. Impulse (순간적, mass 고려)
        // 즉시 속도 변경, mass가 클수록 효과 작음
        rb.AddForce(Vector3.forward * 5f, ForceMode.Impulse);

        // 3. Acceleration (지속적, mass 무시)
        rb.AddForce(Vector3.forward * 10f, ForceMode.Acceleration);

        // 4. VelocityChange (순간적, mass 무시)
        rb.AddForce(Vector3.forward * 5f, ForceMode.VelocityChange);
    }
}
```

### Rigidbody 회전 (AddTorque)

```csharp
public class TorqueExamples : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 회전력 적용
        if (Input.GetKey(KeyCode.Q))
        {
            rb.AddTorque(Vector3.up * 10f, ForceMode.Force);
        }

        // 상대 회전력
        if (Input.GetKey(KeyCode.E))
        {
            rb.AddRelativeTorque(Vector3.up * 10f, ForceMode.Force);
        }
    }
}
```

### Velocity와 MovePosition

```csharp
public class MovementMethods : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 방법 1: Velocity 직접 설정
        Vector3 velocity = new Vector3(horizontal, rb.velocity.y, vertical) * moveSpeed;
        rb.velocity = velocity;

        // 방법 2: MovePosition (권장 - 충돌 감지 유지)
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // 방법 3: AddForce (물리적 가속)
        Vector3 force = new Vector3(horizontal, 0, vertical) * moveSpeed;
        rb.AddForce(force, ForceMode.VelocityChange);
    }
}
```

### 스타크래프트 예시: 시즈 탱크 포탄

```csharp
using UnityEngine;

/// <summary>
/// 시즈 탱크의 포탄 - Rigidbody 물리 사용
/// 중력을 받는 포물선 투사체
/// </summary>
public class SiegeTankProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 70;
    public float explosionRadius = 2f;
    public float launchForce = 30f;
    public float launchAngle = 45f;

    private Rigidbody rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Rigidbody 설정
        rb.mass = 2f;              // 무거운 포탄
        rb.drag = 0.1f;            // 공기 저항 약간
        rb.useGravity = true;      // 중력 적용
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 빠른 투사체

        Debug.Log("[SiegeTank] 포탄 발사 준비");
    }

    /// <summary>
    /// 타겟을 향해 포탄 발사
    /// </summary>
    public void Launch(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 발사각 적용
        float angleInRadians = launchAngle * Mathf.Deg2Rad;
        Vector3 launchDirection = new Vector3(
            direction.x,
            Mathf.Tan(angleInRadians),
            direction.z
        ).normalized;

        // 힘 적용 (Impulse 모드 - 순간적인 힘)
        rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);

        // 회전 효과 (포탄이 회전하며 날아감)
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        Debug.Log($"[SiegeTank] 포탄 발사! 타겟: {targetPosition}, 각도: {launchAngle}도");
        Debug.Log($"[SiegeTank] 초기 속도: {rb.velocity.magnitude} m/s");

        // 10초 후 자동 파괴 (안전장치)
        Destroy(gameObject, 10f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        hasExploded = true;

        Debug.Log($"[SiegeTank] 포탄 착탄! 충돌: {collision.gameObject.name}");
        Debug.Log($"[SiegeTank] 충돌 위치: {collision.contacts[0].point}");

        // 폭발 처리
        Explode(collision.contacts[0].point);

        // 파괴
        Destroy(gameObject);
    }

    void Explode(Vector3 explosionCenter)
    {
        Debug.Log($"[SiegeTank] 폭발! 중심: {explosionCenter}, 반경: {explosionRadius}m");

        // 폭발 범위 내 모든 Collider 찾기
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, explosionRadius);

        Debug.Log($"[SiegeTank] 폭발 범위 내 {hitColliders.Length}개 오브젝트 감지");

        foreach (Collider hitCollider in hitColliders)
        {
            // 유닛인지 확인
            UnitHealth unitHealth = hitCollider.GetComponent<UnitHealth>();
            if (unitHealth != null)
            {
                // 거리에 따른 데미지 감소
                float distance = Vector3.Distance(explosionCenter, hitCollider.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

                unitHealth.TakeDamage(finalDamage);
                Debug.Log($"[SiegeTank] {hitCollider.name}에게 {finalDamage} 데미지 (거리: {distance:F2}m)");
            }

            // Rigidbody가 있으면 폭발력 적용
            Rigidbody hitRb = hitCollider.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                Vector3 forceDirection = (hitCollider.transform.position - explosionCenter).normalized;
                float forceMagnitude = 500f;
                hitRb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);

                Debug.Log($"[SiegeTank] {hitCollider.name}에게 폭발력 적용");
            }
        }
    }

    // Scene View에서 폭발 범위 시각화
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

/// <summary>
/// 시즈 탱크 - 포탄 발사
/// </summary>
public class SiegeTank : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Combat")]
    public float attackRange = 15f;
    public float attackCooldown = 3f;

    private float lastAttackTime = 0f;

    void Update()
    {
        // 스페이스바: 앞쪽으로 발사
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Vector3 targetPosition = transform.position + transform.forward * attackRange;
                Fire(targetPosition);
                lastAttackTime = Time.time;
            }
            else
            {
                Debug.Log($"[SiegeTank] 재장전 중... ({attackCooldown - (Time.time - lastAttackTime):F1}초 남음)");
            }
        }
    }

    public void Fire(Vector3 targetPosition)
    {
        Debug.Log($"[SiegeTank] 발포!");

        // 포탄 생성
        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        // 발사
        SiegeTankProjectile projectileScript = projectile.GetComponent<SiegeTankProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Launch(targetPosition);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 사거리 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
```

**실행 결과:**
```
[SiegeTank] 발포!
[SiegeTank] 포탄 발사! 타겟: (10, 0, 15), 각도: 45도
[SiegeTank] 초기 속도: 21.2 m/s
[SiegeTank] 포탄 착탄! 충돌: Ground
[SiegeTank] 충돌 위치: (10.2, 0, 14.8)
[SiegeTank] 폭발! 중심: (10.2, 0, 14.8), 반경: 2m
[SiegeTank] 폭발 범위 내 5개 오브젝트 감지
[SiegeTank] Marine에게 70 데미지 (거리: 0.5m)
[SiegeTank] Marine에게 35 데미지 (거리: 1.5m)
```

---

## Collider - 충돌체

**Collider**는 GameObject의 **충돌 영역**을 정의하는 컴포넌트입니다.

### Collider 종류

```csharp
using UnityEngine;

public class ColliderTypes : MonoBehaviour
{
    void Start()
    {
        // 1. Box Collider - 상자 형태
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = Vector3.zero;
        boxCollider.size = new Vector3(1, 1, 1);

        // 2. Sphere Collider - 구 형태
        SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.center = Vector3.zero;
        sphereCollider.radius = 0.5f;

        // 3. Capsule Collider - 캡슐 형태 (캐릭터에 적합)
        CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.center = Vector3.zero;
        capsuleCollider.radius = 0.5f;
        capsuleCollider.height = 2f;
        capsuleCollider.direction = 1; // 0: X축, 1: Y축, 2: Z축

        // 4. Mesh Collider - 메시 형태 (복잡한 모델)
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true; // Rigidbody와 함께 사용 시 필수
        meshCollider.sharedMesh = GetComponent<MeshFilter>().mesh;
    }
}
```

**Collider 선택 가이드:**
```
Box Collider:
- 건물, 벽, 상자
- 가장 빠른 충돌 계산

Sphere Collider:
- 공, 폭발 범위
- 두 번째로 빠른 계산

Capsule Collider:
- 캐릭터, 유닛
- 부드러운 충돌

Mesh Collider:
- 지형, 복잡한 구조물
- 가장 느린 계산
- Convex 옵션 필수 (Rigidbody 사용 시)
```

### Collider 속성

```csharp
public class ColliderProperties : MonoBehaviour
{
    void Start()
    {
        BoxCollider collider = GetComponent<BoxCollider>();

        // Is Trigger - 물리 충돌 무시, 이벤트만 발생
        collider.isTrigger = false; // 기본값

        // Material - 물리 재질 (마찰, 탄성)
        PhysicMaterial material = new PhysicMaterial();
        material.dynamicFriction = 0.6f;  // 동적 마찰
        material.staticFriction = 0.6f;   // 정적 마찰
        material.bounciness = 0f;         // 탄성 (0~1)
        collider.material = material;

        // Center - 중심점 (로컬 좌표)
        collider.center = new Vector3(0, 0.5f, 0);

        // Size - 크기
        collider.size = new Vector3(1, 2, 1);
    }
}
```

### Compound Collider (복합 콜라이더)

```csharp
using UnityEngine;

/// <summary>
/// 오버워치 바스티온 - 복합 Collider 예시
/// 여러 Collider를 조합하여 복잡한 형태 구현
/// </summary>
public class BastionColliders : MonoBehaviour
{
    void Start()
    {
        CreateBastionColliders();
    }

    void CreateBastionColliders()
    {
        // 바스티온의 몸체를 여러 Collider로 구성

        // 1. 몸통 (Box Collider)
        GameObject body = new GameObject("Body");
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        BoxCollider bodyCollider = body.AddComponent<BoxCollider>();
        bodyCollider.size = new Vector3(1f, 1.5f, 0.8f);

        // 2. 머리 (Sphere Collider)
        GameObject head = new GameObject("Head");
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 2f, 0);
        SphereCollider headCollider = head.AddComponent<SphereCollider>();
        headCollider.radius = 0.4f;

        // 3. 총 (Capsule Collider)
        GameObject gun = new GameObject("Gun");
        gun.transform.SetParent(transform);
        gun.transform.localPosition = new Vector3(0.5f, 1.5f, 0.5f);
        CapsuleCollider gunCollider = gun.AddComponent<CapsuleCollider>();
        gunCollider.radius = 0.1f;
        gunCollider.height = 1f;
        gunCollider.direction = 2; // Z축

        Debug.Log("[Bastion] 복합 Collider 생성 완료");
        Debug.Log($"- Body: Box Collider");
        Debug.Log($"- Head: Sphere Collider (약점)");
        Debug.Log($"- Gun: Capsule Collider");
    }
}
```

### 스타크래프트 예시: 유닛별 Collider 설정

```csharp
using UnityEngine;

public class UnitColliderSetup : MonoBehaviour
{
    public enum UnitType
    {
        Marine,      // 보병
        Firebat,     // 보병
        SiegeTank,   // 차량
        Battlecruiser, // 대형 유닛
        Building     // 건물
    }

    public UnitType unitType = UnitType.Marine;

    void Awake()
    {
        SetupCollider();
    }

    void SetupCollider()
    {
        switch (unitType)
        {
            case UnitType.Marine:
                // 마린: Capsule Collider (캐릭터 형태)
                CapsuleCollider marineCapsule = gameObject.AddComponent<CapsuleCollider>();
                marineCapsule.radius = 0.3f;
                marineCapsule.height = 1.8f;
                marineCapsule.center = new Vector3(0, 0.9f, 0);
                Debug.Log("[Marine] Capsule Collider 설정 (반경: 0.3m, 높이: 1.8m)");
                break;

            case UnitType.Firebat:
                // 파이어뱃: Capsule Collider (조금 더 큼)
                CapsuleCollider firebatCapsule = gameObject.AddComponent<CapsuleCollider>();
                firebatCapsule.radius = 0.4f;
                firebatCapsule.height = 2f;
                firebatCapsule.center = new Vector3(0, 1f, 0);
                Debug.Log("[Firebat] Capsule Collider 설정 (반경: 0.4m, 높이: 2m)");
                break;

            case UnitType.SiegeTank:
                // 시즈 탱크: Box Collider (차량 형태)
                BoxCollider tankBox = gameObject.AddComponent<BoxCollider>();
                tankBox.size = new Vector3(2f, 1.5f, 3f);
                tankBox.center = new Vector3(0, 0.75f, 0);
                Debug.Log("[SiegeTank] Box Collider 설정 (크기: 2x1.5x3m)");
                break;

            case UnitType.Battlecruiser:
                // 배틀크루저: Box Collider (대형)
                BoxCollider battlecruiserBox = gameObject.AddComponent<BoxCollider>();
                battlecruiserBox.size = new Vector3(5f, 3f, 8f);
                battlecruiserBox.center = new Vector3(0, 1.5f, 0);
                Debug.Log("[Battlecruiser] Box Collider 설정 (크기: 5x3x8m)");
                break;

            case UnitType.Building:
                // 건물: Box Collider (큼)
                BoxCollider buildingBox = gameObject.AddComponent<BoxCollider>();
                buildingBox.size = new Vector3(4f, 3f, 4f);
                buildingBox.center = new Vector3(0, 1.5f, 0);
                Debug.Log("[Building] Box Collider 설정 (크기: 4x3x4m)");
                break;
        }
    }
}
```

---

## Collision vs Trigger

**Collision**과 **Trigger**의 차이를 이해하는 것이 중요합니다.

### 비교표

| 특성 | Collision (Is Trigger = false) | Trigger (Is Trigger = true) |
|------|-------------------------------|----------------------------|
| **물리 충돌** | ✅ 있음 (통과 불가) | ❌ 없음 (통과 가능) |
| **이벤트** | OnCollision~ | OnTrigger~ |
| **Rigidbody 필요** | 둘 중 하나 | 둘 중 하나 |
| **사용 예시** | 벽, 바닥, 캐릭터 | 아이템, 감지 범위 |

### Collision 예시 (물리 충돌)

```csharp
using UnityEngine;

/// <summary>
/// Collision 예시: 벽
/// 물리적으로 막힘
/// </summary>
public class Wall : MonoBehaviour
{
    void Awake()
    {
        // Box Collider 추가
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = false; // Collision 모드
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Wall] {collision.gameObject.name}이(가) 벽에 부딪힘");
        Debug.Log($"[Wall] 충돌 속도: {collision.relativeVelocity.magnitude} m/s");
        Debug.Log($"[Wall] 충돌 지점: {collision.contacts[0].point}");
    }
}
```

### Trigger 예시 (감지 영역)

```csharp
using UnityEngine;

/// <summary>
/// Trigger 예시: 아이템
/// 물리적으로 통과 가능
/// </summary>
public class Item : MonoBehaviour
{
    public int healAmount = 20;

    void Awake()
    {
        // Sphere Collider 추가
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true; // Trigger 모드
        collider.radius = 0.5f;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Item] {other.name}이(가) 아이템에 닿음");

        // 플레이어만 획득 가능
        if (other.CompareTag("Player"))
        {
            UnitHealth playerHealth = other.GetComponent<UnitHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                Debug.Log($"[Item] {other.name}이(가) 아이템 획득! (+{healAmount} HP)");
                Destroy(gameObject); // 아이템 제거
            }
        }
    }
}
```

### 스타크래프트 예시: 미네랄 획득

```csharp
using UnityEngine;

/// <summary>
/// 미네랄 - Trigger 사용
/// SCV가 통과하면 자동 수집
/// </summary>
public class Mineral : MonoBehaviour
{
    [Header("Resource Settings")]
    public int mineralAmount = 50;
    private bool isBeingMined = false;

    void Awake()
    {
        // Trigger Collider 설정
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f; // 수집 범위

        gameObject.tag = "Mineral";

        Debug.Log($"[Mineral] 생성 완료 (자원량: {mineralAmount})");
    }

    void OnTriggerEnter(Collider other)
    {
        // SCV가 접촉하면
        if (other.CompareTag("SCV"))
        {
            Debug.Log($"[Mineral] {other.name}이(가) 미네랄에 접근");

            SCV scv = other.GetComponent<SCV>();
            if (scv != null && !isBeingMined)
            {
                isBeingMined = true;
                scv.StartMining(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // SCV가 벗어나면
        if (other.CompareTag("SCV"))
        {
            Debug.Log($"[Mineral] {other.name}이(가) 미네랄에서 멀어짐");

            SCV scv = other.GetComponent<SCV>();
            if (scv != null)
            {
                isBeingMined = false;
                scv.StopMining();
            }
        }
    }

    public int Mine()
    {
        int minedAmount = Mathf.Min(8, mineralAmount); // 한 번에 8씩
        mineralAmount -= minedAmount;

        Debug.Log($"[Mineral] 채굴: {minedAmount}, 남은 자원: {mineralAmount}");

        if (mineralAmount <= 0)
        {
            Debug.Log($"[Mineral] 자원 고갈! 파괴됨");
            Destroy(gameObject);
        }

        return minedAmount;
    }

    void OnDrawGizmos()
    {
        // 수집 범위 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}

/// <summary>
/// SCV - 미네랄 채굴
/// </summary>
public class SCV : MonoBehaviour
{
    private Mineral currentMineral = null;
    private bool isMining = false;

    public void StartMining(Mineral mineral)
    {
        currentMineral = mineral;
        isMining = true;
        Debug.Log($"[SCV] 채굴 시작: {mineral.name}");
    }

    public void StopMining()
    {
        isMining = false;
        currentMineral = null;
        Debug.Log($"[SCV] 채굴 중단");
    }

    void Update()
    {
        if (isMining && currentMineral != null)
        {
            // 1초마다 채굴
            if (Time.frameCount % 60 == 0)
            {
                int mined = currentMineral.Mine();
                Debug.Log($"[SCV] 미네랄 {mined}개 채굴");
            }
        }
    }
}
```

**실행 결과:**
```
[Mineral] 생성 완료 (자원량: 50)
[Mineral] SCV이(가) 미네랄에 접근
[SCV] 채굴 시작: Mineral
[Mineral] 채굴: 8, 남은 자원: 42
[SCV] 미네랄 8개 채굴
[Mineral] 채굴: 8, 남은 자원: 34
...
[Mineral] 채굴: 2, 남은 자원: 0
[Mineral] 자원 고갈! 파괴됨
```

---

## 충돌 이벤트 함수

유니티는 **충돌 시 자동으로 호출**되는 함수를 제공합니다.

### OnCollision 계열 (Collision 모드)

```csharp
using UnityEngine;

public class CollisionEvents : MonoBehaviour
{
    // Rigidbody 필수!
    private Rigidbody rb;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = false; // Collision 모드
    }

    // 충돌 시작 (처음 닿는 순간)
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"충돌 시작: {collision.gameObject.name}");

        // 충돌 정보
        Debug.Log($"충돌 지점 개수: {collision.contactCount}");
        Debug.Log($"상대 속도: {collision.relativeVelocity}");

        // 첫 번째 접촉점 정보
        ContactPoint contact = collision.contacts[0];
        Debug.Log($"접촉 위치: {contact.point}");
        Debug.Log($"접촉 법선: {contact.normal}");
    }

    // 충돌 중 (매 프레임)
    void OnCollisionStay(Collision collision)
    {
        Debug.Log($"충돌 중: {collision.gameObject.name}");
    }

    // 충돌 종료 (떨어지는 순간)
    void OnCollisionExit(Collision collision)
    {
        Debug.Log($"충돌 종료: {collision.gameObject.name}");
    }
}
```

### OnTrigger 계열 (Trigger 모드)

```csharp
using UnityEngine;

public class TriggerEvents : MonoBehaviour
{
    // Rigidbody 필수 (자신 또는 상대)
    private Rigidbody rb;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false; // 중력 끄기
        rb.isKinematic = true; // 물리 계산 끄기

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true; // Trigger 모드
    }

    // Trigger 진입
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger 진입: {other.name}");
    }

    // Trigger 안에 있는 동안 (매 프레임)
    void OnTriggerStay(Collider other)
    {
        Debug.Log($"Trigger 안: {other.name}");
    }

    // Trigger 탈출
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Trigger 탈출: {other.name}");
    }
}
```

### 충돌 이벤트 발생 조건

| GameObject A | GameObject B | 결과 |
|--------------|--------------|------|
| Rigidbody + Collider | Collider | ✅ 발생 |
| Collider | Rigidbody + Collider | ✅ 발생 |
| Collider | Collider | ❌ 발생 안 함 |
| Rigidbody (Kinematic) + Trigger | Rigidbody + Collider | ✅ 발생 |

**핵심 규칙:**
- **최소 하나**는 Rigidbody가 있어야 함
- **둘 다** Collider가 있어야 함
- Trigger의 경우 둘 중 하나는 움직여야 함 (Kinematic도 가능)

### 오버워치 예시: 트레이서 펄스 폭탄

```csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// 트레이서 펄스 폭탄
/// - OnTrigger로 부착 감지
/// - OnCollision으로 바닥 충돌 감지
/// </summary>
public class TracerPulseBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public float fuseTime = 3f;
    public int damage = 300;
    public float explosionRadius = 5f;

    private Rigidbody rb;
    private SphereCollider mainCollider;
    private SphereCollider triggerCollider;
    private bool isAttached = false;
    private Transform attachedTarget = null;

    void Awake()
    {
        // Rigidbody 설정
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.useGravity = true;

        // 메인 Collider (물리 충돌)
        mainCollider = gameObject.AddComponent<SphereCollider>();
        mainCollider.radius = 0.2f;
        mainCollider.isTrigger = false;

        // Trigger Collider (부착 감지)
        GameObject triggerObject = new GameObject("TriggerZone");
        triggerObject.transform.SetParent(transform);
        triggerObject.transform.localPosition = Vector3.zero;
        triggerCollider = triggerObject.AddComponent<SphereCollider>();
        triggerCollider.radius = 0.5f;
        triggerCollider.isTrigger = true;

        Debug.Log("[Tracer] 펄스 폭탄 생성");
    }

    public void Throw(Vector3 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);
        Debug.Log($"[Tracer] 펄스 폭탄 투척! 힘: {force}");

        // 신관 시작
        StartCoroutine(FuseTimer());
    }

    // Trigger: 적 유닛에 부착
    void OnTriggerEnter(Collider other)
    {
        if (isAttached) return;

        // 적 유닛만 부착 가능
        if (other.CompareTag("Enemy"))
        {
            AttachTo(other.transform);
        }
    }

    // Collision: 바닥/벽에 충돌
    void OnCollisionEnter(Collision collision)
    {
        if (isAttached) return;

        Debug.Log($"[Tracer] 펄스 폭탄 충돌: {collision.gameObject.name}");
        Debug.Log($"[Tracer] 충돌 속도: {collision.relativeVelocity.magnitude:F2} m/s");

        // 바닥에 떨어진 경우
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            Debug.Log("[Tracer] 바닥에 떨어짐");
        }
    }

    void AttachTo(Transform target)
    {
        isAttached = true;
        attachedTarget = target;

        // 부모 설정 (타겟을 따라다님)
        transform.SetParent(target);

        // 물리 끄기
        rb.isKinematic = true;
        rb.useGravity = false;

        Debug.Log($"[Tracer] 펄스 폭탄이 {target.name}에 부착됨!");
    }

    IEnumerator FuseTimer()
    {
        float elapsed = 0f;

        while (elapsed < fuseTime)
        {
            elapsed += Time.deltaTime;

            // 경고음 (1초마다)
            if (elapsed % 1f < 0.1f)
            {
                Debug.Log($"[Tracer] 폭발까지: {fuseTime - elapsed:F1}초");
            }

            yield return null;
        }

        // 폭발!
        Explode();
    }

    void Explode()
    {
        Debug.Log($"[Tracer] 펄스 폭탄 폭발! 위치: {transform.position}");

        // 폭발 범위 내 모든 적 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        Debug.Log($"[Tracer] 폭발 범위 내 {hitColliders.Length}개 오브젝트 감지");

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                UnitHealth health = hit.GetComponent<UnitHealth>();
                if (health != null)
                {
                    // 거리에 따른 데미지 감소
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

                    health.TakeDamage(finalDamage);
                    Debug.Log($"[Tracer] {hit.name}에게 {finalDamage} 데미지!");
                }
            }
        }

        // 폭탄 파괴
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // 부착 범위 (Trigger)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 폭발 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
```

**실행 결과:**
```
[Tracer] 펄스 폭탄 생성
[Tracer] 펄스 폭탄 투척! 힘: 10
[Tracer] 폭발까지: 3.0초
[Tracer] 펄스 폭탄이 Reaper에 부착됨!
[Tracer] 폭발까지: 2.0초
[Tracer] 폭발까지: 1.0초
[Tracer] 펄스 폭탄 폭발! 위치: (5, 1, 3)
[Tracer] 폭발 범위 내 3개 오브젝트 감지
[Tracer] Reaper에게 300 데미지!
[Tracer] Soldier76에게 150 데미지!
```

---

## Raycast 심화

**Raycast**는 레이(광선)를 쏴서 충돌을 감지하는 기능입니다.

### Raycast 기본

```csharp
using UnityEngine;

public class RaycastBasics : MonoBehaviour
{
    void Update()
    {
        // 1. 기본 Raycast
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, 10f))
        {
            Debug.Log("앞에 뭔가 있음!");
        }

        // 2. RaycastHit로 정보 받기
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f))
        {
            Debug.Log($"충돌: {hit.collider.name}");
            Debug.Log($"거리: {hit.distance}");
            Debug.Log($"위치: {hit.point}");
            Debug.Log($"법선: {hit.normal}");
        }

        // 3. 간편한 문법
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
        {
            Debug.Log($"충돌: {hit.collider.name}");
        }

        // 4. LayerMask 사용
        int layerMask = LayerMask.GetMask("Enemy");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, layerMask))
        {
            Debug.Log($"적 발견: {hit.collider.name}");
        }

        // 5. 시각화 (Scene View)
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);
    }
}
```

### Raycast 종류

```csharp
public class RaycastVariants : MonoBehaviour
{
    void Update()
    {
        RaycastHit hit;

        // 1. Raycast - 단일 Ray
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
        {
            Debug.Log($"Raycast: {hit.collider.name}");
        }

        // 2. RaycastAll - 모든 충돌
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, 10f);
        foreach (RaycastHit h in hits)
        {
            Debug.Log($"RaycastAll: {h.collider.name} (거리: {h.distance})");
        }

        // 3. SphereCast - 구체 모양 Ray
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, 10f))
        {
            Debug.Log($"SphereCast: {hit.collider.name}");
        }

        // 4. BoxCast - 박스 모양 Ray
        Vector3 halfExtents = new Vector3(0.5f, 0.5f, 0.5f);
        if (Physics.BoxCast(transform.position, halfExtents, transform.forward, out hit, Quaternion.identity, 10f))
        {
            Debug.Log($"BoxCast: {hit.collider.name}");
        }

        // 5. CapsuleCast - 캡슐 모양 Ray
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + Vector3.up;
        if (Physics.CapsuleCast(point1, point2, 0.5f, transform.forward, out hit, 10f))
        {
            Debug.Log($"CapsuleCast: {hit.collider.name}");
        }

        // 6. OverlapSphere - 구 범위 내 모든 Collider
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        Debug.Log($"OverlapSphere: {colliders.Length}개 발견");
    }
}
```

### 스타크래프트 예시: 마린 Raycast 사격

```csharp
using UnityEngine;

/// <summary>
/// 마린 - Raycast 즉시 명중 사격
/// 총알 오브젝트 없이 즉시 데미지
/// </summary>
public class MarineRaycastWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int damage = 6;
    public float range = 10f;
    public float fireRate = 0.86f; // 초당 공격 횟수
    public Transform firePoint;

    [Header("Effects")]
    public LineRenderer laserLine;

    private float lastFireTime = 0f;

    void Awake()
    {
        // LineRenderer 설정 (레이저 시각화)
        if (laserLine == null)
        {
            GameObject laserObj = new GameObject("LaserLine");
            laserObj.transform.SetParent(transform);
            laserLine = laserObj.AddComponent<LineRenderer>();
            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.05f;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = Color.red;
            laserLine.endColor = Color.yellow;
            laserLine.enabled = false;
        }
    }

    void Update()
    {
        // 스페이스바: 발사
        if (Input.GetKey(KeyCode.Space))
        {
            if (Time.time >= lastFireTime + (1f / fireRate))
            {
                Fire();
                lastFireTime = Time.time;
            }
        }
    }

    void Fire()
    {
        Vector3 origin = firePoint.position;
        Vector3 direction = transform.forward;

        Debug.Log($"[Marine] 발사! 위치: {origin}, 방향: {direction}");

        // Raycast 발사
        RaycastHit hit;
        int layerMask = LayerMask.GetMask("Enemy");

        if (Physics.Raycast(origin, direction, out hit, range, layerMask))
        {
            // 명중!
            Debug.Log($"[Marine] 명중! 타겟: {hit.collider.name}");
            Debug.Log($"[Marine] 거리: {hit.distance:F2}m");
            Debug.Log($"[Marine] 명중 위치: {hit.point}");

            // 데미지 처리
            UnitHealth targetHealth = hit.collider.GetComponent<UnitHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }

            // 레이저 라인 표시
            StartCoroutine(ShowLaserLine(origin, hit.point));
        }
        else
        {
            // 빗나감
            Debug.Log($"[Marine] 빗나감!");

            // 레이저 라인 표시
            StartCoroutine(ShowLaserLine(origin, origin + direction * range));
        }

        // 시각화 (Scene View)
        Debug.DrawRay(origin, direction * range, Color.red, 0.5f);
    }

    System.Collections.IEnumerator ShowLaserLine(Vector3 start, Vector3 end)
    {
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        laserLine.enabled = true;

        yield return new WaitForSeconds(0.1f);

        laserLine.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        // 사거리 표시
        Gizmos.color = Color.red;
        Gizmos.DrawRay(firePoint.position, transform.forward * range);

        // 범위 끝 표시
        Gizmos.DrawWireSphere(firePoint.position + transform.forward * range, 0.2f);
    }
}
```

### 오버워치 예시: 위도우메이커 Raycast 저격

```csharp
using UnityEngine;

/// <summary>
/// 위도우메이커 - 헤드샷 감지 Raycast
/// </summary>
public class WidowmakerSniper : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int bodyDamage = 120;
    public int headDamage = 300; // 헤드샷 2.5배
    public float range = 100f;
    public Transform firePoint;

    [Header("Scope")]
    public bool isScoped = false;
    public Camera mainCamera;
    public Camera scopeCamera;

    void Update()
    {
        // 우클릭: 조준경
        if (Input.GetMouseButtonDown(1))
        {
            ToggleScope();
        }

        // 좌클릭: 발사
        if (Input.GetMouseButtonDown(0) && isScoped)
        {
            Fire();
        }
    }

    void ToggleScope()
    {
        isScoped = !isScoped;

        if (isScoped)
        {
            // 조준경 활성화
            mainCamera.fieldOfView = 20f; // 줌인
            Debug.Log("[Widowmaker] 조준경 활성화");
        }
        else
        {
            // 조준경 해제
            mainCamera.fieldOfView = 60f; // 줌아웃
            Debug.Log("[Widowmaker] 조준경 해제");
        }
    }

    void Fire()
    {
        // 화면 중앙에서 Raycast
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Debug.Log($"[Widowmaker] 발사! 원점: {ray.origin}, 방향: {ray.direction}");

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log($"[Widowmaker] 명중! 타겟: {hit.collider.name}");
            Debug.Log($"[Widowmaker] 거리: {hit.distance:F2}m");

            // 헤드샷 판정
            bool isHeadshot = hit.collider.CompareTag("Head");
            int finalDamage = isHeadshot ? headDamage : bodyDamage;

            UnitHealth targetHealth = hit.collider.GetComponentInParent<UnitHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(finalDamage);

                if (isHeadshot)
                {
                    Debug.Log($"[Widowmaker] ★ 헤드샷! ★ {finalDamage} 데미지");
                }
                else
                {
                    Debug.Log($"[Widowmaker] 몸샷: {finalDamage} 데미지");
                }
            }

            // 레이저 라인 표시
            Debug.DrawLine(ray.origin, hit.point, Color.cyan, 1f);
        }
        else
        {
            Debug.Log($"[Widowmaker] 빗나감");
            Debug.DrawRay(ray.origin, ray.direction * range, Color.gray, 1f);
        }
    }
}
```

### SphereCast 예시: 오버워치 스캐터 건

```csharp
using UnityEngine;

/// <summary>
/// 맥크리 스캐터 건 (구형 Raycast)
/// </summary>
public class ScatterGun : MonoBehaviour
{
    public int pelletCount = 6; // 산탄 개수
    public int damagePerPellet = 10;
    public float range = 7f;
    public float spreadAngle = 15f; // 확산 각도

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    void Fire()
    {
        Debug.Log($"[McCree] 스캐터 건 발사! (산탄 {pelletCount}개)");

        for (int i = 0; i < pelletCount; i++)
        {
            // 랜덤 확산
            Vector3 spread = Random.insideUnitSphere * spreadAngle;
            Vector3 direction = (transform.forward + spread).normalized;

            // SphereCast (작은 구체)
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.1f, direction, out hit, range))
            {
                Debug.Log($"[McCree] 산탄 {i + 1} 명중! {hit.collider.name}");

                UnitHealth health = hit.collider.GetComponent<UnitHealth>();
                if (health != null)
                {
                    health.TakeDamage(damagePerPellet);
                }

                Debug.DrawLine(transform.position, hit.point, Color.yellow, 0.5f);
            }
            else
            {
                Debug.DrawRay(transform.position, direction * range, Color.gray, 0.5f);
            }
        }
    }
}
```

---

## Physics Material

**Physics Material**은 물리 재질을 정의합니다 (마찰, 탄성).

### Physics Material 생성

```csharp
using UnityEngine;

public class PhysicsMaterialExample : MonoBehaviour
{
    void Start()
    {
        CreatePhysicsMaterials();
    }

    void CreatePhysicsMaterials()
    {
        Collider collider = GetComponent<Collider>();

        // 1. 얼음 (마찰 없음)
        PhysicMaterial ice = new PhysicMaterial("Ice");
        ice.dynamicFriction = 0f;  // 동적 마찰
        ice.staticFriction = 0f;   // 정적 마찰
        ice.bounciness = 0f;       // 탄성
        ice.frictionCombine = PhysicMaterialCombine.Minimum; // 마찰 결합 방식
        ice.bounceCombine = PhysicMaterialCombine.Minimum;   // 탄성 결합 방식

        // 2. 고무 (탄성 높음)
        PhysicMaterial rubber = new PhysicMaterial("Rubber");
        rubber.dynamicFriction = 0.6f;
        rubber.staticFriction = 0.6f;
        rubber.bounciness = 0.9f; // 많이 튕김
        rubber.frictionCombine = PhysicMaterialCombine.Average;
        rubber.bounceCombine = PhysicMaterialCombine.Maximum;

        // 3. 나무 (보통)
        PhysicMaterial wood = new PhysicMaterial("Wood");
        wood.dynamicFriction = 0.4f;
        wood.staticFriction = 0.4f;
        wood.bounciness = 0.2f;

        // 적용
        collider.material = ice;
    }
}
```

### Combine 모드

```csharp
// Friction Combine (마찰 결합)
PhysicMaterialCombine.Average;   // 평균값
PhysicMaterialCombine.Minimum;   // 최소값 (얼음)
PhysicMaterialCombine.Maximum;   // 최대값
PhysicMaterialCombine.Multiply;  // 곱셈

// Bounce Combine (탄성 결합)
// 동일
```

### 스타크래프트 예시: 지형별 Physics Material

```csharp
using UnityEngine;

public class TerrainPhysics : MonoBehaviour
{
    public enum TerrainType
    {
        Normal,   // 일반 지형
        Ice,      // 얼음 (미끄러움)
        Mud,      // 진흙 (느림)
        Bounce    // 튕기는 지형
    }

    public TerrainType terrainType = TerrainType.Normal;

    void Awake()
    {
        ApplyTerrainPhysics();
    }

    void ApplyTerrainPhysics()
    {
        Collider collider = GetComponent<Collider>();
        PhysicMaterial material = new PhysicMaterial();

        switch (terrainType)
        {
            case TerrainType.Normal:
                material.name = "Normal";
                material.dynamicFriction = 0.6f;
                material.staticFriction = 0.6f;
                material.bounciness = 0f;
                Debug.Log("[Terrain] 일반 지형 적용");
                break;

            case TerrainType.Ice:
                material.name = "Ice";
                material.dynamicFriction = 0.05f; // 거의 없음
                material.staticFriction = 0.05f;
                material.bounciness = 0f;
                material.frictionCombine = PhysicMaterialCombine.Minimum;
                Debug.Log("[Terrain] 얼음 지형 적용 (미끄러움!)");
                break;

            case TerrainType.Mud:
                material.name = "Mud";
                material.dynamicFriction = 1.5f; // 높음
                material.staticFriction = 1.5f;
                material.bounciness = 0f;
                material.frictionCombine = PhysicMaterialCombine.Maximum;
                Debug.Log("[Terrain] 진흙 지형 적용 (느림!)");
                break;

            case TerrainType.Bounce:
                material.name = "Bounce";
                material.dynamicFriction = 0.3f;
                material.staticFriction = 0.3f;
                material.bounciness = 0.8f; // 많이 튕김
                material.bounceCombine = PhysicMaterialCombine.Maximum;
                Debug.Log("[Terrain] 튕기는 지형 적용");
                break;
        }

        collider.material = material;
    }
}
```

---

## Layer Collision Matrix

**Layer Collision Matrix**는 레이어 간 충돌을 제어합니다.

### Layer 설정

```
Edit > Project Settings > Tags and Layers

기본 Layers:
- Default (0)
- TransparentFX (1)
- Ignore Raycast (2)
- Water (4)
- UI (5)

커스텀 Layers (8~31):
- Player
- Enemy
- Projectile
- Ground
- Item
```

### Layer Collision Matrix 설정

```
Edit > Project Settings > Physics > Layer Collision Matrix

체크: 충돌 O
해제: 충돌 X

예시:
[✓] Player ↔ Ground     (플레이어는 바닥과 충돌)
[✓] Player ↔ Enemy      (플레이어는 적과 충돌)
[✗] Player ↔ Item       (플레이어는 아이템과 충돌 안 함)
[✗] Projectile ↔ Player (아군 총알은 플레이어와 충돌 안 함)
[✓] Projectile ↔ Enemy  (아군 총알은 적과 충돌)
```

### LayerMask 사용

```csharp
using UnityEngine;

public class LayerMaskUsage : MonoBehaviour
{
    void Update()
    {
        RaycastHit hit;

        // 1. 특정 레이어만
        int enemyLayer = LayerMask.GetMask("Enemy");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, enemyLayer))
        {
            Debug.Log($"적 감지: {hit.collider.name}");
        }

        // 2. 여러 레이어
        int targetLayers = LayerMask.GetMask("Enemy", "NPC");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, targetLayers))
        {
            Debug.Log($"타겟 감지: {hit.collider.name}");
        }

        // 3. 레이어 제외 (~)
        int allExceptPlayer = ~LayerMask.GetMask("Player");
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10f, allExceptPlayer))
        {
            Debug.Log($"플레이어 제외 충돌: {hit.collider.name}");
        }

        // 4. 모든 레이어
        int allLayers = ~0;
        Physics.Raycast(transform.position, transform.forward, out hit, 10f, allLayers);
    }

    // GameObject의 레이어 변경
    void ChangeLayer()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    // 레이어 체크
    bool IsEnemy(GameObject obj)
    {
        return obj.layer == LayerMask.NameToLayer("Enemy");
    }
}
```

### 스타크래프트 예시: Layer 기반 충돌 시스템

```csharp
using UnityEngine;

/// <summary>
/// 스타크래프트 Layer 시스템
/// - Player: 아군 유닛
/// - Enemy: 적군 유닛
/// - Projectile_Friendly: 아군 투사체
/// - Projectile_Enemy: 적군 투사체
/// - Ground: 지형
/// </summary>
public class StarCraftLayerSetup : MonoBehaviour
{
    public enum UnitTeam
    {
        Player,
        Enemy
    }

    public UnitTeam team = UnitTeam.Player;

    void Awake()
    {
        SetupLayer();
    }

    void SetupLayer()
    {
        switch (team)
        {
            case UnitTeam.Player:
                gameObject.layer = LayerMask.NameToLayer("Player");
                gameObject.tag = "Player";
                Debug.Log($"[{gameObject.name}] 아군 유닛 설정");
                break;

            case UnitTeam.Enemy:
                gameObject.layer = LayerMask.NameToLayer("Enemy");
                gameObject.tag = "Enemy";
                Debug.Log($"[{gameObject.name}] 적군 유닛 설정");
                break;
        }
    }
}

/// <summary>
/// 투사체 - Layer 기반 충돌
/// </summary>
public class SmartProjectile : MonoBehaviour
{
    public enum ProjectileTeam
    {
        Friendly,
        Enemy
    }

    public ProjectileTeam team = ProjectileTeam.Friendly;
    public int damage = 10;

    private Rigidbody rb;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = 0.1f;

        // Layer 설정
        if (team == ProjectileTeam.Friendly)
        {
            gameObject.layer = LayerMask.NameToLayer("Projectile_Friendly");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Projectile_Enemy");
        }
    }

    public void Launch(Vector3 direction, float speed)
    {
        rb.velocity = direction * speed;
        Destroy(gameObject, 5f); // 5초 후 파괴
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Projectile] 충돌: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)})");

        // 적에게만 데미지
        string targetLayer = LayerMask.LayerToName(collision.gameObject.layer);

        bool shouldDamage = false;

        if (team == ProjectileTeam.Friendly && targetLayer == "Enemy")
        {
            shouldDamage = true;
        }
        else if (team == ProjectileTeam.Enemy && targetLayer == "Player")
        {
            shouldDamage = true;
        }

        if (shouldDamage)
        {
            UnitHealth health = collision.gameObject.GetComponent<UnitHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[Projectile] {collision.gameObject.name}에게 {damage} 데미지");
            }
        }

        Destroy(gameObject);
    }
}
```

**Layer Collision Matrix 설정 예시:**
```
         Ground  Player  Enemy  Projectile_Friendly  Projectile_Enemy
Ground     ✓      ✓       ✓            ✓                    ✓
Player     ✓      ✗       ✓            ✗                    ✓
Enemy      ✓      ✓       ✗            ✓                    ✗
Proj_F     ✓      ✗       ✓            ✗                    ✗
Proj_E     ✓      ✓       ✗            ✗                    ✗

설명:
- Player와 Player는 충돌 안 함 (아군끼리)
- Player와 Projectile_Friendly 충돌 안 함 (아군 총알)
- Player와 Projectile_Enemy 충돌 (적 총알)
- Projectile끼리는 충돌 안 함 (총알끼리)
```

---

## 실전 프로젝트: 스타크래프트 전투 시스템

모든 물리 개념을 통합한 완전한 전투 시스템입니다.

```csharp
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 유닛 체력 시스템
/// </summary>
public class UnitHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 40;
    public int currentHP;
    public int armor = 0;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        // 방어력 계산
        int finalDamage = Mathf.Max(1, damage - armor);
        currentHP -= finalDamage;

        Debug.Log($"[{gameObject.name}] 피해: {finalDamage} (방어력: {armor}) - HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"[{gameObject.name}] 회복: {amount} - HP: {currentHP}/{maxHP}");
    }

    void Die()
    {
        Debug.Log($"[{gameObject.name}] 파괴됨!");
        Destroy(gameObject);
    }
}

/// <summary>
/// 마린 총알 - 물리 투사체
/// </summary>
public class MarineBullet : MonoBehaviour
{
    public int damage = 6;
    public float speed = 50f;
    public float lifetime = 3f;

    private Rigidbody rb;
    private TrailRenderer trail;

    void Awake()
    {
        // Rigidbody 설정
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.01f;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Collider 설정
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = 0.05f;

        // Layer 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // Trail Renderer (궤적)
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = Color.red;

        // 수명
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction)
    {
        rb.velocity = direction * speed;
        transform.forward = direction;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Bullet] 충돌: {collision.gameObject.name}");

        // 적에게 데미지
        if (collision.gameObject.CompareTag("Enemy"))
        {
            UnitHealth health = collision.gameObject.GetComponent<UnitHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[Bullet] {collision.gameObject.name}에게 {damage} 데미지");
            }
        }

        // 총알 파괴
        Destroy(gameObject);
    }
}

/// <summary>
/// 마린 - 완전한 전투 시스템
/// Raycast, Rigidbody, Trigger, Collision 모두 사용
/// </summary>
public class CompleteMarine : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Combat")]
    public float detectionRange = 15f;
    public float attackRange = 10f;
    public float fireRate = 1.5f; // 초당 발사 횟수

    [Header("Movement")]
    public float moveSpeed = 3.75f;

    private Rigidbody rb;
    private CapsuleCollider mainCollider;
    private SphereCollider detectionTrigger;
    private GameObject currentTarget = null;
    private float lastFireTime = 0f;
    private List<GameObject> enemiesInRange = new List<GameObject>();

    void Awake()
    {
        SetupComponents();
    }

    void SetupComponents()
    {
        // Rigidbody
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        // 메인 Collider (물리 충돌)
        mainCollider = gameObject.AddComponent<CapsuleCollider>();
        mainCollider.radius = 0.3f;
        mainCollider.height = 1.8f;
        mainCollider.center = new Vector3(0, 0.9f, 0);

        // 감지 Trigger (적 감지)
        GameObject detectionObj = new GameObject("DetectionZone");
        detectionObj.transform.SetParent(transform);
        detectionObj.transform.localPosition = Vector3.zero;
        detectionObj.layer = gameObject.layer;

        detectionTrigger = detectionObj.AddComponent<SphereCollider>();
        detectionTrigger.isTrigger = true;
        detectionTrigger.radius = detectionRange;

        // Fire Point
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(0, 1.2f, 0.5f);
            firePoint = firePointObj.transform;
        }

        // Layer & Tag
        gameObject.layer = LayerMask.NameToLayer("Player");
        gameObject.tag = "Player";

        Debug.Log("[Marine] 컴포넌트 설정 완료");
    }

    void Update()
    {
        HandleCombat();
        HandleManualControl();
    }

    void HandleCombat()
    {
        // 타겟 선택 (가장 가까운 적)
        if (currentTarget == null || !enemiesInRange.Contains(currentTarget))
        {
            FindNearestEnemy();
        }

        // 타겟 공격
        if (currentTarget != null)
        {
            // 타겟 바라보기
            Vector3 direction = (currentTarget.transform.position - transform.position);
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            // 거리 체크
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= attackRange)
            {
                // 사거리 안: 공격
                if (Time.time >= lastFireTime + (1f / fireRate))
                {
                    Fire();
                    lastFireTime = Time.time;
                }
            }
            else if (distance <= detectionRange)
            {
                // 감지 범위 안: 접근
                MoveTowards(currentTarget.transform.position);
            }
        }
    }

    void FindNearestEnemy()
    {
        // 파괴된 적 제거
        enemiesInRange.RemoveAll(e => e == null);

        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            return;
        }

        // 가장 가까운 적 찾기
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        currentTarget = nearest;

        if (currentTarget != null)
        {
            Debug.Log($"[Marine] 타겟 설정: {currentTarget.name} (거리: {nearestDistance:F2}m)");
        }
    }

    void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    void Fire()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[Marine] 총알 Prefab이 없습니다!");
            return;
        }

        Debug.Log($"[Marine] 발사! 타겟: {currentTarget.name}");

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 발사 방향 계산 (예측 사격)
        Vector3 targetPosition = currentTarget.transform.position + Vector3.up * 1f; // 중심 조준
        Vector3 direction = (targetPosition - firePoint.position).normalized;

        // 발사
        MarineBullet bulletScript = bullet.GetComponent<MarineBullet>();
        if (bulletScript != null)
        {
            bulletScript.Launch(direction);
        }
    }

    void HandleManualControl()
    {
        // 수동 이동 (WASD)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

            // 이동 방향 바라보기
            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(movement);
            }
        }
    }

    // Trigger: 적 감지
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other.gameObject))
            {
                enemiesInRange.Add(other.gameObject);
                Debug.Log($"[Marine] 적 감지: {other.name} (총 {enemiesInRange.Count}명)");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.gameObject);
            Debug.Log($"[Marine] 적 범위 이탈: {other.name} (남은 적: {enemiesInRange.Count}명)");

            if (currentTarget == other.gameObject)
            {
                currentTarget = null;
            }
        }
    }

    // Collision: 벽/장애물 충돌
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Marine] 충돌: {collision.gameObject.name}");
    }

    void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 타겟 라인
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}

/// <summary>
/// 테스트 적 유닛
/// </summary>
public class EnemyUnit : MonoBehaviour
{
    void Awake()
    {
        // Rigidbody
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation |
                         RigidbodyConstraints.FreezePositionY;

        // Collider
        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.radius = 0.3f;
        collider.height = 1.8f;
        collider.center = new Vector3(0, 0.9f, 0);

        // Health
        UnitHealth health = gameObject.AddComponent<UnitHealth>();
        health.maxHP = 50;
        health.currentHP = 50;

        // Layer & Tag
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        gameObject.tag = "Enemy";
    }
}
```

**실행 결과:**
```
[Marine] 컴포넌트 설정 완료
[Marine] 적 감지: Zergling (총 1명)
[Marine] 타겟 설정: Zergling (거리: 12.5m)
[Marine] 발사! 타겟: Zergling
[Bullet] 충돌: Zergling
[Bullet] Zergling에게 6 데미지
[Zergling] 피해: 6 (방어력: 0) - HP: 44/50
[Marine] 발사! 타겟: Zergling
...
[Zergling] 피해: 6 (방어력: 0) - HP: 2/50
[Marine] 발사! 타겟: Zergling
[Zergling] 피해: 6 (방어력: 0) - HP: -4/50
[Zergling] 파괴됨!
[Marine] 적 범위 이탈: Zergling (남은 적: 0명)
```

---

## 정리 및 다음 단계

### 이번 강의에서 배운 것

✅ **Rigidbody** - 물리 속성 (질량, 중력, 힘)
✅ **Collider** - 충돌 영역 (Box, Sphere, Capsule, Mesh)
✅ **Collision vs Trigger** - 물리 충돌 vs 감지 영역
✅ **충돌 이벤트** - OnCollision~/OnTrigger~
✅ **Raycast** - 광선 충돌 감지 (즉시 명중)
✅ **Physics Material** - 마찰, 탄성
✅ **Layer Collision Matrix** - 레이어 간 충돌 제어

### 핵심 규칙 정리

```csharp
// 1. 물리 오브젝트는 Rigidbody 필수
Rigidbody rb = gameObject.AddComponent<Rigidbody>();

// 2. 충돌 감지는 Collider 필수
BoxCollider collider = gameObject.AddComponent<BoxCollider>();

// 3. 물리 이동은 FixedUpdate
void FixedUpdate() {
    rb.MovePosition(rb.position + Vector3.forward * speed * Time.fixedDeltaTime);
}

// 4. Trigger는 통과, Collision은 막힘
collider.isTrigger = true;  // 통과
collider.isTrigger = false; // 막힘

// 5. Raycast는 즉시 명중
if (Physics.Raycast(origin, direction, out hit, range)) {
    // 명중!
}
```

### 다음 강의 예고: 8강 - UI 시스템

다음 강의에서는 유니티의 UI 시스템을 배웁니다:
- Canvas와 EventSystem
- Button, Text, Image, Slider
- RectTransform과 Anchor
- 스타크래프트 스타일 UI 구현
- 실전: 자원 표시, 유닛 선택 UI, 커맨드 패널

다음 강의에서 만나요! 🎮
