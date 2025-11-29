# 10강: 오디오 시스템 (Audio System)

## 목차
1. [오디오 시스템 개요](#오디오-시스템-개요)
2. [AudioSource](#audiosource)
3. [AudioListener](#audiolistener)
4. [AudioClip](#audioclip)
5. [3D Sound](#3d-sound)
6. [AudioMixer](#audiomixer)
7. [오디오 풀링](#오디오-풀링)
8. [실전 프로젝트: 사운드 매니저](#실전-프로젝트-사운드-매니저)

---

## 오디오 시스템 개요

유니티의 오디오 시스템은 **AudioSource**와 **AudioListener**로 구성됩니다.

### 오디오 구성 요소

```
씬 구성:
- Camera (AudioListener)
- GameObject (AudioSource)
  └── AudioClip

AudioSource → 스피커 (소리 발생)
AudioListener → 귀 (소리 듣기)
```

---

## AudioSource

**AudioSource**는 소리를 재생하는 컴포넌트입니다.

### AudioSource 기본 설정

```csharp
using UnityEngine;

public class AudioSourceBasics : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip myClip;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // AudioClip 할당
        audioSource.clip = myClip;

        // 기본 설정
        audioSource.volume = 1f;           // 볼륨 (0~1)
        audioSource.pitch = 1f;            // 피치 (0.5~3)
        audioSource.loop = false;          // 반복
        audioSource.playOnAwake = false;   // 시작 시 자동 재생

        // 3D 설정
        audioSource.spatialBlend = 0f;     // 0=2D, 1=3D
        audioSource.minDistance = 1f;      // 최소 거리
        audioSource.maxDistance = 500f;    // 최대 거리
    }

    void Update()
    {
        // 재생
        if (Input.GetKeyDown(KeyCode.P))
        {
            audioSource.Play();
        }

        // 일시정지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            else
            {
                audioSource.UnPause();
            }
        }

        // 정지
        if (Input.GetKeyDown(KeyCode.S))
        {
            audioSource.Stop();
        }
    }
}
```

### AudioSource 재생 방법

```csharp
using UnityEngine;

public class AudioPlayMethods : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip clip1;
    public AudioClip clip2;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 방법 1: Play() - clip 재생
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.clip = clip1;
            audioSource.Play();
        }

        // 방법 2: PlayOneShot() - 한 번만 재생 (추천)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            audioSource.PlayOneShot(clip2);
        }

        // 방법 3: PlayClipAtPoint() - 특정 위치에서 재생
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AudioSource.PlayClipAtPoint(clip1, transform.position, 1f);
        }
    }

    // 지연 재생
    void PlayDelayed()
    {
        audioSource.PlayDelayed(1f); // 1초 후 재생
    }

    // 스케줄 재생
    void PlayScheduled()
    {
        double time = AudioSettings.dspTime + 2.0; // 2초 후
        audioSource.PlayScheduled(time);
    }
}
```

---

## AudioListener

**AudioListener**는 소리를 듣는 컴포넌트입니다 (보통 카메라에 부착).

```csharp
using UnityEngine;

public class AudioListenerSettings : MonoBehaviour
{
    void Start()
    {
        // 전역 볼륨
        AudioListener.volume = 0.5f; // 50%

        // AudioListener 일시정지
        AudioListener.pause = false;
    }
}
```

**규칙:**
- 씬에 **하나만** 있어야 함
- 보통 Main Camera에 부착
- 두 개 이상 있으면 경고

---

## AudioClip

**AudioClip**은 오디오 파일입니다.

### AudioClip 설정 (Inspector)

```
Import Settings:
- Force To Mono: 모노로 변환 (용량 절약)
- Load Type:
  - Decompress On Load: 메모리에 압축 해제 (빠름, 용량 큼)
  - Compressed In Memory: 압축 상태 유지 (느림, 용량 작음)
  - Streaming: 스트리밍 (배경음악용)
- Compression Format:
  - PCM: 무손실 (효과음)
  - Vorbis: 손실 압축 (배경음악)
  - ADPCM: 압축 (효과음)
```

### AudioClip 정보

```csharp
using UnityEngine;

public class AudioClipInfo : MonoBehaviour
{
    public AudioClip clip;

    void Start()
    {
        Debug.Log($"이름: {clip.name}");
        Debug.Log($"길이: {clip.length}초");
        Debug.Log($"샘플: {clip.samples}");
        Debug.Log($"채널: {clip.channels}");
        Debug.Log($"주파수: {clip.frequency}Hz");
        Debug.Log($"로드 상태: {clip.loadState}");
    }
}
```

---

## 3D Sound

**3D Sound**는 거리에 따라 볼륨이 변합니다.

### 3D Sound 설정

```csharp
using UnityEngine;

public class ThreeDSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // 3D 사운드 활성화
        audioSource.spatialBlend = 1f; // 1 = 완전한 3D

        // 거리 설정
        audioSource.minDistance = 5f;   // 5m까지 최대 볼륨
        audioSource.maxDistance = 50f;  // 50m부터 들리지 않음

        // Rolloff Mode
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        // Linear: 선형 감소
        // Logarithmic: 로그 감소 (현실적)
        // Custom: 커스텀 곡선

        // Doppler Effect (도플러 효과)
        audioSource.dopplerLevel = 1f; // 0~5
    }
}
```

### 스타크래프트 예시: 총소리 (3D)

```csharp
using UnityEngine;

/// <summary>
/// 마린 총소리 - 3D Sound
/// </summary>
public class MarineWeaponSound : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;

    private AudioSource audioSource;

    void Awake()
    {
        // AudioSource 생성
        audioSource = gameObject.AddComponent<AudioSource>();

        // 3D 설정
        audioSource.spatialBlend = 1f; // 3D
        audioSource.minDistance = 10f;
        audioSource.maxDistance = 100f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        Debug.Log("[Marine] 무기 사운드 시스템 초기화");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void Fire()
    {
        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound, 0.8f);
            Debug.Log("[Marine] 발사음 재생");
        }
    }

    void Reload()
    {
        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound, 0.6f);
            Debug.Log("[Marine] 재장전 소리");
        }
    }
}
```

---

## AudioMixer

**AudioMixer**는 여러 오디오를 믹싱하고 볼륨을 제어합니다.

### AudioMixer 생성

```
1. Project 우클릭 > Create > Audio Mixer
2. 이름: "MainAudioMixer"
3. Groups 생성:
   - Master
     - BGM
     - SFX
     - Voice
```

### AudioMixer 사용

```csharp
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerController : MonoBehaviour
{
    public AudioMixer mixer;

    void Start()
    {
        // 볼륨 설정 (-80 ~ 20 dB)
        mixer.SetFloat("MasterVolume", 0f);    // 0dB (100%)
        mixer.SetFloat("BGMVolume", -10f);     // -10dB (약 30%)
        mixer.SetFloat("SFXVolume", -5f);      // -5dB (약 56%)

        // 볼륨 가져오기
        float volume;
        mixer.GetFloat("MasterVolume", out volume);
        Debug.Log($"Master Volume: {volume}dB");
    }

    // UI Slider로 볼륨 조절 (0~1)
    public void SetMasterVolume(float sliderValue)
    {
        // 0~1 → -80~0 dB 변환
        float dB = Mathf.Log10(sliderValue) * 20f;
        mixer.SetFloat("MasterVolume", dB);
    }

    public void SetBGMVolume(float sliderValue)
    {
        float dB = Mathf.Log10(sliderValue) * 20f;
        mixer.SetFloat("BGMVolume", dB);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dB = Mathf.Log10(sliderValue) * 20f;
        mixer.SetFloat("SFXVolume", dB);
    }
}
```

---

## 오디오 풀링

효과음을 풀링하여 성능을 최적화합니다.

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 오디오 풀링 시스템
/// </summary>
public class AudioPool : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 3f)] public float pitch = 1f;
    }

    public Sound[] sounds;

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    public int poolSize = 10;

    void Awake()
    {
        // Dictionary 생성
        foreach (Sound s in sounds)
        {
            soundDictionary[s.name] = s.clip;
        }

        // 풀 생성
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }

        Debug.Log($"[AudioPool] 초기화 완료 (풀 크기: {poolSize})");
    }

    AudioSource CreateAudioSource()
    {
        GameObject obj = new GameObject("PooledAudioSource");
        obj.transform.SetParent(transform);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;

        audioSourcePool.Enqueue(source);
        return source;
    }

    AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            activeAudioSources.Add(source);
            return source;
        }
        else
        {
            // 풀 확장
            Debug.Log("[AudioPool] 풀 확장");
            return CreateAudioSource();
        }
    }

    void ReturnAudioSource(AudioSource source)
    {
        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    public void Play(string soundName, Vector3 position, float volume = 1f)
    {
        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioPool] 사운드 없음: {soundName}");
            return;
        }

        AudioSource source = GetAudioSource();
        source.transform.position = position;
        source.clip = soundDictionary[soundName];
        source.volume = volume;
        source.Play();

        StartCoroutine(ReturnAfterPlay(source));
    }

    System.Collections.IEnumerator ReturnAfterPlay(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        ReturnAudioSource(source);
    }

    void Update()
    {
        // 디버그 정보
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"[AudioPool] 활성: {activeAudioSources.Count}, 대기: {audioSourcePool.Count}");
        }
    }
}
```

---

## 실전 프로젝트: 사운드 매니저

완전한 사운드 관리 시스템입니다.

```csharp
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

/// <summary>
/// 통합 사운드 매니저
/// - BGM 관리
/// - SFX 재생
/// - 볼륨 조절
/// - 오디오 풀링
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("BGM")]
    public AudioClip[] bgmClips;
    private AudioSource bgmSource;
    private int currentBGMIndex = 0;

    [Header("SFX")]
    public AudioClip[] sfxClips;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSFX = new List<AudioSource>();
    public int sfxPoolSize = 20;

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    void Awake()
    {
        // 싱글턴
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
        // BGM AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        if (mixer != null)
        {
            bgmSource.outputAudioMixerGroup = mixer.FindMatchingGroups("BGM")[0];
        }

        // SFX Dictionary
        foreach (AudioClip clip in sfxClips)
        {
            sfxDictionary[clip.name] = clip;
        }

        // SFX Pool
        for (int i = 0; i < sfxPoolSize; i++)
        {
            CreateSFXSource();
        }

        // 볼륨 적용
        ApplyVolume();

        Debug.Log("[SoundManager] 초기화 완료");
    }

    AudioSource CreateSFXSource()
    {
        GameObject obj = new GameObject("SFX_Source");
        obj.transform.SetParent(transform);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        if (mixer != null)
        {
            source.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        }

        sfxPool.Enqueue(source);
        return source;
    }

    // === BGM ===

    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Length)
        {
            Debug.LogWarning($"[SoundManager] BGM 인덱스 오류: {index}");
            return;
        }

        currentBGMIndex = index;
        bgmSource.clip = bgmClips[index];
        bgmSource.Play();

        Debug.Log($"[SoundManager] BGM 재생: {bgmClips[index].name}");
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        Debug.Log("[SoundManager] BGM 정지");
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    // === SFX ===

    public void PlaySFX(string clipName, Vector3 position, float volume = 1f)
    {
        if (!sfxDictionary.ContainsKey(clipName))
        {
            Debug.LogWarning($"[SoundManager] SFX 없음: {clipName}");
            return;
        }

        AudioSource source = GetSFXSource();
        source.transform.position = position;
        source.clip = sfxDictionary[clipName];
        source.volume = volume * sfxVolume;
        source.Play();

        StartCoroutine(ReturnSFXSource(source));
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        PlaySFX(clipName, Vector3.zero, volume);
    }

    AudioSource GetSFXSource()
    {
        if (sfxPool.Count > 0)
        {
            AudioSource source = sfxPool.Dequeue();
            activeSFX.Add(source);
            return source;
        }
        else
        {
            Debug.Log("[SoundManager] SFX 풀 확장");
            return CreateSFXSource();
        }
    }

    System.Collections.IEnumerator ReturnSFXSource(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        activeSFX.Remove(source);
        sfxPool.Enqueue(source);
    }

    // === 볼륨 ===

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    void ApplyVolume()
    {
        if (mixer != null)
        {
            mixer.SetFloat("MasterVolume", VolumeToDecibel(masterVolume));
            mixer.SetFloat("BGMVolume", VolumeToDecibel(bgmVolume));
            mixer.SetFloat("SFXVolume", VolumeToDecibel(sfxVolume));
        }
        else
        {
            bgmSource.volume = bgmVolume * masterVolume;
        }
    }

    float VolumeToDecibel(float volume)
    {
        if (volume <= 0f) return -80f;
        return Mathf.Log10(volume) * 20f;
    }

    // === 테스트 ===

    void Update()
    {
        // B: BGM 재생
        if (Input.GetKeyDown(KeyCode.B))
        {
            PlayBGM(0);
        }

        // N: SFX 재생
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlaySFX("GunShot", transform.position);
        }

        // M: 음소거
        if (Input.GetKeyDown(KeyCode.M))
        {
            SetMasterVolume(masterVolume > 0 ? 0f : 1f);
        }
    }
}
```

---

## 정리

### 이번 강의 핵심

✅ **AudioSource** - Play, PlayOneShot, PlayClipAtPoint
✅ **AudioListener** - 씬에 하나만
✅ **3D Sound** - spatialBlend, minDistance, maxDistance
✅ **AudioMixer** - 볼륨 그룹 관리
✅ **오디오 풀링** - 성능 최적화
✅ **사운드 매니저** - 싱글턴 패턴

### 핵심 코드

```csharp
// 효과음 재생
audioSource.PlayOneShot(clip, 0.8f);

// 3D 사운드
audioSource.spatialBlend = 1f;
audioSource.minDistance = 10f;

// 볼륨 조절
float dB = Mathf.Log10(volume) * 20f;
mixer.SetFloat("MasterVolume", dB);

// 사운드 매니저 사용
SoundManager.Instance.PlaySFX("GunShot");
```

---

## 다음 강의 예고: 11강 - 씬 관리와 데이터 영속성

다음 강의에서는 씬 관리와 데이터 저장을 배웁니다! 💾
