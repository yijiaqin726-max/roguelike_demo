using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(-1000)]
public class DemoRunManager : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";
    private const string MenuTitleText = "\u7834\u8a93\u5723\u9a91\u58eb";
    private const string MenuMottoText = "\u8a93\u7ea6\u5df2\u788e\uff0c\u8363\u5149\u5df2\u7a7a\uff0c\u5723\u5149\u6b63\u5728\u5760\u843d";
    private const string MenuSubtitleText = "\u5728\u5b88\u8a93\u4e0e\u8150\u5316\u4e4b\u95f4\u6295\u62e9\uff0c\u7ee7\u7eed\u6df1\u5165\u5730\u7262";
    private const string StartRunText = "\u5f00\u59cb\u5f81\u9014";
    private const string ContinueRunText = "\u7ee7\u7eed\u5f81\u9014";
    private const string QuitText = "\u9000\u51fa\u6e38\u620f";
    private const string StartRunHintText = "\u8e0f\u5165\u88ab\u8150\u5316\u7684\u5723\u57df";
    private const string ContinueRunHintText = "\u5ef6\u7eed\u5c1a\u672a\u7ec8\u7ed3\u7684\u8a93\u7ea6";
    private const string QuitHintText = "\u8ba9\u7070\u70ec\u6682\u65f6\u6c89\u5bc2";

    public static DemoRunManager Instance { get; private set; }

    public int CurrentFloor { get; private set; } = 1;
    public bool HasActiveRun { get; private set; }

    private GameSaveData pendingLoadData;
    private Canvas menuCanvas;
    private Button continueButton;
    private Text floorLabel;
    private Font uiFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureAfterSceneLoad()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        DemoRunManager existing = FindObjectOfType<DemoRunManager>();
        if (existing != null)
        {
            Instance = existing;
            return;
        }

        GameObject root = new GameObject("DemoRunManager");
        root.AddComponent<DemoRunManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        InitializeUiIfNeeded();

        if (!HasActiveRun)
        {
            ShowMenu();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnApplicationQuit()
    {
        SaveCurrentRun();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUiIfNeeded();
        RefreshContinueButtonState();
        ApplyPendingLoadIfNeeded();
        UpdateFloorLabel();

        if (!HasActiveRun)
        {
            ShowMenu();
        }
    }

    public void StartNewRun()
    {
        pendingLoadData = CreateDefaultData();
        CurrentFloor = pendingLoadData.currentFloor;
        HasActiveRun = true;
        SaveSystem.Save(pendingLoadData);
        OpenGameplayScene();
    }

    public void ContinueRun()
    {
        if (!SaveSystem.HasSave())
        {
            return;
        }

        pendingLoadData = SaveSystem.Load();
        if (pendingLoadData == null)
        {
            return;
        }

        CurrentFloor = Mathf.Max(1, pendingLoadData.currentFloor);
        HasActiveRun = true;
        OpenGameplayScene();
    }

    public void CompleteFloor()
    {
        CurrentFloor++;
        SaveCurrentRun();
        UpdateFloorLabel();
    }

    public void SaveCurrentRun()
    {
        if (!HasActiveRun)
        {
            return;
        }

        GameSaveData data = CollectCurrentData();
        SaveSystem.Save(data);
        RefreshContinueButtonState();
    }

    private void OpenGameplayScene()
    {
        HideMenu();

        if (SceneManager.GetActiveScene().name != GameplaySceneName)
        {
            SceneManager.LoadScene(GameplaySceneName);
            return;
        }

        ApplyPendingLoadIfNeeded();

        FloorFlowManager floorFlowManager = FindObjectOfType<FloorFlowManager>();
        if (floorFlowManager != null)
        {
            floorFlowManager.BeginFloor();
        }
    }

    private void ApplyPendingLoadIfNeeded()
    {
        if (pendingLoadData == null)
        {
            return;
        }

        CurrentFloor = Mathf.Max(1, pendingLoadData.currentFloor);

        PlayerController playerController = FindObjectOfType<PlayerController>();
        PlayerExperience playerExperience = FindObjectOfType<PlayerExperience>();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        CorruptionSystem corruptionSystem = FindObjectOfType<CorruptionSystem>();

        if (playerController != null)
        {
            playerController.moveSpeed = pendingLoadData.moveSpeed;
            playerController.attackIntervalSeconds = pendingLoadData.attackInterval;
        }

        if (playerExperience != null)
        {
            playerExperience.level = pendingLoadData.playerLevel;
            playerExperience.currentExp = pendingLoadData.currentExp;
            playerExperience.expToNextLevel = pendingLoadData.expToNextLevel;
        }

        if (playerHealth != null)
        {
            playerHealth.maxHealth = pendingLoadData.maxHealth;
            playerHealth.currentHealth = Mathf.Clamp(pendingLoadData.currentHealth, 0, playerHealth.maxHealth);
        }

        if (corruptionSystem != null)
        {
            int loadedCorruption = Mathf.Max(pendingLoadData.corruptionValue, pendingLoadData.currentCorruption);
            corruptionSystem.corruptionValue = Mathf.Clamp(loadedCorruption, 0, corruptionSystem.maxCorruption);
            corruptionSystem.oathValue = Mathf.Clamp(pendingLoadData.oathValue, 0, corruptionSystem.maxOath);
        }

        pendingLoadData = null;
        Time.timeScale = 1f;
        UpdateFloorLabel();

        FloorFlowManager floorFlowManager = FindObjectOfType<FloorFlowManager>();
        if (floorFlowManager != null)
        {
            floorFlowManager.BeginFloor();
        }
    }

    private GameSaveData CollectCurrentData()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        PlayerExperience playerExperience = FindObjectOfType<PlayerExperience>();
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        CorruptionSystem corruptionSystem = FindObjectOfType<CorruptionSystem>();

        GameSaveData data = new GameSaveData
        {
            currentFloor = Mathf.Max(1, CurrentFloor)
        };

        if (playerController != null)
        {
            data.moveSpeed = playerController.moveSpeed;
            data.attackInterval = playerController.attackIntervalSeconds;
        }

        if (playerExperience != null)
        {
            data.playerLevel = playerExperience.level;
            data.currentExp = playerExperience.currentExp;
            data.expToNextLevel = playerExperience.expToNextLevel;
        }

        if (playerHealth != null)
        {
            data.maxHealth = playerHealth.maxHealth;
            data.currentHealth = playerHealth.currentHealth;
        }

        if (corruptionSystem != null)
        {
            data.currentCorruption = corruptionSystem.corruptionValue;
            data.corruptionValue = corruptionSystem.corruptionValue;
            data.oathValue = corruptionSystem.oathValue;
        }

        return data;
    }

    private static GameSaveData CreateDefaultData()
    {
        return new GameSaveData();
    }

    private void BuildMenuCanvas()
    {
        GameObject canvasObject = new GameObject("RuntimeMenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        menuCanvas = canvasObject.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        menuCanvas.sortingOrder = 500;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject overlay = CreatePanel("Overlay", canvasObject.transform, new Color(0.03f, 0.04f, 0.06f, 0.94f));
        StretchToFullScreen(overlay.GetComponent<RectTransform>());

        GameObject vignette = CreatePanel("Vignette", overlay.transform, new Color(0.12f, 0.03f, 0.04f, 0.12f));
        StretchToFullScreen(vignette.GetComponent<RectTransform>());

        GameObject centerPlate = CreatePanel("CenterPlate", overlay.transform, new Color(0.07f, 0.04f, 0.06f, 0.9f));
        SetRect(centerPlate.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(670f, 404f));
        AddFrame(centerPlate.transform, new Color(0.52f, 0.37f, 0.26f, 0.7f), 2f, new Vector2(18f, 18f));
        CreatePanel("InnerShade", centerPlate.transform, new Color(0.15f, 0.04f, 0.05f, 0.14f));
        StretchToFullScreen(centerPlate.transform.Find("InnerShade").GetComponent<RectTransform>());
        CreateDivider(centerPlate.transform, new Vector2(0.5f, 0.68f), new Vector2(436f, 2f), new Color(0.63f, 0.46f, 0.33f, 0.65f));
        CreateDivider(centerPlate.transform, new Vector2(0.5f, 0.23f), new Vector2(360f, 1.5f), new Color(0.52f, 0.22f, 0.24f, 0.55f));

        GameObject crest = CreatePanel("Crest", centerPlate.transform, new Color(0.55f, 0.17f, 0.16f, 0.82f));
        SetRect(crest.GetComponent<RectTransform>(), new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), Vector2.zero, new Vector2(58f, 6f));

        Text titleText = CreateText("Title", centerPlate.transform, MenuTitleText, 42, new Color(0.96f, 0.9f, 0.78f), TextAnchor.MiddleCenter);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 0.76f), new Vector2(0.5f, 0.76f), Vector2.zero, new Vector2(460f, 60f));
        StyleText(titleText, FontStyle.Bold, new Color(0.18f, 0.04f, 0.05f, 0.75f));

        Text mottoText = CreateText("Motto", centerPlate.transform, MenuMottoText, 14, new Color(0.74f, 0.56f, 0.56f), TextAnchor.MiddleCenter);
        SetRect(mottoText.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(540f, 24f));
        StyleText(mottoText, FontStyle.Italic, new Color(0.08f, 0.02f, 0.03f, 0.82f));

        Text subtitleText = CreateText("Subtitle", centerPlate.transform, MenuSubtitleText, 18, new Color(0.79f, 0.76f, 0.82f), TextAnchor.MiddleCenter);
        SetRect(subtitleText.rectTransform, new Vector2(0.5f, 0.57f), new Vector2(0.5f, 0.57f), Vector2.zero, new Vector2(620f, 40f));
        StyleText(subtitleText, FontStyle.Normal, new Color(0.07f, 0.03f, 0.05f, 0.78f));

        Button startButton = CreateButton("StartButton", centerPlate.transform, StartRunText, StartRunHintText, new Vector2(0.5f, 0.39f), new Color(0.3f, 0.11f, 0.12f, 0.97f), new Color(0.69f, 0.43f, 0.28f, 0.85f));
        startButton.onClick.AddListener(StartNewRun);

        continueButton = CreateButton("ContinueButton", centerPlate.transform, ContinueRunText, ContinueRunHintText, new Vector2(0.5f, 0.27f), new Color(0.12f, 0.14f, 0.2f, 0.96f), new Color(0.47f, 0.51f, 0.6f, 0.82f));
        continueButton.onClick.AddListener(ContinueRun);

        Button quitButton = CreateButton("QuitButton", centerPlate.transform, QuitText, QuitHintText, new Vector2(0.5f, 0.15f), new Color(0.1f, 0.1f, 0.12f, 0.94f), new Color(0.42f, 0.33f, 0.3f, 0.72f));
        quitButton.onClick.AddListener(QuitGame);

        GameObject floorPlate = CreatePanel("FloorPlate", canvasObject.transform, new Color(0.06f, 0.05f, 0.08f, 0.84f));
        SetRect(floorPlate.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-92f, -28f), new Vector2(188f, 40f));
        AddFrame(floorPlate.transform, new Color(0.48f, 0.37f, 0.26f, 0.62f), 2f, new Vector2(10f, 10f));

        floorLabel = CreateText("FloorLabel", floorPlate.transform, BuildFloorLabelText(1), 19, new Color(0.92f, 0.9f, 0.82f), TextAnchor.MiddleCenter);
        StretchToFullScreen(floorLabel.rectTransform);
        StyleText(floorLabel, FontStyle.Bold, new Color(0.08f, 0.02f, 0.03f, 0.76f));

        RefreshContinueButtonState();
        UpdateFloorLabel();
    }

    private void InitializeUiIfNeeded()
    {
        if (menuCanvas != null)
        {
            return;
        }

        uiFont = LoadPreferredFont();
        BuildMenuCanvas();
    }

    private GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private Button CreateButton(string name, Transform parent, string label, string sublabel, Vector2 anchor, Color baseColor, Color accentColor)
    {
        GameObject buttonObject = CreatePanel(name, parent, baseColor);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchor, anchor, Vector2.zero, new Vector2(310f, 66f));

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.08f);
        colors.pressedColor = baseColor * 0.82f;
        colors.disabledColor = new Color(0.16f, 0.16f, 0.16f, 0.6f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        AddFrame(buttonObject.transform, new Color(0.78f, 0.58f, 0.42f, 0.5f), 2f, new Vector2(12f, 12f));

        GameObject accent = CreatePanel("Accent", buttonObject.transform, accentColor);
        SetRect(accent.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(6f, 42f));
        accent.GetComponent<Image>().raycastTarget = false;

        Text text = CreateText("Label", buttonObject.transform, label, 24, new Color(0.95f, 0.92f, 0.82f), TextAnchor.MiddleLeft);
        SetRect(text.rectTransform, new Vector2(0f, 0.62f), new Vector2(1f, 0.62f), new Vector2(18f, 0f), new Vector2(-54f, 26f));
        StyleText(text, FontStyle.Bold, new Color(0.07f, 0.02f, 0.03f, 0.86f));

        Text hintText = CreateText("Hint", buttonObject.transform, sublabel, 12, new Color(0.8f, 0.76f, 0.78f), TextAnchor.MiddleLeft);
        SetRect(hintText.rectTransform, new Vector2(0f, 0.32f), new Vector2(1f, 0.32f), new Vector2(18f, 0f), new Vector2(-54f, 18f));
        StyleText(hintText, FontStyle.Normal, new Color(0.04f, 0.01f, 0.02f, 0.76f));

        return button;
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;

        return text;
    }

    private static void CreateDivider(Transform parent, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject divider = new GameObject("Divider");
        divider.transform.SetParent(parent, false);
        Image image = divider.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        SetRect(image.rectTransform, anchor, anchor, Vector2.zero, size);
    }

    private static void AddFrame(Transform parent, Color color, float thickness, Vector2 inset)
    {
        CreateEdge(parent, "FrameTop", new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -inset.y), new Vector2(-inset.x, thickness), color);
        CreateEdge(parent, "FrameBottom", new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(0f, inset.y), new Vector2(-inset.x, thickness), color);
        CreateEdge(parent, "FrameLeft", new Vector2(0f, 0.5f), new Vector2(0f, 1f), new Vector2(inset.x, 0f), new Vector2(thickness, -inset.y), color);
        CreateEdge(parent, "FrameRight", new Vector2(1f, 0.5f), new Vector2(1f, 1f), new Vector2(-inset.x, 0f), new Vector2(thickness, -inset.y), color);
    }

    private static void CreateEdge(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject edge = new GameObject(name);
        edge.transform.SetParent(parent, false);
        Image image = edge.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        SetRect(image.rectTransform, anchorMin, anchorMax, anchoredPosition, sizeDelta);
    }

    private static void StyleText(Text text, FontStyle fontStyle, Color shadowColor)
    {
        text.fontStyle = fontStyle;

        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = shadowColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private void ShowMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
        }

        RefreshContinueButtonState();
        Time.timeScale = 0f;
    }

    private void HideMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void RefreshContinueButtonState()
    {
        if (continueButton != null)
        {
            continueButton.interactable = SaveSystem.HasSave();
        }
    }

    private void UpdateFloorLabel()
    {
        if (floorLabel == null)
        {
            return;
        }

        floorLabel.text = BuildFloorLabelText(CurrentFloor);
        floorLabel.gameObject.SetActive(HasActiveRun);
    }

    private static string BuildFloorLabelText(int floor)
    {
        return "\u7b2c " + Mathf.Max(1, floor) + " \u5c42";
    }

    private static Font LoadPreferredFont()
    {
        string[] preferredFonts =
        {
            "Microsoft YaHei",
            "SimHei",
            "SimSun",
            "Arial Unicode MS",
            "Arial"
        };

        Font dynamicFont = Font.CreateDynamicFontFromOSFont(preferredFonts, 24);
        if (dynamicFont != null)
        {
            return dynamicFont;
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void QuitGame()
    {
        SaveCurrentRun();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static void StretchToFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
