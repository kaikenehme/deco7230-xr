using NUnit.Framework;
using UnityEngine;

public class HarmonyPaletteTests
{
    static float HueOf(Color c) { Color.RGBToHSV(c, out var h, out _, out _); return h * 360f; }
    static float HueDelta(float a, float b) => Mathf.Abs(Mathf.DeltaAngle(a, b));

    [Test]
    public void Generate_Returns7Colors()
    {
        Assert.AreEqual(7, HarmonyPalette.Generate(new Color(0.6f, 0.4f, 0.2f)).Length);
    }

    [Test]
    public void Generate_HueRelationshipsHold()
    {
        var baseColor = Color.HSVToRGB(0.1f, 0.6f, 0.6f); // 36 degree hue
        var p = HarmonyPalette.Generate(baseColor);
        float h = 36f;
        Assert.LessOrEqual(HueDelta(HueOf(p[0]), h - 30f), 2f, "analogous -30");
        Assert.LessOrEqual(HueDelta(HueOf(p[1]), h + 30f), 2f, "analogous +30");
        Assert.LessOrEqual(HueDelta(HueOf(p[4]), h - 120f), 2f, "triadic -120");
        Assert.LessOrEqual(HueDelta(HueOf(p[5]), h + 120f), 2f, "triadic +120");
        Assert.LessOrEqual(HueDelta(HueOf(p[6]), h + 180f), 2f, "complementary");
    }

    [Test]
    public void Generate_PaleAndDeepVariantsDifferInValue()
    {
        var p = HarmonyPalette.Generate(Color.HSVToRGB(0.5f, 0.6f, 0.6f));
        Color.RGBToHSV(p[2], out _, out _, out var vPale);
        Color.RGBToHSV(p[3], out _, out _, out var vDeep);
        Assert.Greater(vPale, vDeep);
    }

    [Test]
    public void Generate_NeverReturnsMuddyOrBlownValues()
    {
        foreach (var c in HarmonyPalette.Generate(new Color(0.05f, 0.05f, 0.05f)))
        {
            Color.RGBToHSV(c, out _, out var s, out var v);
            Assert.That(v, Is.InRange(0.25f, 0.95f));
            Assert.That(s, Is.InRange(0.15f, 0.85f));
        }
    }
}
