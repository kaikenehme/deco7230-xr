using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class HarmonyThreadTests
{
    [TearDown]
    public void Cleanup() { foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject); }

    [Test]
    public void Endpoints_ToClosestPointOnSource()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localScale = new Vector3(4f, 0.1f, 4f);
        var s = floor.AddComponent<Surface>(); s.SetState(SurfaceState.Keep);
        Physics.SyncTransforms();
        var (from, to) = HarmonyThread.Endpoints(new Vector3(1f, 1f, 1f), s);
        Assert.AreEqual(new Vector3(1f, 1f, 1f), from);
        Assert.AreEqual(1f, to.x, 0.01f); Assert.AreEqual(0.05f, to.y, 0.01f); Assert.AreEqual(1f, to.z, 0.01f);
    }

    [Test]
    public void Colours_StartCurrent_EndBase()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var s = floor.AddComponent<Surface>(); s.SetState(SurfaceState.Keep);
        s.SetBaseSampleColor(new Color(0.2f, 0.5f, 0.3f));
        var go = new GameObject("sample");
        var sample = go.AddComponent<Sample>();
        sample.Init(s);
        sample.CurrentColor = Color.red;
        var (start, end) = HarmonyThread.Colours(sample);
        Assert.AreEqual(Color.red, start);
        Assert.AreEqual(new Color(0.2f, 0.5f, 0.3f), end);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void NoSource_EndpointsCollapse()
    {
        var (from, to) = HarmonyThread.Endpoints(Vector3.one, null);
        Assert.AreEqual(from, to);
    }
}
