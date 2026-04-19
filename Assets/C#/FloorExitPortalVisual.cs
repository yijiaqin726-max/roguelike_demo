using UnityEngine;

public class FloorExitPortalVisual : MonoBehaviour
{
    private static Sprite cachedSprite;

    private SpriteRenderer frameRenderer;
    private SpriteRenderer innerGlowRenderer;
    private SpriteRenderer outerGlowRenderer;
    private SpriteRenderer sigilCrossRenderer;
    private SpriteRenderer sigilCrossRendererB;
    private LineRenderer sigilRingOuter;
    private LineRenderer sigilRingInner;
    private ParticleSystem emberParticles;
    private TextMesh labelMesh;

    private Color frameBaseColor = new Color(0.18f, 0.12f, 0.11f, 0.95f);
    private Color innerGlowBaseColor = new Color(0.82f, 0.22f, 0.26f, 0.78f);
    private Color outerGlowBaseColor = new Color(0.88f, 0.9f, 0.92f, 0.42f);
    private Color ringBaseColor = new Color(0.84f, 0.8f, 0.74f, 0.42f);
    private Color ringAccentColor = new Color(0.74f, 0.2f, 0.24f, 0.32f);
    private Color labelBaseColor = new Color(0.94f, 0.95f, 1f, 1f);

    private float pulseOffset;
    private bool highlighted;

    public void Build()
    {
        Sprite sprite = GetSprite();
        pulseOffset = Random.Range(0f, Mathf.PI * 2f);

        frameRenderer = CreateSpriteLayer("Frame", sprite, frameBaseColor, 4, new Vector3(1.08f, 1.66f, 1f), Vector3.zero);
        innerGlowRenderer = CreateSpriteLayer("InnerGlow", sprite, innerGlowBaseColor, 5, new Vector3(0.62f, 1.08f, 1f), new Vector3(0f, 0.06f, 0f));
        outerGlowRenderer = CreateSpriteLayer("OuterGlow", sprite, outerGlowBaseColor, 3, new Vector3(0.94f, 1.44f, 1f), new Vector3(0f, 0.02f, 0f));

        sigilCrossRenderer = CreateSpriteLayer("SigilCrossA", sprite, new Color(0.88f, 0.84f, 0.78f, 0.22f), 2, new Vector3(1.7f, 0.06f, 1f), new Vector3(0f, -1.08f, 0f));
        sigilCrossRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
        sigilCrossRendererB = CreateSpriteLayer("SigilCrossB", sprite, new Color(0.72f, 0.18f, 0.22f, 0.2f), 2, new Vector3(1.55f, 0.05f, 1f), new Vector3(0f, -1.08f, 0f));
        sigilCrossRendererB.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);

        sigilRingOuter = CreateRing("SigilRingOuter", 0.88f, 0.036f, ringBaseColor, 2, new Vector3(0f, -1.08f, 0f));
        sigilRingInner = CreateRing("SigilRingInner", 0.54f, 0.022f, ringAccentColor, 2, new Vector3(0f, -1.08f, 0f));

        emberParticles = CreateEmbers();
        labelMesh = CreateLabel();
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlighted == isHighlighted)
        {
            return;
        }

        highlighted = isHighlighted;
        if (highlighted)
        {
            PulseVisual.Spawn(transform.position + Vector3.down * 1.05f, 2.2f, new Color(0.86f, 0.22f, 0.24f, 0.28f), 0.45f);
        }
    }

    private void Update()
    {
        float pulse = 0.5f + Mathf.Sin(Time.time * 2.8f + pulseOffset) * 0.5f;
        float hoverBoost = highlighted ? 1f : 0f;

        if (innerGlowRenderer != null)
        {
            Color color = innerGlowBaseColor;
            color.a = 0.62f + pulse * 0.18f + hoverBoost * 0.12f;
            innerGlowRenderer.color = color;
            float scale = 0.96f + pulse * 0.1f + hoverBoost * 0.08f;
            innerGlowRenderer.transform.localScale = new Vector3(0.62f * scale, 1.08f * scale, 1f);
        }

        if (outerGlowRenderer != null)
        {
            Color color = outerGlowBaseColor;
            color.a = 0.26f + pulse * 0.12f + hoverBoost * 0.14f;
            outerGlowRenderer.color = color;
            float scale = 1f + pulse * 0.08f + hoverBoost * 0.12f;
            outerGlowRenderer.transform.localScale = new Vector3(0.94f * scale, 1.44f * scale, 1f);
        }

        if (sigilRingOuter != null)
        {
            Color color = ringBaseColor;
            color.a = 0.22f + pulse * 0.16f + hoverBoost * 0.2f;
            sigilRingOuter.startColor = color;
            sigilRingOuter.endColor = color;
        }

        if (sigilRingInner != null)
        {
            Color color = ringAccentColor;
            color.a = 0.18f + pulse * 0.1f + hoverBoost * 0.18f;
            sigilRingInner.startColor = color;
            sigilRingInner.endColor = color;
        }

        if (labelMesh != null)
        {
            labelMesh.color = Color.Lerp(labelBaseColor, new Color(1f, 0.88f, 0.9f, 1f), hoverBoost * 0.55f);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, highlighted ? new Vector3(1.48f, 2.34f, 1f) : new Vector3(1.4f, 2.2f, 1f), Time.deltaTime * 6f);
    }

    private SpriteRenderer CreateSpriteLayer(string name, Sprite sprite, Color color, int sortingOrder, Vector3 scale, Vector3 localPosition)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = localPosition;
        child.transform.localScale = scale;

        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private LineRenderer CreateRing(string name, float radius, float width, Color color, int sortingOrder, Vector3 localPosition)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = localPosition;

        LineRenderer line = child.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 40;
        line.widthMultiplier = width;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = sortingOrder;
        line.startColor = color;
        line.endColor = color;

        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = (i / (float)line.positionCount) * Mathf.PI * 2f;
            float wobble = 1f + Mathf.Sin(angle * 4f) * 0.05f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius * wobble, Mathf.Sin(angle) * radius * 0.42f * wobble, 0f));
        }

        return line;
    }

    private ParticleSystem CreateEmbers()
    {
        GameObject child = new GameObject("Embers");
        child.transform.SetParent(transform, false);
        child.transform.localPosition = new Vector3(0f, -0.28f, 0f);

        ParticleSystem particles = child.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.26f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.92f, 0.9f, 0.94f, 0.7f),
            new Color(0.68f, 0.18f, 0.22f, 0.65f));
        main.maxParticles = 28;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 12f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.42f;
        shape.radiusThickness = 0.4f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.95f, 0.93f, 0.96f), 0f),
                new GradientColorKey(new Color(0.82f, 0.24f, 0.28f), 0.55f),
                new GradientColorKey(new Color(0.48f, 0.1f, 0.14f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.2f),
                new GradientAlphaKey(0.18f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 6;
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        return particles;
    }

    private TextMesh CreateLabel()
    {
        GameObject textObject = new GameObject("PortalLabel");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = new Vector3(0f, -0.95f, 0f);

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = "DESCEND";
        textMesh.fontSize = 28;
        textMesh.characterSize = 0.075f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = labelBaseColor;
        return textMesh;
    }

    private static Sprite GetSprite()
    {
        if (cachedSprite == null)
        {
            cachedSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        return cachedSprite;
    }
}
