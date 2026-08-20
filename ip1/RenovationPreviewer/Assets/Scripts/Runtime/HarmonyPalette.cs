using UnityEngine;

/// <summary>
/// Generates the constrained option space from a pulled sample's colour.
/// Order runs safe -> bold: analogous pair, pale/deep tonal variants,
/// triadic pair, complementary (concept spec §8 harmony rule).
/// </summary>
public static class HarmonyPalette
{
    // analogous -30/+30, pale +30, deep +30, triadic -120/+120, complementary +180
    static readonly float[] hueOffsets = { -30f, 30f, 30f, 30f, -120f, 120f, 180f };

    public static Color[] Generate(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        s = Mathf.Clamp(s, 0.2f, 0.8f);
        v = Mathf.Clamp(v, 0.35f, 0.85f);

        var result = new Color[hueOffsets.Length];
        for (int i = 0; i < hueOffsets.Length; i++)
        {
            float hue = Mathf.Repeat(h + hueOffsets[i] / 360f, 1f);
            float sat = s, val = v;
            if (i == 2) { sat = Mathf.Clamp(s * 0.6f, 0.15f, 0.85f); val = Mathf.Clamp(v * 1.25f, 0.25f, 0.95f); } // pale
            if (i == 3) { val = Mathf.Clamp(v * 0.6f, 0.25f, 0.95f); }                                            // deep
            result[i] = Color.HSVToRGB(hue, sat, val);
        }
        return result;
    }
}
