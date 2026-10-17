using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class MrModeTests
{
    GameObject shell, managers, frame;
    PresetApplier applier;
    MrModeController mr;

    [SetUp]
    public void Setup()
    {
        shell = new GameObject("RoomShell");
        managers = new GameObject("Managers");
        applier = managers.AddComponent<PresetApplier>();
        frame = new GameObject("YourRoomFrame");
        mr = frame.AddComponent<MrModeController>();
        mr.presets = applier;
        mr.roomShell = shell;
        applier.BeforeApply += () => mr.SetMr(false);   // OnEnable doesn't run reliably in EditMode; wire as SceneBuilder's runtime does
    }

    [TearDown]
    public void Cleanup() { Object.DestroyImmediate(frame); Object.DestroyImmediate(shell); Object.DestroyImmediate(managers); }

    [TestCase(PlaneClassifications.WallFace, SurfaceKind.Wall)]
    [TestCase(PlaneClassifications.Floor, SurfaceKind.Floor)]
    [TestCase(PlaneClassifications.Table, SurfaceKind.None)]
    [TestCase(PlaneClassifications.Ceiling, SurfaceKind.None)]
    [TestCase(PlaneClassifications.InvisibleWallFace, SurfaceKind.None)]
    [TestCase(PlaneClassifications.None, SurfaceKind.None)]
    public void KindFor_MapsWallsAndFloorOnly(PlaneClassifications c, SurfaceKind k) => Assert.AreEqual(k, MrModeController.KindFor(c));

    [Test]
    public void ToggleOn_HidesRoom_ToggleOff_RestoresIt()
    {
        mr.SetMr(true);
        Assert.IsTrue(mr.IsOn);
        Assert.IsFalse(shell.activeSelf);
        mr.SetMr(false);
        Assert.IsFalse(mr.IsOn);
        Assert.IsTrue(shell.activeSelf);
    }

    [Test]
    public void Toggle_RestoresFramePose()
    {
        frame.transform.position = new Vector3(1f, 1.5f, 2.47f);
        var head = new GameObject("head"); head.transform.position = new Vector3(0f, 1.6f, 0f);
        mr.head = head.transform;
        mr.SetMr(true);
        Assert.Less(Vector3.Distance(frame.transform.position, head.transform.position), 1.2f, "floats in front of the user");
        mr.SetMr(false);
        Assert.AreEqual(new Vector3(1f, 1.5f, 2.47f), frame.transform.position);
        Object.DestroyImmediate(head);
    }

    [Test]
    public void PresetApply_TurnsMrOff()
    {
        // Review Focus #4: switching room while in MR returns to VR first.
        mr.SetMr(true);
        applier.Apply(0);   // no presets assigned: BeforeApply still fires
        Assert.IsFalse(mr.IsOn);
        Assert.IsTrue(shell.activeSelf);
    }

    [Test]
    public void PlaneAlpha_InvisibleUntilPainted()
    {
        Assert.AreEqual(0f, MrPlaneSurface.AlphaFor(false, false));
        Assert.AreEqual(MrPlaneSurface.PreviewAlpha, MrPlaneSurface.AlphaFor(true, false));
        Assert.AreEqual(MrPlaneSurface.PreviewAlpha, MrPlaneSurface.AlphaFor(true, true), "preview wins while hovering");
        Assert.AreEqual(MrPlaneSurface.CommittedAlpha, MrPlaneSurface.AlphaFor(false, true));
    }
}
