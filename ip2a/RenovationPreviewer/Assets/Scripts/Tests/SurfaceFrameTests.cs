using NUnit.Framework;
using UnityEngine;

public class SurfaceFrameTests
{
    static readonly Bounds Wall = new(new Vector3(0f, 1.45f, 3.5f), new Vector3(9f, 2.9f, 0.1f));   // Wall_S-ish, room side faces -Z

    [Test]
    public void Corners_LieOnTheHitFace_LiftedTowardTheViewer()
    {
        var c = SurfaceFrame.Corners(Wall, Vector3.back, 0f, 0.005f);
        Assert.AreEqual(4, c.Length);
        foreach (var p in c) Assert.AreEqual(3.5f - 0.05f - 0.005f, p.z, 1e-4f, "on the -Z face, 5 mm into the room");
    }

    [Test]
    public void Corners_SpanTheFaceInsetByMargin()
    {
        var c = SurfaceFrame.Corners(Wall, Vector3.back, 0.03f, 0f);
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in c) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); }
        Assert.AreEqual(-4.5f + 0.03f, minX, 1e-4f);
        Assert.AreEqual(4.5f - 0.03f, maxX, 1e-4f);
        Assert.AreEqual(0f + 0.03f, minY, 1e-4f);
        Assert.AreEqual(2.9f - 0.03f, maxY, 1e-4f);
    }

    [Test]
    public void Corners_FloorHitFromAbove_UsesTheTopFace()
    {
        var floor = new Bounds(new Vector3(0f, -0.05f, 0f), new Vector3(9f, 0.1f, 7f));
        var c = SurfaceFrame.Corners(floor, Vector3.up, 0f, 0.005f);
        foreach (var p in c) Assert.AreEqual(0.005f, p.y, 1e-4f, "top face is y=0");
    }

    [Test]
    public void Corners_SnapsASlantedNormalToTheDominantAxis()
    {
        var a = SurfaceFrame.Corners(Wall, new Vector3(0.2f, 0.1f, -0.9f).normalized, 0f, 0f);
        var b = SurfaceFrame.Corners(Wall, Vector3.back, 0f, 0f);
        for (int i = 0; i < 4; i++) Assert.AreEqual(b[i], a[i]);
    }

    [Test]
    public void Show_WithoutNormal_ReusesTheLastHitFace()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(9f, 2.9f, 0.1f);
        go.transform.position = new Vector3(0f, 1.45f, 3.5f);
        go.AddComponent<Surface>().RebindRenderer();
        var f = go.AddComponent<SurfaceFrame>();
        f.Show(Vector3.back);
        var onHover = f.EnsureLine().GetPosition(0);
        f.Hide();
        f.Show();   // menu opened on it: same face, no ray any more
        Assert.AreEqual(onHover, f.EnsureLine().GetPosition(0));
        Assert.IsTrue(f.IsShown);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void ShowHide_TogglesTheLine_AndDrawsAClosedLoop()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(9f, 2.9f, 0.1f);
        go.transform.position = new Vector3(0f, 1.45f, 3.5f);
        var s = go.AddComponent<Surface>();
        s.RebindRenderer();
        var f = go.AddComponent<SurfaceFrame>();
        var line = f.EnsureLine();
        Assert.IsFalse(line.enabled, "hidden by default");
        f.Show(Vector3.back);
        Assert.IsTrue(line.enabled);
        Assert.IsTrue(line.loop);
        Assert.AreEqual(4, line.positionCount);
        Assert.IsTrue(f.IsShown);
        f.Hide();
        Assert.IsFalse(line.enabled);
        Assert.IsFalse(f.IsShown);
        Object.DestroyImmediate(go);
    }
}
