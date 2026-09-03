using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared look for the runtime-built UI: palette, fonts and procedurally generated
/// sprites (rounded rectangle, ring) so no art assets are needed.
/// </summary>
public static class UiKit
{
    public static readonly Color Panel = new(0.04f, 0.04f, 0.05f, 0.95f);
    public static readonly Color PanelLine = new(1f, 1f, 1f, 0.12f);
    public static readonly Color Surface = new(0.16f, 0.16f, 0.19f, 1f);
    public static readonly Color SurfaceHover = new(0.24f, 0.24f, 0.28f, 1f);
    public static readonly Color Accent = new(0.949f, 0.702f, 0.239f, 1f);   // amber #F2B33D
    public static readonly Color AccentText = new(0.1f, 0.08f, 0.02f, 1f);
    public static readonly Color Text = Color.white;
    public static readonly Color TextDim = new(1f, 1f, 1f, 0.6f);

    static Font font;
    public static Font Font => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

    static Sprite rounded, ring, circle;

    /// <summary>9-sliced rounded rectangle (radius 12 px on a 48 px texture).</summary>
    public static Sprite Rounded => rounded != null ? rounded : (rounded = MakeRounded(48, 12));
    /// <summary>Hollow ring for the reticle and hover highlight.</summary>
    public static Sprite Ring => ring != null ? ring : (ring = MakeRing(64, 0.5f, 0.36f));
    public static Sprite Circle => circle != null ? circle : (circle = MakeRing(64, 0.5f, 0f));

    static Sprite MakeRounded(int size, int radius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiKit_Rounded", filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - 1 - radius), 0);
                float dy = Mathf.Max(radius - y, y - (size - 1 - radius), 0);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(radius + 0.5f - d);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(radius + 2, radius + 2, radius + 2, radius + 2));
        s.name = "UiKit_Rounded";
        return s;
    }

    static Sprite MakeRing(int size, float outerR, float innerR)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "UiKit_Ring", filterMode = FilterMode.Bilinear };
        float c = (size - 1) / 2f, ro = outerR * size, ri = innerR * size;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                float a = Mathf.Clamp01(ro - d) * Mathf.Clamp01(d - ri + 1f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);   // 1 unit = 1 m
        s.name = "UiKit_Ring";
        return s;
    }

    // ---- small builders ----
    public static RectTransform Child(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Fill(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }

    public static Text Label(RectTransform parent, string text, int size, Color color, TextAnchor align, bool bestFit = false, int minSize = 12)
    {
        var rt = Child("Text", parent); Fill(rt);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = Font; t.fontSize = size; t.color = color; t.alignment = align; t.text = text;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        if (bestFit) { t.resizeTextForBestFit = true; t.resizeTextMinSize = minSize; t.resizeTextMaxSize = size; }
        return t;
    }

    public static Image RoundedImage(RectTransform rt, Color color)
    {
        var img = rt.gameObject.GetComponent<Image>();
        if (img == null) img = rt.gameObject.AddComponent<Image>();
        img.sprite = Rounded; img.type = Image.Type.Sliced; img.color = color;
        return img;
    }
}
