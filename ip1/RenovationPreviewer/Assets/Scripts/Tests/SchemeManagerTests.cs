using NUnit.Framework;
using UnityEngine;

public class SchemeManagerTests
{
    SchemeManager mgr;

    Surface MakeChange(Color committed)
    {
        var go = new GameObject("wall");
        var s = go.AddComponent<Surface>();
        s.SetState(SurfaceState.Change);
        s.Commit(committed);
        return s;
    }

    [SetUp]
    public void Setup()
    {
        mgr = new GameObject("mgr").AddComponent<SchemeManager>();
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
        if (mgr != null) Object.DestroyImmediate(mgr.gameObject);
    }

    [Test]
    public void SaveAndApply_RoundTrips()
    {
        var wall = MakeChange(Color.red);
        int slot = mgr.SaveScheme();
        wall.Commit(Color.blue);
        mgr.ApplyScheme(slot);
        Assert.AreEqual(Color.red, wall.CommittedColor);
    }

    [Test]
    public void FourthSave_OverwritesOldest()
    {
        MakeChange(Color.red);
        mgr.SaveScheme();
        mgr.SaveScheme();
        mgr.SaveScheme();
        Assert.AreEqual(0, mgr.SaveScheme(), "wraps to slot 0");
        Assert.AreEqual(3, mgr.Count);
    }

    [Test]
    public void Apply_IgnoresDestroyedSurfaces()
    {
        var wall = MakeChange(Color.red);
        int slot = mgr.SaveScheme();
        Object.DestroyImmediate(wall.gameObject);
        Assert.DoesNotThrow(() => mgr.ApplyScheme(slot));
    }
}
