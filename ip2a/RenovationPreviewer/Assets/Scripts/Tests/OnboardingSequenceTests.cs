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
    public void LabelAnchor_Floor_AtEyeHeight_TowardHead()
    {
        var floor = new Bounds(Vector3.zero, new Vector3(9f, 0.1f, 7f));
        var head = new Vector3(0.3f, 1.6f, 0.8f);
        var p = OnboardingSequence.LabelAnchor(floor, head);
        Assert.AreEqual(OnboardingSequence.LabelHeight, p.y, 1e-4f);
        Assert.Greater(p.z, 0f, "pulled toward the head");
        Assert.Less(Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(head.x, 0, head.z)), 1f, "within a metre of the viewer");
    }
}
