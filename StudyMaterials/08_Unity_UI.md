# 8강: UI 시스템 (Unity UI System)

## 목차
1. [UI 시스템 개요](#ui-시스템-개요)
2. [Canvas - 캔버스](#canvas---캔버스)
3. [RectTransform - UI 좌표계](#recttransform---ui-좌표계)
4. [UI 기본 요소](#ui-기본-요소)
5. [Layout 시스템](#layout-시스템)
6. [EventSystem - 이벤트 시스템](#eventsystem---이벤트-시스템)
7. [UI 스크립팅](#ui-스크립팅)
8. [World Space UI](#world-space-ui)
9. [실전 프로젝트: 스타크래프트 RTS UI](#실전-프로젝트-스타크래프트-rts-ui)

---

## UI 시스템 개요

유니티의 **UI 시스템 (UGUI)**은 게임 인터페이스를 만드는 공식 시스템입니다.

### UI의 핵심 구성 요소

```
UI Hierarchy:
Canvas (최상위)
├── EventSystem (입력 처리)
├── Panel (그룹화)
│   ├── Image (배경)
│   ├── Text (텍스트)
│   └── Button (버튼)
└── ...
```

### UI 생성 방법

```
Hierarchy 우클릭 > UI > Canvas

자동 생성:
- Canvas (모든 UI의 부모)
- EventSystem (입력 이벤트 처리)
```

### UI vs 3D GameObject

| 특성 | UI | 3D GameObject |
|------|-------|---------------|
| **좌표계** | RectTransform (2D) | Transform (3D) |
| **부모** | Canvas 필수 | 자유 |
| **렌더링** | Canvas Renderer | Mesh Renderer |
| **정렬** | Sorting Order | Z Position |
| **입력** | EventSystem | Raycast |

---

## Canvas - 캔버스

**Canvas**는 모든 UI의 부모이며, UI 렌더링을 담당합니다.

### Render Mode

```csharp
using UnityEngine;

public class CanvasRenderModes : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();

        // 1. Screen Space - Overlay (기본)
        // 화면 위에 오버레이, 항상 최상위
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 2. Screen Space - Camera
        // 카메라 앞에 렌더링, 3D 오브젝트와 섞임
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 10f; // 카메라로부터 거리

        // 3. World Space
        // 3D 공간에 배치, 회전/크기 조정 가능
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);
    }
}
```

**Render Mode 비교:**

```
Screen Space - Overlay:
✅ 항상 화면 위에 표시
✅ 3D 오브젝트에 가려지지 않음
✅ 해상도 자동 대응
❌ 3D와 섞을 수 없음
용도: HUD, 메뉴, 일시정지

Screen Space - Camera:
✅ 3D 오브젝트와 섞임 (Z 정렬)
✅ 카메라 효과 적용 가능
✅ 원근감 있음
용도: 3D 공간의 UI

World Space:
✅ 3D 오브젝트처럼 배치
✅ 회전, 크기 자유
✅ VR/AR UI
용도: 게임 내 컴퓨터 화면, 간판
```

### Canvas Scaler

```csharp
using UnityEngine.UI;

public class CanvasScalerSettings : MonoBehaviour
{
    void Start()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        // Scale Mode 1: Constant Pixel Size
        // 픽셀 크기 고정 (해상도가 바뀌면 UI 크기 변함)
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;

        // Scale Mode 2: Scale With Screen Size (추천!)
        // 기준 해상도에 맞춰 자동 스케일
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); // 기준 해상도
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // 0=너비, 1=높이, 0.5=중간

        // Scale Mode 3: Constant Physical Size
        // 물리적 크기 고정 (인치 단위)
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
        scaler.physicalUnit = CanvasScaler.Unit.Points;
    }
}
```

### 스타크래프트 예시: Canvas 설정

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스타크래프트 UI Canvas 설정
/// </summary>
public class StarCraftCanvas : MonoBehaviour
{
    void Awake()
    {
        SetupCanvas();
    }

    void SetupCanvas()
    {
        // Canvas 설정
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 화면 위 오버레이
        canvas.sortingOrder = 0; // 렌더링 순서

        // Canvas Scaler 설정
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); // 풀HD 기준
        scaler.matchWidthOrHeight = 0f; // 너비 우선

        // Graphic Raycaster (클릭 감지)
        GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        Debug.Log("[StarCraft] Canvas 설정 완료 (1920x1080 기준)");
    }
}
```

---

## RectTransform - UI 좌표계

**RectTransform**은 UI의 위치, 크기, 회전을 담당합니다. Transform과 다릅니다!

### RectTransform vs Transform

```csharp
using UnityEngine;

public class RectTransformBasics : MonoBehaviour
{
    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();

        // === 위치 ===
        rect.anchoredPosition = new Vector2(0, 0); // Anchor 기준 위치
        rect.localPosition = new Vector3(0, 0, 0); // 부모 기준 위치

        // === 크기 ===
        rect.sizeDelta = new Vector2(200, 100); // 너비 200, 높이 100
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);

        // === 회전 ===
        rect.rotation = Quaternion.identity; // Transform과 동일

        // === 스케일 ===
        rect.localScale = Vector3.one; // Transform과 동일

        // === Pivot (중심점) ===
        rect.pivot = new Vector2(0.5f, 0.5f); // 중앙 (기본값)
        // (0, 0) = 왼쪽 아래
        // (1, 1) = 오른쪽 위
    }
}
```

### Anchor (앵커)

**Anchor**는 부모 크기가 변해도 UI 위치를 유지하는 기준점입니다.

```csharp
public class AnchorExamples : MonoBehaviour
{
    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();

        // 1. 중앙 고정
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(200, 100);

        // 2. 왼쪽 위 고정
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(100, -50); // 오른쪽 100, 아래 50

        // 3. 오른쪽 아래 고정
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-100, 50); // 왼쪽 100, 위 50

        // 4. 가로 늘림 (Stretch Horizontal)
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(0, 100); // 좌우 여백 0

        // 5. 세로 늘림 (Stretch Vertical)
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(200, 0); // 상하 여백 0

        // 6. 전체 늘림 (Stretch Both)
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(0, 0); // 모든 여백 0
    }
}
```

**Anchor 프리셋:**

```
Inspector > RectTransform > Anchor Presets (왼쪽 위 사각형 클릭)

자주 쓰는 프리셋:
- Center: 중앙 고정
- Top Left: 왼쪽 위
- Bottom Right: 오른쪽 아래
- Stretch: 늘림
```

### 스타크래프트 예시: UI 배치

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스타크래프트 UI 레이아웃
/// - 상단: 자원 표시
/// - 하단: 커맨드 패널
/// - 우측 하단: 미니맵
/// </summary>
public class StarCraftUILayout : MonoBehaviour
{
    void Start()
    {
        CreateResourcePanel();
        CreateCommandPanel();
        CreateMinimap();
    }

    // 상단: 자원 표시
    void CreateResourcePanel()
    {
        GameObject panel = new GameObject("ResourcePanel");
        panel.transform.SetParent(transform);

        RectTransform rect = panel.AddComponent<RectTransform>();

        // 상단 중앙, 가로 늘림
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(0, 60); // 높이 60

        // 배경
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f); // 반투명 검은색

        Debug.Log("[UI] 자원 패널 생성 (상단)");
    }

    // 하단: 커맨드 패널
    void CreateCommandPanel()
    {
        GameObject panel = new GameObject("CommandPanel");
        panel.transform.SetParent(transform);

        RectTransform rect = panel.AddComponent<RectTransform>();

        // 하단 좌측
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(400, 200); // 400x200

        // 배경
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        Debug.Log("[UI] 커맨드 패널 생성 (하단 좌측)");
    }

    // 우측 하단: 미니맵
    void CreateMinimap()
    {
        GameObject minimap = new GameObject("Minimap");
        minimap.transform.SetParent(transform);

        RectTransform rect = minimap.AddComponent<RectTransform>();

        // 우측 하단
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-10, 10);
        rect.sizeDelta = new Vector2(250, 250); // 정사각형 250x250

        // 배경
        Image bg = minimap.AddComponent<Image>();
        bg.color = new Color(0, 0.2f, 0, 0.8f); // 초록 계열

        Debug.Log("[UI] 미니맵 생성 (우측 하단)");
    }
}
```

---

## UI 기본 요소

### 1. Text / TextMeshPro

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro

public class TextExamples : MonoBehaviour
{
    void Start()
    {
        CreateLegacyText();
        CreateTextMeshPro();
    }

    // Legacy Text (기본 Text)
    void CreateLegacyText()
    {
        GameObject textObj = new GameObject("LegacyText");
        textObj.transform.SetParent(transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        Text text = textObj.AddComponent<Text>();
        text.text = "미네랄: 500";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.cyan;
        text.alignment = TextAnchor.MiddleCenter; // 중앙 정렬
        text.fontStyle = FontStyle.Bold; // 굵게
    }

    // TextMeshPro (권장!)
    void CreateTextMeshPro()
    {
        GameObject textObj = new GameObject("TextMeshPro");
        textObj.transform.SetParent(transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "미네랄: 500";
        tmp.fontSize = 24;
        tmp.color = Color.cyan;
        tmp.alignment = TextAlignmentOptions.Center; // 중앙 정렬
        tmp.fontStyle = FontStyles.Bold; // 굵게

        // 외곽선
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;
    }
}
```

**TextMeshPro 장점:**
```
✅ 선명한 텍스트 (SDF 렌더링)
✅ 외곽선, 그림자 효과
✅ 더 많은 스타일 옵션
✅ 성능 우수
✅ 유니코드 완벽 지원
```

### 2. Image

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ImageExamples : MonoBehaviour
{
    void Start()
    {
        CreateImage();
        CreateSpriteImage();
    }

    // 단색 Image
    void CreateImage()
    {
        GameObject imgObj = new GameObject("ColorImage");
        imgObj.transform.SetParent(transform);

        RectTransform rect = imgObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);

        Image img = imgObj.AddComponent<Image>();
        img.color = Color.red;
        img.raycastTarget = true; // 클릭 감지
    }

    // Sprite Image
    void CreateSpriteImage()
    {
        GameObject imgObj = new GameObject("SpriteImage");
        imgObj.transform.SetParent(transform);

        RectTransform rect = imgObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 100);

        Image img = imgObj.AddComponent<Image>();
        // img.sprite = mySprite; // Inspector에서 할당
        img.type = Image.Type.Simple; // Simple, Sliced, Tiled, Filled

        // Filled Type (HP 바 등)
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal; // 가로 채우기
        img.fillOrigin = (int)Image.OriginHorizontal.Left; // 왼쪽에서
        img.fillAmount = 0.7f; // 70% 채움
    }
}
```

### 3. Button

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ButtonExamples : MonoBehaviour
{
    void Start()
    {
        CreateButton();
    }

    void CreateButton()
    {
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(transform);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 50);

        // Image 배경
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f);

        // Button 컴포넌트
        Button btn = btnObj.AddComponent<Button>();

        // 색상 전환 (기본)
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f); // 마우스 오버
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);     // 클릭
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);    // 비활성화
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        // 클릭 이벤트
        btn.onClick.AddListener(OnButtonClick);

        // 자식으로 Text 추가
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = "마린 생산";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    void OnButtonClick()
    {
        Debug.Log("[Button] 마린 생산 버튼 클릭!");
    }
}
```

### 4. Slider

```csharp
using UnityEngine;
using UnityEngine.UI;

public class SliderExamples : MonoBehaviour
{
    void Start()
    {
        CreateSlider();
    }

    void CreateSlider()
    {
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(transform);

        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 20);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 50f;
        slider.wholeNumbers = false; // 정수만 (true) / 실수 (false)
        slider.direction = Slider.Direction.LeftToRight; // 방향

        // 이벤트
        slider.onValueChanged.AddListener(OnSliderValueChanged);

        // 배경 (Background)
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // 채우기 영역 (Fill Area)
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        // 채우기 (Fill)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;

        slider.fillRect = fillRect;

        // 핸들 (Handle) - 선택사항
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRect;
    }

    void OnSliderValueChanged(float value)
    {
        Debug.Log($"[Slider] 값 변경: {value}");
    }
}
```

### 5. Input Field

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldExamples : MonoBehaviour
{
    void Start()
    {
        CreateInputField();
    }

    void CreateInputField()
    {
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(transform);

        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 40);

        // Legacy InputField
        InputField inputField = inputObj.AddComponent<InputField>();
        inputField.text = "플레이어 이름";
        inputField.characterLimit = 20; // 최대 20자
        inputField.contentType = InputField.ContentType.Standard; // 타입

        // Content Types:
        // Standard, Alphanumeric, IntegerNumber, DecimalNumber,
        // Password, Pin, EmailAddress, Name, Custom

        // 이벤트
        inputField.onEndEdit.AddListener(OnInputEndEdit);
        inputField.onValueChanged.AddListener(OnInputValueChanged);

        // TextMeshPro InputField (권장!)
        // TMP_InputField tmpInput = inputObj.AddComponent<TMP_InputField>();
    }

    void OnInputEndEdit(string input)
    {
        Debug.Log($"[Input] 입력 완료: {input}");
    }

    void OnInputValueChanged(string input)
    {
        Debug.Log($"[Input] 입력 중: {input}");
    }
}
```

### 6. Toggle

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ToggleExamples : MonoBehaviour
{
    void Start()
    {
        CreateToggle();
    }

    void CreateToggle()
    {
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(transform);

        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = true; // 기본값

        // 이벤트
        toggle.onValueChanged.AddListener(OnToggleValueChanged);

        // Toggle Group (라디오 버튼)
        ToggleGroup group = gameObject.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false; // 하나는 반드시 선택
        toggle.group = group;
    }

    void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"[Toggle] 상태: {isOn}");
    }
}
```

### 스타크래프트 예시: 자원 표시 UI

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스타크래프트 자원 표시 UI
/// - 미네랄
/// - 베스핀 가스
/// - 인구수
/// </summary>
public class ResourceUI : MonoBehaviour
{
    [Header("Resources")]
    public int minerals = 50;
    public int gas = 0;
    public int supplyUsed = 4;
    public int supplyMax = 10;

    [Header("UI References")]
    private TextMeshProUGUI mineralText;
    private TextMeshProUGUI gasText;
    private TextMeshProUGUI supplyText;

    void Start()
    {
        CreateResourceUI();
        UpdateResourceUI();
    }

    void CreateResourceUI()
    {
        // 배경 패널
        GameObject panel = new GameObject("ResourcePanel");
        panel.transform.SetParent(transform);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 60);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // 미네랄 텍스트
        GameObject mineralObj = new GameObject("MineralText");
        mineralObj.transform.SetParent(panel.transform);

        RectTransform mineralRect = mineralObj.AddComponent<RectTransform>();
        mineralRect.anchorMin = new Vector2(0f, 0.5f);
        mineralRect.anchorMax = new Vector2(0f, 0.5f);
        mineralRect.pivot = new Vector2(0f, 0.5f);
        mineralRect.anchoredPosition = new Vector2(20, 0);
        mineralRect.sizeDelta = new Vector2(200, 40);

        mineralText = mineralObj.AddComponent<TextMeshProUGUI>();
        mineralText.text = $"미네랄: {minerals}";
        mineralText.fontSize = 24;
        mineralText.color = Color.cyan;
        mineralText.fontStyle = FontStyles.Bold;
        mineralText.alignment = TextAlignmentOptions.MidlineLeft;

        // 가스 텍스트
        GameObject gasObj = new GameObject("GasText");
        gasObj.transform.SetParent(panel.transform);

        RectTransform gasRect = gasObj.AddComponent<RectTransform>();
        gasRect.anchorMin = new Vector2(0f, 0.5f);
        gasRect.anchorMax = new Vector2(0f, 0.5f);
        gasRect.pivot = new Vector2(0f, 0.5f);
        gasRect.anchoredPosition = new Vector2(250, 0);
        gasRect.sizeDelta = new Vector2(200, 40);

        gasText = gasObj.AddComponent<TextMeshProUGUI>();
        gasText.text = $"가스: {gas}";
        gasText.fontSize = 24;
        gasText.color = Color.green;
        gasText.fontStyle = FontStyles.Bold;
        gasText.alignment = TextAlignmentOptions.MidlineLeft;

        // 인구수 텍스트
        GameObject supplyObj = new GameObject("SupplyText");
        supplyObj.transform.SetParent(panel.transform);

        RectTransform supplyRect = supplyObj.AddComponent<RectTransform>();
        supplyRect.anchorMin = new Vector2(1f, 0.5f);
        supplyRect.anchorMax = new Vector2(1f, 0.5f);
        supplyRect.pivot = new Vector2(1f, 0.5f);
        supplyRect.anchoredPosition = new Vector2(-20, 0);
        supplyRect.sizeDelta = new Vector2(200, 40);

        supplyText = supplyObj.AddComponent<TextMeshProUGUI>();
        supplyText.text = $"인구수: {supplyUsed}/{supplyMax}";
        supplyText.fontSize = 24;
        supplyText.color = Color.white;
        supplyText.fontStyle = FontStyles.Bold;
        supplyText.alignment = TextAlignmentOptions.MidlineRight;

        Debug.Log("[UI] 자원 표시 UI 생성 완료");
    }

    void UpdateResourceUI()
    {
        if (mineralText != null)
        {
            mineralText.text = $"미네랄: {minerals}";
        }

        if (gasText != null)
        {
            gasText.text = $"가스: {gas}";
        }

        if (supplyText != null)
        {
            // 인구수 색상 변경
            if (supplyUsed >= supplyMax)
            {
                supplyText.color = Color.red; // 인구수 한계
            }
            else
            {
                supplyText.color = Color.white;
            }

            supplyText.text = $"인구수: {supplyUsed}/{supplyMax}";
        }
    }

    void Update()
    {
        // 테스트: 키보드로 자원 증가
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMinerals(50);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGas(25);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            IncreaseSupply();
        }
    }

    public void AddMinerals(int amount)
    {
        minerals += amount;
        UpdateResourceUI();
        Debug.Log($"[Resource] 미네랄 +{amount} (총: {minerals})");
    }

    public void AddGas(int amount)
    {
        gas += amount;
        UpdateResourceUI();
        Debug.Log($"[Resource] 가스 +{amount} (총: {gas})");
    }

    public void IncreaseSupply()
    {
        supplyUsed++;
        UpdateResourceUI();
        Debug.Log($"[Resource] 인구수 +1 ({supplyUsed}/{supplyMax})");
    }

    public bool SpendResources(int mineralCost, int gasCost)
    {
        if (minerals >= mineralCost && gas >= gasCost)
        {
            minerals -= mineralCost;
            gas -= gasCost;
            UpdateResourceUI();
            Debug.Log($"[Resource] 자원 소비: -{mineralCost} 미네랄, -{gasCost} 가스");
            return true;
        }
        else
        {
            Debug.Log("[Resource] 자원 부족!");
            return false;
        }
    }
}
```

---

## Layout 시스템

**Layout Group**은 자식 UI를 자동으로 배치합니다.

### Horizontal Layout Group

```csharp
using UnityEngine;
using UnityEngine.UI;

public class HorizontalLayoutExample : MonoBehaviour
{
    void Start()
    {
        // Horizontal Layout Group 추가
        HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();

        layout.spacing = 10f; // 간격
        layout.padding = new RectOffset(10, 10, 10, 10); // 여백 (좌, 우, 상, 하)
        layout.childAlignment = TextAnchor.MiddleCenter; // 정렬
        layout.childControlWidth = true; // 자식 너비 제어
        layout.childControlHeight = true; // 자식 높이 제어
        layout.childForceExpandWidth = false; // 너비 확장 강제
        layout.childForceExpandHeight = false; // 높이 확장 강제

        // 자식 UI 추가 (자동으로 가로 배치됨)
        for (int i = 0; i < 5; i++)
        {
            GameObject child = new GameObject($"Button_{i}");
            child.transform.SetParent(transform);

            RectTransform rect = child.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 50);

            Image img = child.AddComponent<Image>();
            img.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
```

### Vertical Layout Group

```csharp
public class VerticalLayoutExample : MonoBehaviour
{
    void Start()
    {
        // Vertical Layout Group 추가
        VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 10f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;

        // 자식 UI 추가 (자동으로 세로 배치됨)
        for (int i = 0; i < 5; i++)
        {
            GameObject child = new GameObject($"Item_{i}");
            child.transform.SetParent(transform);

            RectTransform rect = child.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 40);

            Image img = child.AddComponent<Image>();
            img.color = Color.gray;
        }
    }
}
```

### Grid Layout Group

```csharp
public class GridLayoutExample : MonoBehaviour
{
    void Start()
    {
        // Grid Layout Group 추가
        GridLayoutGroup layout = gameObject.AddComponent<GridLayoutGroup>();

        layout.cellSize = new Vector2(80, 80); // 셀 크기
        layout.spacing = new Vector2(10, 10); // 간격
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft; // 시작 위치
        layout.startAxis = GridLayoutGroup.Axis.Horizontal; // 배치 방향
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; // 제약 조건
        layout.constraintCount = 3; // 열 개수 3개

        // 자식 UI 추가 (자동으로 그리드 배치됨)
        for (int i = 0; i < 12; i++)
        {
            GameObject child = new GameObject($"Cell_{i}");
            child.transform.SetParent(transform);

            RectTransform rect = child.AddComponent<RectTransform>();

            Image img = child.AddComponent<Image>();
            img.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
```

### Layout Element

```csharp
public class LayoutElementExample : MonoBehaviour
{
    void Start()
    {
        // Layout Element로 개별 크기 지정
        LayoutElement element = gameObject.AddComponent<LayoutElement>();

        element.minWidth = 100f;
        element.minHeight = 50f;
        element.preferredWidth = 200f;
        element.preferredHeight = 100f;
        element.flexibleWidth = 1f; // 확장 가능
        element.flexibleHeight = 1f;

        element.ignoreLayout = false; // Layout 무시
    }
}
```

### 스타크래프트 예시: 커맨드 패널 (Grid Layout)

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 스타크래프트 커맨드 패널
/// 3x3 그리드로 명령 버튼 배치
/// </summary>
public class CommandPanel : MonoBehaviour
{
    [Header("Settings")]
    public int gridSize = 3; // 3x3 그리드
    public Vector2 cellSize = new Vector2(80, 80);
    public Vector2 spacing = new Vector2(5, 5);

    private List<CommandButton> buttons = new List<CommandButton>();

    void Start()
    {
        CreateCommandPanel();
        SetupMarineCommands();
    }

    void CreateCommandPanel()
    {
        // 배경 패널
        GameObject panel = new GameObject("CommandPanel");
        panel.transform.SetParent(transform);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(20, 20);

        float width = (cellSize.x * gridSize) + (spacing.x * (gridSize - 1)) + 20;
        float height = (cellSize.y * gridSize) + (spacing.y * (gridSize - 1)) + 20;
        panelRect.sizeDelta = new Vector2(width, height);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Grid Layout Group
        GameObject grid = new GameObject("ButtonGrid");
        grid.transform.SetParent(panel.transform);

        RectTransform gridRect = grid.AddComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.sizeDelta = Vector2.zero;
        gridRect.offsetMin = new Vector2(10, 10);
        gridRect.offsetMax = new Vector2(-10, -10);

        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = cellSize;
        layout.spacing = spacing;
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = gridSize;

        // 9개 버튼 생성
        for (int i = 0; i < 9; i++)
        {
            CommandButton btn = CreateCommandButton(grid.transform, i);
            buttons.Add(btn);
        }

        Debug.Log("[CommandPanel] 생성 완료 (3x3 그리드)");
    }

    CommandButton CreateCommandButton(Transform parent, int index)
    {
        GameObject btnObj = new GameObject($"CommandButton_{index}");
        btnObj.transform.SetParent(parent);

        // 배경 Image
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f);

        // Button
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
        btn.colors = colors;

        // 아이콘 (자식)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform);

        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10); // 여백 5

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.gray;

        // 단축키 텍스트 (자식)
        GameObject keyObj = new GameObject("Hotkey");
        keyObj.transform.SetParent(btnObj.transform);

        RectTransform keyRect = keyObj.AddComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0f, 1f);
        keyRect.anchorMax = new Vector2(0f, 1f);
        keyRect.pivot = new Vector2(0f, 1f);
        keyRect.anchoredPosition = new Vector2(5, -5);
        keyRect.sizeDelta = new Vector2(30, 20);

        TextMeshProUGUI keyText = keyObj.AddComponent<TextMeshProUGUI>();
        keyText.fontSize = 14;
        keyText.color = Color.yellow;
        keyText.fontStyle = FontStyles.Bold;
        keyText.alignment = TextAlignmentOptions.TopLeft;

        // CommandButton 컴포넌트
        CommandButton cmdBtn = btnObj.AddComponent<CommandButton>();
        cmdBtn.button = btn;
        cmdBtn.icon = iconImg;
        cmdBtn.hotkeyText = keyText;
        cmdBtn.SetEmpty(); // 기본값: 빈 버튼

        return cmdBtn;
    }

    void SetupMarineCommands()
    {
        // 마린 명령어 설정
        buttons[0].SetCommand("정지", "S", Color.red, OnStopCommand);
        buttons[1].SetCommand("이동", "M", Color.green, OnMoveCommand);
        buttons[2].SetCommand("공격", "A", Color.red, OnAttackCommand);
        buttons[3].SetCommand("홀드", "H", Color.yellow, OnHoldCommand);
        buttons[4].SetCommand("정찰", "P", Color.cyan, OnPatrolCommand);
        buttons[5].SetCommand("스팀팩", "T", Color.magenta, OnStimpackCommand);

        Debug.Log("[CommandPanel] 마린 명령어 설정 완료");
    }

    void OnStopCommand() { Debug.Log("[Command] 정지 (S)"); }
    void OnMoveCommand() { Debug.Log("[Command] 이동 (M)"); }
    void OnAttackCommand() { Debug.Log("[Command] 공격 (A)"); }
    void OnHoldCommand() { Debug.Log("[Command] 홀드 (H)"); }
    void OnPatrolCommand() { Debug.Log("[Command] 정찰 (P)"); }
    void OnStimpackCommand() { Debug.Log("[Command] 스팀팩 (T)"); }
}

/// <summary>
/// 개별 커맨드 버튼
/// </summary>
public class CommandButton : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI hotkeyText;

    private System.Action onClickCallback;

    public void SetCommand(string name, string hotkey, Color color, System.Action callback)
    {
        icon.color = color;
        hotkeyText.text = hotkey;
        onClickCallback = callback;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        button.interactable = true;
        icon.enabled = true;

        Debug.Log($"[CommandButton] 설정: {name} (단축키: {hotkey})");
    }

    public void SetEmpty()
    {
        icon.color = new Color(0.2f, 0.2f, 0.2f);
        hotkeyText.text = "";
        onClickCallback = null;

        button.interactable = false;
        icon.enabled = false;
    }

    void OnClick()
    {
        onClickCallback?.Invoke();
    }
}
```

---

## EventSystem - 이벤트 시스템

**EventSystem**은 UI 입력(클릭, 드래그 등)을 처리합니다.

### Event Interface 종류

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 모든 Event Interface 예시
/// </summary>
public class AllEventInterfaces : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // 마우스 진입
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[Event] 마우스 진입");
    }

    // 마우스 이탈
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[Event] 마우스 이탈");
    }

    // 마우스 누름
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[Event] 마우스 누름: {eventData.button}");
    }

    // 마우스 뗌
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[Event] 마우스 뗌: {eventData.button}");
    }

    // 클릭 (누르고 떼기)
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Event] 클릭: {eventData.button}");
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("[Event] 드래그 시작");
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"[Event] 드래그: {eventData.delta}");
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("[Event] 드래그 종료");
    }
}
```

### 스타크래프트 예시: 유닛 초상화 (Hover 효과)

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 유닛 초상화 - 마우스 오버 시 정보 표시
/// </summary>
public class UnitPortrait : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Unit Info")]
    public string unitName = "마린";
    public int hp = 40;
    public int maxHP = 40;
    public int armor = 0;

    [Header("UI References")]
    private Image portrait;
    private Image hpBar;
    private GameObject tooltip;

    void Awake()
    {
        SetupPortrait();
    }

    void SetupPortrait()
    {
        // 배경
        portrait = gameObject.AddComponent<Image>();
        portrait.color = Color.gray;

        // HP 바 (자식)
        GameObject hpBarBg = new GameObject("HPBar_Background");
        hpBarBg.transform.SetParent(transform);

        RectTransform hpBarBgRect = hpBarBg.AddComponent<RectTransform>();
        hpBarBgRect.anchorMin = new Vector2(0f, 0f);
        hpBarBgRect.anchorMax = new Vector2(1f, 0f);
        hpBarBgRect.pivot = new Vector2(0.5f, 0f);
        hpBarBgRect.anchoredPosition = Vector2.zero;
        hpBarBgRect.sizeDelta = new Vector2(0, 10);

        Image hpBarBgImg = hpBarBg.AddComponent<Image>();
        hpBarBgImg.color = Color.red;

        // HP 바 (채우기)
        GameObject hpBarFill = new GameObject("HPBar_Fill");
        hpBarFill.transform.SetParent(hpBarBg.transform);

        RectTransform hpBarFillRect = hpBarFill.AddComponent<RectTransform>();
        hpBarFillRect.anchorMin = new Vector2(0f, 0f);
        hpBarFillRect.anchorMax = new Vector2(1f, 1f);
        hpBarFillRect.pivot = new Vector2(0f, 0.5f);
        hpBarFillRect.anchoredPosition = Vector2.zero;
        hpBarFillRect.sizeDelta = Vector2.zero;

        hpBar = hpBarFill.AddComponent<Image>();
        hpBar.color = Color.green;
        hpBar.type = Image.Type.Filled;
        hpBar.fillMethod = Image.FillMethod.Horizontal;
        hpBar.fillOrigin = (int)Image.OriginHorizontal.Left;

        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = (float)hp / maxHP;
        }
    }

    // 마우스 오버: 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[Portrait] 마우스 오버: {unitName}");

        // 툴팁 생성
        CreateTooltip();

        // 하이라이트
        portrait.color = Color.white;
    }

    // 마우스 이탈: 툴팁 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Portrait] 마우스 이탈: {unitName}");

        // 툴팁 제거
        if (tooltip != null)
        {
            Destroy(tooltip);
        }

        // 하이라이트 해제
        portrait.color = Color.gray;
    }

    // 클릭: 카메라 이동
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Portrait] 클릭: {unitName}");
        // 카메라를 유닛으로 이동하는 로직
    }

    void CreateTooltip()
    {
        // 툴팁 생성
        tooltip = new GameObject("Tooltip");
        tooltip.transform.SetParent(transform.root); // Canvas 루트

        RectTransform tooltipRect = tooltip.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(200, 100);

        // 마우스 위치에 표시
        tooltipRect.position = Input.mousePosition + new Vector3(10, -10, 0);

        // 배경
        Image bg = tooltip.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        // 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltip.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"{unitName}\n\nHP: {hp}/{maxHP}\n방어력: {armor}";
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
    }

    public void TakeDamage(int damage)
    {
        hp = Mathf.Max(0, hp - damage);
        UpdateHPBar();
        Debug.Log($"[{unitName}] HP: {hp}/{maxHP}");
    }
}
```

---

## UI 스크립팅

코드로 UI를 동적으로 제어하는 방법입니다.

### Button 이벤트

```csharp
using UnityEngine;
using UnityEngine.UI;

public class ButtonScripting : MonoBehaviour
{
    public Button myButton;

    void Start()
    {
        // 방법 1: AddListener
        myButton.onClick.AddListener(OnButtonClick);

        // 방법 2: 람다 표현식
        myButton.onClick.AddListener(() =>
        {
            Debug.Log("버튼 클릭 (람다)");
        });

        // 방법 3: 파라미터 전달
        myButton.onClick.AddListener(() => OnButtonClickWithParam(5));

        // 리스너 제거
        myButton.onClick.RemoveAllListeners();
    }

    void OnButtonClick()
    {
        Debug.Log("버튼 클릭!");
    }

    void OnButtonClickWithParam(int value)
    {
        Debug.Log($"버튼 클릭: {value}");
    }

    void OnDestroy()
    {
        // 메모리 누수 방지
        myButton.onClick.RemoveAllListeners();
    }
}
```

### Slider 이벤트

```csharp
using UnityEngine;
using UnityEngine.UI;

public class SliderScripting : MonoBehaviour
{
    public Slider volumeSlider;
    public Text volumeText;

    void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        volumeText.text = $"볼륨: {value:F0}%";
        AudioListener.volume = value / 100f;
    }
}
```

### 동적 UI 생성

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DynamicUICreation : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform content; // Scroll View의 Content

    private List<GameObject> items = new List<GameObject>();

    void Start()
    {
        // 10개 아이템 생성
        for (int i = 0; i < 10; i++)
        {
            CreateItem($"아이템 {i + 1}");
        }
    }

    void CreateItem(string itemName)
    {
        GameObject item = Instantiate(itemPrefab, content);
        items.Add(item);

        // 텍스트 설정
        Text text = item.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = itemName;
        }

        // 버튼 이벤트
        Button btn = item.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnItemClick(itemName));
        }
    }

    void OnItemClick(string itemName)
    {
        Debug.Log($"[UI] 아이템 클릭: {itemName}");
    }

    void ClearAllItems()
    {
        foreach (GameObject item in items)
        {
            Destroy(item);
        }
        items.Clear();
    }
}
```

---

## World Space UI

**World Space UI**는 3D 공간에 배치되는 UI입니다.

### World Space Canvas 설정

```csharp
using UnityEngine;

public class WorldSpaceCanvas : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2, 1); // 월드 크기

        // 카메라 바라보기
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    void Update()
    {
        // 항상 카메라 바라보기 (Billboard)
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                         Camera.main.transform.rotation * Vector3.up);
    }
}
```

### 오버워치 예시: 유닛 HP 바 (World Space)

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 머리 위 HP 바 (World Space)
/// </summary>
public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("Unit")]
    public int maxHP = 200;
    private int currentHP;

    [Header("UI")]
    private Canvas canvas;
    private Image hpBarFill;
    private GameObject hpBarObject;

    void Awake()
    {
        currentHP = maxHP;
        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        // Canvas (World Space)
        hpBarObject = new GameObject("HealthBar");
        hpBarObject.transform.SetParent(transform);
        hpBarObject.transform.localPosition = new Vector3(0, 2.5f, 0); // 머리 위

        canvas = hpBarObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.5f, 0.2f); // 월드 크기

        CanvasScaler scaler = hpBarObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // 배경
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(hpBarObject.transform);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // HP 바 (채우기)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4); // 테두리 여백

        hpBarFill = fillObj.AddComponent<Image>();
        hpBarFill.color = Color.green;
        hpBarFill.type = Image.Type.Filled;
        hpBarFill.fillMethod = Image.FillMethod.Horizontal;
        hpBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        hpBarFill.fillAmount = 1f;

        Debug.Log("[HealthBar] World Space HP 바 생성");
    }

    void Update()
    {
        // 카메라 바라보기 (Billboard)
        if (hpBarObject != null)
        {
            hpBarObject.transform.LookAt(
                hpBarObject.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        UpdateHealthBar();

        Debug.Log($"[{gameObject.name}] HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (hpBarFill != null)
        {
            float hpPercent = (float)currentHP / maxHP;
            hpBarFill.fillAmount = hpPercent;

            // 색상 변경
            if (hpPercent > 0.5f)
            {
                hpBarFill.color = Color.green;
            }
            else if (hpPercent > 0.25f)
            {
                hpBarFill.color = Color.yellow;
            }
            else
            {
                hpBarFill.color = Color.red;
            }
        }
    }

    void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망!");
        Destroy(gameObject);
    }
}
```

---

## 실전 프로젝트: 스타크래프트 RTS UI

모든 UI 개념을 통합한 완전한 RTS UI 시스템입니다.

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 스타크래프트 완전한 UI 시스템
/// - 자원 표시
/// - 유닛 선택 UI
/// - 커맨드 패널
/// - 미니맵
/// </summary>
public class CompleteRTSUI : MonoBehaviour
{
    [Header("Game State")]
    public int minerals = 50;
    public int gas = 0;
    public int supplyUsed = 4;
    public int supplyMax = 10;

    [Header("Selected Units")]
    public List<GameObject> selectedUnits = new List<GameObject>();

    [Header("UI References")]
    private TextMeshProUGUI mineralText;
    private TextMeshProUGUI gasText;
    private TextMeshProUGUI supplyText;
    private GameObject selectedUnitPanel;
    private Image selectedUnitPortrait;
    private TextMeshProUGUI selectedUnitName;
    private Image selectedUnitHPBar;
    private GameObject commandPanel;
    private List<CommandButton> commandButtons = new List<CommandButton>();
    private GameObject minimap;

    void Start()
    {
        CreateCompleteUI();
        UpdateUI();
    }

    void CreateCompleteUI()
    {
        Debug.Log("[RTS UI] UI 시스템 생성 시작");

        CreateResourcePanel();
        CreateSelectedUnitPanel();
        CreateCommandPanel();
        CreateMinimap();

        Debug.Log("[RTS UI] UI 시스템 생성 완료");
    }

    // === 자원 표시 ===
    void CreateResourcePanel()
    {
        GameObject panel = new GameObject("ResourcePanel");
        panel.transform.SetParent(transform);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0, 60);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // 미네랄
        mineralText = CreateResourceText(panel.transform, "미네랄", new Vector2(20, 0), Color.cyan);

        // 가스
        gasText = CreateResourceText(panel.transform, "가스", new Vector2(250, 0), Color.green);

        // 인구수
        supplyText = CreateResourceText(panel.transform, "인구수", new Vector2(-20, 0), Color.white);
        supplyText.alignment = TextAlignmentOptions.MidlineRight;
        RectTransform supplyRect = supplyText.GetComponent<RectTransform>();
        supplyRect.anchorMin = new Vector2(1f, 0.5f);
        supplyRect.anchorMax = new Vector2(1f, 0.5f);
        supplyRect.pivot = new Vector2(1f, 0.5f);
    }

    TextMeshProUGUI CreateResourceText(Transform parent, string label, Vector2 position, Color color)
    {
        GameObject textObj = new GameObject($"{label}Text");
        textObj.transform.SetParent(parent);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.5f);
        textRect.anchorMax = new Vector2(0f, 0.5f);
        textRect.pivot = new Vector2(0f, 0.5f);
        textRect.anchoredPosition = position;
        textRect.sizeDelta = new Vector2(200, 40);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 24;
        text.color = color;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        return text;
    }

    // === 선택된 유닛 표시 ===
    void CreateSelectedUnitPanel()
    {
        selectedUnitPanel = new GameObject("SelectedUnitPanel");
        selectedUnitPanel.transform.SetParent(transform);

        RectTransform panelRect = selectedUnitPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(10, 220);
        panelRect.sizeDelta = new Vector2(200, 150);

        Image panelBg = selectedUnitPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // 초상화
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(selectedUnitPanel.transform);

        RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.5f, 1f);
        portraitRect.anchorMax = new Vector2(0.5f, 1f);
        portraitRect.pivot = new Vector2(0.5f, 1f);
        portraitRect.anchoredPosition = new Vector2(0, -10);
        portraitRect.sizeDelta = new Vector2(80, 80);

        selectedUnitPortrait = portraitObj.AddComponent<Image>();
        selectedUnitPortrait.color = Color.gray;

        // 유닛 이름
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(selectedUnitPanel.transform);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.5f);
        nameRect.anchorMax = new Vector2(1f, 0.5f);
        nameRect.pivot = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = new Vector2(0, 0);
        nameRect.sizeDelta = new Vector2(-20, 30);

        selectedUnitName = nameObj.AddComponent<TextMeshProUGUI>();
        selectedUnitName.text = "마린";
        selectedUnitName.fontSize = 20;
        selectedUnitName.color = Color.white;
        selectedUnitName.fontStyle = FontStyles.Bold;
        selectedUnitName.alignment = TextAlignmentOptions.Center;

        // HP 바 배경
        GameObject hpBgObj = new GameObject("HPBar_BG");
        hpBgObj.transform.SetParent(selectedUnitPanel.transform);

        RectTransform hpBgRect = hpBgObj.AddComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0f, 0f);
        hpBgRect.anchorMax = new Vector2(1f, 0f);
        hpBgRect.pivot = new Vector2(0.5f, 0f);
        hpBgRect.anchoredPosition = new Vector2(0, 10);
        hpBgRect.sizeDelta = new Vector2(-20, 15);

        Image hpBg = hpBgObj.AddComponent<Image>();
        hpBg.color = Color.red;

        // HP 바 채우기
        GameObject hpFillObj = new GameObject("HPBar_Fill");
        hpFillObj.transform.SetParent(hpBgObj.transform);

        RectTransform hpFillRect = hpFillObj.AddComponent<RectTransform>();
        hpFillRect.anchorMin = Vector2.zero;
        hpFillRect.anchorMax = Vector2.one;
        hpFillRect.sizeDelta = Vector2.zero;

        selectedUnitHPBar = hpFillObj.AddComponent<Image>();
        selectedUnitHPBar.color = Color.green;
        selectedUnitHPBar.type = Image.Type.Filled;
        selectedUnitHPBar.fillMethod = Image.FillMethod.Horizontal;
        selectedUnitHPBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        selectedUnitHPBar.fillAmount = 0.75f;

        selectedUnitPanel.SetActive(false);
    }

    // === 커맨드 패널 ===
    void CreateCommandPanel()
    {
        commandPanel = new GameObject("CommandPanel");
        commandPanel.transform.SetParent(transform);

        RectTransform panelRect = commandPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 0f);
        panelRect.pivot = new Vector2(0f, 0f);
        panelRect.anchoredPosition = new Vector2(220, 10);
        panelRect.sizeDelta = new Vector2(270, 270);

        Image panelBg = commandPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Grid
        GameObject grid = new GameObject("Grid");
        grid.transform.SetParent(commandPanel.transform);

        RectTransform gridRect = grid.AddComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.sizeDelta = Vector2.zero;
        gridRect.offsetMin = new Vector2(10, 10);
        gridRect.offsetMax = new Vector2(-10, -10);

        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(80, 80);
        layout.spacing = new Vector2(5, 5);
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;

        // 9개 버튼
        for (int i = 0; i < 9; i++)
        {
            CommandButton btn = CreateCommandButton(grid.transform, i);
            commandButtons.Add(btn);
        }

        commandPanel.SetActive(false);
    }

    CommandButton CreateCommandButton(Transform parent, int index)
    {
        GameObject btnObj = new GameObject($"Cmd_{index}");
        btnObj.transform.SetParent(parent);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
        btn.colors = colors;

        // 아이콘
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(btnObj.transform);

        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10);

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = Color.gray;

        // 단축키
        GameObject keyObj = new GameObject("Hotkey");
        keyObj.transform.SetParent(btnObj.transform);

        RectTransform keyRect = keyObj.AddComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0f, 1f);
        keyRect.anchorMax = new Vector2(0f, 1f);
        keyRect.pivot = new Vector2(0f, 1f);
        keyRect.anchoredPosition = new Vector2(5, -5);
        keyRect.sizeDelta = new Vector2(30, 20);

        TextMeshProUGUI keyText = keyObj.AddComponent<TextMeshProUGUI>();
        keyText.fontSize = 14;
        keyText.color = Color.yellow;
        keyText.fontStyle = FontStyles.Bold;

        CommandButton cmdBtn = btnObj.AddComponent<CommandButton>();
        cmdBtn.button = btn;
        cmdBtn.icon = iconImg;
        cmdBtn.hotkeyText = keyText;
        cmdBtn.SetEmpty();

        return cmdBtn;
    }

    // === 미니맵 ===
    void CreateMinimap()
    {
        minimap = new GameObject("Minimap");
        minimap.transform.SetParent(transform);

        RectTransform minimapRect = minimap.AddComponent<RectTransform>();
        minimapRect.anchorMin = new Vector2(1f, 0f);
        minimapRect.anchorMax = new Vector2(1f, 0f);
        minimapRect.pivot = new Vector2(1f, 0f);
        minimapRect.anchoredPosition = new Vector2(-10, 10);
        minimapRect.sizeDelta = new Vector2(250, 250);

        Image minimapBg = minimap.AddComponent<Image>();
        minimapBg.color = new Color(0, 0.2f, 0, 0.8f);

        // 미니맵 타이틀
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(minimap.transform);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, 0);
        titleRect.sizeDelta = new Vector2(0, 30);

        Image titleBg = titleObj.AddComponent<Image>();
        titleBg.color = new Color(0, 0, 0, 0.5f);

        GameObject titleTextObj = new GameObject("Text");
        titleTextObj.transform.SetParent(titleObj.transform);

        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "미니맵";
        titleText.fontSize = 16;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
    }

    // === UI 업데이트 ===
    void UpdateUI()
    {
        UpdateResourceUI();
        UpdateSelectedUnitUI();
        UpdateCommandUI();
    }

    void UpdateResourceUI()
    {
        mineralText.text = $"미네랄: {minerals}";
        gasText.text = $"가스: {gas}";

        if (supplyUsed >= supplyMax)
        {
            supplyText.color = Color.red;
        }
        else
        {
            supplyText.color = Color.white;
        }

        supplyText.text = $"인구수: {supplyUsed}/{supplyMax}";
    }

    void UpdateSelectedUnitUI()
    {
        if (selectedUnits.Count > 0)
        {
            selectedUnitPanel.SetActive(true);

            GameObject unit = selectedUnits[0];
            selectedUnitName.text = unit.name;

            // HP 바 업데이트 (임시 값)
            selectedUnitHPBar.fillAmount = Random.Range(0.5f, 1f);
        }
        else
        {
            selectedUnitPanel.SetActive(false);
        }
    }

    void UpdateCommandUI()
    {
        if (selectedUnits.Count > 0)
        {
            commandPanel.SetActive(true);

            // 마린 명령어 설정 (예시)
            commandButtons[0].SetCommand("정지", "S", Color.red, () => Debug.Log("정지"));
            commandButtons[1].SetCommand("이동", "M", Color.green, () => Debug.Log("이동"));
            commandButtons[2].SetCommand("공격", "A", Color.red, () => Debug.Log("공격"));
            commandButtons[3].SetCommand("홀드", "H", Color.yellow, () => Debug.Log("홀드"));
        }
        else
        {
            commandPanel.SetActive(false);
        }
    }

    // === 테스트 ===
    void Update()
    {
        // M: 미네랄 증가
        if (Input.GetKeyDown(KeyCode.M))
        {
            minerals += 50;
            UpdateUI();
        }

        // G: 가스 증가
        if (Input.GetKeyDown(KeyCode.G))
        {
            gas += 25;
            UpdateUI();
        }

        // U: 유닛 선택/해제
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (selectedUnits.Count > 0)
            {
                selectedUnits.Clear();
            }
            else
            {
                GameObject dummy = new GameObject("Marine");
                selectedUnits.Add(dummy);
            }
            UpdateUI();
        }
    }
}
```

**실행 결과:**
```
[RTS UI] UI 시스템 생성 시작
[RTS UI] UI 시스템 생성 완료

[키보드 입력]
M: 미네랄 +50
G: 가스 +25
U: 유닛 선택/해제

[UI 표시]
- 상단: 미네랄, 가스, 인구수
- 좌측 하단: 선택된 유닛 초상화, HP 바
- 중앙 하단: 커맨드 패널 (3x3)
- 우측 하단: 미니맵
```

---

## 정리 및 다음 단계

### 이번 강의에서 배운 것

✅ **Canvas** - Render Mode, Canvas Scaler
✅ **RectTransform** - Anchor, Pivot, 위치/크기
✅ **UI 기본 요소** - Text, Image, Button, Slider, Input Field, Toggle
✅ **Layout 시스템** - Horizontal, Vertical, Grid Layout Group
✅ **EventSystem** - IPointerEnterHandler, IDragHandler 등
✅ **UI 스크립팅** - 동적 UI 생성, 이벤트 처리
✅ **World Space UI** - 3D 공간의 UI, HP 바

### 핵심 규칙 정리

```csharp
// 1. Canvas는 모든 UI의 부모
Canvas canvas = gameObject.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;

// 2. RectTransform으로 위치/크기 제어
RectTransform rect = GetComponent<RectTransform>();
rect.anchoredPosition = Vector2.zero;
rect.sizeDelta = new Vector2(200, 100);

// 3. Button 이벤트는 AddListener
button.onClick.AddListener(OnClick);

// 4. Anchor로 해상도 대응
rect.anchorMin = new Vector2(0.5f, 0.5f); // 중앙 고정
rect.anchorMax = new Vector2(0.5f, 0.5f);

// 5. Layout Group으로 자동 배치
GridLayoutGroup layout = gameObject.AddComponent<GridLayoutGroup>();
layout.cellSize = new Vector2(80, 80);
```

### 다음 강의 예고: 9강 - 애니메이션 시스템

다음 강의에서는 유니티의 애니메이션 시스템을 배웁니다:
- Animator와 Animation Clip
- Animator Controller (State Machine)
- Transition과 Parameter
- Blend Tree
- 스크립트에서 애니메이션 제어
- 실전: 마린 애니메이션 시스템 (Idle, Walk, Attack, Die)

다음 강의에서 만나요! 🎮
