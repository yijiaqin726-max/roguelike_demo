using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlignmentHUD : MonoBehaviour
{
    public CorruptionSystem corruptionSystem;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.07f, 0.06f, 0.065f, 0.92f);
    [SerializeField] private Color frameColor = new Color(0.42f, 0.33f, 0.28f, 0.9f);
    [SerializeField] private Color titleColor = new Color(0.94f, 0.88f, 0.8f, 1f);
    [SerializeField] private Color labelColor = new Color(0.8f, 0.79f, 0.77f, 1f);
    [SerializeField] private Color oathColor = new Color(0.82f, 0.86f, 0.9f, 1f);
    [SerializeField] private Color corruptionColor = new Color(0.76f, 0.2f, 0.24f, 1f);
    [SerializeField] private Color neutralBiasColor = new Color(0.84f, 0.81f, 0.76f, 1f);
    [SerializeField] private Color oathBiasColor = new Color(0.82f, 0.88f, 0.96f, 1f);
    [SerializeField] private Color fallenBiasColor = new Color(0.9f, 0.4f, 0.43f, 1f);

    private Text oathValueText;
    private Text corruptionValueText;
    private Text biasValueText;
    private Text stageValueText;
    private Image oathFillImage;
    private Image corruptionFillImage;
    private Font uiFont;
    private Text stageEventText;
    private Coroutine stageEventRoutine;

    private void Start()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }

        if (corruptionSystem != null)
        {
            corruptionSystem.CorruptionStageChanged += HandleCorruptionStageChanged;
        }

        BuildHudIfNeeded();
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        if (corruptionSystem != null)
        {
            corruptionSystem.CorruptionStageChanged -= HandleCorruptionStageChanged;
        }
    }

    private void Update()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
            if (corruptionSystem != null)
            {
                corruptionSystem.CorruptionStageChanged += HandleCorruptionStageChanged;
            }
        }

        RefreshDisplay();
    }

    private void BuildHudIfNeeded()
    {
        if (oathValueText != null && corruptionValueText != null && biasValueText != null)
        {
            return;
        }

        uiFont = StatusHudCardLayout.LoadPreferredFont();
        StatusHudCardLayout.EnsureCard(uiFont, panelColor, frameColor, titleColor);

        RectTransform oathBarSlot;
        StatusHudCardLayout.EnsureStatusRow(
            uiFont,
            "OathRow",
            "OATH",
            labelColor,
            oathColor,
            out oathBarSlot,
            out oathValueText);
        oathFillImage = StatusHudCardLayout.EnsureBar(
            oathBarSlot,
            "Oath",
            new Color(0.11f, 0.11f, 0.12f, 0.96f),
            oathColor);

        RectTransform corruptionBarSlot;
        StatusHudCardLayout.EnsureStatusRow(
            uiFont,
            "CorruptionRow",
            "CORRUPTION",
            labelColor,
            corruptionColor,
            out corruptionBarSlot,
            out corruptionValueText);
        corruptionFillImage = StatusHudCardLayout.EnsureBar(
            corruptionBarSlot,
            "Corruption",
            new Color(0.12f, 0.095f, 0.1f, 0.96f),
            corruptionColor);

        StatusHudCardLayout.EnsureTendencyTexts(
            uiFont,
            labelColor,
            neutralBiasColor,
            out _,
            out biasValueText);

        RectTransform cardRect = GameplayHudLayout.EnsureLeftTopRoot().Find(StatusHudCardLayout.CardName) as RectTransform;
        if (cardRect != null)
        {
            RectTransform tendencySection = cardRect.Find("TendencySection") as RectTransform;
            if (tendencySection != null)
            {
                stageValueText = EnsureStageText(tendencySection);
            }
        }

        stageEventText = EnsureStageEventText();
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

        if (stageValueText != null)
        {
            CorruptionSystem.CorruptionStage stage = corruptionSystem.GetCurrentStage();
            stageValueText.text = "Stage: " + corruptionSystem.GetStageLabel();
            switch (stage)
            {
                case CorruptionSystem.CorruptionStage.Unsteady:
                    stageValueText.color = new Color(0.88f, 0.56f, 0.62f, 1f);
                    if (corruptionFillImage != null)
                    {
                        corruptionFillImage.color = new Color(0.85f, 0.34f, 0.4f, 1f);
                    }
                    break;
                case CorruptionSystem.CorruptionStage.Uncontrolled:
                    stageValueText.color = new Color(0.98f, 0.72f, 0.76f, 1f);
                    if (corruptionFillImage != null)
                    {
                        corruptionFillImage.color = new Color(0.98f, 0.2f, 0.28f, 1f);
                    }
                    break;
                default:
                    stageValueText.color = new Color(0.82f, 0.79f, 0.76f, 1f);
                    if (corruptionFillImage != null)
                    {
                        corruptionFillImage.color = corruptionColor;
                    }
                    break;
            }
        }
    }

    private void HandleCorruptionStageChanged(CorruptionSystem.CorruptionStage previousStage, CorruptionSystem.CorruptionStage currentStage)
    {
        if (corruptionSystem == null)
        {
            return;
        }

        string message = corruptionSystem.GetStageTransitionMessage(currentStage);
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (stageEventRoutine != null)
        {
            StopCoroutine(stageEventRoutine);
        }

        stageEventRoutine = StartCoroutine(StageEventRoutine(message));
    }

    private IEnumerator StageEventRoutine(string message)
    {
        if (stageEventText == null)
        {
            yield break;
        }

        stageEventText.text = message;
        Color visibleColor = new Color(0.96f, 0.86f, 0.88f, 0.95f);
        stageEventText.color = visibleColor;
        yield return new WaitForSeconds(1.35f);

        float elapsed = 0f;
        float fadeDuration = 0.35f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            stageEventText.color = new Color(visibleColor.r, visibleColor.g, visibleColor.b, Mathf.Lerp(visibleColor.a, 0f, t));
            yield return null;
        }

        stageEventText.text = string.Empty;
        stageEventRoutine = null;
    }

    private Text EnsureStageText(RectTransform parent)
    {
        Transform existing = parent.Find("CorruptionStageValue");
        RectTransform rect;
        if (existing == null)
        {
            GameObject textObject = new GameObject("CorruptionStageValue", typeof(RectTransform));
            rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
        }
        else
        {
            rect = existing as RectTransform;
        }

        Text text = rect.GetComponent<Text>();
        if (text == null)
        {
            text = rect.gameObject.AddComponent<Text>();
        }

        text.font = uiFont;
        text.fontSize = 12;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
        text.raycastTarget = false;

        Outline outline = rect.GetComponent<Outline>();
        if (outline == null)
        {
            outline = rect.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
        outline.effectDistance = new Vector2(1f, -1f);

        LayoutElement layout = rect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = rect.gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredHeight = 16f;

        return text;
    }

    private Text EnsureStageEventText()
    {
        Transform overlayRoot = GameplayHudLayout.EnsureOverlayRoot();
        Transform existing = overlayRoot.Find("CorruptionStageEvent");
        RectTransform rect;
        if (existing == null)
        {
            GameObject textObject = new GameObject("CorruptionStageEvent", typeof(RectTransform));
            rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(overlayRoot, false);
        }
        else
        {
            rect = existing as RectTransform;
        }

        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -146f);
        rect.sizeDelta = new Vector2(380f, 24f);

        Text text = rect.GetComponent<Text>();
        if (text == null)
        {
            text = rect.gameObject.AddComponent<Text>();
        }

        text.font = uiFont;
        text.fontSize = 15;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.text = string.Empty;
        text.color = new Color(0f, 0f, 0f, 0f);

        Outline outline = rect.GetComponent<Outline>();
        if (outline == null)
        {
            outline = rect.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        outline.effectDistance = new Vector2(1f, -1f);

        return text;
    }
}
