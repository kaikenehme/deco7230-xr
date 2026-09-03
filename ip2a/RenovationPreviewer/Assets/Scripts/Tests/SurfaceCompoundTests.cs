using NUnit.Framework;
using UnityEngine;

/// <summary>Compound surfaces (several renderers/colliders under one root) and the preset baseline API.</summary>
public class SurfaceCompoundTests
{
    static Material Lit(string name) => new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };

    static GameObject Cube(string name, Transform parent, Vector3 localPos)
    {
        var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.name = name;
        c.transform.SetParent(parent, false);
        c.transform.localPosition = localPos;
        return c;
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Renderers_IncludesRootAndChildren()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Cube("child", root.transform, Vector3.right);
        var s = root.AddComponent<Surface>();
        Assert.AreEqual(2, s.Renderers.Count);
        Assert.AreEqual(2, s.Colliders.Count);
    }

    [Test]
    public void ClosestPoint_UsesNearestChildCollider()
    {
        var root = new GameObject("Wall");
        Cube("a", root.transform, Vector3.zero);
        Cube("b", root.transform, new Vector3(5f, 0f, 0f));
        var s = root.AddComponent<Surface>();
        Physics.SyncTransforms();
        var q = s.ClosestPoint(new Vector3(4.4f, 0f, 0f));
        Assert.AreEqual(4.5f, q.x, 0.01f, "face of cube b, not cube a");
    }

    [Test]
    public void ClosestPoint_NoCollider_FallsBackToBounds()
    {
        var root = new GameObject("Slot");
        var part = Cube("part", root.transform, Vector3.zero);
        Object.DestroyImmediate(part.GetComponent<Collider>());
        var s = root.AddComponent<Surface>();
        Assert.AreEqual(0, s.Colliders.Count);
        var q = s.ClosestPoint(new Vector3(3f, 0f, 0f));
        Assert.AreEqual(0.5f, q.x, 0.01f);
    }

    [Test]
    public void SetBaseLook_TexturedKeep_TintWhite_SampleColourAuthored()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var s = root.AddComponent<Surface>();
        var oak = Lit("oak"); var brown = new Color(0.5f, 0.35f, 0.2f);
        s.SetBaseLook(oak, Color.white, brown, SurfaceState.Keep);
        Assert.AreEqual(SurfaceState.Keep, s.State);
        Assert.AreEqual(Color.white, s.DisplayColor, "textured base shows untinted");
        Assert.AreEqual(brown, s.SampleColor, "pulled sample starts from the authored colour");
        Assert.IsFalse(s.HasUserColour);
        Assert.IsNull(s.CommittedMaterial);
    }

    [Test]
    public void SetBaseLook_ResetsUserColourAndCommittedMaterial()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var s = root.AddComponent<Surface>();
        s.CommitMaterial(Lit("tiles"));
        s.Commit(Color.red);
        s.SetBaseLook(Lit("plaster"), Color.white, Color.grey, SurfaceState.Change);
        Assert.IsNull(s.CommittedMaterial);
        Assert.IsNull(s.DisplayMaterial);
        Assert.IsFalse(s.HasUserColour);
        Assert.AreEqual(Color.white, s.DisplayColor);
        Assert.AreEqual(Color.white, s.CommittedColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void SetBaseLook_NullMaterial_KeepsShippedAndTints()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var s = root.AddComponent<Surface>();
        var cream = new Color(0.95f, 0.93f, 0.85f);
        s.SetBaseLook(null, cream, cream, SurfaceState.Change);
        Assert.AreEqual(cream, s.DisplayColor);
        Assert.AreEqual(cream, s.SampleColor);
    }

    [Test]
    public void SampleColor_UserPaint_OverridesAuthored()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var s = root.AddComponent<Surface>();
        s.SetBaseSampleColor(Color.green);
        Assert.AreEqual(Color.green, s.SampleColor);
        s.Commit(Color.red);
        Assert.AreEqual(Color.red, s.SampleColor);
    }

    [Test]
    public void SampleColor_NoAuthored_IsDisplayColor()
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var blue = Lit("blue"); blue.color = Color.blue;
        root.GetComponent<Renderer>().sharedMaterial = blue;
        var s = root.AddComponent<Surface>();
        Assert.AreEqual(Color.blue, s.SampleColor);
    }
}
