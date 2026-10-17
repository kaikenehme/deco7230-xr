using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PresetApplierTests
{
    Catalogue cat; RoomPreset a, b; GameObject managers; PresetApplier applier;

    static FurnitureOption Option(string id)
    {
        var prefab = new GameObject(id);
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.transform.SetParent(prefab.transform);
        mesh.transform.localPosition = new Vector3(0, 0.4f, 0);
        return new FurnitureOption { name = id, sourceId = id, prefab = prefab, sampleColor = Color.magenta };
    }

    static Surface Named(string name, SurfaceState state = SurfaceState.Change)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        var s = go.AddComponent<Surface>();
        s.SetState(state);
        return s;
    }

    static RoomPreset Preset(string name, Material floorMat, Color wall, params (string id, bool keep)[] pieces)
    {
        var p = ScriptableObject.CreateInstance<RoomPreset>();
        p.displayName = name;
        p.looks.Add(new SurfaceLookSpec { surfaceName = "Floor", state = SurfaceState.Keep, material = floorMat, color = Color.white, sampleColor = Color.yellow });
        p.looks.Add(new SurfaceLookSpec { surfaceName = "Wall_N", state = SurfaceState.Change, color = wall, sampleColor = wall });
        foreach (var (id, keep) in pieces)
            p.furniture.Add(new PlacementSpec { sourceId = id, position = new Vector3(1f, 0f, 1f), yaw = 45f, keep = keep, sampleColor = Color.cyan });
        return p;
    }

    [SetUp]
    public void Setup()
    {
        cat = ScriptableObject.CreateInstance<Catalogue>();
        cat.furniture.Add(Option("sofa")); cat.furniture.Add(Option("chair"));
        var oak = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "oak" };
        a = Preset("A", oak, Color.white, ("sofa", true), ("chair", false));
        b = Preset("B", null, Color.blue, ("chair", false));
        Named("Floor", SurfaceState.Keep); Named("Wall_N");
        managers = new GameObject("Managers");
        applier = managers.AddComponent<PresetApplier>();
        applier.catalogue = cat; applier.presets = new[] { a, b };
        applier.floorBounds = new Bounds(Vector3.zero, new Vector3(9, 0.1f, 7));
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var slot in Object.FindObjectsByType<FurnitureSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(slot.gameObject);
        foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject);
        foreach (var f in cat.furniture) Object.DestroyImmediate(f.prefab);
        Object.DestroyImmediate(managers); Object.DestroyImmediate(cat); Object.DestroyImmediate(a); Object.DestroyImmediate(b);
    }

    static FurnitureSlot[] Slots() => Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None);

    [Test]
    public void Apply_SetsSurfaceLooksByName()
    {
        applier.Apply(1);
        var wall = Surface.All.Single(s => s.name == "Wall_N");
        Assert.AreEqual(Color.blue, wall.DisplayColor);
        Assert.AreEqual(Color.blue, wall.SampleColor);
        Assert.IsFalse(wall.HasUserColour);
    }

    [Test]
    public void Apply_KeepFloor_IsSampleSourceWithAuthoredColour()
    {
        applier.Apply(0);
        var floor = Surface.All.Single(s => s.name == "Floor");
        Assert.AreEqual(SurfaceState.Keep, floor.State);
        Assert.AreEqual(Color.yellow, floor.SampleColor, "textured floor pulls its authored colour");
        Assert.AreEqual(Color.white, floor.DisplayColor, "texture untinted");
    }

    [Test]
    public void Apply_SpawnsPlacements_WithOriginPreset_AndKeepSurface()
    {
        applier.Apply(0);
        var slots = Slots();
        Assert.AreEqual(2, slots.Length);
        Assert.IsTrue(slots.All(s => s.Origin == SlotOrigin.Preset));
        var sofa = slots.Single(s => s.Current.sourceId == "sofa");
        Assert.AreEqual(SurfaceState.Keep, sofa.GetComponent<Surface>().State);
        Assert.AreEqual(Color.cyan, sofa.GetComponent<Surface>().SampleColor);
        Assert.AreEqual(45f, sofa.transform.eulerAngles.y, 0.01f);
        Assert.IsNull(slots.Single(s => s.Current.sourceId == "chair").GetComponent<Surface>(), "not kept → not a source");
    }

    [Test]
    public void Apply_Twice_NoDuplicateSlots()
    {
        applier.Apply(0); applier.Apply(0);
        Assert.AreEqual(2, Slots().Length);
        applier.Apply(1);
        Assert.AreEqual(1, Slots().Length);
        Assert.AreEqual(1, applier.Current);
    }

    [Test]
    public void Apply_ClearsUserSlots_KeepsSceneSlots()
    {
        var user = FurnitureSlot.Spawn(cat.furniture[1], Vector3.zero, applier.floorBounds);
        var scene = new GameObject("SceneSofa").AddComponent<FurnitureSlot>(); scene.Origin = SlotOrigin.Scene;
        applier.Apply(1);
        Assert.IsTrue(user == null, "user piece cleared");
        Assert.IsTrue(scene != null, "scene piece survives");
        Assert.AreEqual(2, Slots().Length, "scene + preset B's chair");
    }

    [Test]
    public void Apply_RaisesApplied_AndSkipsUnknownIds()
    {
        b.furniture.Add(new PlacementSpec { sourceId = "nope" });
        int got = -1; applier.Applied += i => got = i;
        applier.Apply(1);
        Assert.AreEqual(1, got);
        Assert.AreEqual(1, Slots().Length);
    }
}
