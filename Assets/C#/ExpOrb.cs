using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public int expAmount = 1;
    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        baseScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
        transform.localScale = baseScale * pulse;

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0.85f + Mathf.Sin(Time.time * 5f) * 0.15f;
            spriteRenderer.color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerExperience playerExp = other.GetComponent<PlayerExperience>();

            if (playerExp != null)
            {
                playerExp.GainExp(expAmount);
            }

            Destroy(gameObject);
        }
    }
}
