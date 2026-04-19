using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;

    [Header("Layout")]
    [SerializeField] private Vector2 barSize = new Vector2(300f, 34f);
    [SerializeField] private Vector2 barAnchorPosition = new Vector2(24f, -20f);
    [SerializeField] private int healthTextFontSize = 16;
    [SerializeField] private int headerFontSize = 11;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.09f, 0.05f, 0.06f, 0.95f);
    [SerializeField] private Color fillColor = new Color(0.78f, 0.18f, 0.22f, 1f);
    [SerializeField] private Color frameColor = new Color(0.78f, 0.63f, 0.45f, 0.75f);
    [SerializeField] private Color textColor = new Color(0.98f, 0.95f, 0.9f, 1f);
    [SerializeField] private Color headerColor = new Color(0.88f, 0.82f, 0.72f, 0.92f);

    private Text healthText;
    private Text headerText;

    private void Start()
    {
        StyleHealthBar();
        EnsureHealthText();
    }

    private void Update()
    {
        if (playerHealth == null || healthFill == null || playerHealth.maxHealth <= 0)
        {
            return;
        }

        float ratio = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        ratio = Mathf.Clamp01(ratio);
        healthFill.fillAmount = ratio;

        StyleHealthBar();
        EnsureHealthText();
        if (healthText != null)
        {
            int percent = Mathf.RoundToInt(ratio * 100f);
            healthText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth + " (" + percent + "%)";
        }
    }

    private void StyleHealthBar()
    {
        if (healthFill == null || healthFill.transform.parent == null)
        {
            return;
        }

        RectTransform parentRect = healthFill.transform.parent as RectTransform;
        if (parentRect != null)
        {
            parentRect.anchorMin = new Vector2(0f, 1f);
            parentRect.anchorMax = new Vector2(0f, 1f);
            parentRect.pivot = new Vector2(0f, 1f);
            parentRect.anchoredPosition = barAnchorPosition;
            parentRect.sizeDelta = barSize;
        }

        Image backgroundImage = healthFill.transform.parent.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        }

        Outline backgroundOutline = healthFill.transform.parent.GetComponent<Outline>();
        if (backgroundOutline == null)
        {
            backgroundOutline = healthFill.transform.parent.gameObject.AddComponent<Outline>();
        }

        backgroundOutline.effectColor = frameColor;
        backgroundOutline.effectDistance = new Vector2(2f, -2f);

        Shadow backgroundShadow = healthFill.transform.parent.GetComponent<Shadow>();
        if (backgroundShadow == null)
        {
            backgroundShadow = healthFill.transform.parent.gameObject.AddComponent<Shadow>();
        }

        backgroundShadow.effectColor = new Color(0f, 0f, 0f, 0.42f);
        backgroundShadow.effectDistance = new Vector2(0f, -3f);

        RectTransform fillRect = healthFill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        healthFill.color = fillColor;
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;

        EnsureHeaderText();
    }

    private void EnsureHealthText()
    {
        if (healthText != null || healthFill == null || healthFill.transform.parent == null)
        {
            return;
        }

        Transform parent = healthFill.transform.parent;
        Transform existing = parent.Find("HealthValueText");
        if (existing != null)
        {
            healthText = existing.GetComponent<Text>();
            StyleText(healthText, healthTextFontSize, textColor, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(2f, -2f));
            return;
        }

        GameObject textObject = new GameObject("HealthValueText");
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        healthText = textObject.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        StyleText(healthText, healthTextFontSize, textColor, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(2f, -2f));
    }

    private void EnsureHeaderText()
    {
        if (headerText != null || healthFill == null || healthFill.transform.parent == null)
        {
            return;
        }

        Transform parent = healthFill.transform.parent;
        Transform existing = parent.Find("HealthHeaderText");
        if (existing != null)
        {
            headerText = existing.GetComponent<Text>();
            StyleText(headerText, headerFontSize, headerColor, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(1f, -1f));
            return;
        }

        GameObject textObject = new GameObject("HealthHeaderText");
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -4f);
        rect.sizeDelta = new Vector2(0f, 14f);

        headerText = textObject.AddComponent<Text>();
        headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        headerText.text = "VITALITY";
        StyleText(headerText, headerFontSize, headerColor, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(1f, -1f));
        headerText.raycastTarget = false;
    }

    private static void StyleText(Text text, int fontSize, Color color, FontStyle fontStyle, TextAnchor alignment, Vector2 outlineOffset)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = fontStyle;
        text.raycastTarget = false;

        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.06f, 0.02f, 0.03f, 0.82f);
        outline.effectDistance = outlineOffset;
    }
}
