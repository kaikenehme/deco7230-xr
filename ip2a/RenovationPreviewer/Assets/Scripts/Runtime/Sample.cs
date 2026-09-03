using UnityEngine;

/// <summary>
/// Data for a held sample pulled off a kept surface. Never knows what a wall is
/// (concept spec §8) — HoldUpPreviewer is the sole coupling point.
/// </summary>
public class Sample : MonoBehaviour
{
    public Color BaseColor { get; private set; }
    public Color CurrentColor { get; set; }
    public Surface SourceSurface { get; private set; }
    public Color[] Palette { get; private set; }

    public void Init(Surface source)
    {
        SourceSurface = source;
        BaseColor = source.DisplayColor;
        Palette = HarmonyPalette.Generate(BaseColor);
        CurrentColor = Palette[0];
        var rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = CurrentColor;
    }
}
