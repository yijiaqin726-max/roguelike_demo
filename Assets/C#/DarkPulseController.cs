using UnityEngine;

[RequireComponent(typeof(CorruptionSystem))]
public class DarkPulseController : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseInternalCooldown = 0.18f;
    [SerializeField] private LayerMask enemyLayers = Physics2D.DefaultRaycastLayers;
    [SerializeField] private Color unsteadyPulseColor = new Color(0.82f, 0.22f, 0.28f, 0.6f);
    [SerializeField] private Color uncontrolledPulseColor = new Color(0.54f, 0.12f, 0.18f, 0.78f);

    [Header("Player Feedback")]
    [SerializeField] private Color unsteadyAuraColor = new Color(0.62f, 0.18f, 0.22f, 0.24f);
    [SerializeField] private Color uncontrolledAuraColor = new Color(0.78f, 0.14f, 0.2f, 0.38f);
    [SerializeField] private Vector3 auraOffset = new Vector3(0f, -0.08f, 0f);

    private CorruptionSystem corruptionSystem;
    private PlayerHealth playerHealth;
    private SpriteRenderer auraRenderer;
    private float pulseCooldownTimer;
    private float auraPulseOffset;

    private void Awake()
    {
        corruptionSystem = GetComponent<CorruptionSystem>();
        playerHealth = GetComponent<PlayerHealth>();
        EnsureAuraRenderer();
        auraPulseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (pulseCooldownTimer > 0f)
        {
            pulseCooldownTimer = Mathf.Max(0f, pulseCooldownTimer - Time.deltaTime);
        }

        RefreshAuraVisual();
    }

    public void TryTriggerDarkPulse()
    {
        if (!CanTriggerDarkPulse())
        {
            return;
        }

        if (Random.value > corruptionSystem.GetPulseChance())
        {
            return;
        }

        ReleaseDarkPulse();
        pulseCooldownTimer = pulseInternalCooldown;
    }

    private bool CanTriggerDarkPulse()
    {
        if (corruptionSystem == null)
        {
            return false;
        }

        if (Time.timeScale <= 0f)
        {
            return false;
        }

        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            return false;
        }

        if (pulseCooldownTimer > 0f)
        {
            return false;
        }

        return corruptionSystem.GetCurrentStage() != CorruptionSystem.CorruptionStage.Ordered;
    }

    private void ReleaseDarkPulse()
    {
        float radius = corruptionSystem.GetPulseRadius();
        int damage = corruptionSystem.GetPulseDamage();
        float knockback = corruptionSystem.GetPulseKnockback();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayers);

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemyHealth = hits[i].GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                continue;
            }

            enemyHealth.TakeDamage(damage);
            enemyHealth.Knockback(transform.position, knockback);
        }

        Color pulseColor = corruptionSystem.GetCurrentStage() == CorruptionSystem.CorruptionStage.Uncontrolled
            ? uncontrolledPulseColor
            : unsteadyPulseColor;
        PulseVisual.Spawn(transform.position, radius * 2f, pulseColor, 0.24f);
    }

    private void EnsureAuraRenderer()
    {
        if (auraRenderer != null)
        {
            return;
        }

        GameObject auraObject = new GameObject("CorruptionAura");
        auraObject.transform.SetParent(transform, false);
        auraObject.transform.localPosition = auraOffset;

        SpriteRenderer renderer = auraObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        renderer.sortingOrder = -1;
        renderer.color = new Color(0f, 0f, 0f, 0f);
        auraRenderer = renderer;
    }

    private void RefreshAuraVisual()
    {
        if (auraRenderer == null || corruptionSystem == null)
        {
            return;
        }

        CorruptionSystem.CorruptionStage stage = corruptionSystem.GetCurrentStage();
        if (stage == CorruptionSystem.CorruptionStage.Ordered)
        {
            auraRenderer.color = Color.Lerp(auraRenderer.color, new Color(0f, 0f, 0f, 0f), Time.deltaTime * 10f);
            auraRenderer.transform.localScale = Vector3.Lerp(auraRenderer.transform.localScale, new Vector3(1.05f, 1.35f, 1f), Time.deltaTime * 8f);
            return;
        }

        float pulse = 0.5f + Mathf.Sin(Time.time * (stage == CorruptionSystem.CorruptionStage.Uncontrolled ? 5.8f : 3.4f) + auraPulseOffset) * 0.5f;
        Color targetColor = stage == CorruptionSystem.CorruptionStage.Uncontrolled ? uncontrolledAuraColor : unsteadyAuraColor;
        targetColor.a *= 0.8f + pulse * 0.35f;
        auraRenderer.color = Color.Lerp(auraRenderer.color, targetColor, Time.deltaTime * 10f);

        Vector3 targetScale = stage == CorruptionSystem.CorruptionStage.Uncontrolled
            ? new Vector3(1.55f, 1.95f, 1f)
            : new Vector3(1.3f, 1.65f, 1f);
        targetScale *= 0.94f + pulse * 0.1f;
        auraRenderer.transform.localScale = Vector3.Lerp(auraRenderer.transform.localScale, targetScale, Time.deltaTime * 8f);
    }
}
