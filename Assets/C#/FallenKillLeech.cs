using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class FallenKillLeech : MonoBehaviour
{
    [Range(0f, 1f)]
    public float baseHealChance = 0.25f;
    public float chancePerLevel = 0.12f;
    public int baseHealAmount = 1;

    private PlayerHealth playerHealth;
    private int level;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        EnemyHealth.EnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyHealth.EnemyDied -= HandleEnemyDied;
    }

    public void AddLevel()
    {
        level++;
    }

    private void HandleEnemyDied(Vector3 enemyPosition)
    {
        if (playerHealth == null || level <= 0)
        {
            return;
        }

        float chance = Mathf.Clamp01(baseHealChance + chancePerLevel * Mathf.Max(0, level - 1));
        if (Random.value <= chance)
        {
            playerHealth.Heal(baseHealAmount + Mathf.Max(0, level - 1));
        }
    }
}
