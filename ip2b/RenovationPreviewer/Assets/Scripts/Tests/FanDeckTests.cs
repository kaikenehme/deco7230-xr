using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class FanDeckTests
{
    [TearDown]
    public void Cleanup()
    {
        foreach (var d in Object.FindObjectsByType<Sample>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(d.gameObject);
        foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    static (FanDeck deck, Sample sample) MakeDeck()
    {
        var src = new GameObject("src").AddComponent<Surface>();
        src.SetBaseSampleColor(new Color(0.71f, 0.38f, 0.25f));
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = Vector3.one * 0.08f;
        var sample = go.AddComponent<Sample>();
        var deck = go.AddComponent<FanDeck>();
        sample.Init(src);
        return (deck, sample);
    }

    [Test]
    public void ChipPositions_FormArcAboveSample()
    {
        var a = FanDeck.ChipLocalPosition(0, 7); var m = FanDeck.ChipLocalPosition(3, 7); var z = FanDeck.ChipLocalPosition(6, 7);
        Assert.Greater(m.y, a.y);
        Assert.AreEqual(-a.x, z.x, 1e-4f);
        Assert.AreEqual(FanDeck.Radius, m.magnitude, 1e-4f);
    }

    [Test]
    public void Init_BuildsSevenChips_InPaletteOrder_DefaultFirstNonWhite()
    {
        var (deck, sample) = MakeDeck();
        Assert.AreEqual(7, deck.Chips.Count);
        for (int i = 0; i < 7; i++) Assert.AreEqual(sample.Palette[i], deck.Chips[i].Colour);
        Assert.AreEqual(2, deck.Selected);
        Assert.AreEqual(sample.Palette[2], sample.CurrentColor);
    }

    [Test]
    public void Chips_AreRealSizeInTheWorld_DespiteTheScaledSample()
    {
        var (deck, _) = MakeDeck();
        Assert.AreEqual(0.05f, deck.Chips[0].transform.lossyScale.x, 0.005f, "5 cm wide card");
    }

    [Test]
    public void Pick_SetsSampleColourAndSelected()
    {
        var (deck, sample) = MakeDeck();
        deck.Pick(4);
        Assert.AreEqual(4, deck.Selected);
        Assert.AreEqual(sample.Palette[4], sample.CurrentColor);
    }

    [Test]
    public void Pick_OutOfRange_Ignored()
    {
        var (deck, sample) = MakeDeck();
        deck.Pick(9); deck.Pick(-1);
        Assert.AreEqual(2, deck.Selected);
    }

    [Test]
    public void ChipTouch_PicksItsIndex()
    {
        var (deck, sample) = MakeDeck();
        ((ITouchable)deck.Chips[5]).Touch();
        Assert.AreEqual(5, deck.Selected);
    }

    [Test]
    public void HoldingHandCue_DoesNotPick_OtherHandDoes()
    {
        Assert.IsFalse(FanChip.IsOtherHand(new Vector3(0, 1, 0), new Vector3(0.02f, 1f, 0f)), "the hand holding the sample sits on it");
        Assert.IsTrue(FanChip.IsOtherHand(new Vector3(0.3f, 1.1f, 0), new Vector3(0f, 1f, 0f)));
    }

    [Test]
    public void SecondPull_ReplacesPreviousSample()
    {
        // Review Focus #1: one fan per hand — a new pull replaces the sample this hand held.
        var puller = new GameObject("puller").AddComponent<SamplePuller>();
        var (_, first) = MakeDeck();
        var (_, second) = MakeDeck();
        puller.ReplaceHeld(first);
        puller.ReplaceHeld(second);
        Assert.IsTrue(first == null, "previous sample destroyed");
        Assert.AreSame(second, puller.Held);
        Object.DestroyImmediate(puller.gameObject);
    }
}
