using UnityEngine;

public class PulseVisual : MonoBehaviour
{
    private static Sprite cachedSprite;

    private float duration;
    private float elapsed;
    private float startScale;
    private float endScale;
    private SpriteRenderer spriteRenderer;

    public static void Spawn(Vector3 position, float diameter, Color color, float lifetime)
    {
        GameObject root = new GameObject("PulseVisual");
        root.transform.position = position;

        PulseVisual pulseVisual = root.AddComponent<PulseVisual>();
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite();
        renderer.color = color;
        renderer.sortingOrder = 3;

        pulseVisual.spriteRenderer = renderer;
        pulseVisual.duration = Mathf.Max(0.05f, lifetime);
        pulseVisual.startScale = 0.2f;
        pulseVisual.endScale = Mathf.Max(0.2f, diameter);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float scale = Mathf.Lerp(startScale, endScale, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a *= 1f - t;
            spriteRenderer.color = color;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    private static Sprite GetSprite()
    {
        if (cachedSprite == null)
        {
            cachedSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        return cachedSprite;
    }
}
