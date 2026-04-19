using UnityEngine;
using UnityEngine.UI;

public static class StatusHudCardLayout
{
    public const string CardName = "StatusCard";
    private const int CardTitleFontSize = 16;
    private const int PrimaryLabelFontSize = 13;
    private const int PrimaryValueFontSize = 17;
    private const int SecondaryLabelFontSize = 12;
    private const int SecondaryValueFontSize = 14;
    private const int TertiaryLabelFontSize = 10;
    private const int TertiaryValueFontSize = 15;
    private const float RowSpacing = 8f;
    private const float PrimaryRowHeight = 30f;
    private const float SecondaryRowHeight = 24f;
    private const float PrimaryBarHeight = 18f;
    private const float SecondaryBarHeight = 12f;
    private const string BackPlateName = "BackPlate";
    private const string InnerPanelName = "InnerPanel";
    private const string HeaderBandName = "HeaderBand";
    private const string CornerAccentTopName = "CornerAccentTop";
    private const string CornerAccentBottomName = "CornerAccentBottom";
    private const string TitleName = "Title";
    private const string DividerName = "TitleDivider";
    private const string RowsName = "Rows";
    private const string TendencySectionName = "TendencySection";
    private const string TendencyLabelName = "TendencyLabel";
    private const string TendencyValueName = "TendencyValue";

    private static Font cachedFont;

    public static Font LoadPreferredFont()
    {
        if (cachedFont != null)
        {
            return cachedFont;
        }

        string[] preferredFonts =
        {
            "Microsoft YaHei",
            "SimHei",
            "SimSun",
            "Arial Unicode MS",
            "Arial"
        };

        cachedFont = Font.CreateDynamicFontFromOSFont(preferredFonts, 24);
        if (cachedFont == null)
        {
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return cachedFont;
    }

    public static RectTransform EnsureCard(Font font, Color panelColor, Color frameColor, Color titleColor)
    {
        Transform leftTopRoot = GameplayHudLayout.EnsureLeftTopRoot();
        Transform existing = leftTopRoot.Find(CardName);
        RectTransform cardRect;

        if (existing == null)
        {
            GameObject cardObject = new GameObject(CardName, typeof(RectTransform), typeof(Image));
            cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.SetParent(leftTopRoot, false);
        }
        else
        {
            cardRect = existing as RectTransform;
        }

        if (cardRect == null)
        {
            return null;
        }

        cardRect.anchorMin = new Vector2(0f, 1f);
        cardRect.anchorMax = new Vector2(0f, 1f);
        cardRect.pivot = new Vector2(0f, 1f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(340f, 0f);

        Image background = cardRect.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0f);
        background.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        background.type = Image.Type.Sliced;

        Shadow shadow = cardRect.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = cardRect.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        shadow.effectDistance = new Vector2(0f, -6f);

        VerticalLayoutGroup layout = cardRect.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = cardRect.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        layout.padding = new RectOffset(18, 18, 15, 16);
        layout.spacing = 9f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = cardRect.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = cardRect.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement cardLayout = cardRect.GetComponent<LayoutElement>();
        if (cardLayout == null)
        {
            cardLayout = cardRect.gameObject.AddComponent<LayoutElement>();
        }
        cardLayout.preferredWidth = 340f;

        EnsureBackdrop(cardRect, panelColor, frameColor);
        EnsureTitle(cardRect, font, titleColor);
        EnsureDivider(cardRect);
        EnsureRowsContainer(cardRect);
        EnsureTendencySection(cardRect, font, titleColor, titleColor);

        return cardRect;
    }

    public static RectTransform EnsureStatusRow(
        Font font,
        string rowName,
        string labelText,
        Color labelColor,
        Color valueColor,
        out RectTransform barSlot,
        out Text valueText)
    {
        RectTransform cardRect = FindExistingCard();
        if (cardRect == null)
        {
            cardRect = EnsureCard(
                font,
                new Color(0.06f, 0.055f, 0.07f, 0.88f),
                new Color(0.32f, 0.25f, 0.22f, 0.75f),
                new Color(0.92f, 0.86f, 0.76f, 1f));
        }

        RectTransform rowsContainer = EnsureRowsContainer(cardRect);

        Transform existing = rowsContainer.Find(rowName);
        RectTransform rowRect;
        if (existing == null)
        {
            GameObject rowObject = new GameObject(rowName, typeof(RectTransform));
            rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(rowsContainer, false);
        }
        else
        {
            rowRect = existing as RectTransform;
        }

        HorizontalLayoutGroup rowLayout = rowRect.GetComponent<HorizontalLayoutGroup>();
        if (rowLayout == null)
        {
            rowLayout = rowRect.gameObject.AddComponent<HorizontalLayoutGroup>();
        }
        rowLayout.spacing = 10f;
        rowLayout.padding = new RectOffset(0, 0, 0, 0);
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlWidth = false;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;

        ContentSizeFitter rowFitter = rowRect.GetComponent<ContentSizeFitter>();
        if (rowFitter == null)
        {
            rowFitter = rowRect.gameObject.AddComponent<ContentSizeFitter>();
        }
        rowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement rowLayoutElement = rowRect.GetComponent<LayoutElement>();
        if (rowLayoutElement == null)
        {
            rowLayoutElement = rowRect.gameObject.AddComponent<LayoutElement>();
        }
        rowLayoutElement.minHeight = IsHealthRow(rowName) ? PrimaryRowHeight : SecondaryRowHeight;
        rowLayoutElement.preferredHeight = IsHealthRow(rowName) ? PrimaryRowHeight : SecondaryRowHeight;

        Text label = EnsureTextChild(
            rowRect,
            "Label",
            font,
            labelText,
            IsHealthRow(rowName) ? PrimaryLabelFontSize : SecondaryLabelFontSize,
            labelColor,
            FontStyle.Bold,
            TextAnchor.MiddleLeft);
        LayoutElement labelLayout = EnsureLayoutElement(label.gameObject);
        labelLayout.preferredWidth = 76f;
        labelLayout.minWidth = 76f;

        barSlot = EnsureRectChild(rowRect, "BarSlot");
        LayoutElement barLayout = EnsureLayoutElement(barSlot.gameObject);
        barLayout.flexibleWidth = 1f;
        barLayout.minWidth = 132f;
        barLayout.preferredWidth = 152f;
        barLayout.preferredHeight = IsHealthRow(rowName) ? PrimaryBarHeight : SecondaryBarHeight;

        valueText = EnsureTextChild(
            rowRect,
            "Value",
            font,
            "0",
            IsHealthRow(rowName) ? PrimaryValueFontSize : SecondaryValueFontSize,
            valueColor,
            FontStyle.Bold,
            TextAnchor.MiddleRight);
        LayoutElement valueLayout = EnsureLayoutElement(valueText.gameObject);
        valueLayout.preferredWidth = 72f;
        valueLayout.minWidth = 72f;

        rowRect.SetSiblingIndex(GetRowOrder(rowName));
        return rowRect;
    }

    public static void EnsureTendencyTexts(
        Font font,
        Color labelColor,
        Color valueColor,
        out Text tendencyLabel,
        out Text tendencyValue)
    {
        RectTransform card = FindExistingCard();
        if (card == null)
        {
            card = EnsureCard(
                font,
                new Color(0.06f, 0.055f, 0.07f, 0.88f),
                new Color(0.32f, 0.25f, 0.22f, 0.75f),
                new Color(0.92f, 0.86f, 0.76f, 1f));
        }

        RectTransform section = EnsureTendencySection(card, font, labelColor, valueColor);
        tendencyLabel = EnsureTextChild(section, TendencyLabelName, font, "CURRENT TENDENCY", TertiaryLabelFontSize, labelColor, FontStyle.Bold, TextAnchor.MiddleLeft);
        tendencyValue = EnsureTextChild(section, TendencyValueName, font, "Balanced", TertiaryValueFontSize, valueColor, FontStyle.Bold, TextAnchor.MiddleLeft);
    }

    public static Image EnsureBar(RectTransform barSlot, string barName, Color backgroundColor, Color fillColor)
    {
        RectTransform backgroundRect = EnsureRectChild(barSlot, barName + "Background");
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundRect.GetComponent<Image>();
        if (background == null)
        {
            background = backgroundRect.gameObject.AddComponent<Image>();
        }
        background.color = backgroundColor;
        background.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        background.type = Image.Type.Sliced;

        Outline outline = backgroundRect.GetComponent<Outline>();
        if (outline == null)
        {
            outline = backgroundRect.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.04f, 0.03f, 0.03f, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform fillRect = EnsureRectChild(backgroundRect, barName + "Fill");
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        Image fill = fillRect.GetComponent<Image>();
        if (fill == null)
        {
            fill = fillRect.gameObject.AddComponent<Image>();
        }
        fill.color = fillColor;
        fill.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0f;

        Shadow fillGlow = fillRect.GetComponent<Shadow>();
        if (fillGlow == null)
        {
            fillGlow = fillRect.gameObject.AddComponent<Shadow>();
        }
        fillGlow.effectColor = new Color(fillColor.r, fillColor.g, fillColor.b, 0.18f);
        fillGlow.effectDistance = new Vector2(0f, 0f);

        return fill;
    }

    private static void EnsureBackdrop(RectTransform cardRect, Color panelColor, Color frameColor)
    {
        RectTransform backPlate = EnsureRectChild(cardRect, BackPlateName);
        backPlate.SetAsFirstSibling();
        StretchToParent(backPlate, new Vector2(6f, 6f), new Vector2(6f, 6f));
        SetIgnoreLayout(backPlate.gameObject);

        Image backPlateImage = backPlate.GetComponent<Image>();
        if (backPlateImage == null)
        {
            backPlateImage = backPlate.gameObject.AddComponent<Image>();
        }
        backPlateImage.color = new Color(0.015f, 0.012f, 0.018f, 0.86f);
        backPlateImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        backPlateImage.type = Image.Type.Sliced;

        Outline backPlateOutline = backPlate.GetComponent<Outline>();
        if (backPlateOutline == null)
        {
            backPlateOutline = backPlate.gameObject.AddComponent<Outline>();
        }
        backPlateOutline.effectColor = new Color(frameColor.r, frameColor.g, frameColor.b, 0.42f);
        backPlateOutline.effectDistance = new Vector2(1f, -1f);

        RectTransform innerPanel = EnsureRectChild(cardRect, InnerPanelName);
        innerPanel.SetAsFirstSibling();
        StretchToParent(innerPanel, Vector2.zero, Vector2.zero);
        SetIgnoreLayout(innerPanel.gameObject);

        Image innerPanelImage = innerPanel.GetComponent<Image>();
        if (innerPanelImage == null)
        {
            innerPanelImage = innerPanel.gameObject.AddComponent<Image>();
        }
        innerPanelImage.color = panelColor;
        innerPanelImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        innerPanelImage.type = Image.Type.Sliced;

        Outline innerPanelOutline = innerPanel.GetComponent<Outline>();
        if (innerPanelOutline == null)
        {
            innerPanelOutline = innerPanel.gameObject.AddComponent<Outline>();
        }
        innerPanelOutline.effectColor = new Color(frameColor.r, frameColor.g, frameColor.b, 0.82f);
        innerPanelOutline.effectDistance = new Vector2(1f, -1f);

        RectTransform headerBand = EnsureRectChild(cardRect, HeaderBandName);
        headerBand.SetAsFirstSibling();
        headerBand.anchorMin = new Vector2(0f, 1f);
        headerBand.anchorMax = new Vector2(1f, 1f);
        headerBand.pivot = new Vector2(0.5f, 1f);
        headerBand.anchoredPosition = Vector2.zero;
        headerBand.sizeDelta = new Vector2(0f, 34f);
        SetIgnoreLayout(headerBand.gameObject);

        Image headerBandImage = headerBand.GetComponent<Image>();
        if (headerBandImage == null)
        {
            headerBandImage = headerBand.gameObject.AddComponent<Image>();
        }
        headerBandImage.color = new Color(0.19f, 0.16f, 0.15f, 0.82f);
        headerBandImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        headerBandImage.type = Image.Type.Sliced;

        RectTransform topAccent = EnsureRectChild(cardRect, CornerAccentTopName);
        topAccent.SetAsFirstSibling();
        topAccent.anchorMin = new Vector2(0f, 1f);
        topAccent.anchorMax = new Vector2(0f, 1f);
        topAccent.pivot = new Vector2(0f, 1f);
        topAccent.anchoredPosition = new Vector2(12f, -10f);
        topAccent.sizeDelta = new Vector2(42f, 2f);
        SetIgnoreLayout(topAccent.gameObject);

        Image topAccentImage = topAccent.GetComponent<Image>();
        if (topAccentImage == null)
        {
            topAccentImage = topAccent.gameObject.AddComponent<Image>();
        }
        topAccentImage.color = new Color(0.78f, 0.71f, 0.61f, 0.65f);
        topAccentImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        topAccentImage.type = Image.Type.Sliced;

        RectTransform bottomAccent = EnsureRectChild(cardRect, CornerAccentBottomName);
        bottomAccent.SetAsFirstSibling();
        bottomAccent.anchorMin = new Vector2(1f, 0f);
        bottomAccent.anchorMax = new Vector2(1f, 0f);
        bottomAccent.pivot = new Vector2(1f, 0f);
        bottomAccent.anchoredPosition = new Vector2(-12f, 10f);
        bottomAccent.sizeDelta = new Vector2(60f, 2f);
        SetIgnoreLayout(bottomAccent.gameObject);

        Image bottomAccentImage = bottomAccent.GetComponent<Image>();
        if (bottomAccentImage == null)
        {
            bottomAccentImage = bottomAccent.gameObject.AddComponent<Image>();
        }
        bottomAccentImage.color = new Color(0.43f, 0.16f, 0.18f, 0.55f);
        bottomAccentImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        bottomAccentImage.type = Image.Type.Sliced;
    }

    private static void EnsureTitle(RectTransform cardRect, Font font, Color titleColor)
    {
        Text title = EnsureTextChild(cardRect, TitleName, font, "BROKEN VOW", CardTitleFontSize, titleColor, FontStyle.Bold, TextAnchor.MiddleLeft);
        LayoutElement layout = EnsureLayoutElement(title.gameObject);
        layout.preferredHeight = 20f;
    }

    private static void EnsureDivider(RectTransform cardRect)
    {
        RectTransform dividerRect = EnsureRectChild(cardRect, DividerName);
        LayoutElement layout = EnsureLayoutElement(dividerRect.gameObject);
        layout.preferredHeight = 1f;

        Image divider = dividerRect.GetComponent<Image>();
        if (divider == null)
        {
            divider = dividerRect.gameObject.AddComponent<Image>();
        }
        divider.color = new Color(0.56f, 0.49f, 0.43f, 0.46f);
        divider.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        divider.type = Image.Type.Sliced;
    }

    private static RectTransform EnsureRowsContainer(RectTransform cardRect)
    {
        RectTransform rowsRect = EnsureRectChild(cardRect, RowsName);

        VerticalLayoutGroup layout = rowsRect.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = rowsRect.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        layout.spacing = RowSpacing;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = rowsRect.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = rowsRect.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return rowsRect;
    }

    private static RectTransform EnsureTendencySection(RectTransform cardRect, Font font, Color labelColor, Color valueColor)
    {
        RectTransform section = EnsureRectChild(cardRect, TendencySectionName);
        section.SetSiblingIndex(3);

        VerticalLayoutGroup layout = section.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = section.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        layout.spacing = 2f;
        layout.padding = new RectOffset(0, 0, 6, 0);
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = section.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = section.gameObject.AddComponent<ContentSizeFitter>();
        }
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Text label = EnsureTextChild(section, TendencyLabelName, font, "CURRENT TENDENCY", TertiaryLabelFontSize, labelColor, FontStyle.Bold, TextAnchor.MiddleLeft);
        LayoutElement labelLayout = EnsureLayoutElement(label.gameObject);
        labelLayout.preferredHeight = 14f;

        Text value = EnsureTextChild(section, TendencyValueName, font, "Balanced", TertiaryValueFontSize, valueColor, FontStyle.Bold, TextAnchor.MiddleLeft);
        LayoutElement valueLayout = EnsureLayoutElement(value.gameObject);
        valueLayout.preferredHeight = 20f;

        return section;
    }

    private static RectTransform EnsureRectChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        RectTransform rect;
        if (existing == null)
        {
            GameObject childObject = new GameObject(name, typeof(RectTransform));
            rect = childObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
        }
        else
        {
            rect = existing as RectTransform;
        }

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        return rect;
    }

    private static RectTransform FindExistingCard()
    {
        Transform leftTopRoot = GameplayHudLayout.EnsureLeftTopRoot();
        Transform existing = leftTopRoot.Find(CardName);
        return existing as RectTransform;
    }

    private static void StretchToParent(RectTransform rect, Vector2 insetMin, Vector2 insetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-insetMin.x, -insetMin.y);
        rect.offsetMax = new Vector2(insetMax.x, insetMax.y);
    }

    private static Text EnsureTextChild(
        Transform parent,
        string name,
        Font font,
        string content,
        int fontSize,
        Color color,
        FontStyle fontStyle,
        TextAnchor alignment)
    {
        RectTransform rect = EnsureRectChild(parent, name);
        Text text = rect.GetComponent<Text>();
        if (text == null)
        {
            text = rect.gameObject.AddComponent<Text>();
        }

        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.supportRichText = false;

        Outline outline = rect.GetComponent<Outline>();
        if (outline == null)
        {
            outline = rect.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0f, 0f, 0f, 0.52f);
        outline.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    private static LayoutElement EnsureLayoutElement(GameObject target)
    {
        LayoutElement layout = target.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = target.AddComponent<LayoutElement>();
        }

        return layout;
    }

    private static void SetIgnoreLayout(GameObject target)
    {
        LayoutElement layout = EnsureLayoutElement(target);
        layout.ignoreLayout = true;
    }

    private static int GetRowOrder(string rowName)
    {
        switch (rowName)
        {
            case "HealthRow":
                return 0;
            case "OathRow":
                return 1;
            case "CorruptionRow":
                return 2;
            default:
                return 999;
        }
    }

    private static bool IsHealthRow(string rowName)
    {
        return rowName == "HealthRow";
    }
}
