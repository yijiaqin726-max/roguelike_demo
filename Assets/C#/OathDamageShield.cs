using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class OathDamageShield : MonoBehaviour
{
    public float baseReductionMultiplier = 0.75f;
    public float reductionStepPerLevel = 0.1f;
    public float baseDuration = 1.25f;
    public float durationStepPerLevel = 0.2f;

    private PlayerHealth playerHealth;
    private Coroutine activeRoutine;
    private int level;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Damaged -= HandleDamaged;
        }
    }

    public void AddLevel()
    {
        level++;
    }

    private void HandleDamaged(int damageAmount)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        float multiplier = Mathf.Clamp(baseReductionMultiplier - reductionStepPerLevel * Mathf.Max(0, level - 1), 0.35f, 1f);
        float duration = baseDuration + durationStepPerLevel * Mathf.Max(0, level - 1);

        playerHealth.incomingDamageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        playerHealth.incomingDamageMultiplier = 1f;
        activeRoutine = null;
    }
}
