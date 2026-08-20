using NUnit.Framework;
using UnityEngine;

public class SurfaceTests
{
    Surface Make(SurfaceState s)
    {
        var go = new GameObject("surface");
        var surf = go.AddComponent<Surface>();
        surf.SetState(s);
        return surf;
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Preview_OnChangeSurface_SetsDisplayNotCommitted()
    {
        var s = Make(SurfaceState.Change);
        s.Commit(Color.white);
        s.Preview(Color.red);
        Assert.AreEqual(Color.red, s.DisplayColor);
        Assert.AreEqual(Color.white, s.CommittedColor);
        Assert.IsTrue(s.IsPreviewing);
    }

    [Test]
    public void Revert_RestoresCommittedColor()
    {
        var s = Make(SurfaceState.Change);
        s.Commit(Color.white);
        s.Preview(Color.red);
        s.Revert();
        Assert.AreEqual(Color.white, s.DisplayColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Commit_EndsPreviewAndSticks()
    {
        var s = Make(SurfaceState.Change);
        s.Preview(Color.red);
        s.Commit(Color.red);
        Assert.AreEqual(Color.red, s.CommittedColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Preview_OnKeepSurface_IsRefused()
    {
        var s = Make(SurfaceState.Keep);
        var before = s.DisplayColor;
        s.Preview(Color.red);
        Assert.AreEqual(before, s.DisplayColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Registry_TracksEnabledSurfaces()
    {
        int before = Surface.All.Count;
        var s = Make(SurfaceState.Change);
        Assert.AreEqual(before + 1, Surface.All.Count);
        Object.DestroyImmediate(s.gameObject);
        Assert.AreEqual(before, Surface.All.Count);
    }

    [Test]
    public void PreviewThenRevert_AcrossThreeWalls_LeavesNoTrail()
    {
        var walls = new[] { Make(SurfaceState.Change), Make(SurfaceState.Change), Make(SurfaceState.Change) };
        foreach (var w in walls) { w.Commit(Color.white); w.Preview(Color.red); w.Revert(); }
        foreach (var w in walls)
        {
            Assert.AreEqual(Color.white, w.DisplayColor);
            Assert.IsFalse(w.IsPreviewing);
        }
    }
}
