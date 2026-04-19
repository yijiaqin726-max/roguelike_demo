using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject overlay = CreatePanel("Overlay", canvasObject.transform, new Color(0.04f, 0.06f, 0.09f, 0.92f));
        StretchToFullScreen(overlay.GetComponent<RectTransform>());

        GameObject centerPlate = CreatePanel("CenterPlate", overlay.transform, new Color(0.06f, 0.04f, 0.06f, 0.78f));
        SetRect(centerPlate.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(620f, 360f));

        Text titleText = CreateText("Title", centerPlate.transform, MenuTitleText, 42, new Color(0.96f, 0.9f, 0.78f), TextAnchor.MiddleCenter);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(420f, 60f));

        Text mottoText = CreateText("Motto", centerPlate.transform, MenuMottoText, 14, new Color(0.74f, 0.56f, 0.56f), TextAnchor.MiddleCenter);
        SetRect(mottoText.rectTransform, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(500f, 24f));

        Text subtitleText = CreateText("Subtitle", centerPlate.transform, MenuSubtitleText, 18, new Color(0.79f, 0.76f, 0.82f), TextAnchor.MiddleCenter);
        SetRect(subtitleText.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero, new Vector2(620f, 40f));

        Button startButton = CreateButton("StartButton", centerPlate.transform, StartRunText, new Vector2(0.5f, 0.39f), new Color(0.32f, 0.12f, 0.13f, 0.96f));
        startButton.onClick.AddListener(StartNewRun);

        continueButton = CreateButton("ContinueButton", centerPlate.transform, ContinueRunText, new Vector2(0.5f, 0.27f), new Color(0.16f, 0.18f, 0.24f, 0.96f));
        continueButton.onClick.AddListener(ContinueRun);

        Button quitButton = CreateButton("QuitButton", centerPlate.transform, QuitText, new Vector2(0.5f, 0.15f), new Color(0.12f, 0.12f, 0.14f, 0.92f));
        quitButton.onClick.AddListener(QuitGame);

        floorLabel = CreateText("FloorLabel", canvasObject.transform, BuildFloorLabelText(1), 20, new Color(0.92f, 0.94f, 0.96f), TextAnchor.MiddleRight);
        SetRect(floorLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(180f, 30f));

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

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchor, Color baseColor)
    {
        GameObject buttonObject = CreatePanel(name, parent, baseColor);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchor, anchor, Vector2.zero, new Vector2(250f, 56f));

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = baseColor * 1.15f;
        colors.pressedColor = baseColor * 0.75f;
        colors.disabledColor = new Color(0.16f, 0.16f, 0.16f, 0.6f);
        button.colors = colors;

        Text text = CreateText("Label", buttonObject.transform, label, 22, new Color(0.95f, 0.92f, 0.82f), TextAnchor.MiddleCenter);
        StretchToFullScreen(text.rectTransform);

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
