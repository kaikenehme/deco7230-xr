using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class CatalogueTests
{
    Catalogue Make()
    {
        var c = ScriptableObject.CreateInstance<Catalogue>();
        c.paints.Add(new PaintOption { name = "Whisper White", code = "SW1C2", color = new Color(0.95f, 0.94f, 0.91f) });
        c.materials.Add(new MaterialOption { name = "Tiles 040", sourceId = "Tiles040", material = new Material(Shader.Find("Universal Render Pipeline/Lit")), targets = SurfaceKind.Floor });
        c.materials.Add(new MaterialOption { name = "Plaster 001", sourceId = "Plaster001", material = new Material(Shader.Find("Universal Render Pipeline/Lit")), targets = SurfaceKind.Wall | SurfaceKind.Ceiling });
        c.furniture.Add(new FurnitureOption { name = "Arm Chair", sourceId = "ArmChair_01", prefab = new GameObject("ArmChair_01"), category = FurnitureCategory.Seating });
        return c;
    }

    [Test]
    public void MaterialsFor_FiltersByTargetFlags()
    {
        var c = Make();
        Assert.AreEqual(new[] { "Tiles040" }, c.MaterialsFor(SurfaceKind.Floor).Select(m => m.sourceId).ToArray());
        Assert.AreEqual(new[] { "Plaster001" }, c.MaterialsFor(SurfaceKind.Wall).Select(m => m.sourceId).ToArray());
        Assert.AreEqual(new[] { "Plaster001" }, c.MaterialsFor(SurfaceKind.Ceiling).Select(m => m.sourceId).ToArray());
        Assert.IsEmpty(c.MaterialsFor(SurfaceKind.Trim));
    }

    [Test]
    public void SurfaceKind_IsFlags()
    {
        var k = SurfaceKind.Wall | SurfaceKind.Ceiling;
        Assert.IsTrue(k.HasFlag(SurfaceKind.Wall));
        Assert.IsFalse(k.HasFlag(SurfaceKind.Floor));
    }
}
