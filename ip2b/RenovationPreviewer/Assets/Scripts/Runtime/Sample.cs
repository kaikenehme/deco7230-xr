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
        BaseColor = source.SampleColor;   // authored for textured surfaces, else the tint
        Palette = LightnessLadder.Generate(BaseColor);   // IP2b: fan deck, light → dark
        CurrentColor = Palette[2];                        // first non-white
        var deck = GetComponent<FanDeck>();
        if (deck != null) deck.Build(Palette);
        var rend = GetComponent<Renderer>();
        if (rend != null && Application.isPlaying) rend.material.color = CurrentColor;
    }
}
