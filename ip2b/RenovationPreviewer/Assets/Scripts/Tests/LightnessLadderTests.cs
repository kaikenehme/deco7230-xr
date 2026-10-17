using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class LightnessLadderTests
{
    static float V(Color c) { Color.RGBToHSV(c, out _, out _, out var v); return v; }
    static float S(Color c) { Color.RGBToHSV(c, out _, out var s, out _); return s; }

    static readonly Color[] Bases =
    {
        new(0.91f, 0.89f, 0.85f),   // sofa cream
        new(0.71f, 0.38f, 0.25f),   // terracotta
        new(0.56f, 0.60f, 0.48f),   // sage
        new(0.12f, 0.17f, 0.33f),   // navy
        Color.red, Color.black, Color.white,
    };

    [Test]
    public void Ladder_HasSevenSortedLightToDark([ValueSource(nameof(Bases))] Color b)
    {
        var p = LightnessLadder.Generate(b);
        Assert.AreEqual(LightnessLadder.Count, p.Length);
        Assert.AreEqual(7, p.Length);
        for (int i = 1; i < 7; i++) Assert.GreaterOrEqual(V(p[i - 1]), V(p[i]), $"index {i}");
    }

    [Test]
    public void Ladder_FirstTwoAreDistinctOffWhites([ValueSource(nameof(Bases))] Color b)
    {
        var p = LightnessLadder.Generate(b);
        for (int i = 0; i < 2; i++) { Assert.LessOrEqual(S(p[i]), 0.08f, $"S[{i}]"); Assert.GreaterOrEqual(V(p[i]), 0.90f, $"V[{i}]"); }
        Assert.GreaterOrEqual(V(p[0]) - V(p[1]), 0.03f);
    }

    [Test]
    public void Ladder_RestAreSpreadInLightness([ValueSource(nameof(Bases))] Color b)
    {
        var p = LightnessLadder.Generate(b);
        for (int i = 3; i < 7; i++) Assert.GreaterOrEqual(V(p[i - 1]) - V(p[i]), 0.04f, $"ΔV {i - 1}->{i}");
    }

    [Test]
    public void Ladder_HandlesBlackWhiteGrey()
    {
        foreach (var c in new[] { Color.black, Color.white, Color.grey })
            Assert.AreEqual(7, LightnessLadder.Generate(c).Distinct().Count(), c.ToString());
    }

    [Test]
    public void Ladder_KeepsTheSourceColourInTheDeck()
    {
        // The sample you pulled is in the fan (closest chip within a small tolerance), so "match what I like" holds.
        var terracotta = new Color(0.71f, 0.38f, 0.25f);
        float best = LightnessLadder.Generate(terracotta).Min(c => Mathf.Abs(c.r - terracotta.r) + Mathf.Abs(c.g - terracotta.g) + Mathf.Abs(c.b - terracotta.b));
        Assert.Less(best, 0.12f);
    }
}
