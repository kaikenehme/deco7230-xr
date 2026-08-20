# IP1 — XR Renovation Previewer Unity Build Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A horizontal VR prototype (Unity 6, URP, XRI 3.x, controllers) of the full mark → pull → hold → tune → commit loop on walls, plus diegetic 3-state light control, testable in Studio on Fri 28 Aug.

**Architecture:** Seven small MonoBehaviours from the concept spec §8 (`Surface`, `MarkTool`, `SamplePuller`, `Sample`, `HarmonyTuner`, `HoldUpPreviewer`, `SchemeManager`) with `HoldUpPreviewer` as the sole coupling point. Pure-logic classes (`HarmonyPalette`, scheme snapshots, surface state) are TDD'd in EditMode; XR interaction scripts are verified manually with the XR Device/Interaction Simulator in the editor and finally on Quest hardware.

**Tech Stack:** Unity 6000.0.80f1 · URP 17.x · XR Interaction Toolkit 3.x · OpenXR (Meta Quest feature) · Input System · Unity Test Framework (EditMode) · Android/ARM64/IL2CPP build target.

**Spec:** `concept/2026-08-07-xr-renovation-concept-design.md` (v1.1). Sections cited per task. Brief: `IP1 - Unity Prototype1.docx`.

## Global Constraints

- **Due Fri 28 Aug, Studio. 35%, hurdle, no extension, no supplementary.** Feature freeze Mon 24 Aug per `SCHEDULE.md`.
- **No menus. Ever.** Every control diegetic or on-controller. Brief: interactions "should not be basic, e.g., limited to simply pushing VR buttons".
- **Controllers, not hand tracking** (locked decision — spec §7).
- **URP mandatory** (built-in pipeline won't hold Quest framerate).
- **IP1 scope: full loop on walls only.** Keeping: timber floor + sofa. Changing: 4 walls, ceiling, trim, door. Furniture static. Scheme comparison minimal (snapshot + cycle) — deep comparison is IP2a.
- Diegetic light control: exactly **3 states — warm / cool / daylight** (spec §7).
- Unity binary: `/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents/MacOS/Unity` (call it `$UNITY`).
- Project path: `ip1/RenovationPreviewer` — **never add a new path segment containing a space**. Parent dirs already contain spaces; if Gradle fails on that, fallback in Task 13 (copy-out build), do NOT move the repo.
- **Secure assessment: every AI-generated file must be logged** in `ip1/ai-use-log.md` (Task 15 formalises). Log as you go, per task.
- Repo root `.gitignore` is rooted for a repo-root Unity project — Task 1 fixes it for the nested path.
- Unity licence needs one-time Hub GUI sign-in (user-side). Task 1 verifies; if unlicensed, STOP and ask Kaike to open Unity Hub and sign in.

---

### Task 1: Unity project scaffold + packages

**Files:**
- Create: `ip1/RenovationPreviewer/` (Unity project via `-createProject`)
- Create: `ip1/RenovationPreviewer/Packages/manifest.json` (overwrite generated one)
- Modify: `.gitignore` (root — add nested-project rules)

**Interfaces:**
- Consumes: nothing.
- Produces: an openable Unity project with URP 17, XRI 3, OpenXR, Input System, Test Framework resolved. All later tasks assume `-projectPath "<repo>/ip1/RenovationPreviewer"` works headless.

- [ ] **Step 1: Licence / batchmode smoke test**

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents/MacOS/Unity"
"$UNITY" -batchmode -quit -logFile - 2>&1 | tail -20
```

Expected: exits 0, no "No valid Unity Editor license" in output. If licence error: STOP — user must open Unity Hub and sign in once (GUI). Everything else blocks on this.

- [ ] **Step 2: Create project** (background — can take minutes)

```bash
"$UNITY" -batchmode -quit -createProject \
  "/Users/kaikenehme/Desktop/University of Queensland/Semester 4/Digital_Prototype/ip1/RenovationPreviewer" \
  -logFile -
```

Expected: exit 0, `ip1/RenovationPreviewer/Assets` and `Packages/manifest.json` exist.

- [ ] **Step 3: Write manifest with required packages**

Overwrite `ip1/RenovationPreviewer/Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.0.4",
    "com.unity.xr.interaction.toolkit": "3.1.2",
    "com.unity.xr.openxr": "1.14.3",
    "com.unity.xr.management": "4.5.0",
    "com.unity.inputsystem": "1.13.1",
    "com.unity.test-framework": "1.4.6",
    "com.unity.ugui": "2.0.0",
    "com.unity.modules.accessibility": "1.0.0",
    "com.unity.modules.ai": "1.0.0",
    "com.unity.modules.androidjni": "1.0.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.cloth": "1.0.0",
    "com.unity.modules.director": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.screencapture": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.terrainphysics": "1.0.0",
    "com.unity.modules.tilemap": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unityanalytics": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
    "com.unity.modules.unitywebrequestaudio": "1.0.0",
    "com.unity.modules.unitywebrequesttexture": "1.0.0",
    "com.unity.modules.unitywebrequestwww": "1.0.0",
    "com.unity.modules.vehicles": "1.0.0",
    "com.unity.modules.video": "1.0.0",
    "com.unity.modules.vr": "1.0.0",
    "com.unity.modules.wind": "1.0.0",
    "com.unity.modules.xr": "1.0.0"
  }
}
```

If a pinned version fails to resolve (check the log for "Cannot resolve"), drop to the nearest version the log suggests — majors must stay: URP 17.x, XRI 3.x, OpenXR 1.x.

- [ ] **Step 4: Resolve packages** (background, 5–15 min first time)

```bash
"$UNITY" -batchmode -quit -projectPath "<repo>/ip1/RenovationPreviewer" -logFile /tmp/unity-resolve.log
```

Expected: exit 0. Grep log for `error` — compilation errors from missing URP config are acceptable at this stage only if exit code is 0; package resolution errors are not.

- [ ] **Step 5: Fix root `.gitignore` for nested project**

Append (patterns for a Unity project not at repo root):

```gitignore
# Unity project nested at ip1/RenovationPreviewer
ip1/RenovationPreviewer/[Ll]ibrary/
ip1/RenovationPreviewer/[Tt]emp/
ip1/RenovationPreviewer/[Oo]bj/
ip1/RenovationPreviewer/[Bb]uild/
ip1/RenovationPreviewer/[Bb]uilds/
ip1/RenovationPreviewer/[Ll]ogs/
ip1/RenovationPreviewer/[Uu]serSettings/
ip1/RenovationPreviewer/*.csproj
ip1/RenovationPreviewer/*.sln
```

- [ ] **Step 6: Commit**

```bash
git add .gitignore ip1/RenovationPreviewer/Assets ip1/RenovationPreviewer/Packages ip1/RenovationPreviewer/ProjectSettings
git commit -m "feat(ip1): scaffold Unity 6 project with URP + XRI 3 + OpenXR packages"
```

---

### Task 2: Project configuration automation (URP, XR, Android)

**Files:**
- Create: `ip1/RenovationPreviewer/Assets/Editor/ProjectConfigurator.cs`
- Create (generated by it): `Assets/Settings/URP-Asset.asset`, `Assets/Settings/URP-Renderer.asset`

**Interfaces:**
- Consumes: resolved packages from Task 1.
- Produces: `ProjectConfigurator.ConfigureAll()` (menu-less, `-executeMethod` entry point). After it runs: URP active in Graphics settings, linear colour space, OpenXR enabled for Android + Standalone with Meta Quest feature + Oculus Touch profile, Android = IL2CPP/ARM64/ASTC/minSdk 32, XRI Starter Assets + simulator sample imported.

- [ ] **Step 1: Write `ProjectConfigurator.cs`**

```csharp
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ProjectConfigurator
{
    public static void ConfigureAll()
    {
        SetupUrp();
        SetupColorSpaceAndAndroid();
        ImportXriSamples();
        AssetDatabase.SaveAssets();
        Debug.Log("ProjectConfigurator: done");
    }

    static void SetupUrp()
    {
        Directory.CreateDirectory("Assets/Settings");
        var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(renderer, "Assets/Settings/URP-Renderer.asset");
        var pipeline = UniversalRenderPipelineAsset.Create(renderer);
        AssetDatabase.CreateAsset(pipeline, "Assets/Settings/URP-Asset.asset");
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
    }

    static void SetupColorSpaceAndAndroid()
    {
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.textureCompressionFormats =
            new[] { TextureCompressionFormat.ASTC };
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,
            "com.kaikenehme.renovationpreviewer");
    }

    static void ImportXriSamples()
    {
        foreach (var sample in Sample.FindByPackage("com.unity.xr.interaction.toolkit", null))
        {
            if (sample.displayName.Contains("Starter Assets") ||
                sample.displayName.Contains("Simulator"))
                sample.Import(Sample.ImportOptions.OverridePreviousImports);
        }
    }
}
```

- [ ] **Step 2: Run it**

```bash
"$UNITY" -batchmode -quit -projectPath "<repo>/ip1/RenovationPreviewer" \
  -executeMethod ProjectConfigurator.ConfigureAll -logFile /tmp/unity-config.log
```

Expected: exit 0, log contains `ProjectConfigurator: done`, `Assets/Settings/URP-Asset.asset` exists, `Assets/Samples/XR Interaction Toolkit/` exists.

- [ ] **Step 3: Enable OpenXR loaders + features**

XR Plug-in Management's per-target loader config is awkward from pure script; use `XRGeneralSettingsPerBuildTarget` API in a second method `ConfigureXr()` appended to `ProjectConfigurator.cs`:

```csharp
    public static void ConfigureXr()
    {
        ConfigureXrForTarget(BuildTargetGroup.Android);
        ConfigureXrForTarget(BuildTargetGroup.Standalone);
        AssetDatabase.SaveAssets();
        Debug.Log("ProjectConfigurator: XR done");
    }

    static void ConfigureXrForTarget(BuildTargetGroup group)
    {
        UnityEditor.XR.Management.Metadata.XRPackageMetadataStore.AssignLoader(
            UnityEngine.XR.Management.XRGeneralSettings.Instance == null
                ? GetOrCreateSettingsManager(group)
                : GetOrCreateSettingsManager(group),
            "UnityEngine.XR.OpenXR.OpenXRLoader", group);

        var openxr = UnityEngine.XR.OpenXR.OpenXRSettings.GetSettingsForBuildTargetGroup(group);
        foreach (var feature in openxr.GetFeatures())
        {
            var name = feature.GetType().Name;
            if (name.Contains("OculusTouchControllerProfile") || name.Contains("MetaQuestFeature"))
                feature.enabled = true;
        }
    }

    static UnityEngine.XR.Management.XRManagerSettings GetOrCreateSettingsManager(BuildTargetGroup group)
    {
        UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.GetOrCreate();
        var perTarget = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(group)
            ?? UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.GetOrCreate().SettingsForBuildTarget(group);
        return perTarget.Manager;
    }
```

API names drift between XR Management versions — if this doesn't compile, read the actual API in `Library/PackageCache/com.unity.xr.management*/Editor/` and adjust; the goal is fixed: OpenXR loader active for Android + Standalone, Meta Quest Support + Oculus Touch Controller Profile features enabled. Run with `-executeMethod ProjectConfigurator.ConfigureXr`. Verify: `ProjectSettings/XRGeneralSettings.asset` references OpenXR loader (grep the YAML), `Assets/XR/Settings/OpenXR Package Settings.asset` shows the features enabled.

- [ ] **Step 4: Commit**

```bash
git add -A ip1/RenovationPreviewer
git commit -m "feat(ip1): configure URP, OpenXR (Quest), Android build settings via editor script"
```

---

### Task 3: Test scaffolding + `Surface.cs` (TDD)

**Files:**
- Create: `ip1/RenovationPreviewer/Assets/Scripts/Runtime/Renovation.Runtime.asmdef`
- Create: `ip1/RenovationPreviewer/Assets/Scripts/Tests/Renovation.EditModeTests.asmdef`
- Create: `ip1/RenovationPreviewer/Assets/Scripts/Runtime/Surface.cs`
- Test: `ip1/RenovationPreviewer/Assets/Scripts/Tests/SurfaceTests.cs`

**Interfaces:**
- Produces (spec §8): `enum SurfaceState { Keep, Change }` · `class Surface : MonoBehaviour` with `SurfaceState State { get; }`, `Color CommittedColor { get; }`, `Color DisplayColor { get; }`, `bool IsPreviewing { get; }`, `void SetState(SurfaceState)`, `void ToggleState()`, `void Preview(Color)`, `void Commit(Color)`, `void Revert()`. `Preview` on a `Keep` surface is refused (no-op). Static registry `Surface.All` (List) for `HoldUpPreviewer`/`SchemeManager` queries.

- [ ] **Step 1: asmdefs**

`Renovation.Runtime.asmdef`:
```json
{ "name": "Renovation.Runtime", "references": ["Unity.XR.Interaction.Toolkit", "Unity.InputSystem", "Unity.XR.CoreUtils"], "noEngineReferences": false }
```
`Renovation.EditModeTests.asmdef`:
```json
{ "name": "Renovation.EditModeTests", "references": ["Renovation.Runtime", "UnityEngine.TestRunner", "UnityEditor.TestRunner"], "includePlatforms": ["Editor"], "defineConstraints": ["UNITY_INCLUDE_TESTS"] }
```

- [ ] **Step 2: Failing tests**

```csharp
using NUnit.Framework;
using UnityEngine;

public class SurfaceTests
{
    Surface Make(SurfaceState s)
    {
        var go = new GameObject("s");
        var surf = go.AddComponent<Surface>();
        surf.SetState(s);
        return surf;
    }

    [Test]
    public void Preview_OnChangeSurface_SetsDisplayNotCommitted()
    {
        var s = Make(SurfaceState.Change);
        s.Commit(Color.white);
        s.Preview(Color.red);
        Assert.AreEqual(Color.red, s.DisplayColor);
        Assert.AreEqual(Color.white, s.CommittedColor);
        Assert.IsTrue(s.IsPreviewing);
    }

    [Test]
    public void Revert_RestoresCommittedColor()
    {
        var s = Make(SurfaceState.Change);
        s.Commit(Color.white);
        s.Preview(Color.red);
        s.Revert();
        Assert.AreEqual(Color.white, s.DisplayColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Commit_EndsPreviewAndSticks()
    {
        var s = Make(SurfaceState.Change);
        s.Preview(Color.red);
        s.Commit(Color.red);
        Assert.AreEqual(Color.red, s.CommittedColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Preview_OnKeepSurface_IsRefused()
    {
        var s = Make(SurfaceState.Keep);
        var before = s.DisplayColor;
        s.Preview(Color.red);
        Assert.AreEqual(before, s.DisplayColor);
        Assert.IsFalse(s.IsPreviewing);
    }

    [Test]
    public void Registry_TracksEnabledSurfaces()
    {
        int before = Surface.All.Count;
        var s = Make(SurfaceState.Change);
        Assert.AreEqual(before + 1, Surface.All.Count);
        Object.DestroyImmediate(s.gameObject);
        Assert.AreEqual(before, Surface.All.Count);
    }
}
```

- [ ] **Step 3: Run, verify fail** (compile error = fail is fine at first run with no Surface.cs — add an empty stub so tests fail on assertions, not compilation, only if the runner refuses)

```bash
"$UNITY" -batchmode -projectPath "<repo>/ip1/RenovationPreviewer" -runTests \
  -testPlatform EditMode -testResults /tmp/ip1-tests.xml -logFile /tmp/unity-test.log
grep -E 'result="(Passed|Failed)"' /tmp/ip1-tests.xml | head
```

- [ ] **Step 4: Implement `Surface.cs`**

```csharp
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceState { Keep, Change }

[RequireComponent(typeof(Renderer))]
public class Surface : MonoBehaviour
{
    public static readonly List<Surface> All = new();

    [SerializeField] SurfaceState state = SurfaceState.Change;

    public SurfaceState State => state;
    public Color CommittedColor { get; private set; } = Color.white;
    public Color DisplayColor { get; private set; } = Color.white;
    public bool IsPreviewing { get; private set; }

    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
            CommittedColor = DisplayColor = rend.sharedMaterial.color;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    public void SetState(SurfaceState s) => state = s;
    public void ToggleState() => state = state == SurfaceState.Keep ? SurfaceState.Change : SurfaceState.Keep;

    public void Preview(Color c)
    {
        if (state == SurfaceState.Keep) return; // only kept things are sources; only changing things preview
        IsPreviewing = true;
        Apply(c);
    }

    public void Commit(Color c)
    {
        if (state == SurfaceState.Keep) return;
        CommittedColor = c;
        IsPreviewing = false;
        Apply(c);
    }

    public void Revert()
    {
        IsPreviewing = false;
        Apply(CommittedColor);
    }

    void Apply(Color c)
    {
        DisplayColor = c;
        if (rend != null) rend.material.color = c;
    }
}
```

Note: `OnEnable`/`OnDisable` fire in EditMode tests on `AddComponent`/`DestroyImmediate` even without Play mode — the registry test relies on that. If it proves flaky headless, switch the registry to lazy `FindObjectsByType` inside `SchemeManager`/`HoldUpPreviewer` and delete the test.

- [ ] **Step 5: Run tests, verify pass** (same command). Expected: all 5 pass.

- [ ] **Step 6: Commit** — `git commit -m "feat(ip1): Surface state machine with preview/commit/revert (TDD)"`

---

### Task 4: `HarmonyPalette` + `Sample` (TDD)

**Files:**
- Create: `Assets/Scripts/Runtime/HarmonyPalette.cs`, `Assets/Scripts/Runtime/Sample.cs`
- Test: `Assets/Scripts/Tests/HarmonyPaletteTests.cs`

**Interfaces:**
- Produces: `static Color[] HarmonyPalette.Generate(Color baseColor)` — exactly 7 colours, order: `[analogous −30°, analogous +30°, pale analogous +30°, deep analogous +30°, triadic −120°, triadic +120°, complementary +180°]` (safe → bold, spec §8 harmony rule). `class Sample : MonoBehaviour` with `Color BaseColor`, `Color CurrentColor`, `Surface SourceSurface`, `Color[] Palette`, `void Init(Surface source)`.

- [ ] **Step 1: Failing tests**

```csharp
using NUnit.Framework;
using UnityEngine;

public class HarmonyPaletteTests
{
    static float HueOf(Color c) { Color.RGBToHSV(c, out var h, out _, out _); return h * 360f; }
    static float HueDelta(float a, float b) => Mathf.Abs(Mathf.DeltaAngle(a, b));

    [Test]
    public void Generate_Returns7Colors()
    {
        Assert.AreEqual(7, HarmonyPalette.Generate(new Color(0.6f, 0.4f, 0.2f)).Length);
    }

    [Test]
    public void Generate_HueRelationshipsHold()
    {
        var baseColor = Color.HSVToRGB(0.1f, 0.6f, 0.6f); // 36° hue
        var p = HarmonyPalette.Generate(baseColor);
        float h = 36f;
        Assert.LessOrEqual(HueDelta(HueOf(p[0]), h - 30f), 2f, "analogous -30");
        Assert.LessOrEqual(HueDelta(HueOf(p[1]), h + 30f), 2f, "analogous +30");
        Assert.LessOrEqual(HueDelta(HueOf(p[4]), h - 120f), 2f, "triadic -120");
        Assert.LessOrEqual(HueDelta(HueOf(p[5]), h + 120f), 2f, "triadic +120");
        Assert.LessOrEqual(HueDelta(HueOf(p[6]), h + 180f), 2f, "complementary");
    }

    [Test]
    public void Generate_PaleAndDeepVariantsDifferInValue()
    {
        var p = HarmonyPalette.Generate(Color.HSVToRGB(0.5f, 0.6f, 0.6f));
        Color.RGBToHSV(p[2], out _, out _, out var vPale);
        Color.RGBToHSV(p[3], out _, out _, out var vDeep);
        Assert.Greater(vPale, vDeep);
    }

    [Test]
    public void Generate_NeverReturnsMuddyOrBlownValues()
    {
        foreach (var c in HarmonyPalette.Generate(new Color(0.05f, 0.05f, 0.05f)))
        {
            Color.RGBToHSV(c, out _, out var s, out var v);
            Assert.That(v, Is.InRange(0.25f, 0.95f));
            Assert.That(s, Is.InRange(0.15f, 0.85f));
        }
    }
}
```

- [ ] **Step 2: Run, verify fail.**

- [ ] **Step 3: Implement**

`HarmonyPalette.cs`:
```csharp
using UnityEngine;

public static class HarmonyPalette
{
    // Order: safe -> bold. Spec §8: analogous ±30 / complementary +180 / triadic ±120.
    static readonly float[] hueOffsets = { -30f, 30f, 30f, 30f, -120f, 120f, 180f };

    public static Color[] Generate(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out var h, out var s, out var v);
        s = Mathf.Clamp(s, 0.2f, 0.8f);
        v = Mathf.Clamp(v, 0.35f, 0.85f);
        var result = new Color[7];
        for (int i = 0; i < 7; i++)
        {
            float hue = Mathf.Repeat(h + hueOffsets[i] / 360f, 1f);
            float sat = s, val = v;
            if (i == 2) { sat = Mathf.Clamp(s * 0.6f, 0.15f, 0.85f); val = Mathf.Clamp(v * 1.25f, 0.25f, 0.95f); } // pale
            if (i == 3) { val = Mathf.Clamp(v * 0.6f, 0.25f, 0.95f); } // deep
            result[i] = Color.HSVToRGB(hue, sat, val);
        }
        return result;
    }
}
```

`Sample.cs`:
```csharp
using UnityEngine;

public class Sample : MonoBehaviour
{
    public Color BaseColor { get; private set; }
    public Color CurrentColor { get; set; }
    public Surface SourceSurface { get; private set; }
    public Color[] Palette { get; private set; }

    public void Init(Surface source)
    {
        SourceSurface = source;
        BaseColor = source.DisplayColor;
        Palette = HarmonyPalette.Generate(BaseColor);
        CurrentColor = Palette[0];
        var rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = CurrentColor;
    }
}
```

- [ ] **Step 4: Run tests, verify pass.**
- [ ] **Step 5: Commit** — `git commit -m "feat(ip1): harmony palette generation from pulled sample colour (TDD)"`

---

### Task 5: `HarmonyTuner` roll mapping (TDD)

**Files:**
- Create: `Assets/Scripts/Runtime/HarmonyTuner.cs`
- Test: `Assets/Scripts/Tests/HarmonyTunerTests.cs`

**Interfaces:**
- Consumes: `Sample.Palette`, `Sample.CurrentColor`.
- Produces: `class HarmonyTuner : MonoBehaviour` with `static int RollToIndex(float rollDegrees, int paletteSize)` (pure, testable: clamps roll to ±90°, maps linearly to 0..size−1) and `void Tick(float rollDegrees)` (sets `sample.CurrentColor`, updates held renderer). Roll = controller Z-rotation delta from grab pose, supplied by `SamplePuller`/XRI attach transform in Task 9–10.

- [ ] **Step 1: Failing tests**

```csharp
using NUnit.Framework;

public class HarmonyTunerTests
{
    [Test] public void NeutralRoll_IsFirstIndex() => Assert.AreEqual(0, HarmonyTuner.RollToIndex(-90f, 7));
    [Test] public void FullRoll_IsLastIndex() => Assert.AreEqual(6, HarmonyTuner.RollToIndex(90f, 7));
    [Test] public void MidRoll_IsMiddleIndex() => Assert.AreEqual(3, HarmonyTuner.RollToIndex(0f, 7));
    [Test] public void OverRotation_Clamps() => Assert.AreEqual(6, HarmonyTuner.RollToIndex(400f, 7));
    [Test] public void UnderRotation_Clamps() => Assert.AreEqual(0, HarmonyTuner.RollToIndex(-400f, 7));
}
```

- [ ] **Step 2: Run, verify fail.**
- [ ] **Step 3: Implement**

```csharp
using UnityEngine;

[RequireComponent(typeof(Sample))]
public class HarmonyTuner : MonoBehaviour
{
    Sample sample;
    Renderer rend;

    void Awake() { sample = GetComponent<Sample>(); rend = GetComponent<Renderer>(); }

    public static int RollToIndex(float rollDegrees, int paletteSize)
    {
        float t = Mathf.InverseLerp(-90f, 90f, Mathf.Clamp(rollDegrees, -90f, 90f));
        return Mathf.Clamp(Mathf.RoundToInt(t * (paletteSize - 1)), 0, paletteSize - 1);
    }

    public void Tick(float rollDegrees)
    {
        if (sample.Palette == null || sample.Palette.Length == 0) return;
        var c = sample.Palette[RollToIndex(rollDegrees, sample.Palette.Length)];
        if (c == sample.CurrentColor) return;
        sample.CurrentColor = c;
        if (rend != null) rend.material.color = c;
    }
}
```

- [ ] **Step 4: Run tests, verify pass.**
- [ ] **Step 5: Commit** — `git commit -m "feat(ip1): twist-to-tune roll mapping over harmony palette (TDD)"`

---

### Task 6: `SchemeManager` (TDD)

**Files:**
- Create: `Assets/Scripts/Runtime/SchemeManager.cs`
- Test: `Assets/Scripts/Tests/SchemeManagerTests.cs`

**Interfaces:**
- Consumes: `Surface.All`, `Surface.CommittedColor`, `Surface.Commit`.
- Produces: `class SchemeManager : MonoBehaviour`, `int SaveScheme()` (snapshot all Change-surface committed colours; max 3 slots, oldest overwritten, returns slot index), `void ApplyScheme(int slot)`, `void CycleScheme()` (save-current-then-advance behaviour lives in Task 12 wiring, not here), `int Count { get; }`.

- [ ] **Step 1: Failing tests**

```csharp
using NUnit.Framework;
using UnityEngine;

public class SchemeManagerTests
{
    Surface MakeChange(Color committed)
    {
        var go = new GameObject("wall");
        var s = go.AddComponent<Surface>();
        s.SetState(SurfaceState.Change);
        s.Commit(committed);
        return s;
    }

    [Test]
    public void SaveAndApply_RoundTrips()
    {
        var wall = MakeChange(Color.red);
        var mgr = new GameObject("mgr").AddComponent<SchemeManager>();
        int slot = mgr.SaveScheme();
        wall.Commit(Color.blue);
        mgr.ApplyScheme(slot);
        Assert.AreEqual(Color.red, wall.CommittedColor);
    }

    [Test]
    public void FourthSave_OverwritesOldest()
    {
        MakeChange(Color.red);
        var mgr = new GameObject("mgr").AddComponent<SchemeManager>();
        mgr.SaveScheme(); mgr.SaveScheme(); mgr.SaveScheme();
        Assert.AreEqual(0, mgr.SaveScheme(), "wraps to slot 0");
        Assert.AreEqual(3, mgr.Count);
    }

    [Test]
    public void Apply_IgnoresDestroyedSurfaces()
    {
        var wall = MakeChange(Color.red);
        var mgr = new GameObject("mgr").AddComponent<SchemeManager>();
        int slot = mgr.SaveScheme();
        Object.DestroyImmediate(wall.gameObject);
        Assert.DoesNotThrow(() => mgr.ApplyScheme(slot));
    }
}
```

- [ ] **Step 2: Run, verify fail.**
- [ ] **Step 3: Implement**

```csharp
using System.Collections.Generic;
using UnityEngine;

public class SchemeManager : MonoBehaviour
{
    public const int MaxSchemes = 3;

    readonly List<Dictionary<Surface, Color>> schemes = new();
    int nextSlot;

    public int Count => schemes.Count;

    public int SaveScheme()
    {
        var snapshot = new Dictionary<Surface, Color>();
        foreach (var s in Surface.All)
            if (s.State == SurfaceState.Change) snapshot[s] = s.CommittedColor;

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
            if (kv.Key != null) kv.Key.Commit(kv.Value);
    }
}
```

- [ ] **Step 4: Run tests, verify pass.**
- [ ] **Step 5: Commit** — `git commit -m "feat(ip1): scheme snapshot/restore, 3 slots (TDD)"`

---

### Task 7: Scene blockout via `SceneBuilder`

**Files:**
- Create: `Assets/Editor/SceneBuilder.cs`
- Create (generated): `Assets/Scenes/Room.unity`, `Assets/Materials/*.mat`

**Interfaces:**
- Consumes: XRI Starter Assets prefab `Assets/Samples/XR Interaction Toolkit/<version>/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` (locate with a `Directory.GetFiles` glob — version dir varies).
- Produces: `SceneBuilder.Build()` (`-executeMethod` entry). Scene contents, all named exactly: `Floor` (Keep, timber brown `#8B5A2B`), `Sofa` (Keep, grey, 3-box compound, static), `Wall_N/S/E/W` (Change, off-white), `Ceiling` (Change), `Door` + `Trim` (Change, quads on Wall_N), `Lamp` (side table + bulb sphere, Task 11 target), `Directional Light` named `Sun`, `XR Origin (XR Rig)` instance at (0, 0, 0), `Managers` (empty GO holding `SchemeManager`). Room 4m × 3m × 2.7m. Every Surface GO: `Surface` component + `BoxCollider`. Layer `Surfaces` (layer 6) created via `TagManager` and assigned.

- [ ] **Step 1: Write `SceneBuilder.cs`** — creates materials (URP Lit via `Shader.Find("Universal Render Pipeline/Lit")`), builds primitives with exact transforms:

```csharp
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneBuilder
{
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Scenes");

        var timber = MakeMat("Timber", new Color(0.545f, 0.353f, 0.169f));
        var offwhite = MakeMat("OffWhite", new Color(0.93f, 0.91f, 0.87f));
        var sofaGrey = MakeMat("SofaGrey", new Color(0.45f, 0.45f, 0.48f));
        var trimWhite = MakeMat("TrimWhite", new Color(0.98f, 0.98f, 0.96f));

        // Floor 4x3, timber, Keep
        MakeSurface("Floor", PrimitiveType.Cube, new Vector3(0, -0.05f, 0), new Vector3(4, 0.1f, 3), timber, SurfaceState.Keep);
        // Walls, 2.7 high, Change
        MakeSurface("Wall_N", PrimitiveType.Cube, new Vector3(0, 1.35f, 1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_S", PrimitiveType.Cube, new Vector3(0, 1.35f, -1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_E", PrimitiveType.Cube, new Vector3(2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_W", PrimitiveType.Cube, new Vector3(-2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change);
        MakeSurface("Ceiling", PrimitiveType.Cube, new Vector3(0, 2.75f, 0), new Vector3(4, 0.1f, 3), offwhite, SurfaceState.Change);
        MakeSurface("Door", PrimitiveType.Cube, new Vector3(1.2f, 1.05f, 1.49f), new Vector3(0.9f, 2.1f, 0.05f), trimWhite, SurfaceState.Change);
        MakeSurface("Trim", PrimitiveType.Cube, new Vector3(0, 0.075f, 1.49f), new Vector3(4, 0.15f, 0.05f), trimWhite, SurfaceState.Change);

        // Sofa: compound, Keep, static prop
        var sofa = new GameObject("Sofa");
        MakePart(sofa, "Seat", new Vector3(-1.2f, 0.25f, -0.9f), new Vector3(1.8f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "Back", new Vector3(-1.2f, 0.65f, -1.2f), new Vector3(1.8f, 0.8f, 0.2f), sofaGrey);
        MakePart(sofa, "ArmL", new Vector3(-2.0f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "ArmR", new Vector3(-0.4f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        var sofaSurf = sofa.AddComponent<Surface>(); // whole sofa is one Keep source
        sofaSurf.SetState(SurfaceState.Keep);
        var sofaCol = sofa.AddComponent<BoxCollider>();
        sofaCol.center = new Vector3(-1.2f, 0.45f, -1.0f); sofaCol.size = new Vector3(1.9f, 1.0f, 1.1f);

        // Lamp (Task 11 wires behaviour)
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "SideTable"; table.transform.position = new Vector3(1.5f, 0.25f, -1.1f);
        table.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = "Lamp"; lamp.transform.position = new Vector3(1.5f, 0.75f, -1.1f);
        lamp.transform.localScale = Vector3.one * 0.25f;
        lamp.GetComponent<Collider>().isTrigger = true;
        var bulb = new GameObject("BulbLight").AddComponent<Light>();
        bulb.type = LightType.Point; bulb.transform.SetParent(lamp.transform, false); bulb.range = 6f;

        var sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);

        new GameObject("Managers").AddComponent<SchemeManager>();

        // XR rig from Starter Assets
        var rigPath = Directory.GetFiles("Assets/Samples", "XR Origin (XR Rig).prefab", SearchOption.AllDirectories).FirstOrDefault();
        if (rigPath != null)
        {
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(rigPath));
            rig.transform.position = new Vector3(0.5f, 0, 0.3f);
        }
        else Debug.LogWarning("SceneBuilder: XR Origin prefab not found — add rig manually");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Room.unity");
        var buildScenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Room.unity", true) };
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("SceneBuilder: done");
    }

    static Material MakeMat(string name, Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = c };
        AssetDatabase.CreateAsset(m, $"Assets/Materials/{name}.mat");
        return m;
    }

    static GameObject MakeSurface(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material m, SurfaceState state)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        var s = go.AddComponent<Surface>();
        s.SetState(state);
        return go;
    }

    static void MakePart(GameObject parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = name;
        p.transform.SetParent(parent.transform);
        p.transform.position = pos;
        p.transform.localScale = scale;
        p.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(p.GetComponent<Collider>());
    }
}
```

- [ ] **Step 2: Run** `-executeMethod SceneBuilder.Build`. Expected: exit 0, `Assets/Scenes/Room.unity` exists, log has `SceneBuilder: done`, no `rig not found` warning.
- [ ] **Step 3: Screenshot sanity check** — open project GUI once OR run a playmode screenshot script; verify room reads as a room (walls enclosing, sofa on floor, lamp on table). Fix transforms if not.
- [ ] **Step 4: Commit** — `git commit -m "feat(ip1): scripted scene blockout — room, kept floor+sofa, lamp, XR rig"`

---

### Task 8: `MarkTool` — sweep to toggle keep/change

**Files:**
- Create: `Assets/Scripts/Runtime/MarkTool.cs`
- Modify: `SceneBuilder.cs` (attach `MarkTool` + trigger collider to both controller GOs; re-run Build)

**Interfaces:**
- Consumes: `Surface.ToggleState()`; controller GameObjects inside the XR rig (`Left Controller`/`Right Controller`, found by name).
- Produces: sweeping a controller across a surface while holding **grip** toggles its state. Visual feedback: a `SurfaceStateCue` (thin emissive edge — implemented here, also answers open Q4): kept surfaces get a subtle green edge glow via `material.EnableKeyword("_EMISSION")`-equivalent URP emissive tint applied to a duplicated cue material.

Behaviour spec: on `OnTriggerStay` with a `Surface`, track controller world velocity (position delta / `Time.deltaTime`, smoothed). If speed > 0.6 m/s continuously for 0.15 s while grip held → `ToggleState()`, then 0.5 s debounce per surface. No grip = no marking (prevents accidental sweeps — open Q3).

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MarkTool : MonoBehaviour
{
    public InputActionProperty gripAction; // bound to <XRController>{LeftHand|RightHand}/gripPressed
    const float SpeedThreshold = 0.6f, HoldTime = 0.15f, Debounce = 0.5f;

    Vector3 lastPos;
    float speed, sweepTimer;
    Surface candidate;
    float lastToggleTime = -10f;

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Update()
    {
        speed = Mathf.Lerp(speed, (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 1e-4f), 0.5f);
        lastPos = transform.position;
    }

    void OnTriggerStay(Collider other)
    {
        var surf = other.GetComponent<Surface>();
        if (surf == null) return;
        bool grip = gripAction.action != null && gripAction.action.ReadValue<float>() > 0.5f;
        if (!grip || speed < SpeedThreshold) { sweepTimer = 0; candidate = null; return; }
        if (candidate != surf) { candidate = surf; sweepTimer = 0; }
        sweepTimer += Time.deltaTime;
        if (sweepTimer >= HoldTime && Time.time - lastToggleTime > Debounce)
        {
            surf.ToggleState();
            lastToggleTime = Time.time;
            sweepTimer = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (candidate != null && other.GetComponent<Surface>() == candidate) { candidate = null; sweepTimer = 0; }
    }
}
```

`SceneBuilder` addition (in `Build()`, after rig instantiation): find `Left Controller` and `Right Controller` children, add `SphereCollider (isTrigger, radius 0.06)`, `Rigidbody (kinematic)`, `MarkTool`, bind `gripAction` to the Starter Assets input action reference (load `InputActionAsset` from Starter Assets folder, find `XRI Left Interaction/Select` — inspect actual action names in the imported asset and use those).

- [ ] **Step 1: Write script + SceneBuilder wiring; re-run Build; project compiles headless.**
- [ ] **Step 2: Manual sim check** — GUI: Play with XR Interaction Simulator, sweep controller across Wall_N with grip: state toggles (watch Surface inspector), no toggle without grip, no double-toggle from one sweep.
- [ ] **Step 3: Commit** — `git commit -m "feat(ip1): sweep-to-mark keep/change with grip gate and debounce"`

---

### Task 9: `SamplePuller` — pull sample off kept surface

**Files:**
- Create: `Assets/Scripts/Runtime/SamplePuller.cs`
- Modify: `SceneBuilder.cs` (sample prefab creation + assignment; controllers get `SamplePuller`)

**Interfaces:**
- Consumes: `Surface.State`, `Sample.Init(Surface)`, XRI `XRInteractionManager` + `NearFarInteractor`/`XRDirectInteractor` on controllers (Starter Assets rig ships with interactors — reuse, don't rebuild).
- Produces: touching a **Keep** surface with the controller and pressing **trigger** spawns a sample — an 8 cm sphere with `XRGrabInteractable`, `Sample`, `HarmonyTuner` — already selected (held) by that controller via `interactionManager.SelectEnter`. Pull from a Change surface: refused, brief red flash of the controller cue (edge case, spec §9).

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SamplePuller : MonoBehaviour
{
    public InputActionProperty triggerAction; // <XRController>{hand}/triggerPressed
    public GameObject samplePrefab;
    public XRBaseInteractor interactor; // direct/near-far interactor on this controller

    Surface touching;

    void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponent<Surface>();
        if (s != null) touching = s;
    }

    void OnTriggerExit(Collider other)
    {
        if (touching != null && other.GetComponent<Surface>() == touching) touching = null;
    }

    void Update()
    {
        if (touching == null || triggerAction.action == null) return;
        if (!triggerAction.action.WasPressedThisFrame()) return;
        if (touching.State != SurfaceState.Keep) return; // only kept things are sources — this IS the concept

        var go = Instantiate(samplePrefab, transform.position, Quaternion.identity);
        var sample = go.GetComponent<Sample>();
        sample.Init(touching);
        var grab = go.GetComponent<XRGrabInteractable>();
        interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
    }
}
```

Sample prefab (created in `SceneBuilder`, saved to `Assets/Prefabs/Sample.prefab`): sphere, scale 0.08, `Rigidbody (useGravity false)`, `XRGrabInteractable (movementType Instantaneous, throwOnDetach false)`, `Sample`, `HarmonyTuner`, `HoldUpPreviewer` (Task 10).

- [ ] **Step 1: Write script, prefab creation in SceneBuilder, re-run Build, compile headless.**
- [ ] **Step 2: Manual sim check** — touch floor (Keep), press trigger: sample appears in hand, coloured from palette[0] (timber-analogous). Touch Wall_N (Change), trigger: nothing spawns.
- [ ] **Step 3: Commit** — `git commit -m "feat(ip1): pull sample off kept surface into hand"`

---

### Task 10: `HoldUpPreviewer` — proximity preview, release commits

**Files:**
- Create: `Assets/Scripts/Runtime/HoldUpPreviewer.cs`

**Interfaces:**
- Consumes: `Surface.All`, `Surface.Preview/Commit/Revert`, `Sample.CurrentColor`, `XRGrabInteractable` select events, `HarmonyTuner.Tick(roll)`.
- Produces: while held, nearest Change surface within `PreviewRadius = 0.45f` m of the sample gets `Preview(CurrentColor)` (nearest by collider `ClosestPoint` distance — spec §9 two-in-range rule); leaving range → `Revert()` (never a stuck preview); release while in range → `Commit` + destroy sample; release in mid-air → destroy sample (discard). Also computes roll: signed angle of interactor's Z-axis rotation about its forward, passed to `HarmonyTuner.Tick` every frame while held (this is where twist-to-tune becomes physical). While previewing, tuning updates the previewed surface live.

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Sample), typeof(XRGrabInteractable))]
public class HoldUpPreviewer : MonoBehaviour
{
    public const float PreviewRadius = 0.45f;

    Sample sample;
    HarmonyTuner tuner;
    XRGrabInteractable grab;
    Surface previewing;
    Transform holdingHand;
    float grabRollRef;

    void Awake()
    {
        sample = GetComponent<Sample>();
        tuner = GetComponent<HarmonyTuner>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        holdingHand = args.interactorObject.GetAttachTransform(grab);
        grabRollRef = RollOf(holdingHand);
    }

    static float RollOf(Transform t) => t.eulerAngles.z;

    void Update()
    {
        if (holdingHand == null) return;

        float roll = Mathf.DeltaAngle(grabRollRef, RollOf(holdingHand));
        tuner.Tick(roll);

        Surface nearest = null;
        float best = PreviewRadius;
        foreach (var s in Surface.All)
        {
            if (s.State != SurfaceState.Change) continue;
            var col = s.GetComponent<Collider>();
            if (col == null) continue;
            float d = Vector3.Distance(col.ClosestPoint(transform.position), transform.position);
            if (d < best) { best = d; nearest = s; }
        }

        if (nearest != previewing)
        {
            if (previewing != null) previewing.Revert(); // a preview must never stick
            previewing = nearest;
        }
        if (previewing != null) previewing.Preview(sample.CurrentColor);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        holdingHand = null;
        if (previewing != null)
        {
            previewing.Commit(sample.CurrentColor); // release in range = commit
            previewing = null;
        }
        Destroy(gameObject); // mid-air release = discard; either way the sample is gone
    }
}
```

- [ ] **Step 1: Write, compile headless.**
- [ ] **Step 2: EditMode test for the never-stick invariant** — add to `SurfaceTests.cs`:

```csharp
    [Test]
    public void PreviewThenRevert_AcrossThreeWalls_LeavesNoTrail()
    {
        var walls = new[] { Make(SurfaceState.Change), Make(SurfaceState.Change), Make(SurfaceState.Change) };
        foreach (var w in walls) { w.Commit(Color.white); w.Preview(Color.red); w.Revert(); }
        foreach (var w in walls) { Assert.AreEqual(Color.white, w.DisplayColor); Assert.IsFalse(w.IsPreviewing); }
    }
```

- [ ] **Step 3: Manual sim check** — pull sample from floor, carry past Wall_N → Wall_E: colour follows sample, trailing wall reverts (the §9 embarrassment case). Twist wrist: colour steps through 7 options, previewed wall updates live. Release near wall: commits. Release mid-air: sample vanishes, nothing changed.
- [ ] **Step 4: Run all EditMode tests, verify pass. Commit** — `git commit -m "feat(ip1): hold-up proximity preview with commit-on-release and revert guarantee"`

---

### Task 11: Diegetic light control — lamp, 3 states

**Files:**
- Create: `Assets/Scripts/Runtime/LampController.cs`
- Test: append to a new `LampControllerTests.cs` (state-cycling logic only)

**Interfaces:**
- Consumes: `Lamp` GO + `BulbLight` + `Sun` from SceneBuilder; controller trigger colliders (already present from Task 8).
- Produces: touching the lamp with a controller cycles Warm → Cool → Daylight → Warm. Exact states:

| State | Sun colour | Sun intensity | Bulb colour | Bulb intensity | Ambient |
|---|---|---|---|---|---|
| Warm | `(1.0, 0.83, 0.66)` | 0.15 | `(1.0, 0.75, 0.45)` | 1.6 | 0.35× warm |
| Cool | `(0.75, 0.86, 1.0)` | 0.4 | `(0.85, 0.92, 1.0)` | 1.0 | 0.4× cool |
| Daylight | `(1.0, 0.96, 0.89)` | 1.0 | off (0) | 0 | 0.5× neutral |

```csharp
using UnityEngine;

public class LampController : MonoBehaviour
{
    public enum LightState { Warm, Cool, Daylight }

    public Light sun;
    public Light bulb;
    public LightState Current { get; private set; } = LightState.Daylight;

    const float Debounce = 0.6f;
    float lastTouch = -10f;

    public static LightState Next(LightState s) =>
        (LightState)(((int)s + 1) % 3);

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() == null) return; // only controller cues count
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        Apply(Next(Current));
    }

    public void Apply(LightState s)
    {
        Current = s;
        switch (s)
        {
            case LightState.Warm:
                Set(sun, new Color(1f, 0.83f, 0.66f), 0.15f);
                Set(bulb, new Color(1f, 0.75f, 0.45f), 1.6f);
                RenderSettings.ambientLight = new Color(0.35f, 0.30f, 0.25f);
                break;
            case LightState.Cool:
                Set(sun, new Color(0.75f, 0.86f, 1f), 0.4f);
                Set(bulb, new Color(0.85f, 0.92f, 1f), 1.0f);
                RenderSettings.ambientLight = new Color(0.30f, 0.35f, 0.40f);
                break;
            case LightState.Daylight:
                Set(sun, new Color(1f, 0.96f, 0.89f), 1.0f);
                Set(bulb, Color.black, 0f);
                RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.45f);
                break;
        }
    }

    static void Set(Light l, Color c, float intensity)
    {
        if (l == null) return;
        l.color = c; l.intensity = intensity;
    }
}
```

Test (pure cycle logic): `Next(Warm)==Cool`, `Next(Cool)==Daylight`, `Next(Daylight)==Warm`.

- [ ] **Step 1: Failing test → run → implement → pass.**
- [ ] **Step 2: SceneBuilder wiring** — `Lamp` gets `LampController` with `sun`/`bulb` references; re-run Build.
- [ ] **Step 3: Manual sim check** — touch lamp: whole room shifts warm; committed wall colours read differently under each state ("does this green survive a warm bulb at 9pm?").
- [ ] **Step 4: Commit** — `git commit -m "feat(ip1): diegetic lamp cycling warm/cool/daylight"`

---

### Task 12: Scheme cycling on-controller + polish pass

**Files:**
- Create: `Assets/Scripts/Runtime/SchemeCycler.cs`
- Modify: `SceneBuilder.cs` (attach; static alternate-material prop; kept-surface cue)

**Interfaces:**
- Consumes: `SchemeManager.SaveScheme/ApplyScheme/Count`.
- Produces: controller **primary button (A/X)**: save current scheme (max 3, haptic pulse confirms via `XRBaseController.SendHapticImpulse` or OpenXR haptic action). **Secondary button (B/Y)**: cycle applied scheme. On-controller buttons are inputs, not floating VR menus — the no-menus rule holds. Kept-surface affordance cue: `SurfaceStateCue.cs` — small floating swatch-corner (2 cm emissive quad) at each Keep surface's edge, toggled by `Surface.State` (answers open Q4 with a visible-but-quiet cue). Alternate material display: one framed quad on Wall_S showing a second timber tone, static (spec §7 "present but shallow").

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class SchemeCycler : MonoBehaviour
{
    public InputActionProperty saveAction;   // primaryButton
    public InputActionProperty cycleAction;  // secondaryButton
    public SchemeManager manager;

    int current = -1;

    void Update()
    {
        if (saveAction.action != null && saveAction.action.WasPressedThisFrame())
            current = manager.SaveScheme();
        if (cycleAction.action != null && cycleAction.action.WasPressedThisFrame() && manager.Count > 0)
        {
            current = (current + 1) % manager.Count;
            manager.ApplyScheme(current);
        }
    }
}
```

- [ ] **Step 1: Write + wire + compile.**
- [ ] **Step 2: Manual sim check** — paint room red-ish, save; paint blue-ish, save; cycle: room flips whole-scheme. Third + fourth saves wrap.
- [ ] **Step 3: Full-loop rehearsal in simulator** — the complete protocol run: mark → pull → hold → tune → commit → relight → save → second scheme → cycle. Fix anything that breaks the flow. This rehearsal is the task's real deliverable.
- [ ] **Step 4: Commit** — `git commit -m "feat(ip1): scheme save/cycle on controller buttons, kept-surface cues, polish"`

---

### Task 13: Android APK — prove the Quest pipeline

**Files:**
- Create: `Assets/Editor/BuildScript.cs`

**Interfaces:**
- Consumes: everything above.
- Produces: `BuildScript.BuildAndroid()` → `ip1/RenovationPreviewer/Builds/ip1.apk` (gitignored; artifact kept locally + copy to a USB/drive for studio).

```csharp
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        var report = BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/Room.unity" },
            "Builds/ip1.apk",
            BuildTarget.Android,
            BuildOptions.None);
        Debug.Log($"Build result: {report.summary.result}, size {report.summary.totalSize}");
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }
}
```

- [ ] **Step 1: Run** `-buildTarget Android -executeMethod BuildScript.BuildAndroid` (background — first IL2CPP build 15–40 min). Expected: `Build result: Succeeded`.
- [ ] **Step 2: If Gradle fails on spaces in path** (error mentioning path fragments like `University of`): fallback — `ditto` the project to `/private/tmp/claude-501/.../scratchpad/ip1build` (space-free), build there, copy APK back. Document in `ip1/ai-use-log.md`. Do NOT move the repo.
- [ ] **Step 3: On-device smoke test when a Quest is available** — `adb install -r Builds/ip1.apk` (adb at `$UNITY_EDITOR/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`), run full loop once on hardware. **Book the Quest — CLAUDE.md says borrowed from UQ/studio; if none before Friday, first device contact is Studio morning — arrive early.**
- [ ] **Step 4: Commit build script** — `git commit -m "feat(ip1): headless Android build script"`

---

### Task 14: IP1 testing plan document (brief p.3 template)

**Files:**
- Create: `ip1/2026-08-28-ip1-testing-plan.md` (+ print HTML/PDF via the Chrome-headless pipeline in CLAUDE.md)
- Create: `ip1/data-collection-sheet.md` (one row per measure, printable)

**Interfaces:**
- Consumes: concept §10 (protocol, measures, A1/A2) — already drafted there; this task reformats into the brief's exact template headings: Pitch (3 sentences) / Testing Objective / Testing Methodologies / Prototype description / Data collection method / Testing Setup / Testing process with timings.
- Produces: plan robust enough that "another person could perform your testing" — rubric criterion 2.

- [ ] **Step 1: Write the plan** — lift §10 protocol verbatim where possible; objective centres **A2** (constraint helps vs frustrates — "the one worth the testing time"), A1 secondary. Include the 0:30/0:30/2:00/1:00/1:00 timed script, all seven measures with types, and the no-instruction first phase.
- [ ] **Step 2: Data sheet** — table: participant #, time-to-first-commit, prompt tally, twist-discovered y/n, samples-tried count, confidence 1–5, helpful/restrictive quote, rejection think-aloud notes. 6 copies.
- [ ] **Step 3: PDF via Chrome headless** (CLAUDE.md recipe). **Step 4: Commit** — `git commit -m "docs(ip1): testing plan and data collection sheets"`

---

### Task 15: AI-use log + Statement of Originality scaffold

**Files:**
- Create: `ip1/ai-use-log.md` (running log — start at Task 1, finalise here)
- Create: `ip1/statement-of-originality.md`

**Interfaces:**
- Consumes: the actual AI-use history of Tasks 1–14.
- Produces: UQ Library acknowledgement table (Tool / Use / Prompt-or-input / Output-used-in / Date) covering **every** AI-generated script and doc; Statement of Originality naming what is Kaike's (concept, design decisions, testing plan intent, in-class testing) vs AI-assisted (implementation scaffolding, per the log). **Secure assessment — under-declaring is the real risk. Kaike must review and own this file personally before Thu 27 Aug commit.**

- [ ] **Step 1: Consolidate log into the UQ table format.**
- [ ] **Step 2: Draft statement; flag to Kaike for personal review + voice pass.**
- [ ] **Step 3: Commit Thu 27 Aug at latest** — `git commit -m "docs(ip1): statement of originality and AI acknowledgement table"`

---

## Schedule fit (today = Fri 21 Aug)

| Day | Tasks |
|---|---|
| Fri 21 | 1–2 (scaffold + config; licence gate), start 3–6 (pure logic, TDD — no XR needed) |
| Sat 22 – Sun 23 | 7–10 (scene + core loop) — the heavy lift |
| Mon 24 | 11–12 · **feature freeze per SCHEDULE.md** · Task 13 pipeline proof |
| Tue 25 | Pilot test on one person (simulator or Quest if booked); fix breakage only |
| Wed 26 | Task 14 finalise; print sheets; participant briefing script |
| Thu 27 | Task 15 commit; standalone runnable copy; hardware prep |
| Fri 28 | **STUDIO. 5+ participants. Results to GitHub before leaving the room.** |

## Self-review notes

- Spec coverage: mark (T8), pull (T9), hold/tune/commit (T10, T5), light (T11), compare-minimal (T6, T12), edge cases §9 (T10 + refusal in T9), walls-only scope (scene T7 sets floor+sofa Keep), no-menus (all controls diegetic or on-controller), testing plan §10 (T14), AI declaration (T15).
- Known API-drift risks called out inline: XR loader assignment (T2 S3), XRI sample import API, Starter Assets action names (T8), `SelectEnter` cast signatures (T9). Resolution path for each: read `Library/PackageCache` source, adjust, keep the stated goal fixed.
- Deliberately absent (IP2a/IP2b scope — do not add): furniture grab/move/rotate, deep scheme comparison UI, MR passthrough, VR↔MR toggle, hand tracking.
