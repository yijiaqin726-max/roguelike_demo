using UnityEngine;
using UnityEngine.UI;

public static class GameplayHudLayout
{
    private const string MainCanvasName = "Canvas";
    private const string HudRootName = "GameplayHUDRoot";
    private const string LeftTopRootName = "LeftTopHUDRoot";
    private const string TopCenterRootName = "TopCenterHUDRoot";
    private const string OverlayRootName = "OverlayHUDRoot";
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
    private const float CanvasMatch = 0.5f;

    public static Transform EnsureLeftTopRoot()
    {
        return EnsureRoot(
            LeftTopRootName,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(28f, -28f),
            new Vector2(340f, 240f));
    }

    public static Transform EnsureTopCenterRoot()
    {
        return EnsureRoot(
            TopCenterRootName,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, -16f),
            new Vector2(640f, 220f));
    }

    public static Transform EnsureOverlayRoot()
    {
        return EnsureRoot(
            OverlayRootName,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero);
    }

    private static Transform EnsureRoot(
        string rootName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        Canvas canvas = EnsureMainCanvas();
        Transform hudRoot = canvas.transform.Find(HudRootName);
        if (hudRoot == null)
        {
            GameObject rootObject = new GameObject(HudRootName, typeof(RectTransform));
            hudRoot = rootObject.transform;
            hudRoot.SetParent(canvas.transform, false);

            RectTransform hudRect = hudRoot as RectTransform;
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
        }
        hudRoot.SetAsFirstSibling();

        Transform existingRoot = hudRoot.Find(rootName);
        if (existingRoot == null)
        {
            GameObject rootObject = new GameObject(rootName, typeof(RectTransform));
            existingRoot = rootObject.transform;
            existingRoot.SetParent(hudRoot, false);
        }

        RectTransform rect = existingRoot as RectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        existingRoot.SetAsLastSibling();
        return existingRoot;
    }

    private static Canvas EnsureMainCanvas()
    {
        Canvas canvas = null;
        GameObject canvasObject = GameObject.Find(MainCanvasName);
        if (canvasObject != null)
        {
            canvas = canvasObject.GetComponent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = Object.FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            GameObject createdCanvas = new GameObject(MainCanvasName);
            canvas = createdCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = CanvasMatch;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }
}
