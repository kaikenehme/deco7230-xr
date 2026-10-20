using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

/// <summary>20 Oct: MR is cut. The Meta MR features are off for Android (packages stay installed);
/// the VR setup that ran on the Quest for IP2a stays on.</summary>
public class XrConfigTests
{
    static readonly string[] MetaMr =
    {
        "com.unity.openxr.feature.arfoundation-meta-session",
        "com.unity.openxr.feature.arfoundation-meta-camera",
        "com.unity.openxr.feature.arfoundation-meta-plane",
    };

    [Test]
    public void MetaMrFeatures_AreOffForAndroid()
    {
        foreach (var id in MetaMr)
        {
            var f = FeatureHelpers.GetFeatureWithIdForBuildTarget(BuildTargetGroup.Android, id);
            Assert.IsNotNull(f, id + " (package still installed)");
            Assert.IsFalse(f.enabled, id + " off");
        }
    }

    [Test]
    public void QuestVrFeatures_StillOnForAndroid()
    {
        var features = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android).GetFeatures<OpenXRFeature>();
        Assert.IsTrue(features.Any(f => f.GetType().Name.Contains("MetaQuestFeature") && f.enabled), "Meta Quest feature on");
        Assert.IsTrue(features.Any(f => f.GetType().Name.Contains("OculusTouchControllerProfile") && f.enabled), "Touch controller profile on");
    }
}
