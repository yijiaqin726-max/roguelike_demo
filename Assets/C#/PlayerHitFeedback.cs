using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHitFeedback : MonoBehaviour
{
    public Color hitFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
    public float flashDuration = 0.12f;
    public float shakeDuration = 0.14f;
    public float shakeStrength = 0.08f;

    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    private void HandleDamaged(int damage)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashRoutine = null;
    }

    private IEnumerator ShakeRoutine()
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * shakeStrength;
            transform.localPosition = originalPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }
}
