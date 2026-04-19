using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    [FormerlySerializedAs("spawnDistance")]
    public float spawnRadius = 8f;
    public bool canSpawn = true;

    [Header("Elite Settings")]
    [SerializeField] private float eliteHealthMultiplier = 1.9f;
    [SerializeField] private float eliteSpeedMultiplier = 1.25f;
    [SerializeField] private float eliteScaleMultiplier = 1.3f;

    private float timer;
    private int batchCount = 1;
    private float healthMultiplier = 1f;
    private float speedMultiplier = 1f;
    private float scaleMultiplier = 1f;
    private float eliteChance;
    private Color enemyTint = Color.white;
    private Color eliteTint = new Color(0.78f, 0.26f, 0.3f, 1f);
    private CorruptionSystem corruptionSystem;

    private void Update()
    {
        if (!canSpawn)
        {
            return;
        }

        EnsureReferences();
        timer += Time.deltaTime;
        if (timer < GetEffectiveSpawnInterval())
        {
            return;
        }

        timer = 0f;
        for (int i = 0; i < batchCount; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0f) * spawnRadius;
        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        ApplyEnemyModifiers(enemyInstance, Random.value < GetEffectiveEliteChance());
    }

    private void EnsureReferences()
    {
        if (corruptionSystem == null)
        {
            corruptionSystem = FindObjectOfType<CorruptionSystem>();
        }
    }

    private void ApplyEnemyModifiers(GameObject enemyInstance, bool spawnElite)
    {
        if (enemyInstance == null)
        {
            return;
        }

        float appliedHealthMultiplier = healthMultiplier;
        float appliedSpeedMultiplier = speedMultiplier;
        float appliedScaleMultiplier = scaleMultiplier;
        Color appliedTint = enemyTint;

        if (spawnElite)
        {
            appliedHealthMultiplier *= eliteHealthMultiplier;
            appliedSpeedMultiplier *= eliteSpeedMultiplier;
            appliedScaleMultiplier *= eliteScaleMultiplier;
            appliedTint = eliteTint;
        }

        EnemyHealth enemyHealth = enemyInstance.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth = Mathf.Max(1, Mathf.RoundToInt(enemyHealth.maxHealth * appliedHealthMultiplier));
        }

        EnemyFollow enemyFollow = enemyInstance.GetComponent<EnemyFollow>();
        if (enemyFollow != null)
        {
            enemyFollow.moveSpeed *= appliedSpeedMultiplier;
        }

        enemyInstance.transform.localScale *= appliedScaleMultiplier;

        SpriteRenderer renderer = enemyInstance.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = appliedTint;
        }
    }

    public void SetSpawnEnabled(bool enabled)
    {
        canSpawn = enabled;
        timer = 0f;
    }

    public void ApplyWaveSettings(
        float newSpawnInterval,
        int newBatchCount,
        float newHealthMultiplier,
        float newSpeedMultiplier,
        float newScaleMultiplier,
        float newEliteChance,
        Color newEnemyTint,
        Color newEliteTint)
    {
        spawnInterval = Mathf.Max(0.2f, newSpawnInterval);
        batchCount = Mathf.Max(1, newBatchCount);
        healthMultiplier = Mathf.Max(0.25f, newHealthMultiplier);
        speedMultiplier = Mathf.Max(0.25f, newSpeedMultiplier);
        scaleMultiplier = Mathf.Max(0.35f, newScaleMultiplier);
        eliteChance = Mathf.Clamp01(newEliteChance);
        enemyTint = newEnemyTint;
        eliteTint = newEliteTint;
        timer = 0f;
    }

    private float GetEffectiveSpawnInterval()
    {
        float interval = spawnInterval;
        if (corruptionSystem == null)
        {
            return Mathf.Max(0.2f, interval);
        }

        interval *= 1f + corruptionSystem.GetSpawnIntervalModifier();
        return Mathf.Max(0.2f, interval);
    }

    private float GetEffectiveEliteChance()
    {
        float chance = eliteChance;
        if (corruptionSystem != null)
        {
            chance += corruptionSystem.GetEliteRateBonus();
        }

        return Mathf.Clamp01(chance);
    }
}
