using UnityEngine;
using UnityEngine.UI;

public class AlignmentHUD : MonoBehaviour
{
    public CorruptionSystem corruptionSystem;

    [Header("Layout")]
    [SerializeField] private Vector2 panelAnchorPosition = new Vector2(20f, -72f);
    [SerializeField] private Vector2 panelSize = new Vector2(304f, 164f);
    [SerializeField] private Vector2 barSize = new Vector2(144f, 14f);

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.06f, 0.055f, 0.07f, 0.88f);
    [SerializeField] private Color frameColor = new Color(0.32f, 0.25f, 0.22f, 0.75f);
    [SerializeField] private Color sectionLineColor = new Color(0.54f, 0.44f, 0.36f, 0.35f);
    [SerializeField] private Color titleColor = new Color(0.92f, 0.86f, 0.76f, 1f);
    [SerializeField] private Color labelColor = new Color(0.82f, 0.78f, 0.74f, 1f);
    [SerializeField] private Color oathColor = new Color(0.74f, 0.8f, 0.88f, 1f);
    [SerializeField] private Color corruptionColor = new Color(0.78f, 0.26f, 0.32f, 1f);
    [SerializeField] private Color neutralBiasColor = new Color(0.78f, 0.74f, 0.68f, 1f);
    [SerializeField] private Color oathBiasColor = new Color(0.78f, 0.84f, 0.95f, 1f);
    [SerializeField] private Color fallenBiasColor = new Color(0.92f, 0.42f, 0.46f, 1f);

    private Canvas overlayCanvas;
    private Font uiFont;

    private Text oathValueText;
    private Text corruptionValueText;
    private Text biasValueText;
    private Image oathFillImage;
    private Image corruptionFillImage;
    private Image biasAccentImage;

    private void Start()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        BuildHudIfNeeded();
        RefreshDisplay();
    }

    private void Update()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        RefreshDisplay();
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
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.6f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = CreateImageObject("AlignmentPanel", canvasObject.transform, panelColor);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = panelAnchorPosition;
        panelRect.sizeDelta = panelSize;

        CreateFrame(panelObject.transform);
        CreateSectionLine(panelObject.transform, new Vector2(14f, -34f), new Vector2(panelSize.x - 28f, 1f));
        CreateSectionLine(panelObject.transform, new Vector2(14f, -112f), new Vector2(panelSize.x - 28f, 1f));

        CreateText("Header", panelObject.transform, "BROKEN VOW", 15, titleColor, TextAnchor.UpperLeft, new Vector2(16f, -10f), new Vector2(180f, 22f), FontStyle.Bold);

        CreateBarBlock(
            panelObject.transform,
            "Oath",
            new Vector2(16f, -48f),
            oathColor,
            out oathValueText,
            out oathFillImage);

        CreateBarBlock(
            panelObject.transform,
            "Corruption",
            new Vector2(16f, -82f),
            corruptionColor,
            out corruptionValueText,
            out corruptionFillImage);

        CreateText("BiasLabel", panelObject.transform, "CURRENT TENDENCY", 11, labelColor, TextAnchor.UpperLeft, new Vector2(16f, -122f), new Vector2(130f, 18f), FontStyle.Normal);
        biasValueText = CreateText("BiasValue", panelObject.transform, "Balanced", 20, neutralBiasColor, TextAnchor.MiddleLeft, new Vector2(16f, -142f), new Vector2(196f, 28f), FontStyle.Bold);

        GameObject accentObject = CreateImageObject("BiasAccent", panelObject.transform, neutralBiasColor);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 1f);
        accentRect.anchoredPosition = new Vector2(panelSize.x - 68f, -132f);
        accentRect.sizeDelta = new Vector2(42f, 24f);
        biasAccentImage = accentObject.GetComponent<Image>();

        GameObject accentCore = CreateImageObject("BiasAccentCore", accentObject.transform, panelColor * new Color(1.2f, 1.2f, 1.2f, 1f));
        StretchRect(accentCore.GetComponent<RectTransform>(), new Vector2(5f, 5f));
    }

    private void RefreshDisplay()
    {
        if (corruptionSystem == null || oathValueText == null || corruptionValueText == null || biasValueText == null)
        {
            return;
        }

        oathValueText.text = corruptionSystem.oathValue.ToString();
        corruptionValueText.text = corruptionSystem.corruptionValue.ToString();

        if (oathFillImage != null)
        {
            oathFillImage.fillAmount = corruptionSystem.GetOathRatio();
        }

        if (corruptionFillImage != null)
        {
            corruptionFillImage.fillAmount = corruptionSystem.GetCorruptionRatio();
        }

        CorruptionSystem.AlignmentBias bias = corruptionSystem.GetAlignmentBias();
        string biasLabel = "Balanced";
        Color biasColor = neutralBiasColor;

        if (bias == CorruptionSystem.AlignmentBias.Oathbound)
        {
            biasLabel = "Oathbound";
            biasColor = oathBiasColor;
        }
        else if (bias == CorruptionSystem.AlignmentBias.Fallen)
        {
            biasLabel = "Fallen";
            biasColor = fallenBiasColor;
        }

        biasValueText.text = biasLabel;
        biasValueText.color = biasColor;

        if (biasAccentImage != null)
        {
            biasAccentImage.color = biasColor;
        }
    }

    private void CreateBarBlock(Transform parent, string label, Vector2 anchoredPosition, Color fillColor, out Text valueText, out Image fillImage)
    {
        CreateText(label + "Label", parent, label.ToUpperInvariant(), 11, labelColor, TextAnchor.UpperLeft, anchoredPosition, new Vector2(94f, 18f), FontStyle.Normal);
        valueText = CreateText(label + "Value", parent, "0", 17, fillColor, TextAnchor.UpperRight, anchoredPosition + new Vector2(0f, -1f), new Vector2(250f, 20f), FontStyle.Bold);

        GameObject barBackground = CreateImageObject(label + "BarBG", parent, new Color(0.12f, 0.1f, 0.1f, 0.95f));
        RectTransform backgroundRect = barBackground.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = anchoredPosition + new Vector2(118f, -1f);
        backgroundRect.sizeDelta = barSize;

        Outline backgroundOutline = barBackground.AddComponent<Outline>();
        backgroundOutline.effectColor = new Color(0.05f, 0.02f, 0.02f, 0.8f);
        backgroundOutline.effectDistance = new Vector2(1f, -1f);

        GameObject barFill = CreateImageObject(label + "BarFill", barBackground.transform, fillColor);
        RectTransform fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = barFill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 0f;
    }

    private void CreateFrame(Transform parent)
    {
        CreateImageWithRect(parent, "TopFrame", frameColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, -2f));
        CreateImageWithRect(parent, "BottomFrame", frameColor * new Color(1f, 1f, 1f, 0.7f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f), new Vector2(0f, 0f));
        CreateImageWithRect(parent, "LeftFrame", frameColor * new Color(1f, 1f, 1f, 0.7f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(0f, 0f));
        CreateImageWithRect(parent, "RightFrame", frameColor * new Color(1f, 1f, 1f, 0.7f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-2f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
    }

    private void CreateSectionLine(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject line = CreateImageObject("SectionLine", parent, sectionLineColor);
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, FontStyle fontStyle)
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
        text.supportRichText = false;
        text.raycastTarget = false;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.4f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private GameObject CreateImageObject(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        image.type = Image.Type.Sliced;
        return imageObject;
    }

    private void CreateImageWithRect(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Vector2 sizeDelta)
    {
        GameObject imageObject = CreateImageObject(name, parent, color);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.sizeDelta = sizeDelta;
    }

    private void StretchRect(RectTransform rect, Vector2 padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = padding;
        rect.offsetMax = -padding;
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
