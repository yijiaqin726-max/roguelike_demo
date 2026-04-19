using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;

    private Text healthText;

    private void Start()
    {
        EnsureHealthText();
    }

    private void Update()
    {
        if (playerHealth == null || healthFill == null)
        {
            return;
        }

        float ratio = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        ratio = Mathf.Clamp01(ratio);
        healthFill.fillAmount = ratio;

        EnsureHealthText();
        if (healthText != null)
        {
            int percent = Mathf.RoundToInt(ratio * 100f);
            healthText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth + " (" + percent + "%)";
        }
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
        healthText.fontSize = 12;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = new Color(0.97f, 0.95f, 0.9f, 1f);
        healthText.raycastTarget = false;
    }
}
