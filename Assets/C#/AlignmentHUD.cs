using UnityEngine;
using UnityEngine.UI;

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
    private Image oathFillImage;
    private Image corruptionFillImage;
    private Font uiFont;

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
    }
}
