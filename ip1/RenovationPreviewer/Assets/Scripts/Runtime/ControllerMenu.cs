using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// The controller-anchored menu (spec §5). Builds its own world-space canvas at
/// Initialise(); Show(target) fills tabs from MenuTarget.Tabs and the grid from the
/// Catalogue. Hover previews through Surface.Preview*/Revert so a preview can never
/// stick; click commits. Furniture buttons spawn/swap FurnitureSlots.
/// </summary>
public class ControllerMenu : MonoBehaviour
{
    public static ControllerMenu Instance { get; private set; }

    public Catalogue catalogue;
    public Transform head;              // Main Camera; canvas faces it
    public Bounds floorBounds;          // spawn clamp
    public Vector3 SpawnPoint { get; set; }

    public MenuTarget Current { get; private set; }
    public MenuTab ActiveTab { get; private set; }
    public bool IsOpen => Current != null;
    public int TabButtonCount => tabBar != null ? tabBar.childCount : 0;
    public int GridButtonCount => grid.Count;

    // canvas geometry: 600×400 px at 0.0005 → 0.30 m × 0.20 m
    const float CanvasScale = 0.0005f;
    const int CanvasW = 600, CanvasH = 400;
    const int Cols = 6, Rows = 4;

    Canvas canvas;
    RectTransform panel, tabBar, gridRoot;
    Text title;
    readonly List<SwatchButton> grid = new();
    bool built;

    void Awake()
    {
        Instance = this;
        Initialise();
    }

    public void Initialise()
    {
        if (built) return;
        built = true;
        Instance = this;

        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (gameObject.GetComponent<CanvasScaler>() == null) gameObject.AddComponent<CanvasScaler>();
        if (gameObject.GetComponent<TrackedDeviceGraphicRaycaster>() == null) gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(CanvasW, CanvasH);
        rt.localScale = Vector3.one * CanvasScale;
        // Blocks the scene raycast so a button click never selects the wall behind the menu.
        var blocker = gameObject.GetComponent<BoxCollider>();
        if (blocker == null) blocker = gameObject.AddComponent<BoxCollider>();
        blocker.size = new Vector3(CanvasW, CanvasH, 1f);
        gameObject.tag = "MenuPanel";

        panel = Child("Panel", rt); Fill(panel);
        var bg = panel.gameObject.AddComponent<Image>(); bg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

        var titleRt = Child("Title", panel);
        titleRt.anchorMin = new Vector2(0, 0.86f); titleRt.anchorMax = Vector2.one; titleRt.offsetMin = new Vector2(12, 0); titleRt.offsetMax = new Vector2(-12, -6);
        title = titleRt.gameObject.AddComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); title.fontSize = 34; title.color = Color.white; title.alignment = TextAnchor.MiddleLeft;
        title.raycastTarget = false;

        tabBar = Child("Tabs", panel);
        tabBar.anchorMin = new Vector2(0, 0.72f); tabBar.anchorMax = new Vector2(1, 0.86f); tabBar.offsetMin = new Vector2(12, 0); tabBar.offsetMax = new Vector2(-12, 0);
        var h = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>(); h.spacing = 8; h.childForceExpandWidth = true; h.childForceExpandHeight = true;

        gridRoot = Child("Grid", panel);
        gridRoot.anchorMin = new Vector2(0, 0); gridRoot.anchorMax = new Vector2(1, 0.72f); gridRoot.offsetMin = new Vector2(12, 12); gridRoot.offsetMax = new Vector2(-12, -6);
        var g = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2((CanvasW - 24 - (Cols - 1) * 8) / Cols, (CanvasH * 0.72f - 18 - (Rows - 1) * 8) / Rows);
        g.spacing = new Vector2(8, 8);
        g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = Cols;

        canvas.enabled = false;
    }

    void LateUpdate()
    {
        if (!IsOpen || head == null) return;
        // Face the head, upright.
        var toHead = head.position - transform.position; toHead.y = 0f;
        if (toHead.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
        if (Current == null) Hide();   // target destroyed
    }

    // ---------------------------------------------------------------- public API
    public void Show(MenuTarget target)
    {
        if (target == null) { Hide(); return; }
        if (Current != null && Current != target) RevertPreview();
        Current = target;
        canvas.enabled = true;
        title.text = target.DisplayName;
        BuildTabs(target.Tabs);
        ShowTab(FirstTab(target.Tabs));
    }

    public void ShowTab(MenuTab tab)
    {
        if (!IsOpen) return;
        RevertPreview();
        ActiveTab = tab;
        ClearGrid();
        switch (tab)
        {
            case MenuTab.Paint: BuildPaint(); break;
            case MenuTab.Material: BuildMaterial(); break;
            case MenuTab.Furniture: BuildFurniture(spawn: true); break;
            case MenuTab.Swap: BuildFurniture(spawn: false); break;
            case MenuTab.Remove: BuildRemove(); break;
            case MenuTab.KeepPrompt: BuildKeepPrompt(); break;
        }
        HighlightTab(tab);
    }

    public void Hide()
    {
        RevertPreview();
        Current = null;
        if (canvas != null) canvas.enabled = false;
    }

    // Test/automation hooks — same paths the pointer events use.
    public void HoverGrid(int i) => grid[i].onHover.Invoke();
    public void ExitGrid(int i) => grid[i].onExit.Invoke();
    public void ClickGrid(int i) => grid[i].onClick.Invoke();

    // ------------------------------------------------------------------- tabs
    static readonly MenuTab[] TabOrder = { MenuTab.Paint, MenuTab.Material, MenuTab.Furniture, MenuTab.Swap, MenuTab.Remove };

    static MenuTab FirstTab(MenuTab tabs)
    {
        if (tabs.HasFlag(MenuTab.KeepPrompt)) return MenuTab.KeepPrompt;
        foreach (var t in TabOrder) if (tabs.HasFlag(t)) return t;
        return MenuTab.None;
    }

    void BuildTabs(MenuTab tabs)
    {
        for (int i = tabBar.childCount - 1; i >= 0; i--) { var c = tabBar.GetChild(i); c.SetParent(null, false); DestroyNow(c.gameObject); }
        if (tabs.HasFlag(MenuTab.KeepPrompt)) return;   // no tab bar for the prompt
        foreach (var t in TabOrder)
        {
            if (!tabs.HasFlag(t)) continue;
            var tab = t;
            var b = MakeButton(tabBar, Label(t), new Color(0.2f, 0.2f, 0.24f));
            b.onClick.AddListener(() => ShowTab(tab));
        }
    }

    static string Label(MenuTab t) => t switch
    {
        MenuTab.Paint => "Paint", MenuTab.Material => "Material", MenuTab.Furniture => "Add furniture",
        MenuTab.Swap => "Swap", MenuTab.Remove => "Remove", _ => t.ToString()
    };

    void HighlightTab(MenuTab tab)
    {
        int i = 0;
        foreach (var t in TabOrder)
        {
            if (Current == null || !Current.Tabs.HasFlag(t)) continue;
            if (i < tabBar.childCount)
                tabBar.GetChild(i).GetComponent<Image>().color = t == tab ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.2f, 0.2f, 0.24f);
            i++;
        }
    }

    // ------------------------------------------------------------------- grids
    void BuildPaint()
    {
        var s = Current.Surface;
        foreach (var p in catalogue.paints.Take(Cols * Rows))
        {
            var opt = p;
            var b = MakeSwatch(p.name, p.color, null);
            b.onHover.AddListener(() => { s.Preview(opt.color); Echo(opt.name); });
            b.onExit.AddListener(() => { s.Revert(); Echo(null); });
            b.onClick.AddListener(() => s.Commit(opt.color));
        }
    }

    void BuildMaterial()
    {
        var s = Current.Surface;
        foreach (var m in catalogue.MaterialsFor(s.Kind).Take(Cols * Rows))
        {
            var opt = m;
            var b = MakeSwatch(m.name, Color.white, m.material != null ? m.material.mainTexture : null);
            b.onHover.AddListener(() => { s.PreviewMaterial(opt.material); Echo(opt.name); });
            b.onExit.AddListener(() => { s.Revert(); Echo(null); });
            b.onClick.AddListener(() => { s.CommitMaterial(opt.material); s.Commit(Color.white); });
        }
    }

    void BuildFurniture(bool spawn)
    {
        foreach (var f in catalogue.furniture.Take(Cols * Rows))
        {
            var opt = f;
            var b = MakeSwatch(f.name, new Color(0.3f, 0.32f, 0.36f), null);
            b.onHover.AddListener(() => Echo(opt.name));
            b.onExit.AddListener(() => Echo(null));
            b.onClick.AddListener(() =>
            {
                if (spawn) FurnitureSlot.Spawn(opt, SpawnPoint, floorBounds);
                else Current.Slot.Swap(opt);
                Hide();
            });
        }
    }

    void BuildRemove()
    {
        var b = MakeSwatch("Remove this", new Color(0.6f, 0.15f, 0.15f), null);
        b.onClick.AddListener(() => { Current.Slot.Remove(); Hide(); });
    }

    void BuildKeepPrompt()
    {
        var b = MakeSwatch("This is staying — change it?", new Color(0.2f, 0.45f, 0.25f), null);
        b.onClick.AddListener(() => { Current.Surface.ToggleState(); Show(Current); });
    }

    void ClearGrid()
    {
        foreach (var b in grid) if (b != null) { b.transform.SetParent(null, false); DestroyNow(b.gameObject); }
        grid.Clear();
    }

    /// <summary>Big, legible echo of the hovered option in the title bar.</summary>
    void Echo(string optionName)
    {
        if (Current == null) return;
        title.text = string.IsNullOrEmpty(optionName) ? Current.DisplayName : Current.DisplayName + "  \u2014  " + optionName;
    }

    void RevertPreview()
    {
        if (Current != null && Current.Surface != null && Current.Surface.IsPreviewing) Current.Surface.Revert();
    }

    // --------------------------------------------------------------- UI helpers
    SwatchButton MakeSwatch(string label, Color tint, Texture tex)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(SwatchButton));
        go.transform.SetParent(gridRoot, false);
        var b = go.GetComponent<SwatchButton>();
        b.Build();
        b.Set(label, tint, tex);
        grid.Add(b);
        return b;
    }

    static Button MakeButton(RectTransform parent, string label, Color bg)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = bg;
        var t = new GameObject("Text", typeof(RectTransform));
        t.transform.SetParent(go.transform, false);
        Fill((RectTransform)t.transform);
        var txt = t.AddComponent<Text>();
        txt.text = label; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); txt.fontSize = 28; txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        return go.GetComponent<Button>();
    }

    static RectTransform Child(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Fill(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }

    static void DestroyNow(Object o) { if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
}
