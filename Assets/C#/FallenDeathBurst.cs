using UnityEngine;

public class FallenDeathBurst : MonoBehaviour
{
    public float baseRadius = 1.6f;
    public float radiusPerLevel = 0.25f;
    public int baseDamage = 1;

    private int level;

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
        if (level <= 0)
        {
            return;
        }

        float radius = baseRadius + radiusPerLevel * Mathf.Max(0, level - 1);
        int damage = baseDamage + Mathf.FloorToInt((level - 1) * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(enemyPosition, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemyHealth = hits[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth.Knockback(enemyPosition, 0.35f);
            }
        }

        PulseVisual.Spawn(enemyPosition, radius * 2f, new Color(0.65f, 0.16f, 0.24f, 0.75f), 0.28f);
    }
}
