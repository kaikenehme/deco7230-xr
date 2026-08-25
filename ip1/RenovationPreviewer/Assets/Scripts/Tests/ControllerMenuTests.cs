using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class ControllerMenuTests
{
    Catalogue cat;
    ControllerMenu menu;

    static Material Lit(string n) => new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = n };

    [SetUp]
    public void Setup()
    {
        cat = ScriptableObject.CreateInstance<Catalogue>();
        cat.paints.Add(new PaintOption { name = "Whisper White", color = Color.white });
        cat.paints.Add(new PaintOption { name = "Domino", color = Color.black });
        cat.materials.Add(new MaterialOption { name = "Tiles", sourceId = "Tiles040", material = Lit("tiles"), targets = SurfaceKind.Floor });
        cat.materials.Add(new MaterialOption { name = "Plaster", sourceId = "Plaster001", material = Lit("plaster"), targets = SurfaceKind.Wall });
        cat.furniture.Add(new FurnitureOption { name = "Chair", sourceId = "ArmChair_01", prefab = GameObject.CreatePrimitive(PrimitiveType.Cube), category = FurnitureCategory.Seating });

        var go = new GameObject("Menu", typeof(RectTransform));
        menu = go.AddComponent<ControllerMenu>();
        menu.catalogue = cat;
        menu.head = new GameObject("head").transform;
        menu.floorBounds = new Bounds(Vector3.zero, new Vector3(4, 0.1f, 3));
        menu.Initialise();   // what Awake does; AddComponent in EditMode does not run Awake for plain MonoBehaviours
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var t in Object.FindObjectsByType<MenuTarget>(FindObjectsSortMode.None)) Object.DestroyImmediate(t.gameObject);
        foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject);
        foreach (var f in Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None)) Object.DestroyImmediate(f.gameObject);
        Object.DestroyImmediate(menu.head.gameObject);
        Object.DestroyImmediate(menu.gameObject);
    }

    static MenuTarget Surf(SurfaceState st, SurfaceKind k)
    {
        var go = new GameObject("Wall_N");
        var s = go.AddComponent<Surface>(); s.SetState(st); s.SetKind(k);
        return go.AddComponent<MenuTarget>();
    }

    [Test]
    public void Show_FloorTarget_BuildsThreeTabs_PaintGridFirst()
    {
        menu.Show(Surf(SurfaceState.Change, SurfaceKind.Floor));
        Assert.IsTrue(menu.IsOpen);
        Assert.AreEqual(3, menu.TabButtonCount);
        Assert.AreEqual(MenuTab.Paint, menu.ActiveTab);
        Assert.AreEqual(2, menu.GridButtonCount, "one swatch per paint");
    }

    [Test]
    public void ShowTab_Material_FiltersByKind()
    {
        menu.Show(Surf(SurfaceState.Change, SurfaceKind.Wall));
        menu.ShowTab(MenuTab.Material);
        Assert.AreEqual(1, menu.GridButtonCount, "only the wall material");
    }

    [Test]
    public void KeepTarget_ShowsSinglePrompt_ThatToggles()
    {
        var t = Surf(SurfaceState.Keep, SurfaceKind.Wall);
        menu.Show(t);
        Assert.AreEqual(MenuTab.KeepPrompt, menu.ActiveTab);
        Assert.AreEqual(1, menu.GridButtonCount);
        menu.ClickGrid(0);
        Assert.AreEqual(SurfaceState.Change, t.Surface.State);
        Assert.AreEqual(MenuTab.Paint, menu.ActiveTab, "re-shown with real tabs");
    }

    [Test]
    public void HoverPaint_Previews_ExitReverts_ClickCommits()
    {
        var t = Surf(SurfaceState.Change, SurfaceKind.Wall);
        t.Surface.Commit(Color.grey);
        menu.Show(t);
        menu.HoverGrid(1);
        Assert.AreEqual(Color.black, t.Surface.DisplayColor);
        Assert.IsTrue(t.Surface.IsPreviewing);
        menu.ExitGrid(1);
        Assert.AreEqual(Color.grey, t.Surface.DisplayColor);
        menu.ClickGrid(1);
        Assert.AreEqual(Color.black, t.Surface.CommittedColor);
    }

    [Test]
    public void Hide_RevertsLivePreview()
    {
        var t = Surf(SurfaceState.Change, SurfaceKind.Wall);
        t.Surface.Commit(Color.grey);
        menu.Show(t);
        menu.HoverGrid(1);
        menu.Hide();
        Assert.IsFalse(menu.IsOpen);
        Assert.AreEqual(Color.grey, t.Surface.DisplayColor);
        Assert.IsFalse(t.Surface.IsPreviewing);
    }

    [Test]
    public void ClickFurniture_SpawnsSlotAtSpawnPoint()
    {
        menu.Show(Surf(SurfaceState.Change, SurfaceKind.Floor));
        menu.SpawnPoint = new Vector3(1f, 0f, 0.5f);
        menu.ShowTab(MenuTab.Furniture);
        menu.ClickGrid(0);
        var slot = Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Single();
        Assert.AreEqual(new Vector3(1f, 0f, 0.5f), slot.transform.position);
        Assert.AreEqual("ArmChair_01", slot.Current.sourceId);
    }

    [Test]
    public void SlotTarget_Swap_And_Remove()
    {
        var go = new GameObject("Furniture_x");
        var slot = go.AddComponent<FurnitureSlot>();
        slot.Swap(cat.furniture[0]);
        var t = go.AddComponent<MenuTarget>();
        menu.Show(t);
        Assert.AreEqual(MenuTab.Swap, menu.ActiveTab);
        Assert.AreEqual(1, menu.GridButtonCount);
        menu.ShowTab(MenuTab.Remove);
        menu.ClickGrid(0);
        Assert.IsNull(slot.Current);
        Assert.IsFalse(menu.IsOpen, "menu closes after remove");
    }

    [Test]
    public void Paint_PagesAtEight_AndNavigates()
    {
        for (int i = cat.paints.Count; i < 20; i++) cat.paints.Add(new PaintOption { name = "P" + i, color = Color.grey });
        menu.Show(Surf(SurfaceState.Change, SurfaceKind.Wall));
        Assert.AreEqual(ControllerMenu.PageSize, menu.GridButtonCount);
        Assert.AreEqual(3, menu.PageCount, "20 paints → 3 pages of 8");
        menu.SetPage(2);
        Assert.AreEqual(4, menu.GridButtonCount, "last page holds the remaining 4");
        menu.SetPage(9);
        Assert.AreEqual(2, menu.Page, "clamped");
        menu.SetPage(-1);
        Assert.AreEqual(0, menu.Page);
    }

    [Test]
    public void CommittedOption_ShowsBadge()
    {
        var t = Surf(SurfaceState.Change, SurfaceKind.Wall);
        menu.Show(t);
        menu.ClickGrid(1);   // Domino
        var chips = menu.GetComponentsInChildren<SwatchButton>(true);
        Assert.IsTrue(chips.Count(c => c.IsCommitted) == 1, "exactly one badge");
        menu.Show(t);        // rebuilt from state
        chips = menu.GetComponentsInChildren<SwatchButton>(true);
        Assert.IsTrue(chips.Single(c => c.IsCommitted).name == "Domino");
    }
}
