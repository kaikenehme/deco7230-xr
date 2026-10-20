using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
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
    public static readonly Vector3 MenuOffset = new(0f, 0.16f, 0.40f);   // controller menu, left-hand local
    const int TeleportLayer = RoomSpec.TeleportLayer;
    // Window in Wall_S: centre offset along the wall, sill height, width, height (metres).
    public const float WindowX = 0.3f, WindowSill = 0.6f, WindowW = 2.2f, WindowH = 1.7f;
    public const float StartYaw = 235f;   // start pose looks at the sofa corner: sofa, arc lamp, window

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");

        var timber = MakeMat("Timber", new Color(0.545f, 0.353f, 0.169f));
        var offwhite = MakeMat("OffWhite", Hex("#DCD1BF"));   // warm white walls (IP2b)
        var ceilingMat = MakeMat("Ceiling", Hex("#EBE6DC"));
        var oak = MakeMat("Oak", Hex("#8C6239"));
        var sofaGrey = MakeMat("SofaGrey", new Color(0.45f, 0.45f, 0.48f));
        var trimWhite = MakeMat("TrimWhite", new Color(0.98f, 0.98f, 0.96f));
        var tableWood = MakeMat("TableWood", new Color(0.35f, 0.24f, 0.15f));

        // --- Room shell. RoomSpec.W x RoomSpec.D footprint, RoomSpec.H ceiling ---
        float hw = RoomW / 2f, hd = RoomD / 2f, hh = RoomH / 2f;
        // Floor and ceiling slabs run under/over the walls to their outer faces: flush W x D slabs left an
        // open 10 cm notch along every edge and the sun drew bright lines through the seams.
        var floor = MakeSurface("Floor", new Vector3(0, -T / 2f, 0), new Vector3(RoomW + 2 * T, T, RoomD + 2 * T), timber, SurfaceState.Change, SurfaceKind.Floor);
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
        MakeSurface("Ceiling", new Vector3(0, RoomH + T / 2f, 0), new Vector3(RoomW + 2 * T, T, RoomD + 2 * T), ceilingMat, SurfaceState.Change, SurfaceKind.Ceiling);
        MakeSurface("Door", new Vector3(hw - 0.8f, 1.05f, hd - 0.02f), new Vector3(0.9f, 2.1f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);
        MakeSurface("Trim", new Vector3(-hw + 1.2f, 0.075f, hd - 0.02f), new Vector3(2.4f, 0.15f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);

        // Teleport target: only the rig's Teleport Interactors (layer 31) can select it, so a
        // menu click on the floor never teleports.
        var tele = floor.AddComponent<TeleportationArea>();
        tele.interactionLayers = (UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask)(1 << TeleportLayer);

        var floorBounds = new Bounds(Vector3.zero, new Vector3(RoomW, T, RoomD));

        // --- Arc floor lamp in the SW corner beside the sofa (diegetic light control, spec §7) ---
        var catalogueForShell = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        var arcRoot = new GameObject("ArcLamp");
        arcRoot.transform.position = new Vector3(-2.35f, 0f, -2.05f);
        var shadePos = arcRoot.transform.position + new Vector3(0f, 1.6f, 0f);
        var arcPrefab = catalogueForShell != null ? catalogueForShell.Furniture("arc_floor_lamp")?.prefab : null;
        if (arcPrefab != null)
        {
            var arc = (GameObject)PrefabUtility.InstantiatePrefab(arcPrefab, arcRoot.transform);
            arc.name = "arc_floor_lamp";
            var ab = Bounds(arc);
            shadePos = new Vector3(ab.center.x, ab.max.y - 0.2f, ab.max.z - 0.17f);   // shade hangs at the +Z end of the arc
        }
        else Debug.LogWarning("SceneBuilder: arc_floor_lamp not in catalogue — run Import Catalogue");

        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = "Lamp";
        lamp.transform.SetParent(arcRoot.transform, false);
        lamp.transform.position = shadePos;            // glowing bulb inside the brass shade
        lamp.transform.localScale = Vector3.one * 0.2f;
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
        var clock = MakeClock(new Vector3(-2.15f, 1.9f, -hd + 0.04f), trimWhite, tableWood);   // left of the curtains
        BuildDressing(catalogueForShell, hw, hd, oak, trimWhite);

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

        // Presets: Living, Bedroom and the bare Empty room; preset 0 applied at Start.
        var catalogueAsset = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        var applier = managers.AddComponent<PresetApplier>();
        applier.catalogue = catalogueAsset;
        applier.floorBounds = floorBounds;
        applier.presets = PresetAuthoring.Names
            .Select(n => AssetDatabase.LoadAssetAtPath<RoomPreset>(PresetAuthoring.AssetPath(n)))
            .Where(x => x != null).ToArray();
        if (applier.presets.Length == 0) Debug.LogWarning("SceneBuilder: no presets — run Renovation → Build Presets first");
        MakePresetFrames(applier, new Vector3(-0.5f, 1.5f, hd - 0.03f), trimWhite);   // Wall_N, behind the start pose's right shoulder
        var tod = managers.AddComponent<TimeOfDayController>();
        tod.sun = sun;
        tod.sky = sky;
        var viewGo = GameObject.Find("WindowView");
        if (viewGo != null) tod.windowView = viewGo.GetComponent<Renderer>();
        var curtainsGo = GameObject.Find("Curtains");
        if (curtainsGo != null) curtainsGo.GetComponent<CurtainController>().timeOfDay = tod;
        clock.controller = tod;
        // Empty room (bare preset): these hide, and the room opens in daylight.
        applier.timeOfDay = tod;
        applier.dressing = new[] { "Rug", "Art_0", "Art_1", "ArcLamp" }
            .Select(n => { var g = GameObject.Find(n); if (g == null) Debug.LogWarning($"SceneBuilder: dressing '{n}' not found"); return g; })
            .Where(g => g != null).ToArray();

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
            rig.transform.position = new Vector3(0.9f, 0, 1.3f);
            rig.transform.rotation = Quaternion.Euler(0f, StartYaw, 0f);
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
        temp.AddComponent<FanDeck>();   // IP2b: fan deck replaces twist-to-tune
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
            menu = menuGo.AddComponent<ControllerMenu>();
            // A Canvas RectTransform serializes anchoredPosition, not localPosition x/y, so a plain
            // localPosition lift was dropped on save (the 22 Sep 0.26 m never applied). Set it after
            // AddComponent (Awake adds the Canvas). Out ahead of the left hand: ~0.8 m from the eye,
            // eye level in the simulator.
            ((RectTransform)menuGo.transform).anchoredPosition3D = MenuOffset;
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
            // Editor-only desktop mode (no headset for IP2a): free cursor aims the right ray, right-drag
            // looks, WASD walks. F4 returns to the raw simulator.
            var desk = rig.AddComponent<EditorDesktopRig>();
            desk.head = head;
            if (menu != null) desk.menu = (RectTransform)menu.transform;
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
    /// <summary>One framed picture per preset on Wall_N; touch one to dress the room.</summary>
    static void MakePresetFrames(PresetApplier applier, Vector3 wallInside, Material frameMat)
    {
        for (int i = 0; i < applier.presets.Length; i++)
        {
            var preset = applier.presets[i];
            var root = new GameObject($"PresetFrame_{i}");
            root.transform.rotation = Quaternion.Euler(0f, 90f, 0f);   // built facing +X, turned to face into the room from Wall_N (-Z)
            root.transform.position = wallInside + new Vector3((i - (applier.presets.Length - 1) / 2f) * 0.9f, 0f, 0f);   // three frames: x ≈ -1.4, -0.5, 0.4
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

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    static Bounds Bounds(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        var b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
        return b;
    }

    static GameObject Place(Catalogue cat, string id, string name, Vector3 pos, float yaw)
    {
        var prefab = cat != null ? cat.Furniture(id)?.prefab : null;
        if (prefab == null) { Debug.LogWarning($"SceneBuilder: {id} not in catalogue — run Import Catalogue"); return new GameObject(name); }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = name;
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, yaw, 0f));
        return go;
    }

    /// <summary>IP2b dressing that belongs to the room, not a preset: garden view, beam, art, curtains, pendant.</summary>
    static void BuildDressing(Catalogue cat, float hw, float hd, Material oak, Material trim)
    {
        // Garden photo (Poly Haven "ballawley_park", CC0, tonemapped + cropped) outside the window.
        var view = GameObject.CreatePrimitive(PrimitiveType.Quad);
        view.name = "WindowView";
        Object.DestroyImmediate(view.GetComponent<Collider>());
        view.transform.position = new Vector3(WindowX, WindowSill + WindowH / 2f + 0.2f, -hd - 2.5f);
        view.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // quad faces -Z by default; turn it to face the room
        view.transform.localScale = new Vector3(7f, 3.5f, 1f);
        var viewMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WindowView.mat");
        if (viewMat == null) { viewMat = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(viewMat, "Assets/Materials/WindowView.mat"); }
        viewMat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/WindowView.jpg"));
        viewMat.mainTexture = viewMat.GetTexture("_BaseMap");
        EditorUtility.SetDirty(viewMat);
        view.GetComponent<Renderer>().sharedMaterial = viewMat;

        // Round jute rug between sofa and table (a pull-only source: furniture-like, never painted).
        var rug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rug.name = "Rug";
        rug.transform.position = new Vector3(-0.9f, 0.006f, 0.2f);
        rug.transform.localScale = new Vector3(2.4f, 0.006f, 3.0f);   // 2.4 x 3.0 m oval, long axis along the sofa
        rug.GetComponent<Renderer>().sharedMaterial = MakeMat("Jute", Hex("#B79A6E"));
        Object.DestroyImmediate(rug.GetComponent<Collider>());
        rug.AddComponent<BoxCollider>();   // flat box: the hand finds it, the floor ray isn't blocked by a capsule
        var rugSurface = rug.AddComponent<Surface>();
        rugSurface.SetState(SurfaceState.Keep);
        rugSurface.SetKind(SurfaceKind.None);
        rugSurface.SetBaseSampleColor(Hex("#B79A6E"));
        rug.AddComponent<MenuTarget>();
        rug.AddComponent<PullAffordance>();

        // Oak ceiling beam across the room (the timber warmth of reference F).
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "Beam";
        Object.DestroyImmediate(beam.GetComponent<Collider>());
        beam.transform.position = new Vector3(0f, RoomH - 0.11f, 0.2f);
        beam.transform.localScale = new Vector3(RoomW, 0.22f, 0.18f);
        beam.GetComponent<Renderer>().sharedMaterial = oak;

        // Two abstract prints above the sofa on Wall_W (room side faces +X).
        MakeArt("Art_0", new Vector3(-hw + 0.03f, 1.65f, -0.35f), oak, Hex("#EEE7DA"), Hex("#B5603F"), Hex("#8E9A7B"));
        MakeArt("Art_1", new Vector3(-hw + 0.03f, 1.65f, 0.75f), oak, Hex("#E9E1D2"), Hex("#C9A06A"), Hex("#5A4E45"));

        // Curtains either side of the window, on a brass rail.
        float left = WindowX - WindowW / 2f, right = WindowX + WindowW / 2f, cz = -hd + 0.14f;
        var curtains = new GameObject("Curtains");
        var cl = Place(cat, "linen_curtain", "Curtain_L", new Vector3(left - 0.4f, 0f, cz), 0f);
        var cr = Place(cat, "linen_curtain", "Curtain_R", new Vector3(right + 0.4f, 0f, cz), 0f);
        var curtainCtl = curtains.AddComponent<CurtainController>();
        curtainCtl.left = cl.transform; curtainCtl.right = cr.transform; curtainCtl.windowCentreX = WindowX;
        foreach (var panel in new[] { cl, cr })
        {
            panel.transform.SetParent(curtains.transform, true);
            var box = panel.AddComponent<BoxCollider>();   // touch target: the whole drape
            var pb = Bounds(panel);
            box.center = panel.transform.InverseTransformPoint(pb.center);
            box.size = new Vector3(pb.size.x, pb.size.y, 0.25f);
            box.isTrigger = true;
            panel.AddComponent<CurtainPanel>().controller = curtainCtl;
        }
        var rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rail.name = "CurtainRail";
        Object.DestroyImmediate(rail.GetComponent<Collider>());
        rail.transform.position = new Vector3(WindowX, 2.55f, cz);
        rail.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        rail.transform.localScale = new Vector3(0.025f, (right - left) / 2f + 0.9f, 0.025f);
        rail.GetComponent<Renderer>().sharedMaterial = MakeMat("Brass", Hex("#B8913F"));

        // Woven pendant over the coffee table, cord to the ceiling.
        var pendant = Place(cat, "woven_pendant", "Pendant", new Vector3(-0.9f, 1.75f, 0.2f), 0f);
        float top = 1.75f + 0.31f;
        var cord = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cord.name = "Cord";
        Object.DestroyImmediate(cord.GetComponent<Collider>());
        cord.transform.SetParent(pendant.transform, false);
        cord.transform.position = new Vector3(-0.9f, (top + RoomH) / 2f, 0.2f);
        cord.transform.localScale = new Vector3(0.008f, (RoomH - top) / 2f, 0.008f);
        cord.GetComponent<Renderer>().sharedMaterial = MakeMat("Cord", Hex("#1E1E1E"));
        // Warm light in the shade so a room with the curtains shut is still lit (N4).
        var pl = new GameObject("PendantLight").AddComponent<Light>();
        pl.transform.SetParent(pendant.transform, false);
        pl.transform.position = new Vector3(-0.9f, 1.85f, 0.2f);
        pl.type = LightType.Point; pl.color = LampController.WarmColor; pl.intensity = 0.8f; pl.range = 4f;
        // Touchable switch (Kaike's lap): a trigger sphere round the shade.
        var pcol = pendant.AddComponent<SphereCollider>();
        pcol.isTrigger = true;
        pcol.center = pendant.transform.InverseTransformPoint(new Vector3(-0.9f, 1.9f, 0.2f));
        pcol.radius = 0.3f;
        pendant.AddComponent<PendantLight>().bulb = pl;
    }

    /// <summary>Framed abstract print facing +X: oak frame, canvas, a circle and a block.</summary>
    static void MakeArt(string name, Vector3 pos, Material frame, Color canvas, Color circle, Color block)
    {
        var root = new GameObject(name);
        root.transform.position = pos;
        MakePart(root, "Frame", Vector3.zero, new Vector3(0.03f, 1.1f, 0.9f), frame);
        MakePart(root, "Canvas", new Vector3(0.016f, 0f, 0f), new Vector3(0.005f, 1.02f, 0.82f), MakeMat(name + "_Canvas", canvas));
        var c = MakePart(root, "Circle", new Vector3(0.02f, 0.15f, -0.08f), new Vector3(0.5f, 0.004f, 0.5f), MakeMat(name + "_Circle", circle), PrimitiveType.Cylinder);
        c.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        MakePart(root, "Block", new Vector3(0.022f, -0.25f, 0.08f), new Vector3(0.005f, 0.28f, 0.5f), MakeMat(name + "_Block", block));
    }
}
