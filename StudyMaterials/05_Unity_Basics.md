# 5강: 유니티 기초 (Unity Fundamentals)

## 목차
1. [유니티란?](#유니티란)
2. [유니티 인터페이스](#유니티-인터페이스)
3. [GameObject와 Component](#gameobject와-component)
4. [Transform - 위치, 회전, 크기](#transform---위치-회전-크기)
5. [Prefab 시스템](#prefab-시스템)
6. [Scene 관리](#scene-관리)
7. [좌표계 이해하기](#좌표계-이해하기)
8. [Tag와 Layer](#tag와-layer)
9. [실전 프로젝트: 스타크래프트 유닛 배치](#실전-프로젝트-스타크래프트-유닛-배치)

---

## 유니티란?

유니티(Unity)는 **게임 엔진**입니다. 게임을 만들 때 필요한 물리, 렌더링, 사운드, 충돌 처리 등의 기능을 미리 만들어놓은 도구입니다.

### 왜 유니티를 사용하나?

**유니티 없이 게임을 만든다면:**
```csharp
// ❌ 이런 것들을 직접 구현해야 함
- 3D 모델을 화면에 그리기 (렌더링)
- 중력, 충돌 계산 (물리 엔진)
- 파일에서 이미지 불러오기
- 사운드 재생
- 입력 처리 (키보드, 마우스)
```

**유니티를 사용하면:**
```csharp
// ✅ 이미 다 만들어져 있음!
transform.position = new Vector3(0, 10, 0); // 위치 변경
GetComponent<Rigidbody>().AddForce(Vector3.up * 10); // 물리 힘 적용
audioSource.Play(); // 사운드 재생
```

### 유니티의 핵심 철학

> "Everything is a GameObject"

유니티에서는 **모든 것이 GameObject**입니다. 플레이어, 적, 카메라, 조명, UI 버튼... 모두 GameObject입니다.

---

## 유니티 인터페이스

유니티 에디터는 여러 창(Window)으로 구성되어 있습니다.

### 1. Hierarchy (계층 구조)
- **역할**: 현재 Scene에 있는 모든 GameObject의 목록
- **비유**: 스타크래프트의 유닛 목록
- **사용법**: GameObject를 드래그해서 부모-자식 관계 설정 가능

```
Scene: BattleField
├── Main Camera
├── Directional Light
├── Player
├── Enemies
│   ├── Marine_1
│   ├── Marine_2
│   └── Marine_3
└── Environment
    ├── Ground
    └── Buildings
```

### 2. Scene View (씬 뷰)
- **역할**: 게임 월드를 **개발자 시점**으로 보는 창
- **비유**: 맵 에디터
- **단축키**:
  - `F`: 선택한 GameObject로 포커스
  - `마우스 휠`: 줌 인/아웃
  - `우클릭 + WASD`: 카메라 이동 (FPS 스타일)
  - `Q`: 이동 도구
  - `W`: 위치 이동 도구
  - `E`: 회전 도구
  - `R`: 크기 조절 도구

### 3. Game View (게임 뷰)
- **역할**: 실제 **플레이어가 보는 화면**
- **비유**: 실제 게임 화면
- **중요**: Scene View와 Game View는 다릅니다!

```
Scene View = 개발자의 눈 (위에서 내려다보기 가능)
Game View = 플레이어의 눈 (카메라가 보는 것만)
```

### 4. Inspector (인스펙터)
- **역할**: 선택한 GameObject의 **모든 정보와 설정**
- **비유**: 유닛의 상태창
- **표시 내용**:
  - Transform (위치, 회전, 크기)
  - 붙어있는 모든 Component
  - Component의 속성들

### 5. Project (프로젝트)
- **역할**: 게임의 **모든 리소스 파일** 관리
- **비유**: 윈도우 탐색기
- **저장 내용**:
  - 스크립트 (`.cs`)
  - 이미지 (`.png`, `.jpg`)
  - 3D 모델 (`.fbx`)
  - 오디오 (`.mp3`, `.wav`)
  - Prefab (`.prefab`)
  - Scene (`.unity`)

### 6. Console (콘솔)
- **역할**: 에러, 경고, 로그 메시지 출력
- **사용법**:
```csharp
Debug.Log("일반 메시지");
Debug.LogWarning("경고 메시지");
Debug.LogError("에러 메시지");
```

---

## GameObject와 Component

### GameObject란?

GameObject는 **빈 컨테이너**입니다. 그 자체로는 아무것도 하지 않습니다.

```csharp
// GameObject는 이름과 Transform만 가지고 있음
GameObject emptyObject = new GameObject("빈 오브젝트");
// 아무 기능 없음. 그냥 월드에 존재할 뿐
```

### Component란?

Component는 GameObject에 **기능을 부여**하는 부품입니다.

```
GameObject = 자동차 프레임
Component = 엔진, 바퀴, 헤드라이트 등
```

### 주요 Component들

#### 1. Transform (필수 컴포넌트)
- **역할**: 위치, 회전, 크기 정보
- **특징**: 모든 GameObject가 반드시 가지고 있음
- **삭제 불가**: Transform은 절대 삭제할 수 없음

#### 2. MeshFilter + MeshRenderer
- **역할**: 3D 모델을 화면에 그림
- **MeshFilter**: 어떤 3D 모양인지 (큐브, 구, 커스텀 모델)
- **MeshRenderer**: 어떻게 그릴지 (색상, 재질)

```csharp
// 코드로 큐브 만들기
GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
// CreatePrimitive는 자동으로 MeshFilter + MeshRenderer를 추가함
```

#### 3. Rigidbody
- **역할**: 물리 효과 적용 (중력, 충돌)
- **없으면**: 공중에 떠있음, 충돌해도 통과

#### 4. Collider
- **역할**: 충돌 감지 영역
- **종류**: BoxCollider, SphereCollider, CapsuleCollider

#### 5. Camera
- **역할**: 플레이어가 볼 화면 결정
- **중요**: Scene에 최소 하나의 Camera 필요

#### 6. Light
- **역할**: 조명
- **종류**: Directional (태양), Point (전구), Spot (손전등)

### Component 패턴 (스타크래프트 예시)

```csharp
// 마린 GameObject 구성:
GameObject marine = new GameObject("Marine");

// 1. Transform (자동 추가, 위치 정보)
marine.transform.position = new Vector3(10, 0, 10);

// 2. 모델 표시 (MeshFilter + MeshRenderer)
marine.AddComponent<MeshFilter>();
marine.AddComponent<MeshRenderer>();

// 3. 물리 (Rigidbody)
Rigidbody rb = marine.AddComponent<Rigidbody>();
rb.mass = 1f;

// 4. 충돌 (Collider)
CapsuleCollider collider = marine.AddComponent<CapsuleCollider>();
collider.height = 2f;
collider.radius = 0.5f;

// 5. 커스텀 스크립트 (AI, 공격 등)
marine.AddComponent<MarineController>();
marine.AddComponent<MarineWeapon>();
marine.AddComponent<MarineHealth>();
```

**핵심**: GameObject는 여러 개의 Component를 조합해서 완성됩니다.

---

## Transform - 위치, 회전, 크기

Transform은 GameObject의 **공간상 정보**를 담고 있습니다.

### 1. Position (위치)

```csharp
// 절대 위치 (World Space)
transform.position = new Vector3(10, 5, 0);

// 로컬 위치 (부모 기준)
transform.localPosition = new Vector3(1, 0, 0);

// 위치 변경
transform.position += new Vector3(0, 1, 0); // 위로 1 이동
transform.Translate(Vector3.forward * 2); // 앞으로 2 이동
```

**Vector3 구조:**
```csharp
Vector3 position = new Vector3(x, y, z);
// x: 좌(-) / 우(+)
// y: 아래(-) / 위(+)
// z: 뒤(-) / 앞(+)
```

### 2. Rotation (회전)

```csharp
// Euler Angles (각도)
transform.rotation = Quaternion.Euler(0, 90, 0); // Y축 90도 회전
transform.eulerAngles = new Vector3(0, 90, 0); // 같은 의미

// 로컬 회전
transform.localRotation = Quaternion.Euler(0, 45, 0);

// 회전 추가
transform.Rotate(0, 90, 0); // Y축 기준 90도 회전
```

**주의: Quaternion을 직접 다루지 마세요!**
```csharp
// ✅ 좋은 예
transform.rotation = Quaternion.Euler(0, 90, 0);

// ❌ 나쁜 예
transform.rotation = new Quaternion(0, 0.7071f, 0, 0.7071f); // 복잡!
```

### 3. Scale (크기)

```csharp
// 크기 설정
transform.localScale = new Vector3(2, 2, 2); // 2배 확대
transform.localScale = new Vector3(1, 2, 1); // 세로만 2배
transform.localScale = Vector3.one; // (1, 1, 1)

// 주의: Scale은 localScale만 있음 (World Scale 없음)
```

### 스타크래프트 예시: 유닛 이동

```csharp
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Update()
    {
        // 마우스 우클릭으로 이동 명령
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                targetPosition = hit.point;
                isMoving = true;
                Debug.Log($"이동 명령: {targetPosition}");
            }
        }

        // 목표 지점으로 이동
        if (isMoving)
        {
            // 현재 위치에서 목표 위치로의 방향
            Vector3 direction = (targetPosition - transform.position).normalized;

            // 이동 (Time.deltaTime: 프레임 독립적)
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 목표에 도달했는지 확인
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < 0.1f)
            {
                isMoving = false;
                Debug.Log("목표 지점 도착!");
            }

            // 이동 방향으로 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
            }
        }
    }

    // Scene View에서 이동 경로 표시
    void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }
    }
}
```

### 부모-자식 관계 (Parent-Child)

```csharp
// 자식으로 설정
childObject.transform.SetParent(parentObject.transform);

// 부모에서 분리
childObject.transform.SetParent(null);

// 부모 변경 (World 위치 유지)
childObject.transform.SetParent(newParent.transform, true);

// 부모 변경 (Local 위치 유지)
childObject.transform.SetParent(newParent.transform, false);
```

**부모-자식 관계의 효과:**
```
Parent (탱크)
├── Body (본체)
├── Turret (포탑)
└── Wheels (바퀴들)

Parent가 이동하면 → 모든 자식도 같이 이동
Turret만 회전하면 → Body와 Wheels는 영향 없음
```

---

## Prefab 시스템

Prefab은 **GameObject의 템플릿**입니다. 붕어빵 틀처럼 같은 것을 여러 개 만들 수 있습니다.

### Prefab이 왜 필요한가?

**Prefab 없이:**
```
마린 100개를 배치했는데 HP를 50으로 수정해야 한다면?
→ 100개를 일일이 수정해야 함 😱
```

**Prefab 사용:**
```
마린 Prefab 하나만 수정하면
→ 모든 마린이 자동으로 업데이트됨! 😊
```

### Prefab 만들기

1. **Hierarchy에서 GameObject 준비**
```
Marine
├── Model (MeshFilter + MeshRenderer)
├── Collider (CapsuleCollider)
├── Rigidbody
└── MarineController (Script)
```

2. **Project 창으로 드래그**
   - Hierarchy의 Marine을 Project 창의 Prefabs 폴더로 드래그
   - `Marine.prefab` 파일 생성됨

3. **Prefab 인스턴스 생성**
```csharp
public GameObject marinePrefab; // Inspector에서 할당

void SpawnMarine(Vector3 position)
{
    GameObject marine = Instantiate(marinePrefab, position, Quaternion.identity);
    marine.name = "Marine_" + Random.Range(0, 1000);
}
```

### Prefab의 종류

#### 1. 일반 Prefab
```csharp
// Prefab 인스턴스 생성
GameObject instance = Instantiate(prefab);
```

#### 2. Prefab Variant (변형)
```
Marine (원본)
├── Marine_Elite (변형 1: HP 2배)
└── Marine_Medic (변형 2: 회복 능력 추가)
```

### Prefab 수정

**방법 1: Prefab Mode로 들어가기**
- Project 창에서 Prefab 더블클릭
- 독립된 환경에서 수정
- 변경사항이 모든 인스턴스에 적용됨

**방법 2: Override**
- Scene의 인스턴스를 수정
- Inspector 상단의 `Overrides` 버튼 클릭
- `Apply All` 또는 `Revert All`

### 스타크래프트 예시: 유닛 생산 시스템

```csharp
using UnityEngine;
using System.Collections.Generic;

public class Barracks : MonoBehaviour
{
    [Header("Prefab 설정")]
    public GameObject marinePrefab;
    public GameObject firebatPrefab;
    public GameObject medicPrefab;

    [Header("생산 설정")]
    public Transform spawnPoint; // 유닛이 생성될 위치
    public Transform rallyPoint;  // 집결 지점

    private Queue<GameObject> productionQueue = new Queue<GameObject>();
    private bool isProducing = false;

    void Update()
    {
        // 테스트: 키보드 입력으로 생산
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QueueUnit(marinePrefab);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QueueUnit(firebatPrefab);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            QueueUnit(medicPrefab);
        }

        // 생산 처리
        if (!isProducing && productionQueue.Count > 0)
        {
            StartCoroutine(ProduceUnit());
        }
    }

    // 생산 대기열에 추가
    public void QueueUnit(GameObject unitPrefab)
    {
        productionQueue.Enqueue(unitPrefab);
        Debug.Log($"{unitPrefab.name} 생산 대기열에 추가됨. (대기: {productionQueue.Count})");
    }

    // 유닛 생산 코루틴
    System.Collections.IEnumerator ProduceUnit()
    {
        isProducing = true;

        GameObject unitPrefab = productionQueue.Dequeue();
        Debug.Log($"{unitPrefab.name} 생산 중...");

        // 생산 시간 대기 (마린: 3초)
        yield return new WaitForSeconds(3f);

        // 유닛 생성
        GameObject unit = Instantiate(unitPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"{unit.name} 생산 완료!");

        // 집결 지점으로 이동 명령
        UnitMovement movement = unit.GetComponent<UnitMovement>();
        if (movement != null && rallyPoint != null)
        {
            movement.MoveToPosition(rallyPoint.position);
        }

        isProducing = false;
    }

    // 생산 대기열 시각화
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

## Scene 관리

Scene은 게임의 **하나의 레벨이나 화면**입니다.

### Scene의 예시
```
게임 전체
├── MainMenu.unity (메인 메뉴)
├── Tutorial.unity (튜토리얼)
├── Level1.unity (1스테이지)
├── Level2.unity (2스테이지)
└── GameOver.unity (게임 오버)
```

### Scene 생성

1. **에디터에서**:
   - `File > New Scene`
   - `Ctrl + N`

2. **저장**:
   - `File > Save Scene`
   - `Ctrl + S`

### Scene 전환 (코드)

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 이름으로 Scene 로드
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    // 인덱스로 Scene 로드
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // 다음 Scene 로드
    public void LoadNextScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene + 1);
    }

    // Scene 재시작
    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // 비동기 로딩 (로딩 화면 표시 가능)
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }

    System.Collections.IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = operation.progress / 0.9f; // 0 ~ 1
            Debug.Log($"로딩 진행도: {progress * 100}%");
            yield return null;
        }
    }
}
```

### Build Settings에 Scene 추가

Scene을 전환하려면 **Build Settings**에 등록해야 합니다.

1. `File > Build Settings` (`Ctrl + Shift + B`)
2. Scene을 드래그해서 "Scenes In Build"에 추가
3. 순서를 변경하면 인덱스가 바뀜 (0부터 시작)

### 스타크래프트 예시: 미션 선택

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionSelector : MonoBehaviour
{
    [System.Serializable]
    public class Mission
    {
        public string missionName;
        public string sceneName;
        public string description;
        public bool isUnlocked = true;
    }

    public Mission[] missions;

    void Start()
    {
        // 미션 목록 출력
        Debug.Log("=== 사용 가능한 미션 ===");
        for (int i = 0; i < missions.Length; i++)
        {
            if (missions[i].isUnlocked)
            {
                Debug.Log($"{i + 1}. {missions[i].missionName} - {missions[i].description}");
            }
            else
            {
                Debug.Log($"{i + 1}. ??? (잠금)");
            }
        }
    }

    public void StartMission(int missionIndex)
    {
        if (missionIndex < 0 || missionIndex >= missions.Length)
        {
            Debug.LogError("잘못된 미션 인덱스!");
            return;
        }

        Mission mission = missions[missionIndex];

        if (!mission.isUnlocked)
        {
            Debug.LogWarning("이 미션은 아직 잠겨있습니다!");
            return;
        }

        Debug.Log($"미션 시작: {mission.missionName}");
        SceneManager.LoadScene(mission.sceneName);
    }

    // 미션 완료 시 다음 미션 해금
    public static void UnlockNextMission()
    {
        int currentMission = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt($"Mission_{currentMission + 1}_Unlocked", 1);
        PlayerPrefs.Save();
    }
}
```

---

## 좌표계 이해하기

유니티는 **왼손 좌표계**를 사용합니다.

### World Space vs Local Space

```csharp
// World Space (절대 좌표)
transform.position = new Vector3(10, 0, 5);
// 월드의 원점(0, 0, 0)에서 (10, 0, 5) 위치

// Local Space (상대 좌표)
transform.localPosition = new Vector3(1, 0, 0);
// 부모로부터 오른쪽으로 1m
```

**예시: 탱크와 포탑**
```csharp
// 탱크 (부모)
tankBody.position = new Vector3(10, 0, 10);

// 포탑 (자식)
turret.localPosition = new Vector3(0, 1, 0); // 탱크 위 1m
turret.position = new Vector3(10, 1, 10); // 실제 월드 좌표

// 탱크가 이동하면
tankBody.position = new Vector3(20, 0, 10);
// 포탑도 자동으로 이동
turret.position = new Vector3(20, 1, 10);
```

### 방향 벡터

```csharp
// 월드 기준 방향
Vector3.forward  // (0, 0, 1)  - 전방 (Z+)
Vector3.back     // (0, 0, -1) - 후방 (Z-)
Vector3.up       // (0, 1, 0)  - 위 (Y+)
Vector3.down     // (0, -1, 0) - 아래 (Y-)
Vector3.right    // (1, 0, 0)  - 오른쪽 (X+)
Vector3.left     // (-1, 0, 0) - 왼쪽 (X-)

// 오브젝트 기준 방향 (로컬)
transform.forward  // 오브젝트가 바라보는 방향
transform.up       // 오브젝트의 위쪽
transform.right    // 오브젝트의 오른쪽
```

### 벡터 연산

```csharp
Vector3 a = new Vector3(1, 0, 0);
Vector3 b = new Vector3(0, 1, 0);

// 덧셈
Vector3 sum = a + b; // (1, 1, 0)

// 뺄셈 (방향 계산에 자주 사용)
Vector3 direction = targetPosition - currentPosition;

// 크기 (거리)
float distance = direction.magnitude;
Debug.Log($"거리: {distance}m");

// 정규화 (방향만 남기고 크기는 1)
Vector3 normalized = direction.normalized;

// 스칼라 곱셈
Vector3 doubled = a * 2; // (2, 0, 0)

// 내적 (Dot Product) - 방향이 얼마나 비슷한지
float dot = Vector3.Dot(a, b);
// 1: 같은 방향
// 0: 수직
// -1: 반대 방향

// 외적 (Cross Product) - 수직인 벡터
Vector3 cross = Vector3.Cross(a, b); // (0, 0, 1)
```

### 실전 예시: 적을 향해 회전

```csharp
public class LookAtEnemy : MonoBehaviour
{
    public Transform target; // 적의 Transform

    void Update()
    {
        if (target == null) return;

        // 적을 향하는 방향 계산
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // Y축 회전만 (좌우만, 위아래 X)

        // 방향이 0이 아니면 회전
        if (direction != Vector3.zero)
        {
            // 방향을 바라보는 회전값 생성
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f // 회전 속도
            );
        }
    }
}
```

---

## Tag와 Layer

### Tag (태그)

Tag는 GameObject를 **분류하는 라벨**입니다.

**기본 Tag:**
- `Untagged`: 기본값
- `Player`: 플레이어
- `MainCamera`: 메인 카메라
- `Enemy`: 적 (커스텀 추가 필요)

**Tag 사용 예시:**
```csharp
// Tag로 찾기
GameObject player = GameObject.FindGameObjectWithTag("Player");

// Tag 확인
if (other.gameObject.tag == "Enemy")
{
    Debug.Log("적과 충돌!");
}

// Tag 비교 (더 빠름)
if (other.gameObject.CompareTag("Enemy"))
{
    Debug.Log("적과 충돌!");
}

// 여러 개 찾기
GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
Debug.Log($"적의 수: {enemies.Length}");
```

**Tag 추가 방법:**
1. Inspector에서 Tag 드롭다운 클릭
2. `Add Tag...` 클릭
3. `+` 버튼으로 새 Tag 추가

### Layer (레이어)

Layer는 GameObject를 **그룹으로 묶어서 처리**할 때 사용합니다.

**Layer 용도:**
1. **충돌 필터링**: 어떤 레이어끼리 충돌할지 설정
2. **카메라 렌더링**: 특정 레이어만 보이게
3. **Raycast 필터링**: 특정 레이어만 감지

**기본 Layer:**
- `Default`: 기본값
- `UI`: UI 요소
- `Water`: 물
- `Player`: 플레이어 (커스텀)
- `Enemy`: 적 (커스텀)
- `Ground`: 지형 (커스텀)

**Layer 설정:**
```csharp
// Layer 설정
gameObject.layer = LayerMask.NameToLayer("Enemy");

// Layer 확인
if (gameObject.layer == LayerMask.NameToLayer("Player"))
{
    Debug.Log("플레이어 레이어");
}
```

**LayerMask 사용 (Raycast 필터링):**
```csharp
// Enemy 레이어만 감지
public LayerMask enemyLayer;

void Update()
{
    Ray ray = new Ray(transform.position, transform.forward);
    RaycastHit hit;

    // enemyLayer에 속한 오브젝트만 감지
    if (Physics.Raycast(ray, out hit, 100f, enemyLayer))
    {
        Debug.Log($"적 감지: {hit.collider.name}");
    }
}
```

**Layer Collision Matrix (충돌 설정):**
```
Edit > Project Settings > Physics

        Player  Enemy   Ground  Bullet
Player    O      O       O       X
Enemy     O      X       O       O
Ground    O      O       X       O
Bullet    X      O       O       X

O: 충돌함
X: 충돌 안 함
```

### 스타크래프트 예시: 아군/적군 구분

```csharp
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public enum Team
    {
        Player,
        Enemy,
        Neutral
    }

    public Team team = Team.Player;

    void Start()
    {
        // Team에 따라 Tag와 Layer 자동 설정
        switch (team)
        {
            case Team.Player:
                gameObject.tag = "PlayerUnit";
                gameObject.layer = LayerMask.NameToLayer("PlayerUnits");
                break;

            case Team.Enemy:
                gameObject.tag = "EnemyUnit";
                gameObject.layer = LayerMask.NameToLayer("EnemyUnits");
                break;

            case Team.Neutral:
                gameObject.tag = "NeutralUnit";
                gameObject.layer = LayerMask.NameToLayer("Neutral");
                break;
        }

        Debug.Log($"{gameObject.name} - 팀: {team}, 태그: {gameObject.tag}, 레이어: {gameObject.layer}");
    }

    // 다른 유닛이 적인지 확인
    public bool IsEnemy(GameObject other)
    {
        UnitManager otherUnit = other.GetComponent<UnitManager>();
        if (otherUnit == null) return false;

        return team != otherUnit.team && team != Team.Neutral && otherUnit.team != Team.Neutral;
    }

    // 충돌 처리
    void OnTriggerEnter(Collider other)
    {
        if (IsEnemy(other.gameObject))
        {
            Debug.Log($"{gameObject.name}이(가) 적 {other.name}과(와) 조우!");
            // 공격 처리 등...
        }
    }
}
```

---

## 실전 프로젝트: 스타크래프트 유닛 배치

모든 내용을 종합한 실전 프로젝트입니다.

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스타크래프트 스타일 유닛 배치 및 관리 시스템
/// </summary>
public class UnitSpawner : MonoBehaviour
{
    [Header("Prefab 설정")]
    public GameObject marinePrefab;
    public GameObject firebatPrefab;
    public GameObject medicPrefab;

    [Header("배치 설정")]
    public Transform spawnAreaCenter; // 배치 중심점
    public float spawnRadius = 5f;    // 배치 반경
    public int unitsPerRow = 5;       // 줄당 유닛 수

    [Header("그리드 설정")]
    public float unitSpacing = 2f;    // 유닛 간 간격

    private List<GameObject> spawnedUnits = new List<GameObject>();

    void Update()
    {
        // 테스트 키 입력
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnUnitFormation(marinePrefab, 10);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnUnitFormation(firebatPrefab, 5);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnUnitFormation(medicPrefab, 3);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllUnits();
        }
    }

    /// <summary>
    /// 유닛을 대형으로 배치
    /// </summary>
    public void SpawnUnitFormation(GameObject unitPrefab, int count)
    {
        if (unitPrefab == null)
        {
            Debug.LogError("유닛 Prefab이 할당되지 않았습니다!");
            return;
        }

        Vector3 centerPos = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;

        int rows = Mathf.CeilToInt((float)count / unitsPerRow);
        int spawned = 0;

        for (int row = 0; row < rows; row++)
        {
            int unitsInThisRow = Mathf.Min(unitsPerRow, count - spawned);

            for (int col = 0; col < unitsInThisRow; col++)
            {
                // 그리드 위치 계산
                float xOffset = (col - (unitsInThisRow - 1) / 2f) * unitSpacing;
                float zOffset = (row - (rows - 1) / 2f) * unitSpacing;

                Vector3 spawnPos = centerPos + new Vector3(xOffset, 0, zOffset);

                // 지형 높이에 맞추기 (Raycast)
                Ray ray = new Ray(spawnPos + Vector3.up * 10f, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 20f))
                {
                    spawnPos = hit.point;
                }

                // 유닛 생성
                GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
                unit.name = $"{unitPrefab.name}_{spawned + 1}";

                // 생성된 유닛 목록에 추가
                spawnedUnits.Add(unit);

                spawned++;
            }
        }

        Debug.Log($"{unitPrefab.name} {count}개 배치 완료!");
    }

    /// <summary>
    /// 원형으로 배치
    /// </summary>
    public void SpawnUnitCircle(GameObject unitPrefab, int count, float radius)
    {
        Vector3 centerPos = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;

        for (int i = 0; i < count; i++)
        {
            // 원 둘레를 따라 각도 계산
            float angle = i * Mathf.PI * 2f / count;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 spawnPos = centerPos + new Vector3(x, 0, z);

            // 유닛 생성
            GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            unit.name = $"{unitPrefab.name}_Circle_{i + 1}";

            // 중심을 바라보도록 회전
            Vector3 lookDir = centerPos - spawnPos;
            lookDir.y = 0;
            unit.transform.rotation = Quaternion.LookRotation(lookDir);

            spawnedUnits.Add(unit);
        }

        Debug.Log($"{unitPrefab.name} {count}개 원형 배치 완료!");
    }

    /// <summary>
    /// 모든 유닛 제거
    /// </summary>
    public void ClearAllUnits()
    {
        foreach (GameObject unit in spawnedUnits)
        {
            if (unit != null)
            {
                Destroy(unit);
            }
        }

        spawnedUnits.Clear();
        Debug.Log("모든 유닛 제거 완료!");
    }

    /// <summary>
    /// 특정 Tag의 유닛만 제거
    /// </summary>
    public void ClearUnitsByTag(string tag)
    {
        spawnedUnits.RemoveAll(unit =>
        {
            if (unit != null && unit.CompareTag(tag))
            {
                Destroy(unit);
                return true;
            }
            return false;
        });

        Debug.Log($"Tag '{tag}' 유닛 제거 완료!");
    }

    /// <summary>
    /// 생성된 모든 유닛 선택
    /// </summary>
    public List<GameObject> GetAllUnits()
    {
        // null 제거 (파괴된 유닛)
        spawnedUnits.RemoveAll(unit => unit == null);
        return spawnedUnits;
    }

    /// <summary>
    /// Scene View에서 배치 영역 표시
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Vector3 center = spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;

        // 배치 반경
        Gizmos.color = Color.yellow;
        DrawCircle(center, spawnRadius, 32);

        // 그리드 표시
        Gizmos.color = Color.cyan;
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < unitsPerRow; col++)
            {
                float xOffset = (col - (unitsPerRow - 1) / 2f) * unitSpacing;
                float zOffset = (row - 2f) * unitSpacing;
                Vector3 pos = center + new Vector3(xOffset, 0.5f, zOffset);

                Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
            }
        }
    }

    // 원 그리기 헬퍼 함수
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
```

### 사용 방법

1. **빈 GameObject 생성**:
   - Hierarchy에서 우클릭 > `Create Empty`
   - 이름: `UnitSpawner`

2. **스크립트 연결**:
   - `UnitSpawner` 스크립트를 GameObject에 추가

3. **Prefab 할당**:
   - Inspector에서 Marine, Firebat, Medic Prefab 할당

4. **배치 중심점 설정**:
   - 빈 GameObject 생성 (이름: `SpawnCenter`)
   - `Spawn Area Center`에 할당

5. **테스트**:
   - Play 모드 실행
   - `M`: 마린 10개 배치
   - `F`: 파이어뱃 5개 배치
   - `H`: 메딕 3개 배치
   - `C`: 모든 유닛 제거

---

## 정리 및 다음 단계

### 이번 강의에서 배운 것

✅ 유니티 인터페이스 (Hierarchy, Scene, Inspector, Project)
✅ GameObject와 Component 패턴
✅ Transform (위치, 회전, 크기)
✅ Prefab 시스템 (템플릿, 재사용)
✅ Scene 관리 및 전환
✅ 좌표계와 벡터 연산
✅ Tag와 Layer (분류 및 필터링)
✅ 실전: 유닛 배치 시스템

### 다음 강의 예고: 6강 - 유니티 스크립팅 기초

다음 강의에서는 유니티의 핵심인 **MonoBehaviour 생명주기**를 배웁니다:
- Awake, Start, Update, FixedUpdate, LateUpdate
- GetComponent와 컴포넌트 통신
- Input 시스템
- Coroutine (코루틴)
- 실전: 유닛 이동 및 공격 시스템

### 실습 과제

1. **Prefab 만들기**: 마린, 파이어뱃, 메딕 3가지 유닛 Prefab 제작
2. **배치 시스템**: UnitSpawner 스크립트를 활용해서 유닛 배치
3. **Tag 설정**: Player, Enemy Tag 생성 및 적용
4. **Scene 제작**: MainMenu와 GamePlay Scene 2개 만들고 전환 구현

다음 강의에서 만나요! 🎮
