using UnityEngine;

public class OathPurificationPulse : MonoBehaviour
{
    public float baseInterval = 6f;
    public float intervalReductionPerLevel = 0.6f;
    public float baseRadius = 2.2f;
    public float radiusIncreasePerLevel = 0.35f;
    public int baseDamage = 1;

    private float timer;
    private int level;

    private void Update()
    {
        if (level <= 0)
        {
            return;
        }

        timer += Time.deltaTime;
        float interval = Mathf.Max(2.5f, baseInterval - intervalReductionPerLevel * Mathf.Max(0, level - 1));
        if (timer < interval)
        {
            return;
        }

        timer = 0f;
        ReleasePulse();
    }

    public void AddLevel()
    {
        level++;
        timer = 0f;
    }

    private void ReleasePulse()
    {
        float radius = baseRadius + radiusIncreasePerLevel * Mathf.Max(0, level - 1);
        int damage = baseDamage + Mathf.FloorToInt((level - 1) * 0.5f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemyHealth = hits[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth.Knockback(transform.position, 0.25f);
            }
        }

        PulseVisual.Spawn(transform.position, radius * 2f, new Color(0.75f, 0.95f, 1f, 0.65f), 0.35f);
    }
}
