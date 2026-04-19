using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;

    void Update()
    {
        if (playerHealth != null && healthFill != null)
        {
            float ratio = (float)playerHealth.currentHealth / playerHealth.maxHealth;
            ratio = Mathf.Clamp01(ratio);
            healthFill.fillAmount = ratio;
        }
    }
}