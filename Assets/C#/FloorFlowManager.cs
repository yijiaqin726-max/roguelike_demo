using UnityEngine;
using UnityEngine.UI;

public class FloorFlowManager : MonoBehaviour
{
    public SpriteRenderer groundRenderer;
    public EnemySpawner enemySpawner;
    public Transform playerTransform;
    public Sprite[] floorBackgrounds;
    public float floorDuration = 40f;
    public float exitDistance = 3.5f;
    public Vector3 playerSpawnPosition = Vector3.zero;

    private float remainingTime;
    private bool exitSpawned;
    private FloorExitPortal activePortal;
    private Canvas hudCanvas;
    private Text timerLabel;
    private Font uiFont;
    private Sprite cachedWhiteSprite;

    private void Start()
    {
        EnsureReferences();
        BuildHudIfNeeded();
        BeginFloor();
    }

    private void Update()
    {
        if (exitSpawned)
        {
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);
        UpdateTimerLabel();

        if (remainingTime <= 0f)
        {
            SpawnExitPortal();
        }
    }

    public void BeginFloor()
    {
        EnsureReferences();
        RemovePortal();
        ApplyFloorBackground();
        ClearFloorActors();
        ResetPlayerPosition();
        remainingTime = floorDuration;
        exitSpawned = false;

        if (enemySpawner != null)
        {
            enemySpawner.SetSpawnEnabled(true);
        }

        UpdateTimerLabel();
    }

    public void AdvanceToNextFloor()
    {
        if (DemoRunManager.Instance != null)
        {
            DemoRunManager.Instance.CompleteFloor();
        }

        BeginFloor();
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
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject labelObject = new GameObject("FloorTimerLabel");
        labelObject.transform.SetParent(canvasObject.transform, false);
        timerLabel = labelObject.AddComponent<Text>();
        timerLabel.font = uiFont;
        timerLabel.fontSize = 20;
        timerLabel.alignment = TextAnchor.UpperCenter;
        timerLabel.color = new Color(0.94f, 0.94f, 0.98f, 1f);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -22f);
        rect.sizeDelta = new Vector2(420f, 36f);
    }

    private void UpdateTimerLabel()
    {
        if (timerLabel == null)
        {
            return;
        }

        if (exitSpawned)
        {
            timerLabel.text = "Floor cleared - enter the portal";
            return;
        }

        int displaySeconds = Mathf.CeilToInt(remainingTime);
        timerLabel.text = "Time left: " + displaySeconds + "s";
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
}
