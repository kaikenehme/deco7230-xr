using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

public static class ProjectConfigurator
{
    public static void ConfigureAll()
    {
        SetupUrp();
        SetupPlayerAndAndroid();
        SetupXr(BuildTargetGroup.Android);
        SetupXr(BuildTargetGroup.Standalone);
        ImportXriSamples();
        SetupInputForSimulator();
        AssetDatabase.SaveAssets();
        Debug.Log("ProjectConfigurator: done");
    }

    static void SetupUrp()
    {
        Directory.CreateDirectory("Assets/Settings");
        if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-Asset.asset") != null)
        {
            Debug.Log("ProjectConfigurator: URP assets already exist, skipping");
            return;
        }
        var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(renderer, "Assets/Settings/URP-Renderer.asset");
        var pipeline = UniversalRenderPipelineAsset.Create(renderer);
        AssetDatabase.CreateAsset(pipeline, "Assets/Settings/URP-Asset.asset");
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        Debug.Log("ProjectConfigurator: URP configured");
    }

    /// <summary>
    /// Editor-only input behaviour: keyboard and mouse reach the game whichever editor panel
    /// has focus. Without this the Input System default needs the Game view focused, and a
    /// console log or a CLI command stealing focus silently kills every simulator key
    /// (feel pass 22 Sep: "T / Y / G not working", twice). No effect on device.
    /// </summary>
    public static void SetupInputForSimulator()
    {
        const string path = "Assets/Settings/InputSystem.inputsettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputSettings>(path);
        if (settings == null)
        {
            Directory.CreateDirectory("Assets/Settings");
            settings = ScriptableObject.CreateInstance<UnityEngine.InputSystem.InputSettings>();
            AssetDatabase.CreateAsset(settings, path);
        }
        settings.editorInputBehaviorInPlayMode = UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        EditorUtility.SetDirty(settings);
        UnityEngine.InputSystem.InputSystem.settings = settings;
        AssetDatabase.SaveAssets();
        Debug.Log("ProjectConfigurator: input settings → all device input goes to the Game view in play mode");
    }

    /// <summary>Quest-safe shadow settings for the window sun: soft, 15 m, one cascade, 2048 map.
    /// Serialized fields because the URP setters for soft shadows are internal.</summary>
    public static void TuneRendering()
    {
        var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-Asset.asset");
        if (asset == null) { Debug.LogWarning("ProjectConfigurator: URP-Asset.asset missing"); return; }
        var so = new SerializedObject(asset);
        so.FindProperty("m_MainLightShadowsSupported").boolValue = true;
        so.FindProperty("m_SoftShadowsSupported").boolValue = true;
        so.FindProperty("m_ShadowDistance").floatValue = 15f;
        so.FindProperty("m_ShadowCascadeCount").intValue = 1;
        so.FindProperty("m_MainLightShadowmapResolution").intValue = 2048;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        Debug.Log("ProjectConfigurator: rendering tuned (soft shadows, 15 m, 1 cascade)");
    }

    static void SetupPlayerAndAndroid()
    {
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)32;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.kaikenehme.renovationpreviewer");
        Debug.Log("ProjectConfigurator: player/Android configured");
    }

    static void SetupXr(BuildTargetGroup group)
    {
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
                out XRGeneralSettingsPerBuildTarget wrapper) || wrapper == null)
        {
            Directory.CreateDirectory("Assets/XR");
            wrapper = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(wrapper, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, wrapper, true);
        }

        var settings = wrapper.SettingsForBuildTarget(group);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            settings.name = group + " Settings";
            wrapper.SetSettingsForBuildTarget(group, settings);
            AssetDatabase.AddObjectToAsset(settings, wrapper);

            var manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            manager.name = group + " Providers";
            AssetDatabase.AddObjectToAsset(manager, wrapper);
            settings.Manager = manager;
        }

        bool assigned = XRPackageMetadataStore.AssignLoader(settings.Manager,
            "UnityEngine.XR.OpenXR.OpenXRLoader", group);
        Debug.Log($"ProjectConfigurator: OpenXR loader for {group}: {(assigned ? "assigned" : "FAILED")}");

        FeatureHelpers.RefreshFeatures(group);
        var openxr = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
        if (openxr != null)
        {
            foreach (var feature in openxr.GetFeatures<OpenXRFeature>())
            {
                var n = feature.GetType().Name;
                if (n.Contains("OculusTouchControllerProfile") || n.Contains("MetaQuestFeature"))
                {
                    feature.enabled = true;
                    Debug.Log($"ProjectConfigurator: enabled {n} for {group}");
                }
            }
        }
    }

    static void ImportXriSamples()
    {
        int imported = 0;
        foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(
                     "com.unity.xr.interaction.toolkit", null))
        {
            if (sample.displayName.Contains("Starter Assets") ||
                sample.displayName.Contains("Simulator"))
            {
                sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.OverridePreviousImports);
                imported++;
                Debug.Log($"ProjectConfigurator: imported sample '{sample.displayName}'");
            }
        }
        if (imported == 0)
            Debug.LogWarning("ProjectConfigurator: no XRI samples imported — check Sample.FindByPackage");
    }
}
