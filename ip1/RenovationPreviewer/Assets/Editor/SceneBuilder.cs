using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Deterministic scene construction for IP1 (spec §7: one room; keeping timber
/// floor + sofa; changing 4 walls, ceiling, trim, door). Re-runnable: rebuilds
/// Room.unity from scratch each time.
/// </summary>
public static class SceneBuilder
{
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
        MakeSurface("Floor", new Vector3(0, -0.05f, 0), new Vector3(4, 0.1f, 3), timber, SurfaceState.Keep);
        MakeSurface("Wall_N", new Vector3(0, 1.35f, 1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_S", new Vector3(0, 1.35f, -1.55f), new Vector3(4, 2.7f, 0.1f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_E", new Vector3(2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change);
        MakeSurface("Wall_W", new Vector3(-2.05f, 1.35f, 0), new Vector3(0.1f, 2.7f, 3.2f), offwhite, SurfaceState.Change);
        MakeSurface("Ceiling", new Vector3(0, 2.75f, 0), new Vector3(4, 0.1f, 3), offwhite, SurfaceState.Change);
        MakeSurface("Door", new Vector3(1.2f, 1.05f, 1.48f), new Vector3(0.9f, 2.1f, 0.06f), trimWhite, SurfaceState.Change);
        MakeSurface("Trim", new Vector3(-0.8f, 0.075f, 1.48f), new Vector3(2.4f, 0.15f, 0.06f), trimWhite, SurfaceState.Change);

        // --- Sofa: kept prop, one Surface for the whole compound ---
        var sofa = new GameObject("Sofa");
        MakePart(sofa, "Seat", new Vector3(-1.2f, 0.25f, -0.9f), new Vector3(1.8f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "Back", new Vector3(-1.2f, 0.65f, -1.25f), new Vector3(1.8f, 0.8f, 0.2f), sofaGrey);
        MakePart(sofa, "ArmL", new Vector3(-2.05f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        MakePart(sofa, "ArmR", new Vector3(-0.35f, 0.45f, -0.9f), new Vector3(0.2f, 0.5f, 0.8f), sofaGrey);
        var sofaSurf = sofa.AddComponent<Surface>();
        sofaSurf.SetState(SurfaceState.Keep);
        var sofaCol = sofa.AddComponent<BoxCollider>();
        sofaCol.center = new Vector3(-1.2f, 0.5f, -1.0f);
        sofaCol.size = new Vector3(1.95f, 1.1f, 1.1f);

        // --- Alternate material display: framed second timber tone on Wall_S,
        //     present but static (spec §7 "present but shallow") ---
        var altFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altFrame.name = "AltMaterialFrame";
        altFrame.transform.position = new Vector3(1.2f, 1.4f, -1.48f);
        altFrame.transform.localScale = new Vector3(0.55f, 0.55f, 0.04f);
        altFrame.GetComponent<Renderer>().sharedMaterial = trimWhite;
        Object.DestroyImmediate(altFrame.GetComponent<Collider>());
        var altSwatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        altSwatch.name = "AltMaterialSwatch";
        altSwatch.transform.position = new Vector3(1.2f, 1.4f, -1.46f);
        altSwatch.transform.localScale = new Vector3(0.45f, 0.45f, 0.04f);
        altSwatch.GetComponent<Renderer>().sharedMaterial = timberAlt;
        Object.DestroyImmediate(altSwatch.GetComponent<Collider>());

        // --- Lamp on side table (diegetic light control, spec §7) ---
        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "SideTable";
        table.transform.position = new Vector3(1.5f, 0.25f, -1.1f);
        table.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        table.GetComponent<Renderer>().sharedMaterial = tableWood;

        var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lamp.name = "Lamp";
        lamp.transform.position = new Vector3(1.5f, 0.75f, -1.1f);
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
            rig.transform.position = new Vector3(0.5f, 0, 0.3f);
            WireControllers(rig, samplePrefab, schemeMgr);
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

    static void WireControllers(GameObject rig, GameObject samplePrefab, SchemeManager schemeMgr)
    {
        WireHand(rig, "Left Controller", "LeftHand", samplePrefab);
        var right = WireHand(rig, "Right Controller", "RightHand", samplePrefab);

        if (right != null)
        {
            var cycler = right.AddComponent<SchemeCycler>();
            cycler.manager = schemeMgr;
            cycler.saveAction = ButtonAction("SaveScheme", "<XRController>{RightHand}/primaryButton");
            cycler.cycleAction = ButtonAction("CycleScheme", "<XRController>{RightHand}/secondaryButton");
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

    static GameObject MakeSurface(string name, Vector3 pos, Vector3 scale, Material m, SurfaceState state)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
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
