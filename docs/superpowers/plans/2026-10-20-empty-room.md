# Empty Room (replaces MR) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a third, bare "Empty room" preset and remove the MR showcase completely, without changing Living or Bedroom.

**Architecture:** The empty room is a normal `RoomPreset` (authored by `PresetAuthoring`) with a new `bare` flag. `PresetApplier` hides the room-level dressing that `SceneBuilder` builds (rug, two prints, arc lamp) when a preset is bare, and resets the time of day to the start stop. MR is deleted (assembly, prefab, material, scene wiring, tests) and the three Meta MR OpenXR features are switched off; packages stay installed.

**Tech Stack:** Unity 6000.0.80f1, URP, XRI 3.x, OpenXR (Meta Quest feature), NUnit EditMode/PlayMode tests, headless batchmode.

**Spec:** `docs/superpowers/specs/2026-10-20-empty-room-design.md` (amends `2026-10-17-ip2b-design.md`)

## Global Constraints

- **No behaviour change in Living or Bedroom** (layout, looks, sources, dressing visible, time of day carries over on room switch). Every existing Living/Bedroom test must pass unchanged except where a task says otherwise.
- AR Foundation 6.1.1 and Unity OpenXR: Meta 2.1.1 **stay installed**. Only the three features `com.unity.openxr.feature.arfoundation-meta-session`, `-camera`, `-plane` go off for Android. Meta Quest feature + Oculus Touch controller profile stay on.
- URP graphics settings stay as they are (HDR off, intermediate texture Auto).
- The scene is generated: edit `Assets/Editor/SceneBuilder.cs`, never `Room.unity`. Presets are generated: edit `Assets/Editor/PresetAuthoring.cs`, never the `.asset` files.
- Paths below are relative to `ip2b/RenovationPreviewer/` unless they start with `docs/`, `ip2b/` or `testing-data/`.
- Branch `ip2b`. No APK install, no headset.
- Headless (Unity editor GUI must be closed; check `pgrep -fl Unity` first):
  ```bash
  UNITY="/Applications/Unity/Hub/Editor/6000.0.80f1/Unity.app/Contents/MacOS/Unity"
  PROJ="$PWD/ip2b/RenovationPreviewer"      # run from the repo root
  $UNITY -batchmode -quit -projectPath "$PROJ" -executeMethod PresetAuthoring.Build -logFile "$SCRATCH/presets.log"
  $UNITY -batchmode -quit -projectPath "$PROJ" -executeMethod SceneBuilder.Build -logFile "$SCRATCH/scene.log"
  $UNITY -batchmode -projectPath "$PROJ" -runTests -testPlatform EditMode -testResults "$SCRATCH/edit.xml" -logFile "$SCRATCH/edit.log"
  $UNITY -batchmode -projectPath "$PROJ" -runTests -testPlatform PlayMode -testResults "$SCRATCH/play.xml" -logFile "$SCRATCH/play.log"
  ```
  Tests: no `-quit`. Read pass/fail from the XML (`total=`, `passed=`, `failed=` on `<test-run>`). If a run wedges at "Compiling Scripts": kill Unity/`bee_backend`/`netcorerun` by PID, `rm -rf Library/Bee Library/ScriptAssemblies`, rerun.
- Baseline before this plan: EditMode 246/246, PlayMode 44/44.

## Review Focus

1. **Switching Empty → Living after Phase 4** must bring back rug, prints and the arc lamp (lamp still touchable, rug pullable again). → Task 1 PlayMode test `EmptyRoom_HidesDressing_ThenLivingRestoresIt`.
2. **Switching Living ↔ Bedroom in the evening** must keep the evening (only bare resets light). → Task 1 EditMode test `Apply_NonBare_KeepsTimeOfDay`.
3. **A hidden rug must not be pullable** (nothing invisible gives colour). → Task 1 PlayMode test asserts `Rug` absent from `Surface.All` in Empty.
4. **Nothing AR left in the built scene** (no AR session starting, no permission prompt). → Task 2 PlayMode test `Scene_HasNoArFoundationComponents` + Task 4 manifest check.
5. **VR XR config still the IP2a one** (Meta Quest feature + Touch profile on). → Task 2 EditMode test `QuestVrFeatures_StillOnForAndroid`.

---

### Task 1: Empty room preset — bare dressing, daylight, third frame

**Files:**
- Modify: `Assets/Scripts/Runtime/RoomPreset.cs` (add `bare`)
- Modify: `Assets/Scripts/Runtime/TimeOfDayController.cs` (add `StartIndex`)
- Modify: `Assets/Scripts/Runtime/PresetApplier.cs` (dressing + daylight)
- Modify: `Assets/Editor/PresetAuthoring.cs` (author Empty)
- Modify: `Assets/Editor/SceneBuilder.cs` (wire dressing + timeOfDay; frame spacing)
- Test: `Assets/Scripts/Tests/PresetApplierTests.cs`, `Assets/Scripts/Tests/AuthoredPresetTests.cs`, `Assets/Scripts/PlayModeTests/SceneWiringTests.cs`, `Assets/Scripts/PlayModeTests/SnapshotTests.cs`

**Interfaces:**
- Produces: `RoomPreset.bare : bool` — "Empty room: room-level dressing hidden, opens in daylight." · `TimeOfDayController.StartIndex : const int = 1` (the 10:00 stop; the initial `Index` uses it) · `PresetApplier.dressing : GameObject[]` · `PresetApplier.timeOfDay : TimeOfDayController` · `PresetAuthoring.Names = { "Living", "Bedroom", "Empty" }`; Empty's `displayName = "Empty room"`.

- [ ] **Step 1: Write failing EditMode tests in `PresetApplierTests.cs`** (reuse its `Preset(...)` / `Named(...)` helpers and SetUp; add a `TimeOfDayController` with a directional `Light` as `TimeOfDayControllerTests` does)
  - `Apply_BarePreset_HidesDressing_ThenNonBareShowsIt`: two dressing GameObjects in `applier.dressing`; preset with `bare = true` → both `activeSelf == false`; then a non-bare preset → both `true`.
  - `Apply_BarePreset_ResetsToDaylight`: `tod.Apply(4)`; apply bare → `tod.Index == TimeOfDayController.StartIndex`.
  - `Apply_NonBare_KeepsTimeOfDay`: `tod.Apply(4)`; apply non-bare → `tod.Index == 4`.
  - `Apply_NullDressingEntries_AndNoTimeOfDay_DoNotThrow`: `dressing = { null }`, `timeOfDay = null`, bare preset → no exception.
- [ ] **Step 2: Write failing EditMode test in `AuthoredPresetTests.cs`**: `Expected = { "Living", "Bedroom", "Empty" }`; `Presets_AreLivingBedroomEmpty` asserts display names `{ "Living", "Bedroom", "Empty room" }`; new `Empty_IsBare_NoFurniture_WhiteWalls`: `bare == true`, `furniture.Count == 0`, all four walls' look colour == catalogue paint "Whisper White", floor look material == catalogue material `WoodFloor051`; Living and Bedroom `bare == false`.
- [ ] **Step 3: Run EditMode** → the new tests fail (compile error on `bare` / `StartIndex` / `dressing` counts as fail).
- [ ] **Step 4: Implement** `RoomPreset.bare`, `TimeOfDayController.StartIndex` (initial `Index = StartIndex`), and in `PresetApplier.Apply` — after clearing samples/slots, before looks — set every non-null `dressing` entry active = `!preset.bare`; after looks, if `preset.bare && timeOfDay != null` → `timeOfDay.Apply(TimeOfDayController.StartIndex)`. Update the class summary (F1–F3; bare presets).
- [ ] **Step 5: Author Empty in `PresetAuthoring.Build`**: `Preset(cat, "Empty", "Empty room")`, `bare = true`; `Floor(cat, empty, "WoodFloor051")`; `Paint(cat, empty, Walls, "Whisper White")`; ceiling "Whisper White"; trims "Antique White U.S.A."; no `Place`; `Swatches(cat, empty, "WoodFloor051", "Whisper White", "Whisper White")`; include it in the `WriteCard` loop; log "3 presets written (Living, Bedroom, Empty)". Run `PresetAuthoring.Build` headless; confirm `Assets/Presets/Empty.asset` + `Card_Empty.png/.mat` exist.
- [ ] **Step 6: Wire in `SceneBuilder.Build`**: `applier.timeOfDay = tod`; `applier.dressing = { Rug, Art_0, Art_1, ArcLamp }` found by name after they are built (warn if any is missing). Preset frames: spacing `0.9f` instead of `1.5f` (three frames at x ≈ −1.4, −0.5, 0.4; clear of the door at x ≥ 1.5 and the NW monstera). Fix the stale comment "three pre-furnished rooms".
- [ ] **Step 7: Update PlayMode tests in `SceneWiringTests.cs`**:
  - `Room_StartsInPreset0...`: `Assert.AreEqual(3, applier.presets.Length, "Living + Bedroom + Empty wired")`.
  - `Presets_Layout_NoOverlap_KeyPlacesClear_Spread`: `if (applier.presets[p].bare) continue;` before the 7-piece check; "preset frames" keep-clear rect becomes `Rect.MinMaxRect(-1.8f, hd - 0.8f, 0.8f, hd)`.
  - New `EmptyRoom_HidesDressing_ThenLivingRestoresIt`: find the bare preset index; set evening (`tod.Apply(4)`); `Apply(empty)` → no `FurnitureSlot`s, `GameObject.Find` returns null for Rug/Art_0/Art_1/ArcLamp, `Surface.All` has no `"Rug"`, `tod.Index == StartIndex`; then `Apply(0)` → all four found again, `"Rug"` back in `Surface.All`, `Object.FindAnyObjectByType<LampController>() != null`.
- [ ] **Step 8: Snapshots** in `SnapshotTests.cs`: after the preset loop, apply the bare preset and shoot `07_empty_from_start` (eye at the start spot `(0.9, 1.6, 1.3)` looking toward `(-0.5, 1.2, -1.0)`) and `07_empty_wallN_frames` (looking at Wall_N frames); re-apply 0 after.
- [ ] **Step 9: Run `SceneBuilder.Build`, then EditMode and PlayMode** → all pass. Open `Builds/snapshots/06_preset_2_*.png`, `07_empty_*.png`, `02_wallW_frames.png`: bare white room, lit, three labelled frames, no rug/prints/lamp.
- [ ] **Step 10: Commit** `feat(ip2b): empty room — third preset, bare dressing, opens in daylight`.

---

### Task 2: Remove MR

**Files:**
- Delete: `Assets/Scripts/MR/` (+ `MR.meta`), `Assets/Scripts/PlayModeTests/MrWiringTests.cs` (+ `.meta`), `Assets/Prefabs/MrPlane.prefab` (+ `.meta`), `Assets/Materials/MrPaint.mat` (+ `.meta`)
- Modify: `Assets/Scripts/PlayModeTests/Renovation.PlayModeTests.asmdef` (drop `Renovation.MR`, `Unity.XR.ARFoundation`)
- Modify: `Assets/Editor/SceneBuilder.cs` (delete `BuildMr`, `KeepOutOfShell`, the call and its comment)
- Modify: `Assets/Scripts/Runtime/PresetApplier.cs` (delete `BeforeApply` event + its invocation + doc)
- Modify: `Assets/Editor/ProjectConfigurator.cs` (`EnableMetaMr` → `DisableMetaMr`)
- Create: `Assets/Scripts/Tests/XrConfigTests.cs`
- Modify: `Assets/Scripts/Tests/Renovation.EditModeTests.asmdef` (add `Unity.XR.OpenXR`, `Unity.XR.OpenXR.Editor`)
- Test: `Assets/Scripts/PlayModeTests/SceneWiringTests.cs`

**Interfaces:**
- Produces: `ProjectConfigurator.DisableMetaMr()` — public static, headless-callable; sets the three Meta MR features `enabled = false` for Android, saves assets, logs each. `Configure()` calls it instead of `EnableMetaMr`. `SetupPassthroughGraphics` is deleted (its settings stay on disk).

- [ ] **Step 1: Write failing tests**
  - `XrConfigTests.MetaMrFeatures_AreOffForAndroid`: for each of the three ids, `FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, id)` is non-null and `enabled == false`.
  - `XrConfigTests.QuestVrFeatures_StillOnForAndroid`: in `OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android).GetFeatures<OpenXRFeature>()`, a feature whose type name contains `MetaQuestFeature` and one containing `OculusTouchControllerProfile` are both enabled.
  - `SceneWiringTests.Scene_HasNoArFoundationComponents`: no `MonoBehaviour` (include inactive) whose type namespace starts with `UnityEngine.XR.ARFoundation`; `GameObject.Find("YourRoomFrame") == null`.
- [ ] **Step 2: Run EditMode + PlayMode** → these three fail (features on; AR components present).
- [ ] **Step 3: Delete the MR files listed above; edit asmdef, `SceneBuilder`, `PresetApplier`, `ProjectConfigurator`.** `grep -rn "MrMode\|MrPlane\|BeforeApply\|EnableMetaMr\|RoomShell\|ARFoundation" Assets --include='*.cs' --include='*.asmdef'` returns nothing outside comments that say MR was removed.
- [ ] **Step 4: Run `ProjectConfigurator.DisableMetaMr` headless, then `SceneBuilder.Build`.** `git diff -- "Assets/XR/Settings/OpenXR Package Settings.asset"` shows only those three Android features flipping to `m_enabled: 0`.
- [ ] **Step 5: Run EditMode + PlayMode** → all pass. Record counts (expect EditMode ≈ 246 − 13 MR tests + Task 1/2 additions; PlayMode ≈ 44 − 3 + additions).
- [ ] **Step 6: Commit** `feat(ip2b): remove MR showcase — Meta MR features off, no AR in the scene`.

---

### Task 3: Docs follow (drafts for Kaike's voice pass)

**Files:**
- Modify: `ip2b/2026-10-23-ip2b-testing-plan.md`, `ip2b/research-references.md`, `ip2b/ipad-logger.html`, `ip2b/ai-use-log.md`, `CLAUDE.md` (Current state), `docs/superpowers/plans/2026-10-17-ip2b-build.md` (STATUS line)

- [ ] **Step 1: Testing plan.** Prototype table: MR row → **Empty room** row ("Touch the 'Empty room' frame: a bare white room with the same tools. **new**"). Facilitator line: `F1–F3` rooms, no `M`. B6 *(exploratory)* → "In a bare room, the same tools let people make the room they imagine" · measure "How close is this to the room you had in mind?" 1–5 + why, colour source per commit (menu vs fan). Data table row B6 to match. Five principles: drop "MR keeps the real room visible". Setup step 3 → "Check the Empty room frame loads a bare, lit room." Phase 4 → **"Empty room (1 min)."** Read verbatim: **"Touch the frame that says 'Empty room'. Make the room you're imagining."** Log; ask B6. Known limits: delete the passthrough + MR-fallback lines; add "In the Empty room a tall participant can walk into the ceiling pendant (shade ≈ 1.75 m); steer round it." Keep total ≈ 7 min (MR was 45 s; Phase 4 is now 60 s — take 15 s off the questionnaire buffer or note ~7 min 15).
- [ ] **Step 2: Research.** Replace the MR sentence and refs Milgram & Kishino (1994) and de Souza & Tartz (2024) with: Shneiderman (2007) *Creativity support tools: Accelerating discovery and innovation*, CACM 50(12), 20–32, https://doi.org/10.1145/1323688.1323689 and Cherry & Latulipe (2014) *Quantifying the creativity support of digital tools through the Creativity Support Index*, ACM TOCHI 21(4), https://doi.org/10.1145/2617588. **Verify both via `https://api.crossref.org/works/<doi>`** (title, authors, volume, pages); if either fails, pick another verified source on open-ended/creative tools. One new sentence tying the Empty room phase to "low threshold, high ceiling" open-ended creation and B6's closeness rating. Result: 7 references, APA 7, same table format in `research-references.md` (update its "verified" date).
- [ ] **Step 3: Logger** (`ipad-logger.html`): facilitator note → "Rooms: touch a framed picture (desktop **F1–F3**)." Phase 4 → `{ id: "p4", title: "Phase 4 · Empty room", note: "1 min — “Touch the frame that says ‘Empty room’. Make the room you're imagining.” — B6", timer: 3, items: [ r16 "Entered the Empty room?" Yes/No · r17 "How close to the room they had in mind? (1–5)" · r17b "Why (verbatim)" text ] }`; prompt-card text and the timer label `{ label: "Empty room", secs: 60 }`. Keep keys `r16`/`r17` so the export shape stays stable; open the file in Chrome headless or a browser once to check it renders without console errors.
- [ ] **Step 4: AI log + state.** One `ai-use-log.md` row for 20 Oct (empty-room design, builder check, plan, build, MR removal, doc drafts). CLAUDE.md "Current state": MR cut (why), Empty room added, controls `F1–F3`, no `M`, new test counts, G8 result (Task 4). Build plan STATUS: one line pointing to this plan.
- [ ] **Step 5: Commit** `docs(ip2b): empty room replaces MR — plan, research, logger, state`.

---

### Task 4: Verification + build-only APK check (G8)

**Files:**
- Modify: `Assets/Editor/BuildScript.cs` (output `Builds/ip2b.apk`)

- [ ] **Step 1:** `BuildScript.BuildAndroid` writes `Builds/ip2b.apk`.
- [ ] **Step 2: Full EditMode + PlayMode** from clean (`SceneBuilder.Build` first) → 0 failures.
- [ ] **Step 3: Headless Android build, no install:** `$UNITY -batchmode -quit -projectPath "$PROJ" -executeMethod BuildScript.BuildAndroid -logFile "$SCRATCH/apk.log"` → log shows `Build result: Succeeded`. Check free disk first (`df -h /`, need ≥ 5 GB).
- [ ] **Step 4: Manifest check:** `"$SDK/build-tools/36.0.0/aapt2" dump permissions "$PROJ/Builds/ip2b.apk"` (SDK under `PlaybackEngines/AndroidPlayer/SDK`) → **no** `com.oculus.permission.USE_SCENE`; `aapt2 dump xmltree --file AndroidManifest.xml` still shows the Quest/OpenXR entries the IP2a build had (compare with `Builds/ip2a.apk`). Do **not** run `adb install`.
- [ ] **Step 5:** Record result (pass/fail, APK size, build time, permissions list) in CLAUDE.md "Current state". Commit `chore(ip2b): build-only APK check — builds, no USE_SCENE`.
