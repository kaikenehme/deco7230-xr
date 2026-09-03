using NUnit.Framework;
using UnityEngine;

public class PullAffordanceTests
{
    // A 2 x 0.1 x 2 m slab at the origin: top face at y = 0.05.
    PullAffordance Slab(SurfaceState state)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(2f, 0.1f, 2f);
        var s = go.AddComponent<Surface>();
        s.SetState(state);
        s.SetKind(SurfaceKind.Floor);
        Physics.SyncTransforms();
        return go.AddComponent<PullAffordance>();
    }

    static Vector3[] Hands(params Vector3[] h) => h;

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Tick_NoHandNear_Hidden()
    {
        var a = Slab(SurfaceState.Keep);
        a.Tick(Hands(new Vector3(0f, 5f, 0f)), 0f);
        Assert.IsFalse(a.IsVisible);
    }

    [Test]
    public void Tick_HandWithinShowRadius_VisibleAtClosestPoint()
    {
        var a = Slab(SurfaceState.Keep);
        a.Tick(Hands(new Vector3(0.3f, 0.3f, 0.3f)), 0f);   // 0.25 m above the top face
        Assert.IsTrue(a.IsVisible);
        Assert.IsFalse(a.IsLifted);
        Assert.AreEqual(0.3f, a.TabPosition.x, 0.01f);
        Assert.AreEqual(0.05f + PullAffordance.SurfaceOffset, a.TabPosition.y, 0.01f);
        Assert.AreEqual(Vector3.up, a.TabNormal);
    }

    [Test]
    public void Tick_HandWithinTouchRadius_Lifted()
    {
        var a = Slab(SurfaceState.Keep);
        a.Tick(Hands(new Vector3(0f, 0.1f, 0f)), 0f);   // 5 cm above the face
        Assert.IsTrue(a.IsVisible);
        Assert.IsTrue(a.IsLifted);
        Assert.AreEqual(0.05f + PullAffordance.LiftOffset, a.TabPosition.y, 0.01f);
    }

    [Test]
    public void Tick_ChangeSurface_AlwaysHidden()
    {
        var a = Slab(SurfaceState.Change);
        a.Tick(Hands(new Vector3(0f, 0.1f, 0f)), 0f);
        Assert.IsFalse(a.IsVisible);
    }

    [Test]
    public void Tick_UsesNearestOfSeveralHands()
    {
        var a = Slab(SurfaceState.Keep);
        a.Tick(Hands(new Vector3(0f, 5f, 0f), new Vector3(-0.5f, 0.2f, 0.4f)), 0f);
        Assert.IsTrue(a.IsVisible);
        Assert.AreEqual(-0.5f, a.TabPosition.x, 0.01f);
        Assert.AreEqual(0.4f, a.TabPosition.z, 0.01f);
    }

    [Test]
    public void Tab_UsesSurfaceSampleColor()
    {
        var a = Slab(SurfaceState.Keep);
        a.GetComponent<Surface>().SetBaseSampleColor(Color.green);
        Assert.AreEqual(Color.green, a.TabColor);
    }

    [Test]
    public void Peel_HidesForHalfSecond_ThenReturns()
    {
        var a = Slab(SurfaceState.Keep);
        var hand = Hands(new Vector3(0f, 0.1f, 0f));
        a.Peel(10f);
        a.Tick(hand, 10.2f);
        Assert.IsFalse(a.IsVisible, "hidden right after the pull");
        a.Tick(hand, 10f + PullAffordance.PeelHide + 0.05f);
        Assert.IsTrue(a.IsVisible, "back once the beat has passed");
    }

    [Test]
    public void NearestPullable_IgnoresChangeSurfaces_AndBeyondRadius()
    {
        Slab(SurfaceState.Change);
        Assert.IsNull(PullAffordance.NearestPullable(new Vector3(0f, 0.1f, 0f), out _), "Change is never a source");
        var keep = Slab(SurfaceState.Keep);
        keep.transform.position = new Vector3(10f, 0f, 0f);
        Physics.SyncTransforms();
        Assert.IsNull(PullAffordance.NearestPullable(new Vector3(0f, 0.1f, 0f), out _), "10 m away");
        Assert.AreSame(keep.GetComponent<Surface>(), PullAffordance.NearestPullable(new Vector3(10f, 0.2f, 0f), out var d));
        Assert.AreEqual(0.15f, d, 0.01f);
    }
}
