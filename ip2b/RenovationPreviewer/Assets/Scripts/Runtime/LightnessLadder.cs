using System.Linq;
using UnityEngine;

/// <summary>
/// IP2b fan deck palette (Eval 2: people judge on lightness; hidden options lost to a visible
/// catalogue). Seven colours from a pulled sample, lightest first: two off-whites of the
/// sample's hue, then five colours spread in lightness — the sample itself plus analogous,
/// tonal and complementary variants (HarmonyPalette's hue logic).
/// </summary>
public static class LightnessLadder
{
    public const int Count = 7;
    static readonly float[] Slots = { 0.84f, 0.69f, 0.54f, 0.39f, 0.24f };   // value steps for the five colours
    const float MinGap = 0.04f;

    public static Color[] Generate(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        bool grey = s < 0.08f;

        var result = new Color[Count];
        result[0] = Color.HSVToRGB(h, grey ? 0f : Mathf.Min(s, 0.04f), 0.97f);
        result[1] = Color.HSVToRGB(h, grey ? 0f : Mathf.Min(s, 0.07f), 0.92f);

        // The sample keeps its own value if it fits the ladder; otherwise it moves to the nearest slot.
        var values = (float[])Slots.Clone();
        int home = 0; float bestD = float.MaxValue;
        for (int i = 0; i < values.Length; i++) { float d = Mathf.Abs(values[i] - v); if (d < bestD) { bestD = d; home = i; } }
        float lo = home + 1 < values.Length ? values[home + 1] + MinGap : 0f;
        float hi = home > 0 ? values[home - 1] - MinGap : 0.88f;
        values[home] = Mathf.Clamp(v, lo, hi);

        float sat = grey ? s : Mathf.Clamp(s, 0.2f, 0.75f);
        float[] hueOffsets = { -30f, 30f, 0f, 180f };   // analogous, analogous, tonal, complementary
        int k = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (i == home) { result[2 + i] = Color.HSVToRGB(h, s, values[i]); continue; }
            float hue = Mathf.Repeat(h + hueOffsets[k] / 360f, 1f);
            float ss = hueOffsets[k] == 180f ? sat * 0.6f : sat;   // complement muted so it reads as an accent
            result[2 + i] = Color.HSVToRGB(hue, ss, values[i]);
            k++;
        }
        return result;
    }
}
