using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorFlowManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveStage
    {
        public string displayName = "Ashen Approach";
        [Range(0f, 1f)] public float startTimeNormalized;
        public float spawnInterval = 2f;
        public int batchCount = 1;
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float scaleMultiplier = 1f;
        [Range(0f, 1f)] public float eliteChance;
        public Color enemyTint = Color.white;
        public Color eliteTint = new Color(0.78f, 0.26f, 0.3f, 1f);
        public string corruptionMessage = "";
        public int corruptionIncrease;
    }

    public SpriteRenderer groundRenderer;
    public EnemySpawner enemySpawner;
    public Transform playerTransform;
    public Sprite[] floorBackgrounds;
    public float floorDuration = 150f;
    public float exitDistance = 3.5f;
    public Vector3 playerSpawnPosition = Vector3.zero;
    public List<WaveStage> waveStages = new List<WaveStage>();

    private float remainingTime;
    private float elapsedTime;
    private bool exitSpawned;
    private FloorExitPortal activePortal;
    private Canvas hudCanvas;
    private Text timerLabel;
    private Text waveLabel;
    private Text eventLabel;
    private Text promptLabel;
    private Text titleCardFloorLabel;
    private Text titleCardNameLabel;
    private Text titleCardFlavorLabel;
    private Font uiFont;
    private Sprite cachedWhiteSprite;
    private int currentWaveIndex = -1;
    private Coroutine eventRoutine;
    private Coroutine transitionRoutine;
    private Coroutine titleCardRoutine;
    private CorruptionSystem corruptionSystem;
    private Image fadeOverlay;
    private GameObject promptPlate;
    private GameObject titleCardPlate;
    private bool transitionInProgress;

    private void Start()
    {
        EnsureReferences();
        EnsureDefaultWaveStages();
        BuildHudIfNeeded();
        BeginFloor();
    }

    private void Update()
    {
        if (exitSpawned || transitionInProgress)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        remainingTime = Mathf.Max(0f, floorDuration - elapsedTime);
        UpdateWaveStage();
        UpdateTimerLabel();

        if (remainingTime <= 0f)
        {
            SpawnExitPortal();
        }
    }

    public void BeginFloor()
    {
        EnsureReferences();
        EnsureDefaultWaveStages();
        RemovePortal();
        SetPortalPromptVisible(false);
        ApplyFloorBackground();
        ClearFloorActors();
        ResetPlayerPosition();
        remainingTime = floorDuration;
        elapsedTime = 0f;
        exitSpawned = false;
        transitionInProgress = false;
        currentWaveIndex = -1;

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawnEnabled(true);
        }

        UpdateWaveStage();
        UpdateTimerLabel();
        ShowFloorIntroTitle();
    }

    public void AdvanceToNextFloor()
    {
        if (DemoRunManager.Instance != null)
        {
            DemoRunManager.Instance.CompleteFloor();
        }

        BeginFloor();
    }

    public void TryEnterExitPortal(FloorExitPortal portal)
    {
        if (transitionInProgress || portal == null || portal != activePortal)
        {
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(TransitionToNextFloorRoutine());
    }

    public void SetPortalPromptVisible(bool visible)
    {
        if (promptPlate == null)
        {
            return;
        }

        promptPlate.SetActive(visible && exitSpawned && !transitionInProgress);
    }

    private void EnsureReferences()
    {
        if (groundRenderer == null)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundRenderer = ground.GetComponent<SpriteRenderer>();
            }
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        if (playerTransform == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }
    }

    private void EnsureDefaultWaveStages()
    {
        if (waveStages != null && waveStages.Count > 0)
        {
            return;
        }

        waveStages = new List<WaveStage>
        {
            new WaveStage
            {
                displayName = "Ashen Approach",
                startTimeNormalized = 0f,
                spawnInterval = 2.15f,
                batchCount = 1,
                healthMultiplier = 0.9f,
                speedMultiplier = 0.92f,
                scaleMultiplier = 0.95f,
                eliteChance = 0f,
                enemyTint = new Color(0.84f, 0.84f, 0.84f, 1f),
                corruptionMessage = "Corruption is faint. The enemy line is still probing."
            },
            new WaveStage
            {
                displayName = "Corruption Rising",
                startTimeNormalized = 0.28f,
                spawnInterval = 1.65f,
                batchCount = 1,
                healthMultiplier = 1.1f,
                speedMultiplier = 1.08f,
                scaleMultiplier = 1f,
                eliteChance = 0.08f,
                enemyTint = new Color(0.76f, 0.86f, 0.76f, 1f),
                eliteTint = new Color(0.68f, 0.24f, 0.26f, 1f),
                corruptionMessage = "Corruption deepens. The horde grows more restless.",
                corruptionIncrease = 8
            },
            new WaveStage
            {
                displayName = "Fractured Vows",
                startTimeNormalized = 0.58f,
                spawnInterval = 1.2f,
                batchCount = 2,
                healthMultiplier = 1.35f,
                speedMultiplier = 1.16f,
                scaleMultiplier = 1.08f,
                eliteChance = 0.18f,
                enemyTint = new Color(0.84f, 0.72f, 0.72f, 1f),
                eliteTint = new Color(0.8f, 0.22f, 0.28f, 1f),
                corruptionMessage = "The vow fractures spread. The battlefield slips from control.",
                corruptionIncrease = 12
            },
            new WaveStage
            {
                displayName = "Abyss Pressure",
                startTimeNormalized = 0.82f,
                spawnInterval = 0.82f,
                batchCount = 2,
                healthMultiplier = 1.7f,
                speedMultiplier = 1.3f,
                scaleMultiplier = 1.14f,
                eliteChance = 0.35f,
                enemyTint = new Color(0.92f, 0.78f, 0.78f, 1f),
                eliteTint = new Color(0.92f, 0.18f, 0.24f, 1f),
                corruptionMessage = "The abyss presses in. Elite enemies are now emerging.",
                corruptionIncrease = 18
            }
        };
    }

    private void ApplyFloorBackground()
    {
        if (groundRenderer == null || floorBackgrounds == null || floorBackgrounds.Length == 0)
        {
            return;
        }

        int floorIndex = 0;
        if (DemoRunManager.Instance != null)
        {
            floorIndex = Mathf.Max(0, DemoRunManager.Instance.CurrentFloor - 1);
        }

        groundRenderer.sprite = floorBackgrounds[floorIndex % floorBackgrounds.Length];
    }

    private void SpawnExitPortal()
    {
        if (exitSpawned)
        {
            return;
        }

        exitSpawned = true;
        if (enemySpawner != null)
        {
            enemySpawner.SetSpawnEnabled(false);
        }

        ShowFloorClearTitle();
        ShowEventMessage("The gate to the next floor has opened.");

        Vector3 portalPosition = playerSpawnPosition + Vector3.up * exitDistance;
        if (playerTransform != null)
        {
            portalPosition = playerTransform.position + Vector3.up * exitDistance;
        }

        GameObject portalRoot = new GameObject("ExitPortal");
        portalRoot.transform.position = portalPosition;
        portalRoot.transform.localScale = new Vector3(1.4f, 2.2f, 1f);

        SpriteRenderer frameRenderer = portalRoot.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = GetWhiteSprite();
        frameRenderer.color = new Color(0.15f, 0.08f, 0.05f, 0.95f);
        frameRenderer.sortingOrder = 4;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(portalRoot.transform, false);
        glow.transform.localScale = new Vector3(0.65f, 0.78f, 1f);
        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = GetWhiteSprite();
        glowRenderer.color = new Color(0.22f, 0.85f, 0.92f, 0.92f);
        glowRenderer.sortingOrder = 5;

        GameObject textObject = new GameObject("PortalLabel");
        textObject.transform.SetParent(portalRoot.transform, false);
        textObject.transform.localPosition = new Vector3(0f, -0.95f, 0f);
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = "NEXT";
        textMesh.fontSize = 28;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(0.94f, 0.95f, 1f, 1f);

        BoxCollider2D trigger = portalRoot.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(0.95f, 1.1f);

        activePortal = portalRoot.AddComponent<FloorExitPortal>();
        activePortal.Initialize(this);
        UpdateTimerLabel();
    }

    private void RemovePortal()
    {
        if (activePortal != null)
        {
            Destroy(activePortal.gameObject);
            activePortal = null;
        }
    }

    private void ClearFloorActors()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }

        ExpOrb[] orbs = FindObjectsOfType<ExpOrb>();
        for (int i = 0; i < orbs.Length; i++)
        {
            Destroy(orbs[i].gameObject);
        }

        Bullet[] bullets = FindObjectsOfType<Bullet>();
        for (int i = 0; i < bullets.Length; i++)
        {
            Destroy(bullets[i].gameObject);
        }
    }

    private void ResetPlayerPosition()
    {
        if (playerTransform == null)
        {
            return;
        }

        playerTransform.position = playerSpawnPosition;
    }

    private void BuildHudIfNeeded()
    {
        if (hudCanvas != null)
        {
            return;
        }

        uiFont = LoadPreferredFont();

        GameObject canvasObject = new GameObject("FloorHudCanvas");
        canvasObject.transform.SetParent(transform, false);
        hudCanvas = canvasObject.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 320;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.6f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject plate = CreateImageObject("TopPlate", canvasObject.transform, new Color(0.06f, 0.055f, 0.07f, 0.9f), true);
        RectTransform plateRect = plate.GetComponent<RectTransform>();
        plateRect.anchorMin = new Vector2(0.5f, 1f);
        plateRect.anchorMax = new Vector2(0.5f, 1f);
        plateRect.pivot = new Vector2(0.5f, 1f);
        plateRect.anchoredPosition = new Vector2(0f, -16f);
        plateRect.sizeDelta = new Vector2(340f, 104f);

        CreateFrameLine(plate.transform, "PlateTop", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -2f), new Vector2(0f, 0f), new Color(0.36f, 0.29f, 0.23f, 0.85f));
        CreateFrameLine(plate.transform, "PlateBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f), new Color(0.36f, 0.29f, 0.23f, 0.55f));
        CreateFrameLine(plate.transform, "PlateLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(2f, 0f), new Color(0.36f, 0.29f, 0.23f, 0.55f));
        CreateFrameLine(plate.transform, "PlateRight", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-2f, 0f), new Vector2(0f, 0f), new Color(0.36f, 0.29f, 0.23f, 0.55f));

        timerLabel = CreateText(
            "FloorTimerLabel",
            plate.transform,
            "02:30",
            42,
            new Color(0.95f, 0.92f, 0.84f, 1f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, -18f),
            new Vector2(300f, 44f),
            FontStyle.Bold,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f));

        waveLabel = CreateText(
            "WaveLabel",
            plate.transform,
            "Stage: Ashen Approach",
            16,
            new Color(0.77f, 0.74f, 0.7f, 1f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, -64f),
            new Vector2(290f, 24f),
            FontStyle.Normal,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f));

        GameObject eventPlate = CreateImageObject("EventPlate", canvasObject.transform, new Color(0.09f, 0.06f, 0.07f, 0.78f), true);
        RectTransform eventPlateRect = eventPlate.GetComponent<RectTransform>();
        eventPlateRect.anchorMin = new Vector2(0.5f, 1f);
        eventPlateRect.anchorMax = new Vector2(0.5f, 1f);
        eventPlateRect.pivot = new Vector2(0.5f, 1f);
        eventPlateRect.anchoredPosition = new Vector2(0f, -128f);
        eventPlateRect.sizeDelta = new Vector2(620f, 42f);

        eventLabel = CreateText(
            "CorruptionEventLabel",
            eventPlate.transform,
            string.Empty,
            20,
            new Color(0.96f, 0.86f, 0.88f, 0f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, 0f),
            new Vector2(580f, 28f),
            FontStyle.Bold,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));

        promptPlate = CreateImageObject("PortalPromptPlate", canvasObject.transform, new Color(0.09f, 0.06f, 0.07f, 0.84f), true);
        RectTransform promptPlateRect = promptPlate.GetComponent<RectTransform>();
        promptPlateRect.anchorMin = new Vector2(0.5f, 0f);
        promptPlateRect.anchorMax = new Vector2(0.5f, 0f);
        promptPlateRect.pivot = new Vector2(0.5f, 0f);
        promptPlateRect.anchoredPosition = new Vector2(0f, 52f);
        promptPlateRect.sizeDelta = new Vector2(360f, 42f);

        promptLabel = CreateText(
            "PortalPromptLabel",
            promptPlate.transform,
            "Press E to Descend",
            20,
            new Color(0.97f, 0.92f, 0.84f, 1f),
            TextAnchor.MiddleCenter,
            Vector2.zero,
            new Vector2(320f, 26f),
            FontStyle.Bold,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        promptPlate.SetActive(false);

        titleCardPlate = CreateImageObject("FloorTitleCard", canvasObject.transform, new Color(0.07f, 0.04f, 0.05f, 0.88f), true);
        RectTransform titlePlateRect = titleCardPlate.GetComponent<RectTransform>();
        titlePlateRect.anchorMin = new Vector2(0.5f, 0.5f);
        titlePlateRect.anchorMax = new Vector2(0.5f, 0.5f);
        titlePlateRect.pivot = new Vector2(0.5f, 0.5f);
        titlePlateRect.anchoredPosition = new Vector2(0f, 18f);
        titlePlateRect.sizeDelta = new Vector2(560f, 136f);
        CreateFrameLine(titleCardPlate.transform, "TitleTop", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -2f), new Vector2(-20f, 0f), new Color(0.62f, 0.47f, 0.32f, 0.82f));
        CreateFrameLine(titleCardPlate.transform, "TitleBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 0f), new Vector2(-20f, 2f), new Color(0.62f, 0.47f, 0.32f, 0.45f));

        titleCardFloorLabel = CreateText(
            "TitleCardFloorLabel",
            titleCardPlate.transform,
            "FLOOR 1",
            22,
            new Color(0.87f, 0.76f, 0.62f, 1f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, 42f),
            new Vector2(420f, 28f),
            FontStyle.Bold,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));

        titleCardNameLabel = CreateText(
            "TitleCardNameLabel",
            titleCardPlate.transform,
            "Ashen Chapel",
            34,
            new Color(0.97f, 0.92f, 0.84f, 1f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, 8f),
            new Vector2(500f, 42f),
            FontStyle.Bold,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));

        titleCardFlavorLabel = CreateText(
            "TitleCardFlavorLabel",
            titleCardPlate.transform,
            "Corruption deepens...",
            18,
            new Color(0.84f, 0.72f, 0.74f, 1f),
            TextAnchor.MiddleCenter,
            new Vector2(0f, -34f),
            new Vector2(470f, 26f),
            FontStyle.Italic,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f));
        titleCardPlate.SetActive(false);

        GameObject fadeObject = CreateImageObject("FloorFadeOverlay", canvasObject.transform, new Color(0.02f, 0.01f, 0.02f, 0f), false);
        RectTransform fadeRect = fadeObject.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        fadeOverlay = fadeObject.GetComponent<Image>();
        fadeOverlay.raycastTarget = false;
    }

    private void UpdateTimerLabel()
    {
        if (timerLabel == null)
        {
            return;
        }

        if (exitSpawned)
        {
            timerLabel.text = "GATE OPEN";
            if (waveLabel != null)
            {
                waveLabel.text = "Stage: Descend";
            }
            return;
        }

        int displaySeconds = Mathf.CeilToInt(remainingTime);
        int minutes = displaySeconds / 60;
        int seconds = displaySeconds % 60;
        timerLabel.text = minutes.ToString("00") + ":" + seconds.ToString("00");

        if (waveLabel != null && currentWaveIndex >= 0 && currentWaveIndex < waveStages.Count)
        {
            waveLabel.text = "Stage: " + waveStages[currentWaveIndex].displayName;
        }
    }

    private void UpdateWaveStage()
    {
        if (waveStages == null || waveStages.Count == 0)
        {
            return;
        }

        float normalizedTime = floorDuration <= 0f ? 1f : Mathf.Clamp01(elapsedTime / floorDuration);
        int newWaveIndex = 0;
        for (int i = 0; i < waveStages.Count; i++)
        {
            if (normalizedTime >= waveStages[i].startTimeNormalized)
            {
                newWaveIndex = i;
            }
        }

        if (newWaveIndex == currentWaveIndex)
        {
            return;
        }

        currentWaveIndex = newWaveIndex;
        WaveStage stage = waveStages[currentWaveIndex];

        if (enemySpawner != null)
        {
            enemySpawner.ApplyWaveSettings(
                stage.spawnInterval,
                stage.batchCount,
                stage.healthMultiplier,
                stage.speedMultiplier,
                stage.scaleMultiplier,
                stage.eliteChance,
                stage.enemyTint,
                stage.eliteTint
            );
        }

        if (!string.IsNullOrEmpty(stage.corruptionMessage))
        {
            ShowEventMessage(stage.corruptionMessage);
        }

        if (stage.corruptionIncrease > 0 && corruptionSystem != null)
        {
            corruptionSystem.AddCorruption(stage.corruptionIncrease);
        }
    }

    private void ShowEventMessage(string message)
    {
        if (eventLabel == null)
        {
            return;
        }

        if (eventRoutine != null)
        {
            StopCoroutine(eventRoutine);
        }

        eventRoutine = StartCoroutine(EventMessageRoutine(message));
    }

    private IEnumerator EventMessageRoutine(string message)
    {
        eventLabel.text = message;
        Color visibleColor = new Color(0.95f, 0.86f, 0.88f, 0.96f);
        eventLabel.color = visibleColor;
        yield return new WaitForSeconds(1.75f);

        float fadeDuration = 0.6f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            eventLabel.color = new Color(visibleColor.r, visibleColor.g, visibleColor.b, Mathf.Lerp(visibleColor.a, 0f, t));
            yield return null;
        }

        eventLabel.text = string.Empty;
        eventRoutine = null;
    }

    private void ShowFloorClearTitle()
    {
        ShowTitleCard(
            "FLOOR CLEARED",
            "The path below yawns open",
            "The gate to deeper ruins now answers your vow.",
            1.6f);
    }

    private void ShowFloorIntroTitle()
    {
        int floorNumber = DemoRunManager.Instance != null ? Mathf.Max(1, DemoRunManager.Instance.CurrentFloor) : 1;
        ShowTitleCard(
            "FLOOR " + floorNumber,
            GetFloorThemeName(floorNumber),
            GetFloorFlavorText(floorNumber),
            1.8f);
    }

    private void ShowTitleCard(string header, string title, string flavor, float holdDuration)
    {
        if (titleCardPlate == null || titleCardFloorLabel == null || titleCardNameLabel == null || titleCardFlavorLabel == null)
        {
            return;
        }

        if (titleCardRoutine != null)
        {
            StopCoroutine(titleCardRoutine);
        }

        titleCardRoutine = StartCoroutine(TitleCardRoutine(header, title, flavor, holdDuration));
    }

    private IEnumerator TitleCardRoutine(string header, string title, string flavor, float holdDuration)
    {
        titleCardFloorLabel.text = header;
        titleCardNameLabel.text = title;
        titleCardFlavorLabel.text = flavor;
        titleCardPlate.SetActive(true);

        CanvasGroup group = titleCardPlate.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = titleCardPlate.AddComponent<CanvasGroup>();
        }

        yield return StartCoroutine(FadeCanvasGroupRoutine(group, 0f, 1f, 0.2f));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeCanvasGroupRoutine(group, 1f, 0f, 0.35f));

        titleCardPlate.SetActive(false);
        titleCardRoutine = null;
    }

    private IEnumerator TransitionToNextFloorRoutine()
    {
        transitionInProgress = true;
        SetPortalPromptVisible(false);

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawnEnabled(false);
        }

        yield return StartCoroutine(FadeOverlayRoutine(0f, 0.88f, 0.28f));

        AdvanceToNextFloor();
        ShowEventMessage("You descend deeper into the broken sanctuary.");

        yield return new WaitForSeconds(0.08f);
        yield return StartCoroutine(FadeOverlayRoutine(0.88f, 0f, 0.32f));

        transitionInProgress = false;
        transitionRoutine = null;
    }

    private IEnumerator FadeCanvasGroupRoutine(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator FadeOverlayRoutine(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeOverlay == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color color = fadeOverlay.color;
        color.a = fromAlpha;
        fadeOverlay.color = color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = toAlpha;
        fadeOverlay.color = color;
    }

    private GameObject CreateImageObject(string name, Transform parent, Color color, bool sliced)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        return imageObject;
    }

    private void CreateFrameLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject line = CreateImageObject(name, parent, color, false);
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private Text CreateText(
        string name,
        Transform parent,
        string content,
        int fontSize,
        Color color,
        TextAnchor alignment,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        FontStyle fontStyle,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.text = content;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        return text;
    }

    private Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite == null)
        {
            cachedWhiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        return cachedWhiteSprite;
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

    private static string GetFloorThemeName(int floorNumber)
    {
        switch ((Mathf.Max(1, floorNumber) - 1) % 3)
        {
            case 0:
                return "Ashen Chapel";
            case 1:
                return "Blighted Wilds";
            default:
                return "Sunken Badlands";
        }
    }

    private static string GetFloorFlavorText(int floorNumber)
    {
        if (floorNumber <= 1)
        {
            return "The air is still. The first vow has not yet broken.";
        }

        if (floorNumber <= 3)
        {
            return "Corruption deepens. The old sanctity no longer answers.";
        }

        if (floorNumber <= 6)
        {
            return "The deeper halls remember every broken oath.";
        }

        return "Only ash, ruin, and a dying light remain below.";
    }
}
