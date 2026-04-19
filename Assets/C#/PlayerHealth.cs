using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;
    public float incomingDamageMultiplier = 1f;

    public event Action<int> Damaged;
    public event Action<int> Healed;

    void Start()
    {
        currentHealth = maxHealth;

        if (GetComponent<PlayerHitFeedback>() == null)
        {
            gameObject.AddComponent<PlayerHitFeedback>();
        }
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * incomingDamageMultiplier));
        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0, currentHealth);
        Damaged?.Invoke(finalDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        int healedAmount = currentHealth - previousHealth;
        if (healedAmount > 0)
        {
            Healed?.Invoke(healedAmount);
        }
    }

    public float GetHealthRatio()
    {
        if (maxHealth <= 0)
        {
            return 0f;
        }

        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Time.timeScale = 0f;
    }
}
