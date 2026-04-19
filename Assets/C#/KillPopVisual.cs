using UnityEngine;

public class KillPopVisual : MonoBehaviour
{
    private float duration;
    private float elapsed;
    private Vector3 startScale;
    private Vector3 endScale;
    private SpriteRenderer spriteRenderer;

    public static void SpawnFrom(SpriteRenderer sourceRenderer, Vector3 position)
    {
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            return;
        }

        GameObject root = new GameObject("KillPopVisual");
        root.transform.position = position;

        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sourceRenderer.sprite;
        renderer.flipX = sourceRenderer.flipX;
        renderer.flipY = sourceRenderer.flipY;
        renderer.sortingOrder = sourceRenderer.sortingOrder + 1;
        renderer.color = new Color(1f, 0.86f, 0.78f, 0.92f);

        KillPopVisual visual = root.AddComponent<KillPopVisual>();
        visual.spriteRenderer = renderer;
        visual.duration = 0.18f;
        visual.startScale = sourceRenderer.transform.lossyScale;
        visual.endScale = visual.startScale * 0.55f;
        root.transform.localScale = visual.startScale;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        transform.localScale = Vector3.Lerp(startScale, endScale, t);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.92f, 0f, t);
            spriteRenderer.color = color;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
