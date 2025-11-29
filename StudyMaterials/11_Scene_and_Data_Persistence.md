# 11강: 씬 관리와 데이터 영속성 (Scene Management & Data Persistence)

## 목차
1. [씬 관리 개요](#씬-관리-개요)
2. [SceneManager](#scenemanager)
3. [비동기 씬 로딩](#비동기-씬-로딩)
4. [DontDestroyOnLoad](#dontdestroyonload)
5. [데이터 저장 방법](#데이터-저장-방법)
6. [PlayerPrefs](#playerprefs)
7. [JSON 직렬화](#json-직렬화)
8. [실전 프로젝트: 세이브 시스템](#실전-프로젝트-세이브-시스템)

---

## 씬 관리 개요

**씬(Scene)**은 게임의 레벨이나 화면입니다.

### 씬 구성 예시

```
게임 씬 구조:
- MainMenu (메인 메뉴)
- LoadingScene (로딩 화면)
- GameScene (게임 플레이)
- ResultScene (결과 화면)
```

---

## SceneManager

**SceneManager**는 씬 전환을 관리합니다.

### 기본 씬 전환

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerBasics : MonoBehaviour
{
    void Update()
    {
        // 1. 씬 이름으로 로드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("GameScene");
        }

        // 2. 씬 인덱스로 로드
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(1);
        }

        // 3. 현재 씬 다시 로드
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }

        // 4. 다음 씬 로드
        if (Input.GetKeyDown(KeyCode.N))
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
        }
    }
}
```

**씬 Build Settings 추가:**
```
File > Build Settings
Add Open Scenes 버튼 클릭
```

### 씬 이벤트

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEvents : MonoBehaviour
{
    void OnEnable()
    {
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        // 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[Scene] 로드 완료: {scene.name}");
        Debug.Log($"[Scene] 로드 모드: {mode}");
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[Scene] 언로드: {scene.name}");
    }
}
```

### Multi-Scene 로딩

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneLoading : MonoBehaviour
{
    void Start()
    {
        // Additive 모드 (씬 추가)
        SceneManager.LoadScene("UI_Scene", LoadSceneMode.Additive);
        SceneManager.LoadScene("Audio_Scene", LoadSceneMode.Additive);

        // 씬 언로드
        SceneManager.UnloadSceneAsync("UI_Scene");
    }

    void GetSceneInfo()
    {
        // 현재 로드된 씬 개수
        int sceneCount = SceneManager.sceneCount;
        Debug.Log($"로드된 씬: {sceneCount}개");

        // 모든 씬 순회
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Debug.Log($"씬 {i}: {scene.name}, 로드됨: {scene.isLoaded}");
        }

        // 활성 씬 (기본 씬)
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log($"활성 씬: {activeScene.name}");
    }

    void SetActiveScene()
    {
        Scene scene = SceneManager.GetSceneByName("GameScene");
        if (scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
        }
    }
}
```

---

## 비동기 씬 로딩

**AsyncOperation**으로 씬을 비동기로 로드합니다.

### 비동기 로딩 기본

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AsyncSceneLoading : MonoBehaviour
{
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Debug.Log($"[Loading] 씬 로딩 시작: {sceneName}");

        // 비동기 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 자동 활성화 끄기 (로딩 완료 후 수동 활성화)
        asyncLoad.allowSceneActivation = false;

        // 로딩 중
        while (!asyncLoad.isDone)
        {
            // 진행도 (0~0.9)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            Debug.Log($"[Loading] 진행도: {progress * 100}%");

            // 0.9에 도달하면 로딩 완료
            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("[Loading] 로딩 완료! 스페이스바를 눌러 시작하세요.");

                // 스페이스바를 기다림
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                // 씬 활성화
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log($"[Loading] 씬 전환 완료: {sceneName}");
    }
}
```

### 스타크래프트 예시: 로딩 화면

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// 로딩 화면 컨트롤러
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;

    [Header("Settings")]
    public string targetSceneName = "GameScene";
    public float minimumLoadTime = 2f; // 최소 로딩 시간

    private string[] loadingTips = new string[]
    {
        "팁: 마린은 스팀팩으로 공격 속도를 높일 수 있습니다.",
        "팁: SCV로 미네랄을 채굴하세요.",
        "팁: 서플라이 디폿을 지어 인구수를 늘리세요.",
        "팁: 시즈 탱크는 시즈 모드에서 긴 사거리를 가집니다.",
    };

    void Start()
    {
        StartCoroutine(LoadSceneWithProgress());
    }

    IEnumerator LoadSceneWithProgress()
    {
        // 랜덤 팁 표시
        tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        // 비동기 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        while (!asyncLoad.isDone)
        {
            // 진행도 (0~1)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // UI 업데이트
            progressBar.value = progress;
            progressText.text = $"로딩 중... {progress * 100:F0}%";

            // 최소 로딩 시간 체크
            float elapsedTime = Time.time - startTime;
            bool minimumTimePassed = elapsedTime >= minimumLoadTime;

            // 로딩 완료 & 최소 시간 경과
            if (asyncLoad.progress >= 0.9f && minimumTimePassed)
            {
                progressBar.value = 1f;
                progressText.text = "완료!";

                // 1초 대기 후 전환
                yield return new WaitForSeconds(1f);

                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
```

---

## DontDestroyOnLoad

**DontDestroyOnLoad**는 씬 전환 시 오브젝트를 유지합니다.

```csharp
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    public static PersistentObject Instance { get; private set; }

    void Awake()
    {
        // 싱글턴 + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[Persistent] 오브젝트 유지됨");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("[Persistent] 중복 오브젝트 제거됨");
        }
    }
}
```

**사용 예시:**
- GameManager
- SoundManager
- NetworkManager
- DataManager

---

## 데이터 저장 방법

유니티에서 데이터를 저장하는 3가지 방법입니다.

### 저장 방법 비교

| 방법 | 용도 | 크기 | 보안 |
|------|------|------|------|
| **PlayerPrefs** | 간단한 설정 | 작음 | 낮음 |
| **JSON** | 복잡한 데이터 | 중간 | 중간 |
| **Binary** | 대용량/보안 | 큼 | 높음 |

---

## PlayerPrefs

**PlayerPrefs**는 간단한 키-값 저장소입니다.

```csharp
using UnityEngine;

public class PlayerPrefsExample : MonoBehaviour
{
    void Start()
    {
        // === 저장 ===

        // Int
        PlayerPrefs.SetInt("Level", 5);

        // Float
        PlayerPrefs.SetFloat("Volume", 0.8f);

        // String
        PlayerPrefs.SetString("PlayerName", "Marine");

        // 저장 완료 (자동으로도 됨)
        PlayerPrefs.Save();

        // === 불러오기 ===

        // Int (기본값: 0)
        int level = PlayerPrefs.GetInt("Level", 0);

        // Float (기본값: 1.0f)
        float volume = PlayerPrefs.GetFloat("Volume", 1.0f);

        // String (기본값: "")
        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");

        Debug.Log($"Level: {level}, Volume: {volume}, Name: {playerName}");

        // === 확인 ===

        bool hasKey = PlayerPrefs.HasKey("Level");
        Debug.Log($"Level 키 존재: {hasKey}");

        // === 삭제 ===

        PlayerPrefs.DeleteKey("Level");
        PlayerPrefs.DeleteAll(); // 모든 데이터 삭제
    }
}
```

### 스타크래프트 예시: 게임 설정 저장

```csharp
using UnityEngine;

/// <summary>
/// 게임 설정 저장/불러오기
/// </summary>
public class GameSettings : MonoBehaviour
{
    // 설정 값
    public float masterVolume = 1.0f;
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.8f;
    public int graphicsQuality = 2; // 0=Low, 1=Medium, 2=High
    public bool fullscreen = true;

    void Start()
    {
        LoadSettings();
        ApplySettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("GraphicsQuality", graphicsQuality);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[Settings] 설정 저장 완료");
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        Debug.Log("[Settings] 설정 불러오기 완료");
    }

    void ApplySettings()
    {
        // 볼륨 적용
        AudioListener.volume = masterVolume;

        // 그래픽 품질
        QualitySettings.SetQualityLevel(graphicsQuality);

        // 전체화면
        Screen.fullScreen = fullscreen;

        Debug.Log("[Settings] 설정 적용 완료");
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();

        masterVolume = 1.0f;
        bgmVolume = 0.5f;
        sfxVolume = 0.8f;
        graphicsQuality = 2;
        fullscreen = true;

        ApplySettings();

        Debug.Log("[Settings] 설정 초기화");
    }
}
```

---

## JSON 직렬화

**JSON**으로 복잡한 데이터를 저장합니다.

```csharp
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public string playerName;
    public int level;
    public int experience;
    public int minerals;
    public int gas;
    public Vector3 position;
}

public class JsonSaveSystem : MonoBehaviour
{
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        Debug.Log($"[Save] 저장 경로: {saveFilePath}");
    }

    public void SaveGame(SaveData data)
    {
        // JSON으로 변환
        string json = JsonUtility.ToJson(data, true); // true = Pretty Print

        // 파일에 저장
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"[Save] 저장 완료:\n{json}");
    }

    public SaveData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("[Save] 저장 파일 없음");
            return null;
        }

        // 파일 읽기
        string json = File.ReadAllText(saveFilePath);

        // JSON에서 변환
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"[Save] 불러오기 완료:\n{json}");
        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("[Save] 저장 파일 삭제");
        }
    }

    // 테스트
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveData data = new SaveData
            {
                playerName = "Commander",
                level = 10,
                experience = 5000,
                minerals = 500,
                gas = 200,
                position = transform.position
            };
            SaveGame(data);
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveData data = LoadGame();
            if (data != null)
            {
                Debug.Log($"플레이어: {data.playerName}, 레벨: {data.level}");
                transform.position = data.position;
            }
        }
    }
}
```

---

## 실전 프로젝트: 세이브 시스템

완전한 게임 세이브/로드 시스템입니다.

```csharp
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 게임 데이터 구조
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public string playerName = "Commander";
    public int currentLevel = 1;
    public int totalPlayTime = 0; // 초

    // 자원
    public int minerals = 50;
    public int gas = 0;
    public int supply = 10;

    // 플레이어 위치
    public SerializableVector3 playerPosition;

    // 유닛 정보
    public List<UnitSaveData> units = new List<UnitSaveData>();

    // 저장 시간
    public string saveTime;
}

[System.Serializable]
public class UnitSaveData
{
    public string unitType;
    public SerializableVector3 position;
    public int currentHP;
}

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

/// <summary>
/// 완전한 세이브/로드 매니저
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Save Settings")]
    public int maxSaveSlots = 3;
    private string saveDirectory;

    [Header("Current Game Data")]
    public GameSaveData currentGameData;

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
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");

        // 디렉토리 생성
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        Debug.Log($"[SaveLoad] 초기화 완료");
        Debug.Log($"[SaveLoad] 저장 경로: {saveDirectory}");

        // 새 게임 데이터
        currentGameData = new GameSaveData();
    }

    // === 저장 ===

    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveLoad] 잘못된 슬롯: {slotIndex}");
            return;
        }

        // 현재 게임 상태 수집
        CollectGameData();

        // 저장 시간 기록
        currentGameData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // JSON 변환
        string json = JsonUtility.ToJson(currentGameData, true);

        // 파일 저장
        string filePath = GetSaveFilePath(slotIndex);
        File.WriteAllText(filePath, json);

        Debug.Log($"[SaveLoad] 슬롯 {slotIndex}에 저장 완료");
        Debug.Log($"[SaveLoad] 파일: {filePath}");
    }

    void CollectGameData()
    {
        // 플레이어 위치
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentGameData.playerPosition = new SerializableVector3(player.transform.position);
        }

        // 모든 유닛 수집
        currentGameData.units.Clear();
        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
        foreach (GameObject unit in units)
        {
            UnitSaveData unitData = new UnitSaveData
            {
                unitType = unit.name,
                position = new SerializableVector3(unit.transform.position),
                currentHP = 100 // 실제 HP 컴포넌트에서 가져와야 함
            };
            currentGameData.units.Add(unitData);
        }

        Debug.Log($"[SaveLoad] 데이터 수집 완료 (유닛 {currentGameData.units.Count}개)");
    }

    // === 불러오기 ===

    public bool LoadGame(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            Debug.LogError($"[SaveLoad] 잘못된 슬롯: {slotIndex}");
            return false;
        }

        string filePath = GetSaveFilePath(slotIndex);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveLoad] 슬롯 {slotIndex}에 저장 파일 없음");
            return false;
        }

        // 파일 읽기
        string json = File.ReadAllText(filePath);

        // JSON 파싱
        currentGameData = JsonUtility.FromJson<GameSaveData>(json);

        Debug.Log($"[SaveLoad] 슬롯 {slotIndex}에서 불러오기 완료");
        Debug.Log($"[SaveLoad] 플레이어: {currentGameData.playerName}");
        Debug.Log($"[SaveLoad] 레벨: {currentGameData.currentLevel}");
        Debug.Log($"[SaveLoad] 저장 시간: {currentGameData.saveTime}");

        // 게임 상태 적용
        ApplyGameData();

        return true;
    }

    void ApplyGameData()
    {
        // 플레이어 위치 복원
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && currentGameData.playerPosition.x != 0)
        {
            player.transform.position = currentGameData.playerPosition.ToVector3();
        }

        // 유닛 생성
        // (실제로는 Prefab에서 Instantiate해야 함)
        Debug.Log($"[SaveLoad] 유닛 {currentGameData.units.Count}개 복원 필요");

        // 자원 복원
        // ResourceManager.Instance.SetResources(...)
    }

    // === 삭제 ===

    public void DeleteSave(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[SaveLoad] 슬롯 {slotIndex} 삭제 완료");
        }
    }

    // === 슬롯 정보 ===

    public bool HasSave(int slotIndex)
    {
        string filePath = GetSaveFilePath(slotIndex);
        return File.Exists(filePath);
    }

    public GameSaveData GetSaveInfo(int slotIndex)
    {
        if (!HasSave(slotIndex)) return null;

        string filePath = GetSaveFilePath(slotIndex);
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(saveDirectory, $"save_slot_{slotIndex}.json");
    }

    // === 테스트 ===

    void Update()
    {
        // F5: 슬롯 0에 저장
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame(0);
        }

        // F9: 슬롯 0에서 불러오기
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame(0);
        }

        // F12: 모든 슬롯 정보 출력
        if (Input.GetKeyDown(KeyCode.F12))
        {
            for (int i = 0; i < maxSaveSlots; i++)
            {
                if (HasSave(i))
                {
                    GameSaveData info = GetSaveInfo(i);
                    Debug.Log($"슬롯 {i}: {info.playerName}, Lv.{info.currentLevel}, {info.saveTime}");
                }
                else
                {
                    Debug.Log($"슬롯 {i}: 빈 슬롯");
                }
            }
        }
    }
}
```

---

## 정리

### 이번 강의 핵심

✅ **SceneManager** - LoadScene, LoadSceneAsync
✅ **비동기 로딩** - AsyncOperation, allowSceneActivation
✅ **DontDestroyOnLoad** - 씬 전환 시 오브젝트 유지
✅ **PlayerPrefs** - 간단한 설정 저장
✅ **JSON 직렬화** - 복잡한 데이터 저장
✅ **세이브 시스템** - 완전한 게임 저장/불러오기

### 핵심 코드

```csharp
// 씬 전환
SceneManager.LoadScene("GameScene");

// 비동기 로딩
AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

// 오브젝트 유지
DontDestroyOnLoad(gameObject);

// PlayerPrefs
PlayerPrefs.SetInt("Level", 5);
int level = PlayerPrefs.GetInt("Level", 0);

// JSON 저장
string json = JsonUtility.ToJson(data, true);
File.WriteAllText(path, json);

// JSON 불러오기
string json = File.ReadAllText(path);
SaveData data = JsonUtility.FromJson<SaveData>(json);
```

---

## 다음 강의 예고: 12강 - 실전 프로젝트

마지막 강의에서는 모든 내용을 통합한 미니 게임을 만듭니다! 🎮
