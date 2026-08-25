# IP1 Controller Menu + Catalogue Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a left-controller menu that changes paint colour, surface material (tiles/timber/plaster) and furniture (swap/add/move) on the existing IP1 room, with assets bundled from Poly Haven, ambientCG and Dulux, ready for the Fri 28 Aug studio test.

**Architecture:** A `Catalogue` ScriptableObject (generated once by an Editor importer) feeds a runtime-built world-space uGUI canvas on the left controller. The right controller's trigger raycasts a `MenuTarget` (wrapping a `Surface` or `FurnitureSlot`); the menu previews on hover via the existing `Surface.Preview` machinery and commits on click. Nothing in the existing mark/pull/hold/tune/commit loop changes.

**Tech Stack:** Unity 6000.0.80f1 · URP 17 · XRI 3.1.2 (`XRUIInputModule`, `TrackedDeviceGraphicRaycaster`, `NearFarInteractor`, `XRGrabInteractable`) · Input System 1.13 · uGUI 2.0 (legacy `Text`) · `com.unity.cloud.gltfast` 6.19.0 · NUnit via Unity Test Framework 1.4.6.

**Spec:** `docs/superpowers/specs/2026-08-25-ip1-controller-menu-design.md`

## Global Constraints

- Project: `ip1/RenovationPreviewer` (absolute: `/Users/kaikenehme/Desktop/University of Queensland/Semester 4/Digital_Prototype/ip1/RenovationPreviewer`). Repo root is two levels up. **Paths contain spaces — always quote.**
- Unity **6000.0.80f1** only; URP mandatory; Android build target; controllers, not hand tracking.
- Runtime scripts go in `Assets/Scripts/Runtime/` (asmdef `Renovation.Runtime`, refs: XRI, InputSystem, XR.CoreUtils). EditMode tests in `Assets/Scripts/Tests/` (asmdef `Renovation.EditModeTests`). PlayMode tests in `Assets/Scripts/PlayModeTests/`. Editor scripts in `Assets/Editor/` (no asmdef → Assembly-CSharp-Editor).
- **Do not modify** `Sample`, `SamplePuller`, `HoldUpPreviewer`, `HarmonyPalette`, `HarmonyTuner`, `MarkTool`, `SchemeCycler`, `LampController`.
- Lamp is never a furniture slot. `SideTable` stays a plain prop.
- Every asset bundled must be CC0 (Poly Haven, ambientCG) or public colour data (Dulux names + RGB). Record every asset id + URL in `Assets/Catalogue/CREDITS.md`.
- Texture resolution 1K. Furniture glTF 1k.
- UI: minimum font 28 px at canvas scale (≈ 1.4 cm on the 0.3 m canvas), max 6×4 grid, high contrast.
- The Unity Editor is **already open** on this project (Pipeline server, port 7800). Use it — do not launch batch-mode Unity against the same project.
- Secure assessment: append every AI action to `ip1/ai-use-log.md` (Task 11).

### How to compile + run tests with the open Editor

```bash
# after editing any .cs file: force recompile, then poll
unity command recompile --no-banner --non-interactive --format tsv
until unity command recompile_status --no-banner --non-interactive --format json | grep -qE '"(completed|up_to_date)"'; do sleep 2; done
# compile errors show in the console:
unity command console --level error --tail 20 --no-banner --non-interactive --format json

# run one EditMode test class
unity command run_tests --mode EditMode --filter SurfaceMaterialTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
# run all EditMode / all PlayMode
unity command run_tests --mode EditMode --timeout 300 --no-banner --non-interactive --format json
unity command run_tests --mode PlayMode --timeout 600 --no-banner --non-interactive --format json
```

A test run result is JSON with `passed` / `failed` counts and per-test failure messages. "Expected: FAIL" steps below mean either a compile error (type not found) or a red test — both count.

### Commit convention

Commit from repo root; stage only the files named in the task. Messages: `feat(ip1): …` / `test(ip1): …` / `docs(ip1): …` / `chore(ip1): …`. Never commit `Library/`, `Builds/`, `.utmp/`.

---

## File Map

| Path | Responsibility |
|---|---|
| `Packages/manifest.json` | + `com.unity.cloud.gltfast` |
| `Assets/Scripts/Runtime/SurfaceKind.cs` | `[Flags] enum SurfaceKind` |
| `Assets/Scripts/Runtime/Catalogue.cs` | `Catalogue` SO + `PaintOption`, `MaterialOption`, `FurnitureOption` |
| `Assets/Scripts/Runtime/Surface.cs` | **modify**: `kind`, material preview/commit, child-renderer binding |
| `Assets/Scripts/Runtime/SchemeManager.cs` | **modify**: snapshot `SurfaceLook` (colour + material) |
| `Assets/Scripts/Runtime/FurnitureSlot.cs` | swap / remove / spawn furniture; grab + floor snap |
| `Assets/Scripts/Runtime/MenuTab.cs` | `[Flags] enum MenuTab` |
| `Assets/Scripts/Runtime/MenuTarget.cs` | what the ray hit → which tabs |
| `Assets/Scripts/Runtime/SwatchButton.cs` | one grid cell: hover/exit/click events |
| `Assets/Scripts/Runtime/ControllerMenu.cs` | builds canvas UI from `Catalogue`; preview/commit |
| `Assets/Scripts/Runtime/MenuSelectRelay.cs` | right-controller trigger → raycast → `ControllerMenu.Show` |
| `Assets/Editor/CatalogueImporter.cs` | one-shot download + import → `Assets/Catalogue/**`, `Catalogue.asset`, `CREDITS.md` |
| `Assets/Editor/SceneBuilder.cs` | **modify**: kinds, `MenuTarget`s, sofa slot, menu canvas, EventSystem |
| `Assets/Scripts/Tests/CatalogueTests.cs` · `SurfaceMaterialTests.cs` · `SchemeManagerMaterialTests.cs` · `FurnitureSlotTests.cs` · `MenuTargetTests.cs` · `ControllerMenuTests.cs` | EditMode tests |
| `Assets/Scripts/PlayModeTests/SceneWiringTests.cs` | **modify**: menu wiring assertions |
| `ip1/2026-08-28-ip1-testing-plan.md`, `ip1/data-collection-sheet.md`, `ip1/ai-use-log.md`, `testing-data/ip1/README.md` | docs |

---

### Task 1: glTFast package + Catalogue data types

**Files:**
- Modify: `Packages/manifest.json`
- Create: `Assets/Scripts/Runtime/SurfaceKind.cs`, `Assets/Scripts/Runtime/Catalogue.cs`
- Test: `Assets/Scripts/Tests/CatalogueTests.cs`

**Interfaces:**
- Produces: `enum SurfaceKind { None=0, Floor=1, Wall=2, Ceiling=4, Trim=8 }` (flags); `enum FurnitureCategory { Seating, Table, Storage }`; `[Serializable] class PaintOption { string name; string code; Color color; }`; `[Serializable] class MaterialOption { string name; string sourceId; Material material; SurfaceKind targets; }`; `[Serializable] class FurnitureOption { string name; string sourceId; GameObject prefab; FurnitureCategory category; }`; `class Catalogue : ScriptableObject { List<PaintOption> paints; List<MaterialOption> materials; List<FurnitureOption> furniture; IEnumerable<MaterialOption> MaterialsFor(SurfaceKind k); }`.

- [ ] **Step 1: Add glTFast to the manifest**

Edit `Packages/manifest.json` — inside `"dependencies"` add (alphabetical position is irrelevant):

```json
    "com.unity.cloud.gltfast": "6.19.0",
```

Then trigger the Editor to resolve packages:

```bash
unity command recompile --no-banner --non-interactive --format tsv
until unity command recompile_status --no-banner --non-interactive --format json | grep -qE '"(completed|up_to_date)"'; do sleep 3; done
grep -n gltfast "Packages/packages-lock.json"
```
Expected: `packages-lock.json` gains `com.unity.cloud.gltfast` 6.19.0; no errors in `unity command console --level error --tail 10`.

- [ ] **Step 2: Write the failing test**

`Assets/Scripts/Tests/CatalogueTests.cs`:

```csharp
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
```

- [ ] **Step 3: Recompile — verify it fails**

Run the recompile block from "How to compile". Expected: console errors `The type or namespace name 'Catalogue' could not be found`.

- [ ] **Step 4: Write the types**

`Assets/Scripts/Runtime/SurfaceKind.cs`:

```csharp
using System;

/// <summary>Which room surface a Surface is; drives which catalogue materials apply.</summary>
[Flags]
public enum SurfaceKind
{
    None = 0,
    Floor = 1,
    Wall = 2,
    Ceiling = 4,
    Trim = 8,
}
```

`Assets/Scripts/Runtime/Catalogue.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FurnitureCategory { Seating, Table, Storage }

[Serializable]
public class PaintOption
{
    public string name;
    public string code;      // Dulux colour code, display only
    public Color color;
}

[Serializable]
public class MaterialOption
{
    public string name;
    public string sourceId;  // ambientCG asset id
    public Material material;
    public SurfaceKind targets;
}

[Serializable]
public class FurnitureOption
{
    public string name;
    public string sourceId;  // Poly Haven asset id
    public GameObject prefab;
    public FurnitureCategory category;
}

/// <summary>
/// The single bundled source the controller menu reads. Generated by
/// CatalogueImporter (Editor); never edited by hand.
/// </summary>
[CreateAssetMenu(menuName = "Renovation/Catalogue")]
public class Catalogue : ScriptableObject
{
    public List<PaintOption> paints = new();
    public List<MaterialOption> materials = new();
    public List<FurnitureOption> furniture = new();

    public IEnumerable<MaterialOption> MaterialsFor(SurfaceKind kind) =>
        materials.Where(m => m.material != null && (m.targets & kind) != 0);
}
```

- [ ] **Step 5: Recompile + run test**

```bash
unity command run_tests --mode EditMode --filter CatalogueTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: 2 passed, 0 failed.

- [ ] **Step 6: Commit**

```bash
cd "/Users/kaikenehme/Desktop/University of Queensland/Semester 4/Digital_Prototype"
git add ip1/RenovationPreviewer/Packages/manifest.json ip1/RenovationPreviewer/Packages/packages-lock.json \
  "ip1/RenovationPreviewer/Assets/Scripts/Runtime/SurfaceKind.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/SurfaceKind.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Runtime/Catalogue.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/Catalogue.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/CatalogueTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/CatalogueTests.cs.meta"
git commit -m "feat(ip1): Catalogue data types + glTFast package"
```

---

### Task 2: Surface — kind, material preview/commit, child renderer binding

**Files:**
- Modify: `Assets/Scripts/Runtime/Surface.cs` (whole file replaced below)
- Test: `Assets/Scripts/Tests/SurfaceMaterialTests.cs`

**Interfaces:**
- Consumes: `SurfaceKind` (Task 1).
- Produces on `Surface`: `SurfaceKind Kind {get;}`, `void SetKind(SurfaceKind)`, `Material CommittedMaterial {get;}`, `Material DisplayMaterial {get;}`, `void PreviewMaterial(Material)`, `void CommitMaterial(Material)`, `void RebindRenderer()`. Existing `Preview(Color)/Commit(Color)/Revert()` keep their signatures; `Revert()` now also reverts material.

- [ ] **Step 1: Write the failing tests**

`Assets/Scripts/Tests/SurfaceMaterialTests.cs`:

```csharp
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
```

- [ ] **Step 2: Recompile — verify it fails**

Expected: compile errors `'Surface' does not contain a definition for 'SetKind'` (and `PreviewMaterial`, `CommitMaterial`, `RebindRenderer`, `DisplayMaterial`, `CommittedMaterial`, `Kind`).

- [ ] **Step 3: Replace `Surface.cs`**

```csharp
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceState { Keep, Change }

/// <summary>
/// State machine for one renovatable surface. Holds Keep/Change state and the
/// committed vs previewed colour AND material. Never knows samples or menus
/// exist (concept spec §8). ExecuteAlways so the static registry works in
/// EditMode tests and scene tooling; renderer writes are guarded to play mode.
/// </summary>
[ExecuteAlways]
public class Surface : MonoBehaviour
{
    public static readonly List<Surface> All = new();

    [SerializeField] SurfaceState state = SurfaceState.Change;
    [SerializeField] SurfaceKind kind = SurfaceKind.None;

    public SurfaceState State => state;
    public SurfaceKind Kind => kind;

    public Color CommittedColor { get; private set; } = Color.white;
    public Color DisplayColor { get; private set; } = Color.white;
    /// <summary>Material committed via the menu; null = whatever the scene shipped with.</summary>
    public Material CommittedMaterial { get; private set; }
    public Material DisplayMaterial { get; private set; }
    public bool IsPreviewing { get; private set; }

    Renderer rend;
    Material baseMaterial;   // the material the scene shipped with, for revert-to-original

    void Awake() => RebindRenderer();

    /// <summary>Re-read the renderer (root or first child). Call after swapping the visual.</summary>
    public void RebindRenderer()
    {
        rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
        {
            baseMaterial = rend.sharedMaterial;
            CommittedColor = DisplayColor = rend.sharedMaterial.color;
        }
    }

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable() => All.Remove(this);

    public void SetState(SurfaceState s) => state = s;
    public void SetKind(SurfaceKind k) => kind = k;

    public void ToggleState() =>
        state = state == SurfaceState.Keep ? SurfaceState.Change : SurfaceState.Keep;

    // ---- colour ----
    public void Preview(Color c)
    {
        if (state == SurfaceState.Keep) return; // kept surfaces are sources, never targets
        IsPreviewing = true;
        ApplyColor(c);
    }

    public void Commit(Color c)
    {
        if (state == SurfaceState.Keep) return;
        CommittedColor = c;
        IsPreviewing = false;
        ApplyColor(c);
    }

    // ---- material ----
    public void PreviewMaterial(Material m)
    {
        if (state == SurfaceState.Keep) return;
        IsPreviewing = true;
        ApplyMaterial(m);
    }

    public void CommitMaterial(Material m)
    {
        if (state == SurfaceState.Keep) return;
        CommittedMaterial = m;
        IsPreviewing = false;
        ApplyMaterial(m);
    }

    /// <summary>A preview must never stick: restore committed material then committed colour.</summary>
    public void Revert()
    {
        IsPreviewing = false;
        ApplyMaterial(CommittedMaterial);
        ApplyColor(CommittedColor);
    }

    void ApplyColor(Color c)
    {
        DisplayColor = c;
        if (Application.isPlaying && rend != null)
            rend.material.color = c;
    }

    void ApplyMaterial(Material m)
    {
        DisplayMaterial = m;
        if (!Application.isPlaying || rend == null) return;
        var src = m != null ? m : baseMaterial;
        if (src == null) return;
        // Instance so tint edits never write into the shared asset.
        var inst = new Material(src) { color = DisplayColor };
        rend.material = inst;
    }
}
```

- [ ] **Step 4: Run new + old Surface tests**

```bash
unity command run_tests --mode EditMode --filter SurfaceMaterialTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
unity command run_tests --mode EditMode --filter SurfaceTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: 7 passed (new) and all existing `SurfaceTests` still pass.

- [ ] **Step 5: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/Surface.cs" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/SurfaceMaterialTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/SurfaceMaterialTests.cs.meta"
git commit -m "feat(ip1): Surface kind + material preview/commit; bind child renderers"
```

---

### Task 3: SchemeManager snapshots material + colour

**Files:**
- Modify: `Assets/Scripts/Runtime/SchemeManager.cs` (whole file)
- Test: `Assets/Scripts/Tests/SchemeManagerMaterialTests.cs`

**Interfaces:**
- Consumes: `Surface.CommittedMaterial`, `Surface.CommitMaterial(Material)`, `Surface.Commit(Color)`.
- Produces: `struct SurfaceLook { Color color; Material material; }`; `SchemeManager.SaveScheme()/ApplyScheme(int)/Count/MaxSchemes` unchanged.

- [ ] **Step 1: Write the failing test**

`Assets/Scripts/Tests/SchemeManagerMaterialTests.cs`:

```csharp
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
```

- [ ] **Step 2: Recompile + run — verify it fails**

Expected: `SaveAndApply_RoundTripsMaterialAndColour` FAILS (material not restored — `CommittedMaterial` is "carpet").

- [ ] **Step 3: Replace `SchemeManager.cs`**

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>What a Change surface looked like when a scheme was saved.</summary>
public struct SurfaceLook
{
    public Color color;
    public Material material;   // null = the material the scene shipped with
}

/// <summary>
/// Snapshot / restore of all Change-surface committed looks (material + colour).
/// Up to 3 schemes, oldest slot overwritten on overflow (concept spec §8).
/// </summary>
public class SchemeManager : MonoBehaviour
{
    public const int MaxSchemes = 3;

    readonly List<Dictionary<Surface, SurfaceLook>> schemes = new();
    int nextSlot;

    public int Count => schemes.Count;

    public int SaveScheme()
    {
        var snapshot = new Dictionary<Surface, SurfaceLook>();
        foreach (var s in Surface.All)
            if (s != null && s.State == SurfaceState.Change)
                snapshot[s] = new SurfaceLook { color = s.CommittedColor, material = s.CommittedMaterial };

        int slot = nextSlot;
        if (schemes.Count < MaxSchemes) schemes.Add(snapshot);
        else schemes[slot] = snapshot;
        nextSlot = (slot + 1) % MaxSchemes;
        return slot;
    }

    public void ApplyScheme(int slot)
    {
        if (slot < 0 || slot >= schemes.Count) return;
        foreach (var kv in schemes[slot])
        {
            if (kv.Key == null) continue;
            kv.Key.CommitMaterial(kv.Value.material);   // material first, colour tints it
            kv.Key.Commit(kv.Value.color);
        }
    }
}
```

- [ ] **Step 4: Run both scheme test classes**

```bash
unity command run_tests --mode EditMode --filter SchemeManagerMaterialTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
unity command run_tests --mode EditMode --filter SchemeManagerTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: all pass.

- [ ] **Step 5: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/SchemeManager.cs" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/SchemeManagerMaterialTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/SchemeManagerMaterialTests.cs.meta"
git commit -m "feat(ip1): schemes snapshot material + colour"
```

---

### Task 4: FurnitureSlot — swap, remove, spawn, grab + floor snap

**Files:**
- Create: `Assets/Scripts/Runtime/FurnitureSlot.cs`
- Test: `Assets/Scripts/Tests/FurnitureSlotTests.cs`

**Interfaces:**
- Consumes: `FurnitureOption` (Task 1), `Surface.RebindRenderer()` (Task 2).
- Produces: `class FurnitureSlot : MonoBehaviour { FurnitureOption Current; GameObject Visual; void Swap(FurnitureOption); void Remove(); static FurnitureSlot Spawn(FurnitureOption, Vector3 floorPoint, Bounds floorBounds); const float FloorMargin = 0.3f; }`. Slot root carries `Rigidbody(kinematic)`, `BoxCollider`, `XRGrabInteractable` — added in `Spawn` and by `SceneBuilder` for the sofa (Task 8).

- [ ] **Step 1: Write the failing tests**

`Assets/Scripts/Tests/FurnitureSlotTests.cs`:

```csharp
using NUnit.Framework;
using UnityEngine;

public class FurnitureSlotTests
{
    static FurnitureOption Option(string id, float height = 0.8f)
    {
        var prefab = new GameObject(id);
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.transform.SetParent(prefab.transform);
        mesh.transform.localPosition = new Vector3(0, height / 2f, 0);
        mesh.transform.localScale = new Vector3(0.6f, height, 0.6f);
        return new FurnitureOption { name = id, sourceId = id, prefab = prefab, category = FurnitureCategory.Seating };
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var slot in Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None))
            Object.DestroyImmediate(slot.gameObject);
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Swap_ReplacesVisual_KeepsPose()
    {
        var root = new GameObject("Sofa");
        root.transform.position = new Vector3(-1.2f, 0, -0.9f);
        root.transform.rotation = Quaternion.Euler(0, 90, 0);
        var slot = root.AddComponent<FurnitureSlot>();
        var first = Option("A"); var second = Option("B");

        slot.Swap(first);
        var firstVisual = slot.Visual;
        slot.Swap(second);

        Assert.AreSame(second, slot.Current);
        Assert.IsTrue(firstVisual == null, "old visual destroyed");
        Assert.AreEqual("B", slot.Visual.name);
        Assert.AreEqual(root.transform, slot.Visual.transform.parent);
        Assert.AreEqual(Vector3.zero, slot.Visual.transform.localPosition);
        Assert.AreEqual(new Vector3(-1.2f, 0, -0.9f), root.transform.position);
        Assert.AreEqual(90f, root.transform.eulerAngles.y, 0.01f);
    }

    [Test]
    public void Swap_RefitsColliderAndRebindsSurface()
    {
        var root = new GameObject("Sofa");
        var surf = root.AddComponent<Surface>();
        surf.SetState(SurfaceState.Keep);
        var col = root.AddComponent<BoxCollider>();
        var slot = root.AddComponent<FurnitureSlot>();

        slot.Swap(Option("Tall", height = 1.2f));

        Assert.AreEqual(1.2f, col.size.y, 0.01f, "collider fits new visual");
        Assert.AreEqual(0.6f, col.center.y, 0.01f);
        Assert.AreEqual(surf.CommittedColor, slot.Visual.GetComponentInChildren<Renderer>().sharedMaterial.color, "surface re-bound to new renderer");
    }

    [Test]
    public void Remove_DestroysVisual_KeepsSlot()
    {
        var root = new GameObject("Sofa");
        var slot = root.AddComponent<FurnitureSlot>();
        slot.Swap(Option("A"));
        slot.Remove();
        Assert.IsNull(slot.Current);
        Assert.IsTrue(slot.Visual == null);
        Assert.IsTrue(root != null && slot != null, "slot survives");
    }

    [Test]
    public void Spawn_ClampsToFloorBounds_AndSitsOnFloor()
    {
        var floorBounds = new Bounds(Vector3.zero, new Vector3(4, 0.1f, 3));
        var slot = FurnitureSlot.Spawn(Option("A"), new Vector3(5f, 0.7f, -9f), floorBounds);
        Assert.AreEqual(2f - FurnitureSlot.FloorMargin, slot.transform.position.x, 0.001f);
        Assert.AreEqual(-1.5f + FurnitureSlot.FloorMargin, slot.transform.position.z, 0.001f);
        Assert.AreEqual(0f, slot.transform.position.y, 0.001f);
        Assert.IsNotNull(slot.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>());
        Assert.IsTrue(slot.GetComponent<Rigidbody>().isKinematic);
        Assert.IsNotNull(slot.GetComponent<MenuTarget>());
    }
}
```

Note the last assertion references `MenuTarget` — created in Task 6. Until then this single test fails to compile; keep the test file but comment that one line out, and uncomment it in Task 6 Step 5.

- [ ] **Step 2: Recompile — verify it fails**

Expected: `'FurnitureSlot' could not be found`.

- [ ] **Step 3: Write `FurnitureSlot.cs`**

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// One piece of movable furniture. The slot GameObject is the stable identity
/// (pose, collider, grab, Surface for sample-pulling); the visual is a child that
/// Swap() replaces. Remove() leaves the empty slot so nothing else dangles.
/// </summary>
public class FurnitureSlot : MonoBehaviour
{
    public const float FloorMargin = 0.3f;

    public FurnitureOption Current { get; private set; }
    public GameObject Visual { get; private set; }

    Bounds? floorBounds;

    public void Swap(FurnitureOption option)
    {
        if (Visual != null) DestroyNow(Visual);
        Current = option;
        if (option == null || option.prefab == null) { Visual = null; return; }

        Visual = Instantiate(option.prefab, transform);
        Visual.name = option.prefab.name;
        Visual.transform.localPosition = Vector3.zero;
        Visual.transform.localRotation = Quaternion.identity;
        Visual.transform.localScale = Vector3.one;
        // Visual meshes must not carry colliders of their own — the slot's box is the only one.
        foreach (var c in Visual.GetComponentsInChildren<Collider>()) DestroyNow(c);

        FitCollider();
        var surf = GetComponent<Surface>();
        if (surf != null) surf.RebindRenderer();
    }

    public void Remove()
    {
        if (Visual != null) DestroyNow(Visual);
        Visual = null;
        Current = null;
    }

    /// <summary>Create a new grabbable slot on the floor at floorPoint, clamped inside floorBounds.</summary>
    public static FurnitureSlot Spawn(FurnitureOption option, Vector3 floorPoint, Bounds floorBounds)
    {
        var go = new GameObject($"Furniture_{option.sourceId}");
        var slot = go.AddComponent<FurnitureSlot>();
        slot.floorBounds = floorBounds;
        go.transform.position = slot.Clamp(floorPoint);

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        go.AddComponent<BoxCollider>();
        var grab = go.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        grab.selectExited.AddListener(slot.OnReleased);
        go.AddComponent<MenuTarget>();

        slot.Swap(option);
        return slot;
    }

    /// <summary>Wire the release snap on a slot that was built by SceneBuilder (sofa).</summary>
    public void BindGrab(Bounds bounds)
    {
        floorBounds = bounds;
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null) grab.selectExited.AddListener(OnReleased);
    }

    void OnReleased(SelectExitEventArgs _)
    {
        // Snap: flat on the floor, upright, inside the room.
        var p = Clamp(transform.position);
        transform.position = new Vector3(p.x, 0f, p.z);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    Vector3 Clamp(Vector3 p)
    {
        if (floorBounds == null) return new Vector3(p.x, 0f, p.z);
        var b = floorBounds.Value;
        return new Vector3(
            Mathf.Clamp(p.x, b.min.x + FloorMargin, b.max.x - FloorMargin),
            0f,
            Mathf.Clamp(p.z, b.min.z + FloorMargin, b.max.z - FloorMargin));
    }

    void FitCollider()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null || Visual == null) return;
        var rends = Visual.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;
        var b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        col.center = transform.InverseTransformPoint(b.center);
        var s = transform.lossyScale;
        col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
    }

    static void DestroyNow(Object o)
    {
        if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
    }
}
```

- [ ] **Step 4: Run tests**

```bash
unity command run_tests --mode EditMode --filter FurnitureSlotTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: 4 passed (with the `MenuTarget` line commented out).

- [ ] **Step 5: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/FurnitureSlot.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/FurnitureSlot.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/FurnitureSlotTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/FurnitureSlotTests.cs.meta"
git commit -m "feat(ip1): FurnitureSlot swap/remove/spawn with grab + floor snap"
```

---

### Task 5: CatalogueImporter (Editor) — download + import + build `Catalogue.asset`

**Files:**
- Create: `Assets/Editor/CatalogueImporter.cs`
- Produces on disk: `Assets/Catalogue/Furniture/<id>/…`, `Assets/Catalogue/Materials/<id>/…` + `<id>.mat`, `Assets/Catalogue/Prefabs/<id>.prefab`, `Assets/Catalogue/Paints/dulux.json`, `Assets/Catalogue/Catalogue.asset`, `Assets/Catalogue/CREDITS.md`

**Interfaces:**
- Consumes: `Catalogue`, `PaintOption`, `MaterialOption`, `FurnitureOption`, `SurfaceKind`, `FurnitureCategory` (Task 1).
- Produces: menu item **Renovation → Import Catalogue** and `public static void CatalogueImporter.Import()`; `Catalogue.asset` at `Assets/Catalogue/Catalogue.asset`.

No unit test (network + AssetDatabase). Verification = run it and assert the asset contents in Step 3.

- [ ] **Step 1: Write `CatalogueImporter.cs`**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot, idempotent importer. Pulls CC0 furniture (Poly Haven glTF 1k),
/// CC0 PBR materials (ambientCG 1K-JPG) and Dulux colour data, writes them under
/// Assets/Catalogue, builds URP materials + furniture prefabs, and fills
/// Catalogue.asset. Re-running skips files already on disk.
/// </summary>
public static class CatalogueImporter
{
    const string Root = "Assets/Catalogue";
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(5) };

    // ---- what we bundle (ids are the sources' own asset ids) ----
    static readonly (string id, string label, FurnitureCategory cat)[] Furniture =
    {
        ("Sofa_01", "Sofa (grey)", FurnitureCategory.Seating),
        ("sofa_02", "Sofa (leather)", FurnitureCategory.Seating),
        ("ArmChair_01", "Armchair", FurnitureCategory.Seating),
        ("mid_century_lounge_chair", "Lounge chair", FurnitureCategory.Seating),
        ("modern_coffee_table_01", "Coffee table", FurnitureCategory.Table),
        ("coffee_table_round_01", "Round coffee table", FurnitureCategory.Table),
        ("side_table_01", "Side table", FurnitureCategory.Table),
        ("painted_wooden_shelves", "Shelves", FurnitureCategory.Storage),
    };

    static readonly (string id, string label, SurfaceKind targets, float tile)[] Materials =
    {
        ("Tiles040", "Stone tiles", SurfaceKind.Floor, 0.5f),
        ("Tiles107", "Hex tiles", SurfaceKind.Floor | SurfaceKind.Wall, 0.5f),
        ("Tiles133A", "Terrazzo tiles", SurfaceKind.Floor, 0.6f),
        ("Marble012", "Marble", SurfaceKind.Floor | SurfaceKind.Wall, 1.0f),
        ("WoodFloor051", "Oak boards", SurfaceKind.Floor, 1.0f),
        ("WoodFloor043", "Dark timber", SurfaceKind.Floor, 1.0f),
        ("Carpet016", "Carpet", SurfaceKind.Floor, 0.5f),
        ("Plaster001", "Plaster", SurfaceKind.Wall | SurfaceKind.Ceiling, 1.0f),
        ("PaintedPlaster017", "Painted plaster", SurfaceKind.Wall | SurfaceKind.Ceiling, 1.0f),
        ("Concrete034", "Concrete", SurfaceKind.Wall | SurfaceKind.Floor, 1.0f),
    };

    // 24 Dulux AU names; any missing from the dataset are topped up by hue spread.
    static readonly string[] PaintNames =
    {
        "Whisper White", "Antique White U.S.A.", "Highgate", "Grey Pebble", "Clay Pipe", "Sandy Day",
        "Beige Royal", "Warm Neutral", "Silkwort", "Tranquil Retreat", "Powder Blue", "Mustard Seed",
        "Berry Crush", "Domino", "Black", "Natural White", "Lexicon", "Vivid White", "Monument",
        "Wombat", "Deep Ocean", "Teal Waters", "Wild Sage", "Terracotta",
    };

    [MenuItem("Renovation/Import Catalogue")]
    public static void Import()
    {
        Directory.CreateDirectory(Root);
        var credits = new StringBuilder("# Catalogue credits\n\nAll 3D models and textures are CC0 (public domain). Colour data is public Dulux Australia colour information used for display names only.\n\n");
        var cat = AssetDatabase.LoadAssetAtPath<Catalogue>($"{Root}/Catalogue.asset");
        if (cat == null)
        {
            cat = ScriptableObject.CreateInstance<Catalogue>();
            AssetDatabase.CreateAsset(cat, $"{Root}/Catalogue.asset");
        }
        cat.paints.Clear(); cat.materials.Clear(); cat.furniture.Clear();

        try
        {
            ImportPaints(cat, credits);
            ImportMaterials(cat, credits);
            ImportFurniture(cat, credits);
        }
        finally
        {
            File.WriteAllText($"{Root}/CREDITS.md", credits.ToString());
            EditorUtility.SetDirty(cat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
        Debug.Log($"CatalogueImporter: {cat.paints.Count} paints, {cat.materials.Count} materials, {cat.furniture.Count} furniture");
    }

    // ------------------------------------------------------------------ paints
    [Serializable] class DuluxColour { public string code; public string name; public int red, green, blue, lrv; public string url; }
    [Serializable] class DuluxFile { public int count; public List<DuluxColour> colours; }

    static void ImportPaints(Catalogue cat, StringBuilder credits)
    {
        var dir = $"{Root}/Paints"; Directory.CreateDirectory(dir);
        var path = $"{dir}/dulux.json";
        if (!File.Exists(path))
            File.WriteAllBytes(path, Get("https://raw.githubusercontent.com/shanmoorthy/dulux-paint-colour-data/master/data/colours.json"));
        var all = JsonUtility.FromJson<DuluxFile>(File.ReadAllText(path)).colours;

        var picked = new List<DuluxColour>();
        foreach (var n in PaintNames)
        {
            var c = all.FirstOrDefault(x => x.name == n);
            if (c != null && picked.All(p => p.code != c.code)) picked.Add(c);
        }
        // top up to 24 with a hue spread of mid-LRV colours
        var pool = all.Where(x => x.lrv >= 20 && x.lrv <= 75 && picked.All(p => p.code != x.code))
                      .OrderBy(x => Hue(x)).ToList();
        for (int i = 0; picked.Count < 24 && pool.Count > 0; i++)
            picked.Add(pool[(int)((long)i * pool.Count / 12) % pool.Count]);

        foreach (var c in picked)
            cat.paints.Add(new PaintOption { name = c.name, code = c.code, color = new Color(c.red / 255f, c.green / 255f, c.blue / 255f) });
        credits.AppendLine($"## Paint\n- Dulux Australia colour names/RGB via https://github.com/shanmoorthy/dulux-paint-colour-data ({picked.Count} colours)\n");
    }

    static float Hue(DuluxColour c) { Color.RGBToHSV(new Color(c.red / 255f, c.green / 255f, c.blue / 255f), out var h, out _, out _); return h; }

    // --------------------------------------------------------------- materials
    static void ImportMaterials(Catalogue cat, StringBuilder credits)
    {
        credits.AppendLine("## Materials (ambientCG, CC0)");
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        for (int i = 0; i < Materials.Length; i++)
        {
            var (id, label, targets, tile) = Materials[i];
            EditorUtility.DisplayProgressBar("Catalogue", $"Material {id}", (float)i / Materials.Length);
            var dir = $"{Root}/Materials/{id}"; Directory.CreateDirectory(dir);
            var colorPath = $"{dir}/{id}_1K-JPG_Color.jpg";
            var normalPath = $"{dir}/{id}_1K-JPG_NormalGL.jpg";
            if (!File.Exists(colorPath))
            {
                var zip = Get($"https://ambientcg.com/get?file={id}_1K-JPG.zip");
                using var arc = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
                foreach (var e in arc.Entries)
                    if (e.Name.EndsWith("_Color.jpg") || e.Name.EndsWith("_NormalGL.jpg"))
                        e.ExtractToFile($"{dir}/{e.Name}", true);
            }
            AssetDatabase.ImportAsset(colorPath); AssetDatabase.ImportAsset(normalPath);
            MarkNormal(normalPath);

            var matPath = $"{dir}/{id}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(litShader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.shader = litShader;
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath));
            mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            mat.EnableKeyword("_NORMALMAP");
            mat.color = Color.white;
            mat.SetFloat("_Smoothness", 0.35f);
            // Cubes are 1 unit; scale is applied per surface size at runtime by the shader tiling below.
            mat.mainTextureScale = new Vector2(1f / tile, 1f / tile);
            EditorUtility.SetDirty(mat);

            cat.materials.Add(new MaterialOption { name = label, sourceId = id, material = mat, targets = targets });
            credits.AppendLine($"- {id} — https://ambientcg.com/view?id={id}");
        }
        credits.AppendLine();
    }

    static void MarkNormal(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null || imp.textureType == TextureImporterType.NormalMap) return;
        imp.textureType = TextureImporterType.NormalMap;
        imp.SaveAndReimport();
    }

    // --------------------------------------------------------------- furniture
    [Serializable] class PhFile { public string url; public long size; }

    static void ImportFurniture(Catalogue cat, StringBuilder credits)
    {
        credits.AppendLine("## Furniture (Poly Haven, CC0)");
        Directory.CreateDirectory($"{Root}/Prefabs");
        for (int i = 0; i < Furniture.Length; i++)
        {
            var (id, label, category) = Furniture[i];
            EditorUtility.DisplayProgressBar("Catalogue", $"Furniture {id}", (float)i / Furniture.Length);
            try
            {
                var dir = $"{Root}/Furniture/{id}"; Directory.CreateDirectory(dir);
                var gltfPath = $"{dir}/{id}.gltf";
                if (!File.Exists(gltfPath))
                {
                    // files/<id> → { gltf: { "1k": { gltf: { url, include: { "<rel>": { url } } } } } }
                    var json = Encoding.UTF8.GetString(Get($"https://api.polyhaven.com/files/{id}"));
                    var gltfUrl = ExtractString(json, "\"1k\"", "\"gltf\"", "\"url\"");
                    File.WriteAllBytes(gltfPath, Get(gltfUrl));
                    foreach (var (rel, url) in ExtractIncludes(json))
                    {
                        var target = $"{dir}/{rel}";
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        if (!File.Exists(target)) File.WriteAllBytes(target, Get(url));
                    }
                }
                AssetDatabase.ImportAsset(gltfPath, ImportAssetOptions.ForceSynchronousImport);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(gltfPath);
                if (model == null) { Debug.LogWarning($"CatalogueImporter: glTF import produced no GameObject for {id}"); continue; }

                var prefabPath = $"{Root}/Prefabs/{id}.prefab";
                var wrapper = new GameObject(id);
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(model, wrapper.transform);
                inst.transform.localPosition = Vector3.zero;
                // Pivot at the floor: lift so the lowest renderer point sits on y = 0.
                var rends = wrapper.GetComponentsInChildren<Renderer>();
                if (rends.Length > 0)
                {
                    var b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
                    inst.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
                }
                var prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath);
                UnityEngine.Object.DestroyImmediate(wrapper);

                cat.furniture.Add(new FurnitureOption { name = label, sourceId = id, prefab = prefab, category = category });
                credits.AppendLine($"- {id} — https://polyhaven.com/a/{id}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"CatalogueImporter: skipped {id}: {ex.Message}");
            }
        }
        credits.AppendLine();
    }

    // Minimal JSON digging — avoids a JSON dependency for two lookups.
    static string ExtractString(string json, params string[] keysInOrder)
    {
        int pos = 0;
        foreach (var k in keysInOrder)
        {
            pos = json.IndexOf(k, pos, StringComparison.Ordinal);
            if (pos < 0) throw new Exception($"key {k} not found");
            pos += k.Length;
        }
        int q1 = json.IndexOf('"', json.IndexOf(':', pos) + 1);
        int q2 = json.IndexOf('"', q1 + 1);
        return json.Substring(q1 + 1, q2 - q1 - 1);
    }

    static IEnumerable<(string rel, string url)> ExtractIncludes(string json)
    {
        int k1 = json.IndexOf("\"1k\"", StringComparison.Ordinal);
        int inc = json.IndexOf("\"include\"", k1, StringComparison.Ordinal);
        int end = json.IndexOf("}}", inc, StringComparison.Ordinal);   // end of the include object
        var block = json.Substring(inc, end - inc);
        var parts = block.Split(new[] { "\"url\"" }, StringSplitOptions.None);
        for (int i = 1; i < parts.Length; i++)
        {
            var head = parts[i - 1];
            int keyEnd = head.LastIndexOf("\":", StringComparison.Ordinal);
            int keyStart = head.LastIndexOf('"', keyEnd - 1);
            var rel = head.Substring(keyStart + 1, keyEnd - keyStart - 1);
            int q1 = parts[i].IndexOf('"', parts[i].IndexOf(':') + 1);
            int q2 = parts[i].IndexOf('"', q1 + 1);
            yield return (rel, parts[i].Substring(q1 + 1, q2 - q1 - 1));
        }
    }

    static byte[] Get(string url)
    {
        var res = Http.GetAsync(url).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();
        return res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
    }
}
```

- [ ] **Step 2: Recompile, then run the importer through the open Editor**

```bash
unity command recompile --no-banner --non-interactive --format tsv
until unity command recompile_status --no-banner --non-interactive --format json | grep -qE '"(completed|up_to_date)"'; do sleep 2; done
unity command eval --timeout 600 --no-banner --non-interactive --format json --code 'CatalogueImporter.Import(); return "ok";'
unity command console --tail 30 --no-banner --non-interactive --format json | grep -iE "CatalogueImporter|error" 
```
Expected: log line `CatalogueImporter: 24 paints, 10 materials, 8 furniture` (furniture may be fewer if a glTF fails — each skip is logged; ≥6 required).

- [ ] **Step 3: Verify the asset from the Editor**

```bash
unity command eval --no-banner --non-interactive --format json --code '
var c = UnityEditor.AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
var sb = new System.Text.StringBuilder();
sb.Append("paints=" + c.paints.Count + " materials=" + c.materials.Count + " furniture=" + c.furniture.Count + "\n");
foreach (var m in c.materials) sb.Append(m.sourceId + " tex=" + (m.material.GetTexture("_BaseMap") != null) + " nrm=" + (m.material.GetTexture("_BumpMap") != null) + " targets=" + m.targets + "\n");
foreach (var f in c.furniture) { var r = f.prefab.GetComponentsInChildren<Renderer>(); var b = r[0].bounds; foreach (var x in r) b.Encapsulate(x.bounds); sb.Append(f.sourceId + " size=" + b.size.ToString("F2") + " minY=" + b.min.y.ToString("F3") + "\n"); }
return sb.ToString();'
```
Expected: every material `tex=True nrm=True`; every furniture `minY≈0.000` and plausible metre sizes (sofa ≈ 1.6×0.7×0.8; side table ≈ 0.55×0.55×0.45).

- [ ] **Step 4: Screenshot one furniture prefab + one material to eyeball**

```bash
unity command eval --no-banner --non-interactive --format json --code '
var c = UnityEditor.AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(c.furniture[0].prefab); go.name = "__preview"; go.transform.position = new Vector3(0, 0, 0.6f);
var floor = GameObject.Find("Floor").GetComponent<Renderer>(); floor.sharedMaterial = c.materials[0].material;
var sv = UnityEditor.SceneView.lastActiveSceneView; sv.LookAt(new Vector3(0, 0.6f, 0), Quaternion.Euler(25, 200, 0), 2.5f, false, true); return "ok";'
unity command capture_scene_view --width 1200 --height 800 --save_path "Temp/catalogue_check.png" --no-banner --non-interactive --format tsv
```
Look at `Assets/Temp/catalogue_check.png`. Then undo the scene poke (do **not** save the scene):

```bash
unity command eval --no-banner --non-interactive --format json --code 'UnityEngine.Object.DestroyImmediate(GameObject.Find("__preview")); GameObject.Find("Floor").GetComponent<Renderer>().sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Timber.mat"); return "reset";'
unity command delete_asset --asset Assets/Temp --confirm true --no-banner --non-interactive --format tsv
```

- [ ] **Step 5: Commit the importer and the generated assets**

```bash
git add "ip1/RenovationPreviewer/Assets/Editor/CatalogueImporter.cs" "ip1/RenovationPreviewer/Assets/Editor/CatalogueImporter.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Catalogue" "ip1/RenovationPreviewer/Assets/Catalogue.meta"
git commit -m "feat(ip1): bundled catalogue — Poly Haven furniture, ambientCG materials, Dulux paints (CC0)"
```

---

### Task 6: MenuTab + MenuTarget

**Files:**
- Create: `Assets/Scripts/Runtime/MenuTab.cs`, `Assets/Scripts/Runtime/MenuTarget.cs`
- Test: `Assets/Scripts/Tests/MenuTargetTests.cs`
- Modify: `Assets/Scripts/Tests/FurnitureSlotTests.cs` (uncomment the `MenuTarget` assertion)

**Interfaces:**
- Consumes: `Surface` (`State`, `Kind`, `Revert()`), `FurnitureSlot`.
- Produces: `[Flags] enum MenuTab { None=0, Paint=1, Material=2, Furniture=4, Swap=8, Remove=16, KeepPrompt=32 }`; `class MenuTarget : MonoBehaviour { Surface Surface; FurnitureSlot Slot; MenuTab Tabs {get;} string DisplayName {get;} }`.

- [ ] **Step 1: Write the failing test**

`Assets/Scripts/Tests/MenuTargetTests.cs`:

```csharp
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
```

- [ ] **Step 2: Recompile — verify it fails** (`MenuTarget` not found).

- [ ] **Step 3: Write the two files**

`Assets/Scripts/Runtime/MenuTab.cs`:

```csharp
using System;

[Flags]
public enum MenuTab
{
    None = 0,
    Paint = 1,
    Material = 2,
    Furniture = 4,     // "add furniture" — floor only
    Swap = 8,
    Remove = 16,
    KeepPrompt = 32,   // "this is staying — change it?"
}
```

`Assets/Scripts/Runtime/MenuTarget.cs`:

```csharp
using UnityEngine;

/// <summary>
/// Marks something the right-hand ray can select for the controller menu and
/// says which tabs apply. Pure lookup — no UI, no input.
/// </summary>
public class MenuTarget : MonoBehaviour
{
    public Surface Surface { get; private set; }
    public FurnitureSlot Slot { get; private set; }

    void Awake()
    {
        Surface = GetComponent<Surface>();
        Slot = GetComponent<FurnitureSlot>();
    }

    public MenuTab Tabs
    {
        get
        {
            if (Slot == null) Slot = GetComponent<FurnitureSlot>();
            if (Surface == null) Surface = GetComponent<Surface>();
            if (Slot != null) return MenuTab.Swap | MenuTab.Remove;
            if (Surface == null) return MenuTab.None;
            if (Surface.State == SurfaceState.Keep) return MenuTab.KeepPrompt;
            var tabs = MenuTab.Paint;
            if (Surface.Kind is SurfaceKind.Floor or SurfaceKind.Wall or SurfaceKind.Ceiling) tabs |= MenuTab.Material;
            if (Surface.Kind == SurfaceKind.Floor) tabs |= MenuTab.Furniture;
            return tabs;
        }
    }

    public string DisplayName => gameObject.name.Replace('_', ' ');

    void OnDisable()
    {
        // A target vanishing mid-preview must never leave a preview stuck.
        if (Surface != null && Surface.IsPreviewing) Surface.Revert();
    }
}
```

- [ ] **Step 4: Uncomment the `MenuTarget` line in `FurnitureSlotTests.Spawn_ClampsToFloorBounds_AndSitsOnFloor`.**

- [ ] **Step 5: Run tests**

```bash
unity command run_tests --mode EditMode --filter MenuTargetTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
unity command run_tests --mode EditMode --filter FurnitureSlotTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: 6 + 4 passed.

- [ ] **Step 6: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuTab.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuTab.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuTarget.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuTarget.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/MenuTargetTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/MenuTargetTests.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/FurnitureSlotTests.cs"
git commit -m "feat(ip1): MenuTarget — which menu tabs apply to what the ray hit"
```

---

### Task 7: SwatchButton + ControllerMenu (runtime-built world-space UI)

**Files:**
- Create: `Assets/Scripts/Runtime/SwatchButton.cs`, `Assets/Scripts/Runtime/ControllerMenu.cs`
- Test: `Assets/Scripts/Tests/ControllerMenuTests.cs`

**Interfaces:**
- Consumes: `Catalogue`, `MenuTarget`, `MenuTab`, `Surface`, `FurnitureSlot`.
- Produces: `class SwatchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler { UnityEvent onHover, onExit, onClick; void Set(string label, Color tint, Texture tex); }`; `class ControllerMenu : MonoBehaviour { Catalogue catalogue; Transform head; Bounds floorBounds; MenuTarget Current; bool IsOpen; MenuTab ActiveTab; void Show(MenuTarget); void ShowTab(MenuTab); void Hide(); Vector3 SpawnPoint; int GridButtonCount; int TabButtonCount; static ControllerMenu Instance; }`. The canvas is built by `ControllerMenu.Awake` on its own GameObject; `SceneBuilder` only needs to add the component to a child of the left controller and set `catalogue`, `head`, `floorBounds`.

- [ ] **Step 1: Write the failing tests**

`Assets/Scripts/Tests/ControllerMenuTests.cs`:

```csharp
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

        var go = new GameObject("Menu");
        menu = go.AddComponent<ControllerMenu>();
        menu.catalogue = cat;
        menu.head = new GameObject("head").transform;
        menu.floorBounds = new Bounds(Vector3.zero, new Vector3(4, 0.1f, 3));
        menu.Initialise();   // what Awake does; called explicitly because AddComponent in EditMode runs Awake only for ExecuteAlways
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var t in Object.FindObjectsByType<MenuTarget>(FindObjectsSortMode.None)) Object.DestroyImmediate(t.gameObject);
        foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject);
        foreach (var f in Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None)) Object.DestroyImmediate(f.gameObject);
        Object.DestroyImmediate(menu.gameObject);
        Object.DestroyImmediate(menu.head.gameObject);
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
}
```

- [ ] **Step 2: Recompile — verify it fails** (`ControllerMenu` not found).

- [ ] **Step 3: Write `SwatchButton.cs`**

```csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>One cell of the menu grid. Emits hover / exit / click; owns no logic.</summary>
[RequireComponent(typeof(Image))]
public class SwatchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent onHover = new();
    public UnityEvent onExit = new();
    public UnityEvent onClick = new();

    Image image;
    Text label;
    Outline outline;

    void Awake() => Build();

    public void Build()
    {
        if (image != null) return;
        image = GetComponent<Image>();
        outline = gameObject.GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        var t = new GameObject("Label", typeof(RectTransform));
        t.transform.SetParent(transform, false);
        var rt = (RectTransform)t.transform;
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0.38f);
        rt.offsetMin = new Vector2(4, 4); rt.offsetMax = new Vector2(-4, 0);
        var bg = t.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.55f);
        var lt = new GameObject("Text", typeof(RectTransform));
        lt.transform.SetParent(t.transform, false);
        var lrt = (RectTransform)lt.transform; lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        label = lt.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.resizeTextForBestFit = true; label.resizeTextMinSize = 18; label.resizeTextMaxSize = 28;
    }

    public void Set(string text, Color tint, Texture tex)
    {
        Build();
        label.text = text;
        image.color = tint;
        if (tex is Texture2D t2)
        {
            image.sprite = Sprite.Create(t2, new Rect(0, 0, t2.width, t2.height), new Vector2(0.5f, 0.5f));
            image.color = Color.white;
        }
        else image.sprite = null;
    }

    public void OnPointerEnter(PointerEventData e) { outline.effectColor = Color.yellow; onHover.Invoke(); }
    public void OnPointerExit(PointerEventData e) { outline.effectColor = new Color(0, 0, 0, 0.6f); onExit.Invoke(); }
    public void OnPointerClick(PointerEventData e) => onClick.Invoke();
}
```

- [ ] **Step 4: Write `ControllerMenu.cs`**

```csharp
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

        canvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
        gameObject.GetComponent<TrackedDeviceGraphicRaycaster>() ?? gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = (RectTransform)transform;
        rt.sizeDelta = new Vector2(CanvasW, CanvasH);
        rt.localScale = Vector3.one * CanvasScale;
        // Blocks the scene raycast so a button click never selects the wall behind the menu.
        var blocker = gameObject.GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
        blocker.size = new Vector3(CanvasW, CanvasH, 1f);
        gameObject.tag = "MenuPanel";

        panel = Child("Panel", rt); Fill(panel);
        var bg = panel.gameObject.AddComponent<Image>(); bg.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

        var titleRt = Child("Title", panel);
        titleRt.anchorMin = new Vector2(0, 0.86f); titleRt.anchorMax = Vector2.one; titleRt.offsetMin = new Vector2(12, 0); titleRt.offsetMax = new Vector2(-12, -6);
        title = titleRt.gameObject.AddComponent<Text>();
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); title.fontSize = 34; title.color = Color.white; title.alignment = TextAnchor.MiddleLeft;

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
        foreach (Transform c in tabBar) DestroyNow(c.gameObject);
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
            b.onHover.AddListener(() => s.Preview(opt.color));
            b.onExit.AddListener(() => s.Revert());
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
            b.onHover.AddListener(() => s.PreviewMaterial(opt.material));
            b.onExit.AddListener(() => s.Revert());
            b.onClick.AddListener(() => { s.CommitMaterial(opt.material); s.Commit(Color.white); });
        }
    }

    void BuildFurniture(bool spawn)
    {
        foreach (var f in catalogue.furniture.Take(Cols * Rows))
        {
            var opt = f;
            var b = MakeSwatch(f.name, new Color(0.3f, 0.32f, 0.36f), null);
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
        foreach (var b in grid) if (b != null) DestroyNow(b.gameObject);
        grid.Clear();
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
```

The `"MenuPanel"` tag must exist: add it in `ProjectSettings/TagManager.asset` via the Editor —

```bash
unity command eval --no-banner --non-interactive --format json --code '
var tm = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
var tags = tm.FindProperty("tags"); bool has = false;
for (int i = 0; i < tags.arraySize; i++) if (tags.GetArrayElementAtIndex(i).stringValue == "MenuPanel") has = true;
if (!has) { tags.InsertArrayElementAtIndex(tags.arraySize); tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "MenuPanel"; tm.ApplyModifiedProperties(); }
return has ? "already" : "added";'
```

- [ ] **Step 5: Run tests**

```bash
unity command run_tests --mode EditMode --filter ControllerMenuTests --filter_type class --timeout 120 --no-banner --non-interactive --format json
```
Expected: 7 passed. (If `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` throws in EditMode, use `"Arial.ttf"` — Unity 6 ships `LegacyRuntime.ttf`; keep whichever loads.)

- [ ] **Step 6: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/SwatchButton.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/SwatchButton.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Runtime/ControllerMenu.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/ControllerMenu.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Scripts/Tests/ControllerMenuTests.cs" "ip1/RenovationPreviewer/Assets/Scripts/Tests/ControllerMenuTests.cs.meta" \
  "ip1/RenovationPreviewer/ProjectSettings/TagManager.asset"
git commit -m "feat(ip1): ControllerMenu — runtime-built world-space menu with hover preview / click commit"
```

---

### Task 8: MenuSelectRelay + SceneBuilder wiring + PlayMode wiring tests

**Files:**
- Create: `Assets/Scripts/Runtime/MenuSelectRelay.cs`
- Modify: `Assets/Editor/SceneBuilder.cs` (sections shown), `Assets/Scripts/PlayModeTests/SceneWiringTests.cs` (append tests)
- Regenerates: `Assets/Scenes/Room.unity`

**Interfaces:**
- Consumes: `ControllerMenu.Show/Hide/SpawnPoint/IsOpen`, `MenuTarget`, `FurnitureSlot.BindGrab(Bounds)`, `Surface.SetKind`.
- Produces: `class MenuSelectRelay : MonoBehaviour { InputActionProperty selectAction; InputActionProperty closeAction; Transform rayOrigin; ControllerMenu menu; float maxDistance = 6f; }`.

- [ ] **Step 1: Write `MenuSelectRelay.cs`**

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Right controller: trigger → physics ray from the controller → MenuTarget → menu.
/// Hitting the menu panel itself (tag MenuPanel) is ignored so UI clicks never
/// select the wall behind the canvas. Trigger on nothing closes the menu.
/// Left controller secondary button also closes.
/// </summary>
public class MenuSelectRelay : MonoBehaviour
{
    public InputActionProperty selectAction;
    public InputActionProperty closeAction;
    public Transform rayOrigin;      // defaults to this transform
    public ControllerMenu menu;
    public float maxDistance = 6f;

    void OnEnable() { selectAction.action?.Enable(); closeAction.action?.Enable(); }

    void Update()
    {
        if (menu == null) return;
        if (closeAction.action != null && closeAction.action.WasPressedThisFrame()) { menu.Hide(); return; }
        if (selectAction.action == null || !selectAction.action.WasPressedThisFrame()) return;

        var origin = rayOrigin != null ? rayOrigin : transform;
        if (!Physics.Raycast(origin.position, origin.forward, out var hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            menu.Hide();
            return;
        }
        if (hit.collider.CompareTag("MenuPanel")) return;   // clicking the UI, not the room

        var target = hit.collider.GetComponentInParent<MenuTarget>();
        if (target == null) { menu.Hide(); return; }
        if (target.Surface != null && target.Surface.Kind == SurfaceKind.Floor)
            menu.SpawnPoint = hit.point;
        menu.Show(target);
    }
}
```

- [ ] **Step 2: Modify `SceneBuilder.cs`**

(a) Add `using UnityEngine.EventSystems;` and `using UnityEngine.XR.Interaction.Toolkit.UI;` to the usings.

(b) Replace the 8 `MakeSurface(...)` calls in `Build()` with kinds:

```csharp
        MakeSurface("Floor", new Vector3(0, -0.05f, 0), new Vector3(4, 0.1f, 3), timber, SurfaceState.Keep, SurfaceKind.Floor);
        MakeSurface("Wall_N", new Vector3(0, 1.35f, 1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_S", new Vector3(0, 1.35f, -1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_E", new Vector3(2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_W", new Vector3(-2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Ceiling", new Vector3(0, 2.75f, 0), new Vector3(4, 0.1f, 3), offwhite, SurfaceState.Change, SurfaceKind.Ceiling);
        MakeSurface("Door", new Vector3(1.2f, 1.05f, 1.48f), new Vector3(0.9f, 2.1f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);
        MakeSurface("Trim", new Vector3(-0.8f, 0.075f, 1.48f), new Vector3(2.4f, 0.15f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);
```

and change the helper:

```csharp
    static GameObject MakeSurface(string name, Vector3 pos, Vector3 scale, Material m, SurfaceState state, SurfaceKind kind)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        var s = go.AddComponent<Surface>();
        s.SetState(state);
        s.SetKind(kind);
        go.AddComponent<MenuTarget>();
        return go;
    }
```

(c) Sofa becomes a grabbable slot. Replace the sofa block (from `var sofa = new GameObject("Sofa");` through `sofaCol.size = …;`) with:

```csharp
        // --- Sofa: kept prop (sample source) AND a furniture slot (swap/move) ---
        var sofa = new GameObject("Sofa");
        sofa.transform.position = new Vector3(-1.2f, 0f, -0.9f);
        MakePart(sofa, "Seat", new Vector3(-1.2f, 0.25f, -0.9f), new Vector3(1.8f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "Back", new Vector3(-1.2f, 0.65f, -1.25f), new Vector3(1.8f, 0.8f, 0.2f), sofaGrey);
        MakePart(sofa, "ArmL", new Vector3(-2.05f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "ArmR", new Vector3(-0.35f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        var sofaSurf = sofa.AddComponent<Surface>();
        sofaSurf.SetState(SurfaceState.Keep);
        var sofaRb = sofa.AddComponent<Rigidbody>(); sofaRb.isKinematic = true; sofaRb.useGravity = false;
        var sofaCol = sofa.AddComponent<BoxCollider>();
        sofaCol.center = new Vector3(0f, 0.5f, -0.1f);
        sofaCol.size = new Vector3(1.95f, 1.1f, 1.1f);
        var sofaGrab = sofa.AddComponent<XRGrabInteractable>();
        sofaGrab.movementType = XRBaseInteractable.MovementType.Kinematic;
        sofaGrab.throwOnDetach = false;
        sofaGrab.useDynamicAttach = true;
        var floorBounds = new Bounds(Vector3.zero, new Vector3(4f, 0.1f, 3f));
        var sofaSlot = sofa.AddComponent<FurnitureSlot>();
        sofaSlot.BindGrab(floorBounds);
        sofa.AddComponent<MenuTarget>();
```

Note the sofa root now sits at (-1.2, 0, -0.9) with parts placed in world space by `MakePart` (unchanged), so visuals land where they were; collider centre is now local.

(d) After the `// --- Managers ---` block, add the EventSystem:

```csharp
        // --- UI event system for the world-space menu (XRI input module) ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<XRUIInputModule>();
```

(e) In `WireControllers`, after `var right = WireHand(...)`, add the menu + relay and pass `floorBounds` in (change the signature to `WireControllers(GameObject rig, GameObject samplePrefab, SchemeManager schemeMgr, Bounds floorBounds)` and the call site accordingly):

```csharp
        var left = rig.transform.Find("Camera Offset/Left Controller")?.gameObject;
        var head = rig.transform.Find("Camera Offset/Main Camera");
        var catalogue = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        if (catalogue == null) Debug.LogWarning("SceneBuilder: Assets/Catalogue/Catalogue.asset missing — run Renovation → Import Catalogue first");

        ControllerMenu menu = null;
        if (left != null)
        {
            var menuGo = new GameObject("ControllerMenu", typeof(RectTransform));
            menuGo.transform.SetParent(left.transform, false);
            menuGo.transform.localPosition = new Vector3(0.12f, 0.08f, 0.05f);   // beside/above the left hand
            menu = menuGo.AddComponent<ControllerMenu>();
            menu.catalogue = catalogue;
            menu.head = head;
            menu.floorBounds = floorBounds;
        }

        if (right != null && menu != null)
        {
            var relay = right.AddComponent<MenuSelectRelay>();
            relay.menu = menu;
            relay.rayOrigin = right.GetComponentInChildren<NearFarInteractor>()?.transform ?? right.transform;
            relay.selectAction = ButtonAction("MenuSelect", "<XRController>{RightHand}/triggerPressed");
            relay.closeAction = ButtonAction("MenuClose", "<XRController>{LeftHand}/secondaryButton");
        }
```

- [ ] **Step 3: Append PlayMode wiring tests to `SceneWiringTests.cs`** (inside the class):

```csharp
    [UnityTest]
    public IEnumerator Menu_IsOnLeftController_WithCatalogueAndEventSystem()
    {
        yield return null;
        var menu = Object.FindObjectsByType<ControllerMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsTrue(menu.transform.parent != null && menu.transform.parent.name == "Left Controller", "menu parented to Left Controller");
        Assert.IsNotNull(menu.catalogue, "catalogue wired");
        Assert.Greater(menu.catalogue.paints.Count, 0);
        Assert.Greater(menu.catalogue.materials.Count, 0);
        Assert.Greater(menu.catalogue.furniture.Count, 0);
        Assert.IsNotNull(menu.head, "head wired");
        Assert.IsNotNull(Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>(), "XRUIInputModule present");
        var relay = Object.FindObjectsByType<MenuSelectRelay>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreSame(menu, relay.menu);
        Assert.IsNotNull(relay.selectAction.action);
    }

    [UnityTest]
    public IEnumerator EverySurface_HasMenuTargetAndKind()
    {
        yield return null;
        foreach (var s in Surface.All.Where(s => s != null))
        {
            Assert.IsNotNull(s.GetComponent<MenuTarget>(), $"{s.name}: MenuTarget");
            if (s.name != "Sofa") Assert.AreNotEqual(SurfaceKind.None, s.Kind, $"{s.name}: kind set");
        }
        var sofa = Surface.All.Single(s => s != null && s.name == "Sofa");
        Assert.IsNotNull(sofa.GetComponent<FurnitureSlot>(), "sofa is a furniture slot");
        Assert.IsNotNull(sofa.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(), "sofa grabbable");
    }
```

- [ ] **Step 4: Recompile, rebuild the scene, run PlayMode + all EditMode tests**

```bash
unity command recompile --no-banner --non-interactive --format tsv
until unity command recompile_status --no-banner --non-interactive --format json | grep -qE '"(completed|up_to_date)"'; do sleep 2; done
unity command console --level error --tail 10 --no-banner --non-interactive --format json
unity command eval --timeout 120 --no-banner --non-interactive --format json --code 'SceneBuilder.Build(); return "built";'
unity command run_tests --mode PlayMode --timeout 600 --no-banner --non-interactive --format json
unity command run_tests --mode EditMode --timeout 300 --no-banner --non-interactive --format json
```
Expected: all PlayMode (existing 4 + new 2) and all EditMode pass. `Room_HasNineSurfaces_TwoKept` must still pass — the sofa still has its `Surface`.

- [ ] **Step 5: Commit**

```bash
git add "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuSelectRelay.cs" "ip1/RenovationPreviewer/Assets/Scripts/Runtime/MenuSelectRelay.cs.meta" \
  "ip1/RenovationPreviewer/Assets/Editor/SceneBuilder.cs" "ip1/RenovationPreviewer/Assets/Scripts/PlayModeTests/SceneWiringTests.cs" \
  "ip1/RenovationPreviewer/Assets/Scenes/Room.unity" "ip1/RenovationPreviewer/Assets/Prefabs" "ip1/RenovationPreviewer/Assets/Materials"
git commit -m "feat(ip1): wire controller menu, select relay, sofa slot and EventSystem into Room"
```

---

### Task 9: Simulator rehearsal + polish

**Files:** any runtime file above, only for fixes found here. No new features.

- [ ] **Step 1: Enter Play mode and drive the menu from code** (the XR Device Simulator can't be driven from the CLI; the code path is the same the pointer uses)

```bash
unity command editor_play --no-banner --non-interactive --format tsv
sleep 4
unity command eval --no-banner --non-interactive --format json --code '
var menu = ControllerMenu.Instance;
var wall = Surface.All.First(s => s.name == "Wall_N");
menu.Show(wall.GetComponent<MenuTarget>());
menu.HoverGrid(3);
return "open=" + menu.IsOpen + " tabs=" + menu.TabButtonCount + " grid=" + menu.GridButtonCount + " previewing=" + wall.IsPreviewing + " display=" + wall.DisplayColor;'
unity command capture_game_view --width 1400 --height 900 --save_path "Temp/rehearsal_paint.png" --no-banner --non-interactive --format tsv
```
Look at the PNG: the menu must be legible (title, 3 tabs, 6×4 swatches with names) and Wall_N must show the hovered colour.

- [ ] **Step 2: Material + furniture path**

```bash
unity command eval --no-banner --non-interactive --format json --code '
var menu = ControllerMenu.Instance;
var floor = Surface.All.First(s => s.name == "Floor");
menu.Show(floor.GetComponent<MenuTarget>());          // Keep → prompt
menu.ClickGrid(0);                                     // toggle to Change, re-show
menu.ShowTab(MenuTab.Material); menu.ClickGrid(0);     // commit first floor material
menu.SpawnPoint = new Vector3(0.6f, 0, -0.4f);
menu.ShowTab(MenuTab.Furniture); menu.ClickGrid(2);    // spawn third furniture item
var slots = UnityEngine.Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None);
return "floorMat=" + (floor.CommittedMaterial != null ? floor.CommittedMaterial.name : "null") + " slots=" + slots.Length;'
unity command capture_game_view --width 1400 --height 900 --save_path "Temp/rehearsal_furniture.png" --no-banner --non-interactive --format tsv
```
Expected: floor shows the tile texture at ≈0.5 m tiles; a Poly Haven piece stands on the floor at the spawn point, right size.

- [ ] **Step 3: Scheme round-trip with materials, then stop**

```bash
unity command eval --no-banner --non-interactive --format json --code '
var mgr = UnityEngine.Object.FindFirstObjectByType<SchemeManager>();
var floor = Surface.All.First(s => s.name == "Floor");
int slot = mgr.SaveScheme();
floor.CommitMaterial(null); floor.Commit(Color.red);
mgr.ApplyScheme(slot);
return "restored=" + (floor.CommittedMaterial != null) + " color=" + floor.CommittedColor;'
unity command editor_stop --no-banner --non-interactive --format tsv
unity command delete_asset --asset Assets/Temp --confirm true --no-banner --non-interactive --format tsv
```
Expected: `restored=True`.

- [ ] **Step 4: Fix anything the screenshots show** (typical: tile scale wrong → adjust `mainTextureScale` in `CatalogueImporter` and re-run Import; menu too small/far → `CanvasScale`/`localPosition` in `SceneBuilder`; text unreadable → font sizes). Re-run the relevant tests after each fix.

- [ ] **Step 5: Kaike's manual sim run.** With the XR Device Simulator (Play in Editor, keyboard/mouse): point right ray at a wall, trigger → menu on left hand; hover swatches; click; grab spawned furniture and move it; press left secondary to close; pull a sample off the floor and hold it up as before. Every step of the Friday script (Task 10) must work in one continuous run.

- [ ] **Step 6: Commit fixes**

```bash
git add -A "ip1/RenovationPreviewer/Assets/Scripts" "ip1/RenovationPreviewer/Assets/Editor" "ip1/RenovationPreviewer/Assets/Scenes" "ip1/RenovationPreviewer/Assets/Catalogue"
git commit -m "fix(ip1): rehearsal fixes — menu legibility, tiling, spawn placement"
```

---

### Task 10: Testing plan v2, data sheet, results scaffold

**Files:**
- Modify: `ip1/2026-08-28-ip1-testing-plan.md`, `ip1/data-collection-sheet.md`
- Create: `testing-data/ip1/README.md`

- [ ] **Step 1: Testing plan edits** (exact replacements)

In **Pitch**, replace the last two sentences with:

> Samples are pulled directly off the things staying in the room — the timber floor, the sofa — and those samples generate the options that work with them. Alongside that, a menu on the left controller offers the full catalogue: paint colours, floor and wall materials, and furniture to swap or add. The test asks which of the two people actually use, and which they trust.

In **Testing Objective**, after A2 add:

> - **A3** — When both are available, people reach for the constrained hold-up loop *or* the open catalogue menu — and the open catalogue raises, or lowers, their confidence in the final choice compared with the constrained options.

and change the last sentence of that section to end with: "…(c) which method is used first and which produces the choice the participant keeps (A3)."

In **Prototype description / requirements**, replace the closing paragraph ("There are no menus; …") with:

> - **Menu** — pointing the right controller at a surface or piece of furniture and pressing trigger opens a menu on the left hand: paint colours (Dulux), materials (tiles, timber, carpet, plaster) and furniture to add; hovering previews, clicking commits, moving away reverts. Furniture can be swapped, removed, grabbed and moved.
>
> Every control is either a physical act on the room or a controller input; the menu is the one non-diegetic element and exists so the two approaches can be compared (A3). The lamp cannot be swapped.

In **Data collection method** table add two rows:

| Measure | Type | Validates |
|---|---|---|
| Method used first after Task 1 prompt (hold-up / menu) | boolean | A3 |
| Method that produced the kept choice (hold-up / menu / both) | categorical | A3 |

In **Testing process**, replace Task 2 with:

> - Task 2, read verbatim: **"Now change the floor to tiles and add one piece of furniture, then save this as a second version and switch between the two."** Log menu discovery, spawn/move success, whether saving/cycling is used. **(1 minute 30)**

and shorten Task 1 to **(1 minute 30)**.

In **Post-test**, add the question: "You had two ways to choose — pulling from the room and the menu. Which did you trust more, and why?"

- [ ] **Step 2: Data sheet** — add rows 13–15 before the scripted prompts:

```markdown
| 13 | Method used first after Task 1 prompt | ☐ hold-up ☐ menu |
| 14 | Method that produced the kept choice | ☐ hold-up ☐ menu ☐ both |
| 15 | Menu: opened unprompted? furniture placed? | ☐ opened ☐ prompted · ☐ placed ☐ moved ☐ failed |
```

and a fifth scripted prompt: `5. "Point at the floor and press the trigger."`

- [ ] **Step 3: Results scaffold** `testing-data/ip1/README.md`:

```markdown
# IP1 testing results — Fri 28 Aug 2026

Posted before leaving the studio (brief requirement). One row per participant, anonymised (P1…). Raw sheets/photos go in `raw-private/` (gitignored).

| P | First reached for | Floor/sofa touched | Time to 1st commit | Prompts | Twist found | Samples tried | Method first (13) | Kept via (14) | Menu (15) | Save/cycle | Lamp | Confidence | Helpful/restrictive (verbatim) | Trusted method (verbatim) | Confused by |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| P1 | | | | | | | | | | | | | | | |
| P2 | | | | | | | | | | | | | | | |
| P3 | | | | | | | | | | | | | | | |
| P4 | | | | | | | | | | | | | | | |
| P5 | | | | | | | | | | | | | | | |
| P6 | | | | | | | | | | | | | | | |

## Observer notes
```

- [ ] **Step 4: Regenerate the testing-plan PDF** per `CLAUDE.md` "Generating the PDF" (Chrome headless recipe), then commit:

```bash
git add ip1/2026-08-28-ip1-testing-plan.md ip1/data-collection-sheet.md testing-data/ip1/README.md
git commit -m "docs(ip1): testing plan v2 — menu vs hold-up comparison (A3), data sheet rows, results scaffold"
```

---

### Task 11: AI-use log + Android build

**Files:**
- Modify: `ip1/ai-use-log.md`
- Produces: `ip1/RenovationPreviewer/Builds/ip1.apk`

- [ ] **Step 1: Append to `ip1/ai-use-log.md`** (one row per item, today's date):

| Date | What AI did | Files touched | Human role |
|---|---|---|---|
| 2026-08-25 | Installed Unity CLI skill + MCP config for Claude Code; added `com.unity.pipeline` (editor-only) to drive the open Editor | `~/.claude/…`, `Packages/manifest.json` | Kaike ran the two install commands |
| 2026-08-25 | Researched licence-clean online 3D/material/colour sources (Poly Haven, ambientCG, Dulux data; IKEA rejected on ToS) | — | Kaike chose "bundle" over runtime download |
| 2026-08-25 | Wrote spec + plan for the controller menu / catalogue pivot | `docs/superpowers/specs/2026-08-25-…`, `docs/superpowers/plans/2026-08-25-…` | Direction change (scan → rebuild → menu) is Kaike's; AI proposed phasing and A3 |
| 2026-08-25/26 | Implemented `Catalogue`, `Surface` material ext, `SchemeManager` looks, `FurnitureSlot`, `MenuTarget`, `SwatchButton`, `ControllerMenu`, `MenuSelectRelay`, `CatalogueImporter`, SceneBuilder wiring, EditMode + PlayMode tests | `Assets/Scripts/**`, `Assets/Editor/**`, `Assets/Catalogue/**` | Kaike reviewed, ran the simulator rehearsal |
| 2026-08-26 | Revised testing plan (A3), data sheet, results scaffold | `ip1/*.md`, `testing-data/ip1/README.md` | Protocol decisions Kaike's |

- [ ] **Step 2: Build the APK through the open Editor** (async; poll):

```bash
unity command build --target Android --outputPath "Builds/ip1.apk" --confirm true --no-banner --non-interactive --format json
until unity command build_status --no-banner --non-interactive --format json | grep -qE '"status"\s*:\s*"(completed|failed)"'; do sleep 20; done
unity command build_status --no-banner --non-interactive --format json | head -c 1500
ls -la "ip1/RenovationPreviewer/Builds/ip1.apk"
```
Expected: `"status":"completed"`, 0 errors, APK present. If the Gradle step fails on the space in the path, follow the fallback in `docs/superpowers/plans/2026-08-21-ip1-unity-build.md` Task 13 Step 2 (build from a space-free `ditto` copy in the scratchpad), and log it.

- [ ] **Step 3: Commit the log; never commit `Builds/`**

```bash
git add ip1/ai-use-log.md
git commit -m "docs(ip1): AI-use log — controller menu + catalogue work"
```

- [ ] **Step 4: Friday morning:** `adb install -r Builds/ip1.apk` on the studio Quest (adb at `/Applications/Unity/Hub/Editor/6000.0.80f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`); run the full script once before the first participant.

---

## Self-review

**Spec coverage:** §2 scope — menu (T7/T8), tabs (T6), hover/commit (T7), furniture swap/add/move/remove (T4/T7), schemes with material (T3), importer (T5), tests (T1–T8), testing plan v2 (T10) ✓. §3 assets + CREDITS (T5) ✓; glTFast (T1) ✓. §5 interaction flow — close on empty/left secondary (T8 relay), keep-prompt (T6/T7), spawn 0.5 m toward player: **simplified to spawn at hit point clamped** (T4) — deviation noted, acceptable. §6 edge cases — preview revert on hide/target destroyed (T6 `OnDisable`, T7 `Hide`/`LateUpdate`), tint white on material commit (T7 `BuildMaterial`), scheme restore order (T3), clamp (T4), one menu instance (T7 `Instance`), missing catalogue warning (T8) ✓. §9 schedule ✓. §10 risk "keep v1 reachable" — tag `ip1-v1` exists.

**Placeholders:** none. **Type consistency:** `Surface.SetKind/Kind/PreviewMaterial/CommitMaterial/RebindRenderer/DisplayMaterial/CommittedMaterial` used identically in T2–T8; `FurnitureSlot.Spawn(FurnitureOption, Vector3, Bounds)`/`Swap`/`Remove`/`BindGrab` consistent T4/T7/T8; `MenuTarget.Tabs/Surface/Slot/DisplayName` consistent T6/T7/T8; `ControllerMenu.Show/ShowTab/Hide/HoverGrid/ExitGrid/ClickGrid/SpawnPoint/Initialise/Instance/TabButtonCount/GridButtonCount/ActiveTab/IsOpen` consistent T7/T8/T9.

---

## STATUS — Tue 25 Aug 2026 13:15 (resume here)

**Green + committed on `feat/ip1-controller-menu` @ `df72be0`:** Tasks 1–9 (automated part). Furniture now = Poly Haven **FBX** baked to standalone Mesh `.asset` + URP/Lit `.mat` (glTFast removed — its ScriptedImporter failed on every reload; folders must be created via `AssetDatabase.CreateFolder`, else `CreateAsset` throws a modal "Moving file failed" that killed the Editor once). EditMode 50/50, PlayMode 6/6, reload error-free. Play-mode renders verified: menu on left hand with title echo, tabs, texture swatches; floor material tiling; Poly Haven armchair/coffee table/sofa swap; scheme round-trip.

**Task 10 done + committed (`240df47`, PDF `ip1/IP1-Testing-Plan-Kaike-Nehme.pdf`):** testing plan v2 (A3, Menu bullet, data rows, Task 2 rewrite, trust question), data sheet rows 13–15 + prompt 5, `testing-data/ip1/README.md`. **Task 11:** AI-log rows appended; **APK built OK** 25 Aug 13:17 (`Builds/ip1.apk`, 51.8 MB, Gradle fine from this path; `Builds/` is untracked).

**25 Aug pm — menu v2 + ray feedback (reticle/glow) shipped; select ray ignores the rig; XR Device Simulator auto-spawns in Editor Play. EditMode 59/59, PlayMode 7/7.**

**Still Kaike's:** Task 9 step 5 (XR Device Simulator run of the full Friday script) · `ip1/statement-of-originality.md` personal rewrite (mentions the "diegetic no-menus rule" — now historical; say the IP1 build adds a menu for the A3 comparison) · Friday `adb install -r Builds/ip1.apk`.
