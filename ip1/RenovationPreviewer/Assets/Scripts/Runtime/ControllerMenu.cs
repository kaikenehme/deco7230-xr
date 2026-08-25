using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// The controller-anchored menu (spec §5), v2 look: dark rounded panel above the
/// left hand facing the eyes; header (target + hovered option), segmented tabs,
/// a 4×2 page of chips with names, page arrows, ✓ on the committed option.
/// Builds its own world-space canvas at Initialise(); Show(target) fills tabs from
/// MenuTarget.Tabs and the grid from the Catalogue. Hover previews through
/// Surface.Preview*/Revert so a preview can never stick; click commits.
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
    public int Page { get; private set; }
    public int PageCount { get; private set; } = 1;

    // canvas geometry: 600×440 px at 0.0005 → 0.30 m × 0.22 m
    public const float CanvasScale = 0.0005f;
    public const int CanvasW = 600, CanvasH = 440;
    public const int Cols = 4, Rows = 2, PageSize = Cols * Rows;

    Canvas canvas;
    RectTransform panel, tabBar, gridRoot;
    Text title, subtitle, pageLabel, hint;
    Button prevBtn, nextBtn;
    readonly List<SwatchButton> grid = new();
    bool built;
    float hintUntil;

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

        panel = UiKit.Child("Panel", rt); UiKit.Fill(panel);
        UiKit.RoundedImage(panel, UiKit.Panel);

        // header: title + hovered option
        var titleRt = UiKit.Child("Title", panel);
        titleRt.anchorMin = new Vector2(0, 0.88f); titleRt.anchorMax = new Vector2(0.86f, 1f); titleRt.offsetMin = new Vector2(18, 0); titleRt.offsetMax = new Vector2(0, -8);
        title = UiKit.Label(titleRt, "", 36, UiKit.Text, TextAnchor.MiddleLeft); title.name = "Title";
        var subRt = UiKit.Child("Subtitle", panel);
        subRt.anchorMin = new Vector2(0, 0.80f); subRt.anchorMax = new Vector2(1, 0.88f); subRt.offsetMin = new Vector2(18, 0); subRt.offsetMax = new Vector2(-18, 0);
        subtitle = UiKit.Label(subRt, "", 26, UiKit.Accent, TextAnchor.MiddleLeft); subtitle.name = "Subtitle";

        var closeRt = UiKit.Child("Close", panel);
        closeRt.anchorMin = new Vector2(0.88f, 0.89f); closeRt.anchorMax = new Vector2(0.97f, 0.985f); closeRt.offsetMin = closeRt.offsetMax = Vector2.zero;
        var closeBtn = MakeButton(closeRt, "✕", UiKit.Surface, UiKit.Text, 30);
        closeBtn.onClick.AddListener(Hide);

        // tabs
        tabBar = UiKit.Child("Tabs", panel);
        tabBar.anchorMin = new Vector2(0, 0.68f); tabBar.anchorMax = new Vector2(1, 0.79f); tabBar.offsetMin = new Vector2(14, 0); tabBar.offsetMax = new Vector2(-14, 0);
        var h = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>(); h.spacing = 8; h.childForceExpandWidth = true; h.childForceExpandHeight = true;

        // grid
        gridRoot = UiKit.Child("Grid", panel);
        gridRoot.anchorMin = new Vector2(0, 0.13f); gridRoot.anchorMax = new Vector2(1, 0.67f); gridRoot.offsetMin = new Vector2(14, 4); gridRoot.offsetMax = new Vector2(-14, -4);
        var g = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
        g.cellSize = new Vector2((CanvasW - 28 - (Cols - 1) * 10) / Cols, (CanvasH * 0.54f - 8 - (Rows - 1) * 10) / Rows);
        g.spacing = new Vector2(10, 10);
        g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = Cols;

        // footer: paging
        var prevRt = UiKit.Child("Prev", panel);
        prevRt.anchorMin = new Vector2(0.03f, 0.02f); prevRt.anchorMax = new Vector2(0.15f, 0.12f); prevRt.offsetMin = prevRt.offsetMax = Vector2.zero;
        prevBtn = MakeButton(prevRt, "‹", UiKit.Surface, UiKit.Text, 34); prevBtn.onClick.AddListener(() => SetPage(Page - 1));
        var nextRt = UiKit.Child("Next", panel);
        nextRt.anchorMin = new Vector2(0.85f, 0.02f); nextRt.anchorMax = new Vector2(0.97f, 0.12f); nextRt.offsetMin = nextRt.offsetMax = Vector2.zero;
        nextBtn = MakeButton(nextRt, "›", UiKit.Surface, UiKit.Text, 34); nextBtn.onClick.AddListener(() => SetPage(Page + 1));
        var pageRt = UiKit.Child("PageLabel", panel);
        pageRt.anchorMin = new Vector2(0.15f, 0.02f); pageRt.anchorMax = new Vector2(0.85f, 0.12f); pageRt.offsetMin = pageRt.offsetMax = Vector2.zero;
        pageLabel = UiKit.Label(pageRt, "", 22, UiKit.TextDim, TextAnchor.MiddleCenter);

        // one-line hint under the panel (simulator / first seconds only)
        var hintRt = UiKit.Child("Hint", rt);
        hintRt.anchorMin = new Vector2(0, 0); hintRt.anchorMax = new Vector2(1, 0); hintRt.offsetMin = new Vector2(0, -44); hintRt.offsetMax = new Vector2(0, -6);
        hint = UiKit.Label(hintRt, "Right hand: point + trigger selects  ·  Left Y closes", 20, UiKit.TextDim, TextAnchor.MiddleCenter);
        hint.enabled = false;

        canvas.enabled = false;
    }

    void LateUpdate()
    {
        if (!IsOpen) return;
        if (head != null)
        {
            var toHead = head.position - transform.position;
            if (toHead.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
        }
        if (hint != null && hint.enabled && Time.time > hintUntil) hint.enabled = false;
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
        subtitle.text = "";
        if (hint != null && !UnityEngine.XR.XRSettings.isDeviceActive && Application.isPlaying) { hint.enabled = true; hintUntil = Time.time + 8f; }
        BuildTabs(target.Tabs);
        ShowTab(FirstTab(target.Tabs));
    }

    public void ShowTab(MenuTab tab)
    {
        if (!IsOpen) return;
        RevertPreview();
        ActiveTab = tab;
        Page = 0;
        Rebuild();
        HighlightTab(tab);
    }

    public void SetPage(int page)
    {
        if (!IsOpen) return;
        Page = Mathf.Clamp(page, 0, PageCount - 1);
        RevertPreview();
        Rebuild();
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
            var b = MakeButton(tabBar, Label(t), UiKit.Surface, UiKit.Text, 26);
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
            {
                var b = tabBar.GetChild(i);
                b.GetComponent<Image>().color = t == tab ? UiKit.Accent : UiKit.Surface;
                var txt = b.GetComponentInChildren<Text>(); if (txt != null) txt.color = t == tab ? UiKit.AccentText : UiKit.Text;
            }
            i++;
        }
    }

    // ------------------------------------------------------------------- grids
    void Rebuild()
    {
        ClearGrid();
        switch (ActiveTab)
        {
            case MenuTab.Paint: BuildPaint(); break;
            case MenuTab.Material: BuildMaterial(); break;
            case MenuTab.Furniture: BuildFurniture(spawn: true); break;
            case MenuTab.Swap: BuildFurniture(spawn: false); break;
            case MenuTab.Remove: BuildRemove(); break;
            case MenuTab.KeepPrompt: BuildKeepPrompt(); break;
        }
        UpdatePager();
    }

    IEnumerable<T> PageOf<T>(IList<T> all)
    {
        PageCount = Mathf.Max(1, (all.Count + PageSize - 1) / PageSize);
        Page = Mathf.Clamp(Page, 0, PageCount - 1);
        return all.Skip(Page * PageSize).Take(PageSize);
    }

    void UpdatePager()
    {
        bool paged = PageCount > 1;
        prevBtn.gameObject.SetActive(paged); nextBtn.gameObject.SetActive(paged);
        pageLabel.text = paged ? $"{Page + 1} / {PageCount}" : "";
    }

    void BuildPaint()
    {
        var s = Current.Surface;
        foreach (var p in PageOf(catalogue.paints))
        {
            var opt = p;
            var b = MakeSwatch(p.name, p.color, null);
            b.SetCommitted(s.CommittedColor == opt.color);
            b.onHover.AddListener(() => { s.Preview(opt.color); Echo(opt.name); });
            b.onExit.AddListener(() => { s.Revert(); Echo(null); });
            b.onClick.AddListener(() => { s.Commit(opt.color); MarkCommitted(b); });
        }
    }

    void BuildMaterial()
    {
        var s = Current.Surface;
        foreach (var m in PageOf(catalogue.MaterialsFor(s.Kind).ToList()))
        {
            var opt = m;
            var b = MakeSwatch(m.name, Color.white, m.material != null ? m.material.mainTexture : null);
            b.SetCommitted(s.CommittedMaterial == opt.material);
            b.onHover.AddListener(() => { s.PreviewMaterial(opt.material); Echo(opt.name); });
            b.onExit.AddListener(() => { s.Revert(); Echo(null); });
            b.onClick.AddListener(() => { s.CommitMaterial(opt.material); s.Commit(Color.white); MarkCommitted(b); });
        }
    }

    void BuildFurniture(bool spawn)
    {
        foreach (var f in PageOf(catalogue.furniture))
        {
            var opt = f;
            var b = MakeSwatch(f.name, UiKit.Surface, f.thumbnail);
            if (!spawn && Current.Slot != null) b.SetCommitted(Current.Slot.Current == opt);
            b.onHover.AddListener(() => Echo(opt.name));
            b.onExit.AddListener(() => Echo(null));
            b.onClick.AddListener(() =>
            {
                if (spawn) FurnitureSlot.Spawn(opt, SpawnPoint, floorBounds);
                else Current.Slot.Swap(opt);
                Hide();
            });
        }
        PageCount = Mathf.Max(1, (catalogue.furniture.Count + PageSize - 1) / PageSize);
    }

    void BuildRemove()
    {
        PageCount = 1;
        var b = MakeSwatch("Remove this", new Color(0.6f, 0.15f, 0.15f), null);
        b.onClick.AddListener(() => { Current.Slot.Remove(); Hide(); });
    }

    void BuildKeepPrompt()
    {
        PageCount = 1;
        var b = MakeSwatch("This is staying — change it?", new Color(0.2f, 0.45f, 0.25f), null);
        b.onClick.AddListener(() => { Current.Surface.ToggleState(); Show(Current); });
    }

    void MarkCommitted(SwatchButton chosen)
    {
        foreach (var b in grid) if (b != null) b.SetCommitted(b == chosen);
    }

    void ClearGrid()
    {
        foreach (var b in grid) if (b != null) { b.transform.SetParent(null, false); DestroyNow(b.gameObject); }
        grid.Clear();
    }

    /// <summary>Big, legible echo of the hovered option under the title.</summary>
    void Echo(string optionName)
    {
        if (Current == null) return;
        subtitle.text = string.IsNullOrEmpty(optionName) ? "" : optionName;
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

    static Button MakeButton(RectTransform parent, string label, Color bg, Color fg, int size)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        if (parent.GetComponent<HorizontalLayoutGroup>() == null) UiKit.Fill(rt);
        var img = UiKit.RoundedImage(rt, bg);
        var btn = go.GetComponent<Button>();
        var colors = btn.colors; colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f); colors.pressedColor = new Color(0.8f, 0.8f, 0.8f); btn.colors = colors;
        UiKit.Label(rt, label, size, fg, TextAnchor.MiddleCenter);
        return btn;
    }

    static void DestroyNow(Object o) { if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
}
