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
        Assert.Greater(Vector3.Dot(frame.transform.right, head.transform.position - frame.transform.position), 0f, "I3: picture side (+X) faces the user");
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

    // C2 — a non-convex MeshCollider's ClosestPoint returns the query point; planes use a thin box
    [Test]
    public void PlaneBox_ClosestPointLiesOnThePlane()
    {
        var go = new GameObject("plane");
        var box = go.AddComponent<BoxCollider>();
        MrPlaneSurface.FitBox(box, Vector2.zero, new Vector2(1f, 0.5f));
        Physics.SyncTransforms();
        var q = box.ClosestPoint(new Vector3(0f, 1f, 0f));
        Assert.Less(q.y, 0.05f, "on the plane, not the query point");
        Assert.AreEqual(2f, box.size.x, 1e-4f); Assert.AreEqual(1f, box.size.z, 1e-4f);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void PlanePrefab_UsesBox_NotMeshCollider()
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MrPlane.prefab");
        Assert.IsNotNull(prefab);
        Assert.IsNull(prefab.GetComponent<MeshCollider>());
        Assert.IsNotNull(prefab.GetComponent<BoxCollider>());
    }

    // C1 — Meta scene permission must be asked for, or no planes arrive
    [Test]
    public void ScenePermission_IsMetaUseScene() => Assert.AreEqual("com.oculus.permission.USE_SCENE", MrModeController.ScenePermission);
}
