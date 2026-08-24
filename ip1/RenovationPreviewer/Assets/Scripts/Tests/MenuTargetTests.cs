using NUnit.Framework;
using UnityEngine;

public class MenuTargetTests
{
    [TearDown]
    public void Cleanup()
    {
        foreach (var t in Object.FindObjectsByType<MenuTarget>(FindObjectsSortMode.None))
            Object.DestroyImmediate(t.gameObject);
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    static MenuTarget Surf(SurfaceState st, SurfaceKind k)
    {
        var go = new GameObject("s");
        var s = go.AddComponent<Surface>(); s.SetState(st); s.SetKind(k);
        return go.AddComponent<MenuTarget>();
    }

    [Test] public void Floor_Change_GetsPaintMaterialFurniture() =>
        Assert.AreEqual(MenuTab.Paint | MenuTab.Material | MenuTab.Furniture, Surf(SurfaceState.Change, SurfaceKind.Floor).Tabs);

    [Test] public void Wall_Change_GetsPaintMaterial() =>
        Assert.AreEqual(MenuTab.Paint | MenuTab.Material, Surf(SurfaceState.Change, SurfaceKind.Wall).Tabs);

    [Test] public void Trim_Change_GetsPaintOnly() =>
        Assert.AreEqual(MenuTab.Paint, Surf(SurfaceState.Change, SurfaceKind.Trim).Tabs);

    [Test] public void KeepSurface_GetsKeepPrompt() =>
        Assert.AreEqual(MenuTab.KeepPrompt, Surf(SurfaceState.Keep, SurfaceKind.Floor).Tabs);

    [Test]
    public void Slot_GetsSwapRemove_EvenWithKeepSurface()
    {
        var go = new GameObject("Sofa");
        var s = go.AddComponent<Surface>(); s.SetState(SurfaceState.Keep);
        go.AddComponent<FurnitureSlot>();
        var t = go.AddComponent<MenuTarget>();
        Assert.AreEqual(MenuTab.Swap | MenuTab.Remove, t.Tabs);
    }

    [Test]
    public void DisplayName_IsHumanReadable()
    {
        var t = Surf(SurfaceState.Change, SurfaceKind.Wall);
        t.gameObject.name = "Wall_N";
        Assert.AreEqual("Wall N", t.DisplayName);
    }
}
