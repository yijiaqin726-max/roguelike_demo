using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.11f, 0.055f, 0.06f, 0.96f);
    [SerializeField] private Color fillColor = new Color(0.86f, 0.22f, 0.24f, 1f);
    [SerializeField] private Color frameColor = new Color(0.7f, 0.52f, 0.4f, 0.9f);
    [SerializeField] private Color textColor = new Color(0.99f, 0.97f, 0.94f, 1f);
    [SerializeField] private Color headerColor = new Color(0.93f, 0.87f, 0.78f, 0.98f);

    private RectTransform barSlotRect;
    private Text healthText;
    private bool layoutInitialized;
    private Font uiFont;

    private void Start()
    {
        EnsureLayout();
        RefreshHealth();
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

        EnsureLayout();
        RefreshHealth();
    }

    private void EnsureLayout()
    {
        if (layoutInitialized || healthFill == null || healthFill.transform.parent == null)
        {
            return;
        }

        uiFont = StatusHudCardLayout.LoadPreferredFont();
        StatusHudCardLayout.EnsureCard(
            uiFont,
            new Color(0.07f, 0.06f, 0.065f, 0.92f),
            new Color(0.42f, 0.33f, 0.28f, 0.9f),
            new Color(0.94f, 0.88f, 0.8f, 1f));

        StatusHudCardLayout.EnsureStatusRow(
            uiFont,
            "HealthRow",
            "HEALTH",
            headerColor,
            textColor,
            out barSlotRect,
            out healthText);

        RectTransform barRect = healthFill.transform.parent as RectTransform;
        if (barRect == null || barSlotRect == null)
        {
            return;
        }

        barRect.SetParent(barSlotRect, false);
        barRect.anchorMin = Vector2.zero;
        barRect.anchorMax = Vector2.one;
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;
        barRect.sizeDelta = Vector2.zero;

        Image backgroundImage = barRect.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        }

        Outline backgroundOutline = barRect.GetComponent<Outline>();
        if (backgroundOutline == null)
        {
            backgroundOutline = barRect.gameObject.AddComponent<Outline>();
        }

        backgroundOutline.effectColor = frameColor;
        backgroundOutline.effectDistance = new Vector2(2f, -2f);

        Shadow backgroundShadow = barRect.GetComponent<Shadow>();
        if (backgroundShadow == null)
        {
            backgroundShadow = barRect.gameObject.AddComponent<Shadow>();
        }

        backgroundShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        backgroundShadow.effectDistance = new Vector2(0f, -3f);

        RectTransform fillRect = healthFill.rectTransform;
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);
        healthFill.color = fillColor;
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;

        layoutInitialized = true;
    }

    private void RefreshHealth()
    {
        if (playerHealth == null || healthFill == null || playerHealth.maxHealth <= 0)
        {
            return;
        }

        float ratio = Mathf.Clamp01((float)playerHealth.currentHealth / playerHealth.maxHealth);
        healthFill.fillAmount = ratio;

        if (healthText != null)
        {
            int percent = Mathf.RoundToInt(ratio * 100f);
            healthText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth + "  " + percent + "%";
        }
    }
}
