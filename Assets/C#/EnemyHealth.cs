using UnityEngine;
using System.Collections;
using System;

public class EnemyHealth : MonoBehaviour
{
    public static event Action<Vector3> EnemyDied;

    public int maxHealth = 3;
    private int currentHealth;

    public GameObject expOrbPrefab;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        TriggerHitFlash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Knockback(Vector3 hitSource, float force)
    {
        Vector3 direction = (transform.position - hitSource).normalized;
        transform.position += direction * force;
    }

    void TriggerHitFlash()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(HitFlash());
    }

    IEnumerator HitFlash()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    void Die()
    {
        EnemyDied?.Invoke(transform.position);

        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            KillPopVisual.SpawnFrom(spriteRenderer, transform.position);
        }

        PulseVisual.Spawn(transform.position, 1.6f, new Color(0.92f, 0.32f, 0.26f, 0.7f), 0.18f);

        Destroy(gameObject);
    }
}
