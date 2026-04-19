using UnityEngine;

public class HitSparkVisual : MonoBehaviour
{
    private static Sprite cachedSprite;

    [SerializeField] private float lifetime = 0.1f;
    [SerializeField] private float startScale = 0.18f;
    [SerializeField] private float endScale = 0.68f;

    private float elapsed;
    private SpriteRenderer spriteRenderer;

    public static void Spawn(Vector3 position, Color color, int sortingOrder = 6)
    {
        GameObject root = new GameObject("HitSparkVisual");
        root.transform.position = position;

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        HitSparkVisual visual = root.AddComponent<HitSparkVisual>();
        visual.spriteRenderer = renderer;
    }

    private void Awake()
    {
        transform.localScale = Vector3.one * startScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, t);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.95f, 0f, t);
            spriteRenderer.color = color;
        }

        if (elapsed >= lifetime)
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
