using UnityEngine;
using UnityEngine.UI;

public class AlignmentHUD : MonoBehaviour
{
    public CorruptionSystem corruptionSystem;

    private Canvas overlayCanvas;
    private Text alignmentText;
    private Font uiFont;

    private void Start()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        BuildHudIfNeeded();
    }

    private void Update()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        if (alignmentText == null || corruptionSystem == null)
        {
            return;
        }

        alignmentText.text = corruptionSystem.GetAlignmentSummary();
    }

    private void BuildHudIfNeeded()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        uiFont = LoadPreferredFont();

        GameObject canvasObject = new GameObject("AlignmentHudCanvas");
        canvasObject.transform.SetParent(transform, false);
        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 330;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("AlignmentText");
        textObject.transform.SetParent(canvasObject.transform, false);
        alignmentText = textObject.AddComponent<Text>();
        alignmentText.font = uiFont;
        alignmentText.fontSize = 18;
        alignmentText.alignment = TextAnchor.UpperLeft;
        alignmentText.color = new Color(0.93f, 0.92f, 0.88f, 1f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -48f);
        rect.sizeDelta = new Vector2(420f, 48f);
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
