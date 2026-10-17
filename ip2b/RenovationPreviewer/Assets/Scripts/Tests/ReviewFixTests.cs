using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>Final-review fixes (17 Oct): each test reproduces one finding.</summary>
public class ReviewFixTests
{
    // C3 — passthrough needs an alpha channel in the eye buffer (meta-openxr graphics-settings.md)
    [Test]
    public void Urp_HdrOff_ForPassthrough()
    {
        var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/URP-Asset.asset");
        Assert.IsNotNull(urp);
        Assert.IsFalse(urp.supportsHDR, "HDR on = no alpha → passthrough renders black");
    }

    [Test]
    public void Urp_IntermediateTexture_Auto_ForPassthrough()
    {
        var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/URP-Renderer.asset");
        Assert.IsNotNull(data);
        Assert.AreEqual(IntermediateTextureMode.Auto, data.intermediateTextureMode);
    }

    // I1 — a cancelled select (sample replaced / preset switch) must never commit or throw
    [Test]
    public void Canceled_AlwaysDiscards()
    {
        Assert.AreEqual(ReleaseOutcome.Discard, TossDetector.Decide(true, Vector3.zero, canceled: true));
        Assert.AreEqual(ReleaseOutcome.Discard, TossDetector.Decide(false, new Vector3(0, 0, 5), canceled: true));
    }

    // I2 — a ray on a touchable (fan card, lamp, clock, curtains, frame) wins over pulling
    [Test]
    public void Trigger_RayOnTouchable_DoesNotPull()
    {
        Assert.IsFalse(SamplePuller.ShouldPull(pressed: true, hasPullable: true, rayOnTouchable: true));
        Assert.IsTrue(SamplePuller.ShouldPull(pressed: true, hasPullable: true, rayOnTouchable: false));
        Assert.IsFalse(SamplePuller.ShouldPull(pressed: false, hasPullable: true, rayOnTouchable: false));
        Assert.IsFalse(MenuSelectRelay.PullTakesTrigger(canPull: true, rayOnTouchable: true), "relay touches the prop instead");
        Assert.IsTrue(MenuSelectRelay.PullTakesTrigger(canPull: true, rayOnTouchable: false));
    }

    // I4 — holding a chip next to the window must not flip the curtains
    [Test]
    public void Curtain_IgnoresHandHoldingASample()
    {
        Assert.IsFalse(CurtainPanel.Accepts(isCue: true, handHoldingSample: true));
        Assert.IsTrue(CurtainPanel.Accepts(isCue: true, handHoldingSample: false));
        Assert.IsFalse(CurtainPanel.Accepts(isCue: false, handHoldingSample: false));
    }
}
