using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnDistance = 8f;
    public bool canSpawn = true;

    private float timer;

    private void Update()
    {
        if (!canSpawn)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0f) * spawnDistance;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    public void SetSpawnEnabled(bool enabled)
    {
        canSpawn = enabled;
        if (enabled)
        {
            timer = 0f;
        }
    }
}
