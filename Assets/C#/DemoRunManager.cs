using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class DemoRunManager : MonoBehaviour
{
    private const string GameplaySceneName = "SampleScene";

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
            playerController.speed = pendingLoadData.moveSpeed;
            playerController.attackInterval = pendingLoadData.attackInterval;
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
            corruptionSystem.currentCorruption = Mathf.Clamp(
                pendingLoadData.currentCorruption,
                0,
                corruptionSystem.maxCorruption
            );
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
            data.moveSpeed = playerController.speed;
            data.attackInterval = playerController.attackInterval;
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
            data.currentCorruption = corruptionSystem.currentCorruption;
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

        Text titleText = CreateText(
            "Title",
            overlay.transform,
            "破誓圣骑士",
            38,
            new Color(0.95f, 0.92f, 0.82f),
            TextAnchor.MiddleCenter
        );
        SetRect(titleText.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(420f, 60f));

        Text subtitleText = CreateText(
            "Subtitle",
            overlay.transform,
            "在守誓与腐化之间抉择，继续深入地牢",
            18,
            new Color(0.79f, 0.83f, 0.88f),
            TextAnchor.MiddleCenter
        );
        SetRect(subtitleText.rectTransform, new Vector2(0.5f, 0.61f), new Vector2(0.5f, 0.61f), Vector2.zero, new Vector2(620f, 40f));

        Button startButton = CreateButton("StartButton", overlay.transform, "开始冒险", new Vector2(0.5f, 0.47f));
        startButton.onClick.AddListener(StartNewRun);

        continueButton = CreateButton("ContinueButton", overlay.transform, "继续征途", new Vector2(0.5f, 0.37f));
        continueButton.onClick.AddListener(ContinueRun);

        Button quitButton = CreateButton("QuitButton", overlay.transform, "退出游戏", new Vector2(0.5f, 0.27f));
        quitButton.onClick.AddListener(QuitGame);

        floorLabel = CreateText(
            "FloorLabel",
            canvasObject.transform,
            "第 1 层",
            20,
            new Color(0.92f, 0.94f, 0.96f),
            TextAnchor.MiddleRight
        );
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

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchor)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.18f, 0.21f, 0.27f, 0.94f));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchor, anchor, Vector2.zero, new Vector2(220f, 52f));

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.18f, 0.21f, 0.27f, 0.94f);
        colors.highlightedColor = new Color(0.28f, 0.32f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.13f, 0.18f, 1f);
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

        floorLabel.text = "第 " + Mathf.Max(1, CurrentFloor) + " 层";
        floorLabel.gameObject.SetActive(HasActiveRun);
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
