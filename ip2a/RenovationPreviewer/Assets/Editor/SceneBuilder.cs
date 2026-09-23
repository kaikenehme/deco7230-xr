using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Deterministic scene construction (IP1 → IP2a). One 9 x 7 m room with a window in
/// Wall_S, a lamp, a wall clock, three preset frames on Wall_W; furniture comes from
/// the presets at Start. Re-runnable: rebuilds Room.unity from scratch each time.
/// </summary>
public static class SceneBuilder
{
    // Room envelope (metres) lives in RoomSpec (runtime) so tests can read it. Everything here derives from it.
    const float RoomW = RoomSpec.W, RoomD = RoomSpec.D, RoomH = RoomSpec.H;
    const float T = 0.1f;   // wall/slab thickness
    const int TeleportLayer = RoomSpec.TeleportLayer;
    // Window in Wall_S: centre offset along the wall, sill height, width, height (metres).
    public const float WindowX = 0.8f, WindowSill = 1.0f, WindowW = 2.0f, WindowH = 1.2f;

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");

        var timber = MakeMat("Timber", new Color(0.545f, 0.353f, 0.169f));
        var offwhite = MakeMat("OffWhite", new Color(0.93f, 0.91f, 0.87f));
        var sofaGrey = MakeMat("SofaGrey", new Color(0.45f, 0.45f, 0.48f));
        var trimWhite = MakeMat("TrimWhite", new Color(0.98f, 0.98f, 0.96f));
        var tableWood = MakeMat("TableWood", new Color(0.35f, 0.24f, 0.15f));

        // --- Room shell. RoomSpec.W x RoomSpec.D footprint, RoomSpec.H ceiling ---
        float hw = RoomW / 2f, hd = RoomD / 2f, hh = RoomH / 2f;
        // Floor and ceiling slabs run under/over the walls to their outer faces: flush W x D slabs left an
        // open 10 cm notch along every edge and the sun drew bright lines through the seams.
        var floor = MakeSurface("Floor", new Vector3(0, -T / 2f, 0), new Vector3(RoomW + 2 * T, T, RoomD + 2 * T), timber, SurfaceState.Keep, SurfaceKind.Floor);
        MakeSurface("Wall_N", new Vector3(0, hh, hd + T / 2f), new Vector3(RoomW, RoomH, T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        // Wall_S carries the window: one logical Surface made of four cubes around the opening.
        var window = new Rect(WindowX, WindowSill, WindowW, WindowH);   // x = centre offset, y = sill height
        MakeWindowWall("Wall_S", new Vector3(0, hh, -(hd + T / 2f)), RoomW, RoomH, T, window, offwhite);
        MakeWindowFrame("WindowFrame", new Vector3(0, 0, -hd + 0.03f), window, 0.06f, 0.06f, trimWhite);
        var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = "Glass";
        glass.transform.position = new Vector3(WindowX, WindowSill + WindowH / 2f, -(hd + T / 2f));
        glass.transform.localScale = new Vector3(WindowW, WindowH, 0.02f);
        glass.GetComponent<Renderer>().sharedMaterial = MakeGlassMat("Glass");
        Object.DestroyImmediate(glass.GetComponent<Collider>());   // the ray sees through it
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.02f, 0f);
        ground.transform.localScale = new Vector3(6f, 1f, 6f);      // 60 x 60 m, so the view out of the window has a horizon
        ground.GetComponent<Renderer>().sharedMaterial = MakeMat("Ground", new Color(0.30f, 0.36f, 0.26f));
        MakeSurface("Wall_E", new Vector3(hw + T / 2f, hh, 0), new Vector3(T, RoomH, RoomD + 2 * T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_W", new Vector3(-(hw + T / 2f), hh, 0), new Vector3(T, RoomH, RoomD + 2 * T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Ceiling", new Vector3(0, RoomH + T / 2f, 0), new Vector3(RoomW + 2 * T, T, RoomD + 2 * T), offwhite, SurfaceState.Change, SurfaceKind.Ceiling);
        MakeSurface("Door", new Vector3(hw - 0.8f, 1.05f, hd - 0.02f), new Vector3(0.9f, 2.1f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);
        MakeSurface("Trim", new Vector3(-hw + 1.2f, 0.075f, hd - 0.02f), new Vector3(2.4f, 0.15f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);

        // Teleport target: only the rig's Teleport Interactors (layer 31) can select it, so a
        // menu click on the floor never teleports.
        var tele = floor.AddComponent<TeleportationArea>();
        tele.interactionLayers = (UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask)(1 << TeleportLayer);

        var floorBounds = new Bounds(Vector3.zero, new Vector3(RoomW, T, RoomD));

        // --- Lamp on side table (diegetic light control, spec §7) ---
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "SideTable";
        table.transform.position = new Vector3(hw - 0.5f, 0.25f, -hd + 0.4f);
        table.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        table.GetComponent<Renderer>().sharedMaterial = tableWood;

        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = "Lamp";
        lamp.transform.position = new Vector3(hw - 0.5f, 0.75f, -hd + 0.4f);
        lamp.transform.localScale = Vector3.one * 0.25f;
        var lampCol = (SphereCollider)lamp.GetComponent<Collider>();
        lampCol.isTrigger = true;
        lampCol.center = new Vector3(0f, -0.4f, 0f);   // local; reaches down over the pull cord
        lampCol.radius = 1.1f;                          // 27 cm in the world
        var lampMat = MakeMat("LampShade", new Color(1f, 0.95f, 0.8f));
        lampMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        lampMat.EnableKeyword("_EMISSION");
        lampMat.SetColor("_EmissionColor", LampController.WarmColor * LampController.SteadyEmission);
        EditorUtility.SetDirty(lampMat);
        AssetDatabase.SaveAssets();   // keyword must be saved before the scene save (see HighlightCarrier)
        lamp.GetComponent<Renderer>().sharedMaterial = lampMat;

        // Pull cord: the visible "touch me" of the lamp (Evaluation 1 §05). Children of a
        // 0.25-scaled sphere, so local sizes are world / 0.25.
        MakePart(lamp, "Cord", new Vector3(0.4f, -0.9f, 0f), new Vector3(0.032f, 0.36f, 0.032f), trimWhite, PrimitiveType.Cylinder);
        MakePart(lamp, "CordKnob", new Vector3(0.4f, -1.3f, 0f), Vector3.one * 0.08f, tableWood, PrimitiveType.Sphere);

        var bulbGo = new GameObject("BulbLight");
        bulbGo.transform.SetParent(lamp.transform, false);
        var bulb = bulbGo.AddComponent<Light>();
        bulb.type = LightType.Point;
        bulb.range = 5f;

        var sunGo = new GameObject("Sun");
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.8f;

        var lampCtrl = lamp.AddComponent<LampController>();
        lampCtrl.bulb = bulb;
        lampCtrl.shade = lamp.GetComponent<Renderer>();

        // --- Wall clock beside the window: touch = next time of day (T7) ---
        var clock = MakeClock(new Vector3(-1.4f, 1.9f, -hd + 0.04f), trimWhite, tableWood);

        // --- Sky + ambient. TimeOfDayController is the only runtime writer; these are the 10:00 defaults
        //     so the saved scene and the first frame agree. ---
        var sky = MakeSkyMat("DaySky");
        RenderSettings.skybox = sky;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = TimeOfDay.Stops[1].ambient;
        ProjectConfigurator.TuneRendering();

        // --- Managers ---
        var managers = new GameObject("Managers");
        var schemeMgr = managers.AddComponent<SchemeManager>();
        var config = managers.AddComponent<RenovationConfig>();
        config.outlineShell = MakeOutlineMat("OutlineShell");

        // Presets: three pre-furnished rooms (Evaluation 1 §05), preset 0 applied at Start.
        var catalogueAsset = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        var applier = managers.AddComponent<PresetApplier>();
        applier.catalogue = catalogueAsset;
        applier.floorBounds = floorBounds;
        applier.presets = PresetAuthoring.Names
            .Select(n => AssetDatabase.LoadAssetAtPath<RoomPreset>(PresetAuthoring.AssetPath(n)))
            .Where(x => x != null).ToArray();
        if (applier.presets.Length == 0) Debug.LogWarning("SceneBuilder: no presets — run Renovation → Build Presets first");
        MakePresetFrames(applier, new Vector3(-hw + 0.03f, 1.5f, 0f), trimWhite);
        var tod = managers.AddComponent<TimeOfDayController>();
        tod.sun = sun;
        tod.sky = sky;
        clock.controller = tod;

        // --- UI event system for the world-space menu (XRI input module) ---
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<XRUIInputModule>();

        // --- Sample prefab ---
        var samplePrefab = BuildSamplePrefab();

        // --- XR rig from XRI Starter Assets ---
        GameObject rig = null;
        var rigPath = Directory.Exists("Assets/Samples")
            ? Directory.GetFiles("Assets/Samples", "XR Origin (XR Rig).prefab", SearchOption.AllDirectories).FirstOrDefault()
            : null;
        if (rigPath != null)
        {
            rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(rigPath));
            rig.transform.position = new Vector3(0.3f, 0, 0.8f);
            rig.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // start facing the kept sofa + floor
            WireControllers(rig, samplePrefab, schemeMgr, floorBounds);

            var onboarding = managers.AddComponent<OnboardingSequence>();
            onboarding.presets = applier;
            onboarding.lamp = lampCtrl;
            onboarding.head = rig.transform.Find("Camera Offset/Main Camera");
        }
        else
        {
            Debug.LogWarning("SceneBuilder: XR Origin prefab not found under Assets/Samples — import XRI Starter Assets, then re-run");
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Room.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Room.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("SceneBuilder: done" + (rig == null ? " (NO RIG)" : ""));
    }

    static GameObject BuildSamplePrefab()
    {
        var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        temp.name = "Sample";
        temp.transform.localScale = Vector3.one * 0.08f;
        var rb = temp.AddComponent<Rigidbody>();
        rb.useGravity = false;
        var grab = temp.AddComponent<XRGrabInteractable>();
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
        grab.throwOnDetach = false;
        grab.attachEaseInTime = 0.15f;   // the peel: sample eases from the tab into the hand
        temp.AddComponent<Sample>();
        temp.AddComponent<HarmonyTuner>();
        temp.AddComponent<HoldUpPreviewer>();
        temp.AddComponent<HarmonyThread>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(temp, "Assets/Prefabs/Sample.prefab");
        Object.DestroyImmediate(temp);
        return prefab;
    }

    static void WireControllers(GameObject rig, GameObject samplePrefab, SchemeManager schemeMgr, Bounds floorBounds)
    {
        // Locked decision (concept §7): controllers, not hand tracking. Strip the
        // hand references from the modality manager so hand tracking can never
        // steal the input modality during a test.
        var modality = rig.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager>(true);
        if (modality != null)
        {
            if (modality.leftHand != null) modality.leftHand.SetActive(false);
            if (modality.rightHand != null) modality.rightHand.SetActive(false);
            modality.leftHand = null;
            modality.rightHand = null;
        }

        var fi = rig.AddComponent<FurnitureInput>();
        fi.leftStick = Vector2Action("LeftStick", "<XRController>{LeftHand}/primary2DAxis");
        fi.rightStick = Vector2Action("RightStick", "<XRController>{RightHand}/primary2DAxis");

        WireHand(rig, "Left Controller", "LeftHand", samplePrefab);
        var right = WireHand(rig, "Right Controller", "RightHand", samplePrefab);

        if (right != null)
        {
            var cycler = right.AddComponent<SchemeCycler>();
            cycler.manager = schemeMgr;
            cycler.saveAction = ButtonAction("SaveScheme", "<XRController>{RightHand}/primaryButton");
            cycler.cycleAction = ButtonAction("CycleScheme", "<XRController>{RightHand}/secondaryButton");
        }

        // --- Controller menu on the left hand, select relay on the right ---
        var left = rig.transform.Find("Camera Offset/Left Controller")?.gameObject;
        var head = rig.transform.Find("Camera Offset/Main Camera");
        var catalogue = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        if (catalogue == null) Debug.LogWarning("SceneBuilder: Assets/Catalogue/Catalogue.asset missing — run Renovation → Import Catalogue first");

        ControllerMenu menu = null;
        if (left != null)
        {
            var menuGo = new GameObject("ControllerMenu", typeof(RectTransform));
            menuGo.transform.SetParent(left.transform, false);
            menuGo.transform.localPosition = new Vector3(0f, 0.26f, 0.16f);   // above and ahead of the left hand, clear of the model (feel pass 22 Sep: was 0.15/0.08, sat on the controller)
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
            relay.closeSameHandAction = ButtonAction("MenuCloseRight", "<XRController>{RightHand}/secondaryButton");
            var cyc = right.GetComponent<SchemeCycler>();
            if (cyc != null) cyc.menu = menu;   // right B closes the menu before it cycles a scheme

            // Editor-only gaze aim: right controller rides the head so mouse-look aims the ray.
            // Disables itself outside the editor; F4 toggles back to the raw simulator.
            var gaze = right.AddComponent<EditorGazeAim>();
            gaze.head = head;
            gaze.poseDriver = right.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();

            // Left hand follows the head too, so the menu hand stays in view; no buttons or twist.
            var leftGaze = left.AddComponent<EditorGazeAim>();
            leftGaze.head = head;
            leftGaze.poseDriver = left.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            leftGaze.ConfigureAsFollower();
            relay.ignoreRoot = rig.transform;
            relay.puller = right.GetComponent<SamplePuller>();

            // Facilitator: hold left X for 1 s → floor sample into the right hand (IP2a Task 1 start).
            var fac = right.AddComponent<FacilitatorSpawn>();
            fac.puller = right.GetComponent<SamplePuller>();
            fac.spawnAction = ButtonAction("FacilitatorSpawn", "<XRController>{LeftHand}/primaryButton");

            // Ray feedback: reticle + glow. The carrier material keeps the _EMISSION variant in the build.
            var fb = right.AddComponent<RayFeedback>();
            fb.menu = menu;
            fb.rayOrigin = relay.rayOrigin;
            fb.ignoreRoot = rig.transform;
            var carrier = MakeMat("HighlightCarrier", Color.white);
            carrier.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            carrier.EnableKeyword("_EMISSION");
            carrier.SetColor("_EmissionColor", new Color(0.949f, 0.702f, 0.239f) * 0.3f);
            EditorUtility.SetDirty(carrier);
            AssetDatabase.SaveAssets();   // a keyword set on a just-created material is lost unless saved before the scene save
            fb.emissionCarrier = carrier;
        }
    }

    static GameObject WireHand(GameObject rig, string controllerName, string handUsage, GameObject samplePrefab)
    {
        var t = rig.transform.Find($"Camera Offset/{controllerName}");
        if (t == null)
        {
            Debug.LogWarning($"SceneBuilder: '{controllerName}' not found in rig hierarchy");
            return null;
        }
        var go = t.gameObject;

        var col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.06f;
        // MarkTool's RequireComponent adds the kinematic Rigidbody
        var mark = go.AddComponent<MarkTool>();
        mark.gripAction = ValueAction($"{handUsage}Grip", $"<XRController>{{{handUsage}}}/grip");

        var puller = go.AddComponent<SamplePuller>();
        puller.samplePrefab = samplePrefab;
        puller.triggerAction = ButtonAction($"{handUsage}Trigger", $"<XRController>{{{handUsage}}}/triggerPressed");
        puller.interactor = go.GetComponentInChildren<NearFarInteractor>() as XRBaseInteractor
                            ?? go.GetComponentInChildren<XRBaseInteractor>();
        if (puller.interactor == null)
            Debug.LogWarning($"SceneBuilder: no interactor found under '{controllerName}'");

        go.AddComponent<HandGlow>();
        return go;
    }

    static InputActionProperty ValueAction(string name, string path) =>
        new(new InputAction(name, InputActionType.Value, path, expectedControlType: "Axis"));

    static InputActionProperty ButtonAction(string name, string path) =>
        new(new InputAction(name, InputActionType.Button, path));

    static InputActionProperty Vector2Action(string name, string path) =>
        new(new InputAction(name, InputActionType.Value, path, expectedControlType: "Vector2"));

    static Material MakeMat(string name, Color c)
    {
        var path = $"Assets/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) { existing.color = c; return existing; }
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = c };
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

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
        go.AddComponent<PullAffordance>();
        return go;
    }

    /// <summary>One logical wall (Surface on the root) built from four cubes around an opening.
    /// opening.x = centre offset along the wall, opening.y = sill height above the floor.</summary>
    /// <summary>Three framed pictures on Wall_W (a left head-turn from the start pose); touch one to dress the room.</summary>
    static void MakePresetFrames(PresetApplier applier, Vector3 wallInside, Material frameMat)
    {
        for (int i = 0; i < applier.presets.Length; i++)
        {
            var preset = applier.presets[i];
            var root = new GameObject($"PresetFrame_{i}");
            root.transform.position = wallInside + new Vector3(0f, 0f, (i - (applier.presets.Length - 1) / 2f) * 1.5f);
            var backing = MakePart(root, "Frame", new Vector3(0f, 0f, 0f), new Vector3(0.05f, 0.50f, 0.60f), frameMat, keepCollider: true);
            var picture = MakePart(root, "Picture", new Vector3(0.03f, 0f, 0f), new Vector3(0.01f, 0.40f, 0.50f), frameMat);
            var cardName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(preset));
            var cardMat = AssetDatabase.LoadAssetAtPath<Material>(PresetAuthoring.CardMaterialPath(cardName));
            if (cardMat != null) picture.GetComponent<Renderer>().sharedMaterial = cardMat;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0.04f, -0.34f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);   // readable from inside the room (+X side)
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = preset.displayName;
            tm.font = UiKit.Font;
            tm.fontSize = 64;
            tm.characterSize = 0.018f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.10f, 0.10f, 0.10f);
            labelGo.GetComponent<MeshRenderer>().sharedMaterial = UiKit.Font.material;

            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.3f;
            var pf = root.AddComponent<PresetFrame>();
            pf.applier = applier;
            pf.index = i;
            pf.frame = backing.GetComponent<Renderer>();
        }
    }

    static Material MakeOutlineMat(string name)
    {
        var path = $"Assets/Materials/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(m, path); }
        SelectionOutline.ConfigureShellMaterial(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject MakeWindowWall(string name, Vector3 centre, float width, float height, float thickness, Rect opening, Material m)
    {
        var root = new GameObject(name);
        root.transform.position = centre;
        float hw = width / 2f, yBase = -height / 2f;
        float leftW = opening.x - opening.width / 2f + hw;
        float rightW = hw - (opening.x + opening.width / 2f);
        float headH = height - (opening.y + opening.height);
        MakePart(root, "JambL", new Vector3(-hw + leftW / 2f, 0f, 0f), new Vector3(leftW, height, thickness), m, keepCollider: true);
        MakePart(root, "JambR", new Vector3(hw - rightW / 2f, 0f, 0f), new Vector3(rightW, height, thickness), m, keepCollider: true);
        MakePart(root, "Sill", new Vector3(opening.x, yBase + opening.y / 2f, 0f), new Vector3(opening.width, opening.y, thickness), m, keepCollider: true);
        MakePart(root, "Lintel", new Vector3(opening.x, height / 2f - headH / 2f, 0f), new Vector3(opening.width, headH, thickness), m, keepCollider: true);
        var s = root.AddComponent<Surface>();
        s.SetState(SurfaceState.Change);
        s.SetKind(SurfaceKind.Wall);
        root.AddComponent<MenuTarget>();
        root.AddComponent<PullAffordance>();
        return root;
    }

    /// <summary>Four bars around the opening on the room side of the wall; a paintable Trim surface.</summary>
    static GameObject MakeWindowFrame(string name, Vector3 wallInside, Rect opening, float bar, float depth, Material m)
    {
        var root = new GameObject(name);
        root.transform.position = wallInside;
        float cx = opening.x, cy = opening.y + opening.height / 2f, w = opening.width + bar, h = opening.height + bar;
        MakePart(root, "Top", new Vector3(cx, cy + h / 2f, 0f), new Vector3(w + bar, bar, depth), m, keepCollider: true);
        MakePart(root, "Bottom", new Vector3(cx, cy - h / 2f, 0f), new Vector3(w + bar, bar, depth), m, keepCollider: true);
        MakePart(root, "Left", new Vector3(cx - w / 2f, cy, 0f), new Vector3(bar, h, depth), m, keepCollider: true);
        MakePart(root, "Right", new Vector3(cx + w / 2f, cy, 0f), new Vector3(bar, h, depth), m, keepCollider: true);
        var s = root.AddComponent<Surface>();
        s.SetState(SurfaceState.Change);
        s.SetKind(SurfaceKind.Trim);
        root.AddComponent<MenuTarget>();
        root.AddComponent<PullAffordance>();
        return root;
    }

    /// <summary>Primitive wall clock facing +Z: face disc, rim, two hands on pivots, trigger sphere.</summary>
    static WallClock MakeClock(Vector3 pos, Material faceMat, Material darkMat)
    {
        var root = new GameObject("WallClock");
        root.transform.position = pos;
        var rim = MakePart(root, "Rim", new Vector3(0f, 0f, -0.005f), new Vector3(0.40f, 0.012f, 0.40f), darkMat, PrimitiveType.Cylinder);
        rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var face = MakePart(root, "Face", new Vector3(0f, 0f, 0.008f), new Vector3(0.36f, 0.006f, 0.36f), faceMat, PrimitiveType.Cylinder);
        face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var hourPivot = new GameObject("HourHand").transform; hourPivot.SetParent(root.transform, false); hourPivot.localPosition = new Vector3(0f, 0f, 0.022f);
        MakePart(hourPivot.gameObject, "Hand", new Vector3(0f, 0.045f, 0f), new Vector3(0.014f, 0.10f, 0.006f), darkMat);
        var minutePivot = new GameObject("MinuteHand").transform; minutePivot.SetParent(root.transform, false); minutePivot.localPosition = new Vector3(0f, 0f, 0.030f);
        MakePart(minutePivot.gameObject, "Hand", new Vector3(0f, 0.065f, 0f), new Vector3(0.008f, 0.14f, 0.006f), darkMat);
        var col = root.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.22f;
        var clock = root.AddComponent<WallClock>();
        clock.hourHand = hourPivot;
        clock.minuteHand = minutePivot;
        clock.face = face.GetComponent<Renderer>();
        return clock;
    }

    static Material MakeGlassMat(string name)
    {
        var path = $"Assets/Materials/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.SetFloat("_Surface", 1f);   // transparent
        m.SetFloat("_Blend", 0f);     // alpha
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        m.SetOverrideTag("RenderType", "Transparent");
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
        m.color = new Color(0.80f, 0.90f, 1f, 0.15f);
        m.SetFloat("_Smoothness", 0.9f);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject MakePart(GameObject parent, string name, Vector3 pos, Vector3 scale, Material m, PrimitiveType type = PrimitiveType.Cube, bool keepCollider = false)
    {
        var p = GameObject.CreatePrimitive(type);
        p.name = name;
        p.transform.SetParent(parent.transform, false);
        p.transform.localPosition = pos;
        p.transform.localScale = scale;
        p.GetComponent<Renderer>().sharedMaterial = m;
        if (!keepCollider) Object.DestroyImmediate(p.GetComponent<Collider>());
        return p;
    }

    static Material MakeSkyMat(string name)
    {
        var path = $"Assets/Materials/{name}.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        var m = new Material(Shader.Find("Skybox/Procedural"));
        m.SetFloat("_SunSize", 0.04f);
        m.SetFloat("_AtmosphereThickness", 1.0f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }
}
