using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Deterministic scene construction for IP1 (spec §7: one room; keeping timber
/// floor + sofa; changing 4 walls, ceiling, trim, door). Re-runnable: rebuilds
/// Room.unity from scratch each time.
/// </summary>
public static class SceneBuilder
{
    // Room envelope (metres) lives in RoomSpec (runtime) so tests can read it. Everything here derives from it.
    const float RoomW = RoomSpec.W, RoomD = RoomSpec.D, RoomH = RoomSpec.H;
    const float T = 0.1f;   // wall/slab thickness
    const int TeleportLayer = RoomSpec.TeleportLayer;

    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Materials");
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");

        var timber = MakeMat("Timber", new Color(0.545f, 0.353f, 0.169f));
        var timberAlt = MakeMat("TimberAlt", new Color(0.42f, 0.30f, 0.20f));
        var offwhite = MakeMat("OffWhite", new Color(0.93f, 0.91f, 0.87f));
        var sofaGrey = MakeMat("SofaGrey", new Color(0.45f, 0.45f, 0.48f));
        var trimWhite = MakeMat("TrimWhite", new Color(0.98f, 0.98f, 0.96f));
        var tableWood = MakeMat("TableWood", new Color(0.35f, 0.24f, 0.15f));

        // --- Room shell. 4m x 3m footprint, 2.7m ceiling ---
        float hw = RoomW / 2f, hd = RoomD / 2f, hh = RoomH / 2f;
        var floor = MakeSurface("Floor", new Vector3(0, -T / 2f, 0), new Vector3(RoomW, T, RoomD), timber, SurfaceState.Keep, SurfaceKind.Floor);
        MakeSurface("Wall_N", new Vector3(0, hh, hd + T / 2f), new Vector3(RoomW, RoomH, T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_S", new Vector3(0, hh, -(hd + T / 2f)), new Vector3(RoomW, RoomH, T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_E", new Vector3(hw + T / 2f, hh, 0), new Vector3(T, RoomH, RoomD + 2 * T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Wall_W", new Vector3(-(hw + T / 2f), hh, 0), new Vector3(T, RoomH, RoomD + 2 * T), offwhite, SurfaceState.Change, SurfaceKind.Wall);
        MakeSurface("Ceiling", new Vector3(0, RoomH + T / 2f, 0), new Vector3(RoomW, T, RoomD), offwhite, SurfaceState.Change, SurfaceKind.Ceiling);
        MakeSurface("Door", new Vector3(hw - 0.8f, 1.05f, hd - 0.02f), new Vector3(0.9f, 2.1f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);
        MakeSurface("Trim", new Vector3(-hw + 1.2f, 0.075f, hd - 0.02f), new Vector3(2.4f, 0.15f, 0.06f), trimWhite, SurfaceState.Change, SurfaceKind.Trim);

        // Teleport target: only the rig's Teleport Interactors (layer 31) can select it, so a
        // menu click on the floor never teleports.
        var tele = floor.AddComponent<TeleportationArea>();
        tele.interactionLayers = (UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask)(1 << TeleportLayer);

        // --- Sofa: kept prop (sample source) AND a furniture slot (swap/move) ---
        var sofa = new GameObject("Sofa");
        sofa.transform.position = new Vector3(-hw + 1.3f, 0f, -hd + 0.6f);
        MakePart(sofa, "Seat", new Vector3(0f, 0.25f, 0f), new Vector3(1.8f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "Back", new Vector3(0f, 0.65f, -0.35f), new Vector3(1.8f, 0.8f, 0.2f), sofaGrey);
        MakePart(sofa, "ArmL", new Vector3(-0.85f, 0.45f, 0f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "ArmR", new Vector3(0.85f, 0.45f, 0f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
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
        var floorBounds = new Bounds(Vector3.zero, new Vector3(RoomW, T, RoomD));
        var sofaSlot = sofa.AddComponent<FurnitureSlot>();
        sofaSlot.BindGrab(floorBounds);
        sofa.AddComponent<MenuTarget>();

        // --- Alternate material display: framed second timber tone on Wall_S,
        //     present but static (spec §7 "present but shallow") ---
        var altFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altFrame.name = "AltMaterialFrame";
        altFrame.transform.position = new Vector3(hw - 1.3f, 1.4f, -(hd - 0.02f));
        altFrame.transform.localScale = new Vector3(0.55f, 0.55f, 0.04f);
        altFrame.GetComponent<Renderer>().sharedMaterial = trimWhite;
        Object.DestroyImmediate(altFrame.GetComponent<Collider>());
        var altSwatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altSwatch.name = "AltMaterialSwatch";
        altSwatch.transform.position = new Vector3(hw - 1.3f, 1.4f, -(hd - 0.04f));
        altSwatch.transform.localScale = new Vector3(0.45f, 0.45f, 0.04f);
        altSwatch.GetComponent<Renderer>().sharedMaterial = timberAlt;
        Object.DestroyImmediate(altSwatch.GetComponent<Collider>());

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
        lamp.GetComponent<Collider>().isTrigger = true;
        var lampMat = MakeMat("LampShade", new Color(1f, 0.95f, 0.8f));
        lamp.GetComponent<Renderer>().sharedMaterial = lampMat;

        var bulbGo = new GameObject("BulbLight");
        bulbGo.transform.SetParent(lamp.transform, false);
        var bulb = bulbGo.AddComponent<Light>();
        bulb.type = LightType.Point;
        bulb.range = 6f;

        var sunGo = new GameObject("Sun");
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sunGo.transform.rotation = Quaternion.Euler(50, -30, 0);

        var lampCtrl = lamp.AddComponent<LampController>();
        lampCtrl.sun = sun;
        lampCtrl.bulb = bulb;

        // --- Managers ---
        var managers = new GameObject("Managers");
        var schemeMgr = managers.AddComponent<SchemeManager>();

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
        temp.AddComponent<Sample>();
        temp.AddComponent<HarmonyTuner>();
        temp.AddComponent<HoldUpPreviewer>();
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
            menuGo.transform.localPosition = new Vector3(0f, 0.15f, 0.08f);   // above the left hand, facing the eyes
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
            relay.ignoreRoot = rig.transform;

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

        return go;
    }

    static InputActionProperty ValueAction(string name, string path) =>
        new(new InputAction(name, InputActionType.Value, path, expectedControlType: "Axis"));

    static InputActionProperty ButtonAction(string name, string path) =>
        new(new InputAction(name, InputActionType.Button, path));

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
        return go;
    }

    static void MakePart(GameObject parent, string name, Vector3 pos, Vector3 scale, Material m)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = name;
        p.transform.SetParent(parent.transform, false);
        p.transform.localPosition = pos;
        p.transform.localScale = scale;
        p.GetComponent<Renderer>().sharedMaterial = m;
        Object.DestroyImmediate(p.GetComponent<Collider>());
    }
}
