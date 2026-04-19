using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    public float knockbackForce = 0.5f;
    public Color hitSparkColor = new Color(1f, 0.84f, 0.64f, 0.9f);

    private Transform target;

    public void SetTarget(Transform enemyTarget)
    {
        target = enemyTarget;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                enemy.Knockback(transform.position, knockbackForce);
                HitSparkVisual.Spawn(transform.position, hitSparkColor);
            }

            Destroy(gameObject);
        }
    }
}
