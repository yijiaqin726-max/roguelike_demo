using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class FallenLowHealthFrenzy : MonoBehaviour
{
    public float baseBonusAtZeroHealth = 0.25f;
    public float bonusPerLevel = 0.12f;

    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private int level;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (playerController == null || playerHealth == null || level <= 0)
        {
            return;
        }

        float missingRatio = 1f - playerHealth.GetHealthRatio();
        float maxBonus = baseBonusAtZeroHealth + bonusPerLevel * Mathf.Max(0, level - 1);
        float multiplier = 1f - missingRatio * maxBonus;
        playerController.attackIntervalMultiplier = Mathf.Clamp(multiplier, 0.35f, 1f);
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            playerController.attackIntervalMultiplier = 1f;
        }
    }

    public void AddLevel()
    {
        level++;
    }
}
