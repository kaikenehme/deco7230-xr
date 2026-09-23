using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Loads Room.unity and asserts the SceneBuilder wiring is complete — the
/// closest headless equivalent of opening the scene and checking inspectors.
/// </summary>
public class SceneWiringTests
{
    [UnitySetUp]
    public IEnumerator LoadRoom()
    {
        SceneManager.LoadScene("Room");
        yield return null; // let Awake/OnEnable run
    }

    static readonly string[] Shell = { "Floor", "Wall_N", "Wall_S", "Wall_E", "Wall_W", "Ceiling", "Door", "Trim", "WindowFrame" };

    [UnityTest]
    public IEnumerator Room_StartsInPreset0_ShellNineSurfaces_PlusPresetKeeps()
    {
        yield return null; yield return null;   // PresetApplier.Start ran
        var applier = Object.FindObjectsByType<PresetApplier>(FindObjectsSortMode.None).Single();
        Assert.AreEqual(3, applier.presets.Length, "three presets wired");
        Assert.AreEqual(0, applier.Current, "preset 0 applied at Start");
        var surfaces = Surface.All.Where(s => s != null).ToList();
        var shell = surfaces.Where(s => s.GetComponent<FurnitureSlot>() == null).Select(s => s.name).OrderBy(n => n).ToList();
        CollectionAssert.AreEquivalent(Shell, shell);
        int keptSlots = surfaces.Count(s => s.GetComponent<FurnitureSlot>() != null && s.State == SurfaceState.Keep);
        Assert.AreEqual(applier.presets[0].furniture.Count(f => f.keep), keptSlots, "kept furniture from the preset");
        Assert.AreEqual(SurfaceState.Keep, surfaces.Single(s => s.name == "Floor").State);
        Assert.AreEqual(applier.presets[0].furniture.Count, Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Length, "every placement spawned");
    }

    [UnityTest]
    public IEnumerator PresetFrames_ThreeOnWallW_Wired()
    {
        yield return null; yield return null;
        var frames = Object.FindObjectsByType<PresetFrame>(FindObjectsSortMode.None).OrderBy(f => f.index).ToList();
        Assert.AreEqual(3, frames.Count);
        var applier = Object.FindObjectsByType<PresetApplier>(FindObjectsSortMode.None).Single();
        for (int i = 0; i < 3; i++)
        {
            Assert.AreSame(applier, frames[i].applier);
            Assert.AreEqual(i, frames[i].index);
            Assert.IsNotNull(frames[i].frame);
            Assert.Less(frames[i].transform.position.x, -RoomSpec.W / 2f + 0.1f, "on Wall_W");
            Assert.IsTrue(frames[i].GetComponent<SphereCollider>().isTrigger);
        }
        Assert.IsTrue(frames[0].IsCurrent);
        applier.Apply(2);
        yield return null;
        Assert.IsTrue(frames[2].IsCurrent);
        Assert.AreEqual(applier.presets[2].furniture.Count, Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Length, "old pieces gone, new ones in");
    }

    [UnityTest]
    public IEnumerator Presets_AllSourceIdsResolveInCatalogue_AndLooksNameShellSurfaces()
    {
        yield return null;
        var applier = Object.FindObjectsByType<PresetApplier>(FindObjectsSortMode.None).Single();
        foreach (var p in applier.presets)
        {
            foreach (var f in p.furniture) Assert.IsNotNull(applier.catalogue.Furniture(f.sourceId)?.prefab, $"{p.displayName}: {f.sourceId}");
            foreach (var l in p.looks) CollectionAssert.Contains(Shell, l.surfaceName, $"{p.displayName}: look {l.surfaceName}");
            Assert.IsTrue(p.furniture.Any(f => f.keep), $"{p.displayName}: has a kept piece to pull from");
            Assert.IsTrue(p.looks.Any(l => l.surfaceName == "Floor" && l.state == SurfaceState.Keep), $"{p.displayName}: floor kept");
        }
        // The kept floor really wears each preset's material at runtime.
        var floor = Surface.All.Single(x => x != null && x.name == "Floor");
        for (int i = 0; i < applier.presets.Length; i++)
        {
            applier.Apply(i); yield return null;
            var want = applier.presets[i].looks.Single(l => l.surfaceName == "Floor").material;
            Assert.IsNotNull(want, $"preset {i} floor material");
            Assert.AreEqual(want.mainTexture, floor.Renderers[0].sharedMaterial.mainTexture, $"preset {i}: floor texture applied");
        }
    }

    [UnityTest]
    public IEnumerator Managers_HaveOutlineMaterial_WithFrontCull()
    {
        yield return null;
        var cfg = Object.FindObjectsByType<RenovationConfig>(FindObjectsSortMode.None).Single();
        Assert.IsNotNull(cfg.outlineShell);
        Assert.AreEqual(1f, cfg.outlineShell.GetFloat("_Cull"));
        Assert.AreSame(cfg.outlineShell, SelectionOutline.ShellMaterial);
    }

    [UnityTest]
    public IEnumerator WindowWall_IsOneSurface_WithFourPaintableParts()
    {
        yield return null;
        var wall = Surface.All.Single(s => s != null && s.name == "Wall_S");
        Assert.AreEqual(4, wall.Renderers.Count, "lintel, sill, two jambs");
        Assert.AreEqual(4, wall.Colliders.Count);
        Assert.IsNull(wall.GetComponent<Renderer>(), "root is logical only");
        wall.Commit(Color.red);
        yield return null;
        foreach (var r in wall.Renderers) Assert.AreEqual(Color.red, r.material.color, $"{r.name} painted");
        Assert.IsNotNull(Surface.All.Single(s => s != null && s.name == "WindowFrame"));
        Assert.IsNotNull(GameObject.Find("Glass"));
        Assert.IsNotNull(GameObject.Find("Ground"));
    }

    [UnityTest]
    public IEnumerator Clock_IsWiredToTimeOfDay_AndAdvancesIt()
    {
        yield return null;
        var clock = Object.FindObjectsByType<WallClock>(FindObjectsSortMode.None).Single();
        var tod = Object.FindObjectsByType<TimeOfDayController>(FindObjectsSortMode.None).Single();
        Assert.AreSame(tod, clock.controller);
        Assert.IsNotNull(clock.hourHand); Assert.IsNotNull(clock.face);
        Assert.IsTrue(clock.GetComponent<SphereCollider>().isTrigger, "touch target");
        int before = tod.Index;
        tod.Next();
        Assert.AreEqual(TimeOfDay.Next(before), tod.Index);
        Assert.Less(Quaternion.Angle(WallClock.HandRotation(tod.Current.hour), clock.hourHand.localRotation), 0.01f, "hand follows the stop");
    }

    // Controller GOs are deactivated by XRI's Input Modality Manager when no
    // XR device is present (always true headless), so searches must include
    // inactive objects.
    [UnityTest]
    public IEnumerator BothControllers_HaveMarkAndPullTools()
    {
        yield return null;
        var marks = Object.FindObjectsByType<MarkTool>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(2, marks.Length, "MarkTool on both controllers");

        var pullers = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(2, pullers.Length, "SamplePuller on both controllers");
        foreach (var p in pullers)
        {
            Assert.IsNotNull(p.samplePrefab, $"{p.name}: sample prefab wired");
            Assert.IsNotNull(p.interactor, $"{p.name}: interactor wired");
            Assert.IsNotNull(p.triggerAction.action, $"{p.name}: trigger action wired");
        }
    }

    [UnityTest]
    public IEnumerator Lamp_IsBulbOnly_StartsWarm_WithPullCord_AndSchemeCyclerStillWired()
    {
        yield return null;
        var lamp = Object.FindObjectsByType<LampController>(FindObjectsSortMode.None).Single();
        Assert.IsNotNull(lamp.bulb, "lamp: bulb wired");
        Assert.IsNotNull(lamp.shade, "lamp: shade wired");
        Assert.AreEqual(LampController.LightState.Warm, lamp.Current, "starts on, warm");
        Assert.IsTrue(lamp.bulb.enabled, "Start() applied the state");
        Assert.IsNotNull(lamp.transform.Find("Cord"), "pull cord present");
        Assert.IsTrue(lamp.shade.sharedMaterial.IsKeywordEnabled("_EMISSION"), "shade can glow");

        var cycler = Object.FindObjectsByType<SchemeCycler>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsNotNull(cycler.manager, "cycler: SchemeManager wired (kept, not in the IP2a test)");
    }

    [UnityTest]
    public IEnumerator Sky_AndSun_AreAssigned_TimeOfDayOwnsThem()
    {
        yield return null;
        var tod = Object.FindObjectsByType<TimeOfDayController>(FindObjectsSortMode.None).Single();
        Assert.IsNotNull(tod.sun, "sun wired");
        Assert.IsNotNull(tod.sky, "sky wired");
        Assert.IsNotNull(RenderSettings.sun, "procedural sky follows the sun");
        Assert.AreEqual(UnityEngine.Rendering.AmbientMode.Flat, RenderSettings.ambientMode, "ambient writes must take effect");
        Assert.AreEqual(LightShadows.Soft, tod.sun.shadows, "window sun casts a patch");
        Assert.AreEqual(10, tod.Current.hour, "starts at 10:00");
        Assert.AreEqual(TimeOfDay.Stops[1].ambient, RenderSettings.ambientLight);
        Assert.AreEqual(0, Object.FindObjectsByType<LampController>(FindObjectsSortMode.None).Count(l => l.bulb == tod.sun), "lamp never touches the sun");
    }

    [UnityTest]
    public IEnumerator SamplePrefab_PulledSample_PreviewsAndCommits()
    {
        yield return null;
        var floor = Surface.All.Single(s => s != null && s.name == "Floor");
        var wall = Surface.All.Single(s => s != null && s.name == "Wall_N");
        var prefab = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].samplePrefab;

        // Spawn a sample as SamplePuller would, hold it "near" Wall_N by parking
        // it against the wall, and drive Update manually via frames.
        var go = Object.Instantiate(prefab, wall.transform.position, Quaternion.identity);
        var sample = go.GetComponent<Sample>();
        sample.Init(floor);
        Assert.AreEqual(7, sample.Palette.Length);

        // HoldUpPreviewer only runs while grabbed; simulate its core contract directly:
        wall.Preview(sample.CurrentColor);
        Assert.IsTrue(wall.IsPreviewing);
        wall.Commit(sample.CurrentColor);
        Assert.IsFalse(wall.IsPreviewing);
        Assert.AreEqual(sample.CurrentColor, wall.CommittedColor);
        Assert.AreEqual(floor.SampleColor, sample.BaseColor, "sample starts from the floor's sample colour");

        Object.Destroy(go);
        yield return null;
    }

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
    public IEnumerator EverySurface_HasMenuTargetAndKind_KeptFurnitureIsGrabbableSource()
    {
        yield return null; yield return null;
        foreach (var s in Surface.All.Where(s => s != null))
        {
            Assert.IsNotNull(s.GetComponent<MenuTarget>(), $"{s.name}: MenuTarget");
            if (s.GetComponent<FurnitureSlot>() == null) Assert.AreNotEqual(SurfaceKind.None, s.Kind, $"{s.name}: kind set");
        }
        var kept = Surface.All.First(s => s != null && s.GetComponent<FurnitureSlot>() != null && s.State == SurfaceState.Keep);
        Assert.IsNotNull(kept.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(), "kept furniture grabbable");
        Assert.IsNotNull(kept.GetComponent<PullAffordance>(), "kept furniture has the pull tab");
        Assert.IsNotNull(kept.GetComponent<SelectionOutline>(), "kept furniture outlines on hover");
        Assert.AreNotEqual(Color.white, kept.SampleColor, "textured furniture pulls an authored colour");
    }

    [UnityTest]
    public IEnumerator RayFeedback_IsOnRightController()
    {
        yield return null;
        var fb = Object.FindObjectsByType<RayFeedback>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreEqual("Right Controller", fb.transform.name);
        Assert.IsNotNull(fb.menu, "menu wired");
        Assert.IsNotNull(fb.rayOrigin, "ray origin wired");
        Assert.IsNotNull(fb.emissionCarrier, "emission carrier material wired");
        Assert.IsTrue(fb.emissionCarrier.IsKeywordEnabled("_EMISSION"));
    }

    [UnityTest]
    public IEnumerator Room_IsRoomWByRoomD_AndFloorTeleports()
    {
        yield return null;
        var floor = Surface.All.Single(s => s != null && s.name == "Floor");
        Bounds Of(string n) => Surface.All.Single(x => x != null && x.name == n).WorldBounds;
        Assert.AreEqual(RoomSpec.W, Of("Wall_E").min.x - Of("Wall_W").max.x, 0.001f, "interior width, wall face to wall face");
        Assert.AreEqual(RoomSpec.D, Of("Wall_N").min.z - Of("Wall_S").max.z, 0.001f, "interior depth");
        var tele = floor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
        Assert.IsNotNull(tele, "floor is a teleport area");
        Assert.AreEqual(1 << RoomSpec.TeleportLayer, tele.interactionLayers.value, "teleport layer only");
        var rig = GameObject.Find("XR Origin (XR Rig)");
        Assert.AreEqual(180f, rig.transform.eulerAngles.y, 0.5f, "rig faces the sofa");
    }
    [UnityTest]
    public IEnumerator EverySurface_HasPullAffordance_AndControllersGlow()
    {
        yield return null;
        foreach (var s in Surface.All.Where(s => s != null))
            Assert.IsNotNull(s.GetComponent<PullAffordance>(), $"{s.name}: pull affordance");
        var glows = Object.FindObjectsByType<HandGlow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(2, glows.Length, "HandGlow on both controllers");
    }

    [UnityTest]
    public IEnumerator SamplePrefab_EasesIntoTheHand()
    {
        yield return null;
        var prefab = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].samplePrefab;
        var grab = prefab.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        Assert.Greater(grab.attachEaseInTime, 0f, "peel: the sample flies from the tab into the hand");
    }

    [UnityTest]
    public IEnumerator RightController_HasFacilitatorSpawn_AndMenuRelayKnowsThePuller()
    {
        yield return null;
        var fac = Object.FindObjectsByType<FacilitatorSpawn>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreEqual("Right Controller", fac.transform.name);
        Assert.IsNotNull(fac.puller, "facilitator spawn uses the right hand's puller");
        Assert.IsNotNull(fac.spawnAction.action, "left X bound");
        var relay = Object.FindObjectsByType<MenuSelectRelay>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreSame(fac.puller, relay.puller, "menu relay yields to the puller");
    }
    [UnityTest]
    public IEnumerator Rig_HasFurnitureInput_AndSlotsDoNotTrackPose()
    {
        yield return null;
        var fi = Object.FindObjectsByType<FurnitureInput>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsNotNull(fi.leftStick.action); Assert.IsNotNull(fi.rightStick.action);
        foreach (var slot in Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None))
        {
            var grab = slot.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            Assert.IsFalse(grab.trackPosition, $"{slot.name}: placed by the ray, not floated by XRI");
            Assert.IsFalse(grab.trackRotation, $"{slot.name}: yawed by the stick");
        }
    }
    [UnityTest]
    public IEnumerator Managers_HaveOnboarding_AndSampleHasThread()
    {
        yield return null; yield return null;
        var ob = Object.FindObjectsByType<OnboardingSequence>(FindObjectsSortMode.None).Single();
        Assert.IsNotNull(ob.presets); Assert.IsNotNull(ob.lamp); Assert.IsNotNull(ob.head);
        Assert.IsTrue(ob.Running || ob.Done, "onboarding started once preset 0 applied");
        var prefab = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].samplePrefab;
        Assert.IsNotNull(prefab.GetComponent<HarmonyThread>(), "thread back to the source");
    }

    [UnityTest]
    public IEnumerator Wall_PreviewMaterialTenTimes_OneInstance()
    {
        yield return null;
        var wall = Surface.All.Single(s => s != null && s.name == "Wall_N");
        var menu = Object.FindObjectsByType<ControllerMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        var tiles = menu.catalogue.materials[0].material;
        for (int i = 0; i < 10; i++) { wall.PreviewMaterial(tiles); wall.Revert(); }
        Assert.LessOrEqual(wall.InstanceCount, 2, "one instance per source material, not per hover");
    }

    [UnityTest]
    public IEnumerator RightController_HasSameHandClose_AndEditorGazeAim()
    {
        yield return null;
        var relay = Object.FindObjectsByType<MenuSelectRelay>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsNotNull(relay.closeSameHandAction.action, "right B closes the menu");
        StringAssert.Contains("{RightHand}/secondaryButton", relay.closeSameHandAction.action.bindings[0].path);
        var cycler = relay.GetComponent<SchemeCycler>();
        Assert.AreSame(relay.menu, cycler.menu, "cycler yields to the open menu");
        var gaze = relay.GetComponent<EditorGazeAim>();
        Assert.IsNotNull(gaze.head); Assert.IsNotNull(gaze.poseDriver);
        Assert.AreEqual(Application.isEditor, gaze.enabled, "editor-only crutch");
    }

    /// <summary>Sun leaked through the open notches where walls met the floor/ceiling edges
    /// (bright lines on Wall_W and the floor, 23 Sep renders). Slabs must reach the walls' outer faces.</summary>
    [UnityTest]
    public IEnumerator Shell_FloorAndCeiling_CoverWallFootprint()
    {
        yield return null;
        Bounds Of(string n) => Surface.All.Single(x => x != null && x.name == n).WorldBounds;
        var walls = new[] { "Wall_N", "Wall_S", "Wall_E", "Wall_W" }.Select(Of).ToList();
        var outer = walls[0]; foreach (var w in walls) outer.Encapsulate(w);
        foreach (var slab in new[] { "Floor", "Ceiling" })
        {
            var b = Of(slab);
            Assert.LessOrEqual(b.min.x, outer.min.x + 1e-3f, slab); Assert.GreaterOrEqual(b.max.x, outer.max.x - 1e-3f, slab);
            Assert.LessOrEqual(b.min.z, outer.min.z + 1e-3f, slab); Assert.GreaterOrEqual(b.max.z, outer.max.z - 1e-3f, slab);
        }
        Assert.AreEqual(0f, Of("Floor").max.y, 1e-3f, "floor top stays at y = 0");
        Assert.AreEqual(RoomSpec.H, Of("Ceiling").min.y, 1e-3f, "ceiling underside stays at H");
    }

    /// <summary>Each preset reads as a lived-in room, not a cluster: pieces never overlap, the
    /// participant's start spot, the preset frames, the door and the lamp stay reachable, and the
    /// furniture uses the room (spread over most of its width and depth).</summary>
    [UnityTest]
    public IEnumerator Presets_Layout_NoOverlap_KeyPlacesClear_Spread()
    {
        yield return null; yield return null;
        var applier = Object.FindObjectsByType<PresetApplier>(FindObjectsSortMode.None).Single();
        float hw = RoomSpec.W / 2f, hd = RoomSpec.D / 2f;
        var start = new Vector3(0.3f, 0f, 0.8f);
        var lamp = GameObject.Find("Lamp").transform.position;
        // Floor rectangles (xz) that must stay empty.
        var keepClear = new (string what, Rect r)[]
        {
            ("start spot", Rect.MinMaxRect(start.x - 0.7f, start.z - 0.7f, start.x + 0.7f, start.z + 0.7f)),
            ("preset frames", Rect.MinMaxRect(-hw, -1.2f, -hw + 1.2f, 2.8f)),
            ("door", Rect.MinMaxRect(hw - 1.4f, hd - 1.0f, hw, hd)),
            ("lamp", Rect.MinMaxRect(lamp.x - 0.45f, lamp.z - 0.45f, lamp.x + 0.45f, lamp.z + 0.45f)),
        };
        var problems = new System.Collections.Generic.List<string>();
        void Check(bool bad, string msg) { if (bad) problems.Add(msg); }
        for (int p = 0; p < applier.presets.Length; p++)
        {
            applier.Apply(p); yield return null;
            var name = applier.presets[p].displayName;
            var slots = Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Where(x => x.Visual != null).ToList();
            Check(slots.Count < 7, $"{name}: only {slots.Count} pieces, want 7+ to read as a living room");
            var boxes = slots.Select(x => (name: x.name, b: x.GetComponent<BoxCollider>().bounds)).ToList();
            string Fmt(Bounds b) => $"x {b.min.x:F2}..{b.max.x:F2} z {b.min.z:F2}..{b.max.z:F2}";
            for (int i = 0; i < boxes.Count; i++)
            {
                var bi = boxes[i].b;
                var flat = Rect.MinMaxRect(bi.min.x, bi.min.z, bi.max.x, bi.max.z);
                foreach (var (what, r) in keepClear) Check(flat.Overlaps(r), $"{name}: {boxes[i].name} ({Fmt(bi)}) blocks the {what}");
                Check(bi.min.x < -hw - 0.01f || bi.max.x > hw + 0.01f || bi.min.z < -hd - 0.01f || bi.max.z > hd + 0.01f, $"{name}: {boxes[i].name} ({Fmt(bi)}) through a wall");
                for (int j = i + 1; j < boxes.Count; j++)
                {
                    var bj = boxes[j].b;
                    bool overlap = bi.min.x < bj.max.x - 0.02f && bj.min.x < bi.max.x - 0.02f && bi.min.z < bj.max.z - 0.02f && bj.min.z < bi.max.z - 0.02f;
                    Check(overlap, $"{name}: {boxes[i].name} ({Fmt(bi)}) overlaps {boxes[j].name} ({Fmt(bj)})");
                }
            }
            var all = boxes[0].b; foreach (var x in boxes) all.Encapsulate(x.b);
            Check(all.size.x < RoomSpec.W * 0.7f, $"{name}: spread {all.size.x:F1} m of the width");
            Check(all.size.z < RoomSpec.D * 0.7f, $"{name}: spread {all.size.z:F1} m of the depth");
        }
        Assert.IsEmpty(problems, string.Join("\n", problems));
        applier.Apply(0);
    }

    [UnityTest]
    public IEnumerator Menu_SitsAtItsOffsetFromTheLeftHand()
    {
        yield return null;
        var menu = Object.FindObjectsByType<ControllerMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreEqual("Left Controller", menu.transform.parent.name);
        var p = menu.transform.localPosition;
        Assert.AreEqual(0.16f, p.y, 0.001f, "lift survives the Canvas the menu adds in Awake");
        Assert.AreEqual(0.40f, p.z, 0.001f);
    }
}
