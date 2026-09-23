using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class OnboardingSequenceTests
{
    [TearDown]
    public void Cleanup() { foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject); }

    static Surface Make(SurfaceState st) { var go = new GameObject("s"); var s = go.AddComponent<Surface>(); s.SetState(st); return s; }

    [Test]
    public void Targets_OnlyKeepSurfaces()
    {
        var keep = Make(SurfaceState.Keep); Make(SurfaceState.Change); Make(SurfaceState.Change);
        var t = OnboardingSequence.Targets(Surface.All).ToList();
        Assert.AreEqual(1, t.Count);
        Assert.AreSame(keep, t[0]);
    }

    [Test]
    public void LabelAnchor_AboveFurniture()
    {
        var b = new Bounds(new Vector3(1f, 0.4f, 2f), new Vector3(1.8f, 0.8f, 0.9f));
        var p = OnboardingSequence.LabelAnchor(b, new Vector3(0f, 1.6f, 0f));
        Assert.AreEqual(1f, p.x, 1e-4f); Assert.AreEqual(2f, p.z, 1e-4f);
        Assert.Greater(p.y, b.max.y);
    }

    [Test]
    public void Labelled_FloorPulsesWithoutLabel_FurnitureKeepsIt()
    {
        // The floor's label floated mid-room over the coffee table (23 Sep renders); the pulse alone reads as "the floor".
        Assert.IsFalse(OnboardingSequence.Labelled(SurfaceKind.Floor));
        Assert.IsTrue(OnboardingSequence.Labelled(SurfaceKind.None));
        Assert.IsTrue(OnboardingSequence.Labelled(SurfaceKind.Wall));
    }
}
