using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>IP2b presets as authored by PresetAuthoring (Editor assembly, so read the assets).</summary>
public class AuthoredPresetTests
{
    static readonly string[] Expected = { "Living", "Bedroom" };
    static RoomPreset Load(string n) => AssetDatabase.LoadAssetAtPath<RoomPreset>($"Assets/Presets/{n}.asset");
    static RoomPreset[] All() => Expected.Select(Load).ToArray();

    [Test]
    public void Presets_AreLivingThenBedroom()
    {
        foreach (var n in Expected) Assert.IsNotNull(Load(n), n);
        CollectionAssert.AreEqual(new[] { "Living", "Bedroom" }, All().Select(p => p.displayName).ToArray());
        foreach (var old in new[] { "Scandi", "MidCentury" }) Assert.IsNull(Load(old), old + " retired");
    }

    [Test]
    public void AllPlacements_InsideRoomWithMargin()
    {
        foreach (var p in All())
            foreach (var f in p.furniture)
            {
                Assert.LessOrEqual(Mathf.Abs(f.position.x), RoomSpec.W / 2f - 0.1f, $"{p.displayName}/{f.sourceId} x");
                Assert.LessOrEqual(Mathf.Abs(f.position.z), RoomSpec.D / 2f - 0.1f, $"{p.displayName}/{f.sourceId} z");
            }
    }

    [Test]
    public void Floor_IsPaintable_InEveryPreset()
    {
        foreach (var p in All()) Assert.AreEqual(SurfaceState.Change, p.looks.First(l => l.surfaceName == "Floor").state, p.displayName);
    }

    [Test]
    public void EveryPreset_UnderRoomTriangleBudget()
    {
        var cat = LocalFurnitureTests.Cat;
        foreach (var p in All())
        {
            int tris = p.furniture.Sum(f => cat.Furniture(f.sourceId) is { } o ? LocalFurnitureTests.Triangles(o.prefab) : 0);
            Assert.LessOrEqual(tris, 250000, p.displayName);
        }
    }

    [Test]
    public void Living_UsesTheApprovedPieces()
    {
        var ids = Load("Living").furniture.Select(f => f.sourceId).ToList();
        foreach (var id in new[] { "curved_sofa", "oval_marble_table", "tv_unit", "round_armchair", "pouf", "monstera_stand" })
            CollectionAssert.Contains(ids, id);
    }
}
