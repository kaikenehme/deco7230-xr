using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>IP2b living-room pieces (Sketchfab → Blender → Catalogue/Local): present, under the Quest budget, floor pivot.</summary>
public class LocalFurnitureTests
{
    public static readonly string[] LivingIds =
    {
        "curved_sofa", "round_armchair", "pouf", "oval_marble_table", "slatted_tv_stand",
        "flat_tv", "arc_floor_lamp", "woven_pendant", "linen_curtain", "monstera_stand",
    };

    public static Catalogue Cat => AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");

    public static int Triangles(GameObject prefab) =>
        prefab.GetComponentsInChildren<MeshFilter>(true).Where(m => m.sharedMesh != null).Sum(m => m.sharedMesh.triangles.Length / 3);

    static Bounds BoundsOf(GameObject prefab)
    {
        var go = Object.Instantiate(prefab);
        try
        {
            var rends = go.GetComponentsInChildren<Renderer>();
            var b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
            return b;
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void LocalFurniture_AllInCatalogueWithPrefab()
    {
        foreach (var id in LivingIds)
        {
            var f = Cat.Furniture(id);
            Assert.IsNotNull(f, id);
            Assert.IsNotNull(f.prefab, id);
        }
    }

    [Test]
    public void LocalFurniture_UnderQuestTriangleBudget()
    {
        foreach (var id in LivingIds) Assert.LessOrEqual(Triangles(Cat.Furniture(id).prefab), 30000, id);
    }

    [Test]
    public void LocalFurniture_PivotOnFloor()
    {
        foreach (var id in LivingIds) Assert.AreEqual(0f, BoundsOf(Cat.Furniture(id).prefab).min.y, 0.02f, id);
    }

    [Test]
    public void LocalFurniture_RealWorldSize()
    {
        // largest horizontal extent (m) ± 15 %
        var expect = new (string id, float size)[] { ("curved_sofa", 2.6f), ("round_armchair", 0.85f), ("slatted_tv_stand", 1.8f), ("flat_tv", 1.25f) };
        foreach (var (id, size) in expect)
        {
            var b = BoundsOf(Cat.Furniture(id).prefab);
            Assert.AreEqual(size, Mathf.Max(b.size.x, b.size.z), size * 0.15f, id);
        }
        Assert.AreEqual(1.9f, BoundsOf(Cat.Furniture("arc_floor_lamp").prefab).size.y, 0.25f, "arc_floor_lamp height");
    }
}
