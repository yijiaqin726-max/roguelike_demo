using UnityEngine;

[DefaultExecutionOrder(-25)]
public class SceneAtmosphereLayer : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer groundRenderer;

    [Header("Tone")]
    [SerializeField] private Color backgroundColor = new Color(0.06f, 0.055f, 0.08f, 0f);
    [SerializeField] private Color groundTint = new Color(0.76f, 0.72f, 0.74f, 1f);
    [SerializeField] private Color floorWashColor = new Color(0.1f, 0.085f, 0.095f, 0.5f);
    [SerializeField] private Color edgeShadowColor = new Color(0.04f, 0.03f, 0.04f, 0.72f);

    [Header("Decoration Colors")]
    [SerializeField] private Color crackColor = new Color(0.13f, 0.11f, 0.12f, 0.78f);
    [SerializeField] private Color ashColor = new Color(0.55f, 0.52f, 0.5f, 0.22f);
    [SerializeField] private Color stoneColor = new Color(0.33f, 0.31f, 0.32f, 0.9f);
    [SerializeField] private Color stoneHighlightColor = new Color(0.48f, 0.43f, 0.4f, 0.2f);
    [SerializeField] private Color corruptionColor = new Color(0.14f, 0.34f, 0.22f, 0.42f);
    [SerializeField] private Color corruptionCoreColor = new Color(0.29f, 0.56f, 0.36f, 0.22f);

    [Header("Layout")]
    [SerializeField] private int backgroundSortOrder = -5;
    [SerializeField] private int decorationSortOrder = -1;
    [SerializeField] private float centerSafeRadius = 1.8f;

    private const string GeneratedRootName = "GeneratedAtmosphereRoot";
    private static Sprite cachedWhiteSprite;

    private void Start()
    {
        EnsureReferences();
        ApplyTone();
        BuildDecorations();
    }

    private void EnsureReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (groundRenderer == null)
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                groundRenderer = ground.GetComponent<SpriteRenderer>();
            }
        }
    }

    private void ApplyTone()
    {
        if (targetCamera != null)
        {
            targetCamera.backgroundColor = backgroundColor;
        }

        if (groundRenderer != null)
        {
            groundRenderer.color = groundTint;
        }
    }

    private void BuildDecorations()
    {
        if (groundRenderer == null)
        {
            return;
        }

        Transform existingRoot = transform.Find(GeneratedRootName);
        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        GameObject root = new GameObject(GeneratedRootName);
        root.transform.SetParent(transform, false);

        Bounds bounds = groundRenderer.bounds;
        Vector3 center = bounds.center;
        Vector2 halfSize = bounds.extents;

        CreateBackdropWash(root.transform, center, halfSize);
        CreateEdgeShadows(root.transform, center, halfSize);

        CreateAshTrail(root.transform, ToWorldPosition(center, halfSize, -0.58f, 0.36f), new Vector2(2.1f, 0.28f), 18f);
        CreateAshTrail(root.transform, ToWorldPosition(center, halfSize, -0.18f, -0.42f), new Vector2(1.4f, 0.24f), -14f);
        CreateAshTrail(root.transform, ToWorldPosition(center, halfSize, 0.43f, 0.12f), new Vector2(1.85f, 0.22f), 8f);
        CreateAshTrail(root.transform, ToWorldPosition(center, halfSize, 0.12f, 0.48f), new Vector2(1.25f, 0.18f), -6f);

        CreateCrackCluster(root.transform, ToWorldPosition(center, halfSize, -0.32f, 0.16f), 7, 0.32f, 16f);
        CreateCrackCluster(root.transform, ToWorldPosition(center, halfSize, 0.28f, -0.18f), 6, 0.28f, -24f);
        CreateCrackCluster(root.transform, ToWorldPosition(center, halfSize, 0.02f, 0.34f), 5, 0.22f, 78f);

        CreateCorruptionPool(root.transform, ToWorldPosition(center, halfSize, 0.54f, -0.28f), new Vector2(2.1f, 1.15f), -18f);
        CreateCorruptionPool(root.transform, ToWorldPosition(center, halfSize, -0.46f, -0.12f), new Vector2(1.45f, 0.88f), 10f);
        CreateCorruptionPool(root.transform, ToWorldPosition(center, halfSize, 0.18f, 0.26f), new Vector2(1.25f, 0.74f), -12f);

        CreateStoneCluster(root.transform, ToWorldPosition(center, halfSize, -0.68f, -0.54f), new Vector2(1.35f, 0.72f), 12f);
        CreateStoneCluster(root.transform, ToWorldPosition(center, halfSize, 0.7f, 0.52f), new Vector2(1.2f, 0.66f), -16f);
        CreateStoneCluster(root.transform, ToWorldPosition(center, halfSize, -0.62f, 0.58f), new Vector2(1.05f, 0.58f), -8f);
        CreateStoneCluster(root.transform, ToWorldPosition(center, halfSize, 0.64f, -0.5f), new Vector2(1.1f, 0.62f), 21f);
    }

    private void CreateBackdropWash(Transform parent, Vector3 center, Vector2 halfSize)
    {
        CreateWorldSpriteRect(parent, "FloorWash", center, new Vector2(halfSize.x * 1.85f, halfSize.y * 1.65f), 0f, floorWashColor, backgroundSortOrder);
    }

    private void CreateEdgeShadows(Transform parent, Vector3 center, Vector2 halfSize)
    {
        float width = halfSize.x * 2f;
        float height = halfSize.y * 2f;

        CreateWorldSpriteRect(parent, "TopShadow", center + new Vector3(0f, halfSize.y * 0.82f, 0f), new Vector2(width * 1.05f, height * 0.24f), 0f, edgeShadowColor, backgroundSortOrder + 1);
        CreateWorldSpriteRect(parent, "BottomShadow", center + new Vector3(0f, -halfSize.y * 0.82f, 0f), new Vector2(width * 1.05f, height * 0.2f), 0f, edgeShadowColor * new Color(1f, 1f, 1f, 0.82f), backgroundSortOrder + 1);
        CreateWorldSpriteRect(parent, "LeftShadow", center + new Vector3(-halfSize.x * 0.88f, 0f, 0f), new Vector2(width * 0.16f, height * 1.05f), 0f, edgeShadowColor * new Color(1f, 1f, 1f, 0.72f), backgroundSortOrder + 1);
        CreateWorldSpriteRect(parent, "RightShadow", center + new Vector3(halfSize.x * 0.88f, 0f, 0f), new Vector2(width * 0.16f, height * 1.05f), 0f, edgeShadowColor * new Color(1f, 1f, 1f, 0.72f), backgroundSortOrder + 1);
    }

    private void CreateAshTrail(Transform parent, Vector3 position, Vector2 size, float rotation)
    {
        if (IsInsideSafeZone(position))
        {
            position += new Vector3(size.x * 0.2f, size.y * 0.2f, 0f);
        }

        CreateWorldSpriteRect(parent, "AshTrail", position, size, rotation, ashColor, decorationSortOrder);
        CreateWorldSpriteRect(parent, "AshTrailSoft", position + new Vector3(0.08f, -0.04f, 0f), size * 0.72f, rotation - 6f, ashColor * new Color(1f, 1f, 1f, 0.7f), decorationSortOrder);
    }

    private void CreateCrackCluster(Transform parent, Vector3 position, int segmentCount, float segmentLength, float heading)
    {
        if (IsInsideSafeZone(position))
        {
            position += new Vector3(1.1f, -0.8f, 0f);
        }

        GameObject crackRoot = new GameObject("CrackCluster");
        crackRoot.transform.SetParent(parent, false);
        crackRoot.transform.position = position;

        Vector3 cursor = Vector3.zero;
        float angle = heading;
        for (int i = 0; i < segmentCount; i++)
        {
            float length = segmentLength * Mathf.Lerp(1f, 0.55f, i / (float)segmentCount);
            CreateSpriteRect(crackRoot.transform, "CrackSegment", cursor, new Vector2(length, 0.045f), angle, crackColor, decorationSortOrder);
            cursor += Quaternion.Euler(0f, 0f, angle) * new Vector3(length * 0.75f, 0f, 0f);
            angle += (i % 2 == 0 ? 17f : -12f);
        }
    }

    private void CreateCorruptionPool(Transform parent, Vector3 position, Vector2 size, float rotation)
    {
        if (IsInsideSafeZone(position))
        {
            position += new Vector3(size.x * 0.4f, 0.35f, 0f);
        }

        GameObject poolRoot = new GameObject("CorruptionPool");
        poolRoot.transform.SetParent(parent, false);
        poolRoot.transform.position = position;

        CreateSpriteRect(poolRoot.transform, "PoolOuter", Vector3.zero, size, rotation, corruptionColor, decorationSortOrder);
        CreateSpriteRect(poolRoot.transform, "PoolInner", new Vector3(0.12f, -0.05f, 0f), size * 0.66f, rotation + 14f, corruptionCoreColor, decorationSortOrder);
        CreateSpriteRect(poolRoot.transform, "PoolVein", new Vector3(-0.18f, 0.06f, 0f), new Vector2(size.x * 0.42f, size.y * 0.18f), rotation - 22f, crackColor * new Color(1f, 1.3f, 1f, 0.75f), decorationSortOrder);
    }

    private void CreateStoneCluster(Transform parent, Vector3 position, Vector2 size, float rotation)
    {
        GameObject clusterRoot = new GameObject("StoneCluster");
        clusterRoot.transform.SetParent(parent, false);
        clusterRoot.transform.position = position;

        CreateSpriteRect(clusterRoot.transform, "StoneBase", Vector3.zero, size, rotation, stoneColor, decorationSortOrder);
        CreateSpriteRect(clusterRoot.transform, "StoneChunkA", new Vector3(-size.x * 0.12f, size.y * 0.1f, 0f), new Vector2(size.x * 0.42f, size.y * 0.46f), rotation - 18f, stoneColor * new Color(1.04f, 1.04f, 1.04f, 1f), decorationSortOrder);
        CreateSpriteRect(clusterRoot.transform, "StoneChunkB", new Vector3(size.x * 0.18f, -size.y * 0.06f, 0f), new Vector2(size.x * 0.26f, size.y * 0.36f), rotation + 22f, stoneColor * new Color(0.84f, 0.84f, 0.84f, 1f), decorationSortOrder);
        CreateSpriteRect(clusterRoot.transform, "StoneHighlight", new Vector3(-size.x * 0.06f, size.y * 0.18f, 0f), new Vector2(size.x * 0.5f, size.y * 0.12f), rotation - 6f, stoneHighlightColor, decorationSortOrder);
    }

    private Vector3 ToWorldPosition(Vector3 center, Vector2 halfSize, float normalizedX, float normalizedY)
    {
        return center + new Vector3(halfSize.x * normalizedX, halfSize.y * normalizedY, 0f);
    }

    private bool IsInsideSafeZone(Vector3 position)
    {
        return Vector2.Distance(position, Vector2.zero) < centerSafeRadius;
    }

    private void CreateSpriteRect(Transform parent, string name, Vector3 localPosition, Vector2 worldSize, float rotation, Color color, int sortOrder)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        child.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);

        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = sortOrder;
    }

    private void CreateWorldSpriteRect(Transform parent, string name, Vector3 worldPosition, Vector2 worldSize, float rotation, Color color, int sortOrder)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.position = worldPosition;
        child.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        child.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);

        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = sortOrder;
    }

    private static Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite == null)
        {
            cachedWhiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }

        return cachedWhiteSprite;
    }
}
