using NUnit.Framework;
using UnityEngine;

public class SurfaceMaterialTests
{
    static Material Lit(string name) => new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };

    Surface Make(SurfaceState s, SurfaceKind k = SurfaceKind.Wall)
    {
        var go = new GameObject("surface");
        var surf = go.AddComponent<Surface>();
        surf.SetState(s);
        surf.SetKind(k);
        return surf;
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Kind_RoundTrips()
    {
        var s = Make(SurfaceState.Change, SurfaceKind.Floor);
        Assert.AreEqual(SurfaceKind.Floor, s.Kind);
    }

    [Test]
    public void PreviewMaterial_SetsDisplayNotCommitted()
    {
        var s = Make(SurfaceState.Change);
        var tiles = Lit("tiles");
        s.PreviewMaterial(tiles);
        Assert.AreSame(tiles, s.DisplayMaterial);
        Assert.IsNull(s.CommittedMaterial, "nothing committed yet");
        Assert.IsTrue(s.IsPreviewing);
    }

    [Test]
    public void Revert_RestoresCommittedMaterialAndColour()
    {
        var s = Make(SurfaceState.Change);
        var timber = Lit("timber");
        s.CommitMaterial(timber);
        s.Commit(Color.red);
        s.PreviewMaterial(Lit("tiles"));
        s.Preview(Color.blue);
        s.Revert();
        Assert.AreSame(timber, s.DisplayMaterial);
        Assert.AreEqual(Color.red, s.DisplayColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void CommitMaterial_Sticks()
    {
        var s = Make(SurfaceState.Change);
        var tiles = Lit("tiles");
        s.PreviewMaterial(tiles);
        s.CommitMaterial(tiles);
        Assert.AreSame(tiles, s.CommittedMaterial);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void PreviewMaterial_OnKeepSurface_IsRefused()
    {
        var s = Make(SurfaceState.Keep);
        s.PreviewMaterial(Lit("tiles"));
        Assert.IsNull(s.DisplayMaterial);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Awake_BindsChildRenderer_ForCompoundProps()
    {
        var root = new GameObject("Sofa");
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.transform.SetParent(root.transform);
        var grey = Lit("grey"); grey.color = new Color(0.45f, 0.45f, 0.48f);
        part.GetComponent<Renderer>().sharedMaterial = grey;
        var s = root.AddComponent<Surface>();   // Awake runs (ExecuteAlways)
        Assert.AreEqual(grey.color, s.CommittedColor, "colour read from the child renderer, not white");
    }

    [Test]
    public void RebindRenderer_PicksUpNewChild()
    {
        var root = new GameObject("Slot");
        var s = root.AddComponent<Surface>();
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.transform.SetParent(root.transform);
        var blue = Lit("blue"); blue.color = Color.blue;
        part.GetComponent<Renderer>().sharedMaterial = blue;
        s.RebindRenderer();
        Assert.AreEqual(Color.blue, s.CommittedColor);
    }
}
