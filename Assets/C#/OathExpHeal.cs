using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class OathExpHeal : MonoBehaviour
{
    public int baseHealAmount = 1;
    public int extraOrbThresholdPerLevel = 2;

    private PlayerHealth playerHealth;
    private int level;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        ExpOrb.OrbCollected += HandleOrbCollected;
    }

    private void OnDisable()
    {
        ExpOrb.OrbCollected -= HandleOrbCollected;
    }

    public void AddLevel()
    {
        level++;
    }

    private void HandleOrbCollected(GameObject collector, int expAmount)
    {
        if (collector != gameObject || playerHealth == null || level <= 0)
        {
            return;
        }

        int threshold = Mathf.Max(1, extraOrbThresholdPerLevel - Mathf.Max(0, level - 1));
        int healAmount = baseHealAmount;
        if (expAmount >= threshold)
        {
            healAmount += Mathf.Max(0, level - 1);
        }

        playerHealth.Heal(healAmount);
    }
}
