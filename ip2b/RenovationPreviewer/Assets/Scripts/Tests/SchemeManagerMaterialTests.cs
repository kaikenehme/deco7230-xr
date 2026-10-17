using NUnit.Framework;
using UnityEngine;

public class SchemeManagerMaterialTests
{
    SchemeManager mgr;
    static Material Lit(string n) => new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = n };

    Surface MakeChange()
    {
        var s = new GameObject("floor").AddComponent<Surface>();
        s.SetState(SurfaceState.Change);
        s.SetKind(SurfaceKind.Floor);
        return s;
    }

    [SetUp] public void Setup() => mgr = new GameObject("mgr").AddComponent<SchemeManager>();

    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
        if (mgr != null) Object.DestroyImmediate(mgr.gameObject);
    }

    [Test]
    public void SaveAndApply_RoundTripsMaterialAndColour()
    {
        var floor = MakeChange();
        var tiles = Lit("tiles");
        floor.CommitMaterial(tiles);
        floor.Commit(Color.white);
        int slot = mgr.SaveScheme();

        floor.CommitMaterial(Lit("carpet"));
        floor.Commit(Color.red);
        mgr.ApplyScheme(slot);

        Assert.AreSame(tiles, floor.CommittedMaterial);
        Assert.AreEqual(Color.white, floor.CommittedColor);
    }

    [Test]
    public void Apply_RestoresNullMaterial_AsOriginal()
    {
        var floor = MakeChange();
        int slot = mgr.SaveScheme();          // material null = original
        floor.CommitMaterial(Lit("tiles"));
        mgr.ApplyScheme(slot);
        Assert.IsNull(floor.CommittedMaterial);
    }
}
