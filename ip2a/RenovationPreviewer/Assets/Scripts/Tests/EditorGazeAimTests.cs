using NUnit.Framework;
using UnityEngine;

public class EditorGazeAimTests
{
    static readonly Vector3 Offset = new(0.18f, -0.22f, 0.25f);

    [Test]
    public void Compute_PositionIsHeadPlusOffsetInHeadSpace()
    {
        var head = new Pose(new Vector3(1, 1.6f, -2), Quaternion.Euler(0, 90, 0));
        var p = EditorGazeAim.Compute(head, Offset, 0f);
        var expected = head.position + head.rotation * Offset;
        Assert.Less(Vector3.Distance(expected, p.position), 1e-4f);
    }

    [Test]
    public void Compute_ZeroRoll_FacesWhereHeadFaces()
    {
        var head = new Pose(Vector3.zero, Quaternion.Euler(20, -45, 0));
        var p = EditorGazeAim.Compute(head, Offset, 0f);
        Assert.Less(Vector3.Angle(head.forward, p.forward), 1e-3f);
    }

    [Test]
    public void Compute_Roll_RotatesAboutControllerForwardOnly()
    {
        var head = new Pose(Vector3.zero, Quaternion.Euler(0, 30, 0));
        var p = EditorGazeAim.Compute(head, Offset, 90f);
        Assert.Less(Vector3.Angle(head.forward, p.forward), 1e-3f, "forward unchanged by roll");
        Assert.Less(Vector3.Angle(head.rotation * Vector3.left, p.up), 1e-3f, "90° roll swings up to the head's left");
    }

    [Test]
    public void StepRoll_ScrollNotchAddsDegrees_AndWraps()
    {
        Assert.AreEqual(15f, EditorGazeAim.StepRoll(0f, 1f, 15f), 1e-4f);
        Assert.AreEqual(-15f, EditorGazeAim.StepRoll(0f, -1f, 15f), 1e-4f);
        Assert.AreEqual(-175f, EditorGazeAim.StepRoll(175f, 2f, 5f), 1e-3f);   // 175 + 10 wraps to -175
    }

    [Test]
    public void SimulatorTargetMask_IsRightDeviceAndHmd_NotFps()
    {
        Assert.AreEqual(0, EditorGazeAim.SimulatorTargetMask & 1, "FPS bit off, otherwise buttons never reach a controller");
        Assert.AreEqual(4, EditorGazeAim.SimulatorTargetMask & 4, "RightDevice bit");
        Assert.AreEqual(8, EditorGazeAim.SimulatorTargetMask & 8, "HMD bit");
    }

    [Test]
    public void LeftTargetMask_IsLeftDeviceAndHmd_NotRight()
    {
        Assert.AreEqual(2 | 8, EditorGazeAim.SimulatorTargetMaskLeft);
        Assert.AreEqual(0, EditorGazeAim.SimulatorTargetMaskLeft & 4, "buttons go to one hand at a time");
    }

    [Test]
    public void Reconcile_SimulatorHandChoiceWins_HmdForced_FpsDropped()
    {
        const int Fps = 1, Left = 2, Right = 4, Hmd = 8;
        Assert.AreEqual(Left | Hmd, EditorGazeAim.Reconcile(Left, Right | Hmd), "T in the simulator moves the buttons left");
        Assert.AreEqual(Right | Hmd, EditorGazeAim.Reconcile(Right | Hmd, Left | Hmd), "Y moves them back");
        Assert.AreEqual(Left | Hmd, EditorGazeAim.Reconcile(Hmd, Left | Hmd), "no hand from the simulator keeps ours");
        Assert.AreEqual(Left | Hmd, EditorGazeAim.Reconcile(Fps, Left | Hmd), "FPS mode never survives, ours kept");
        Assert.AreEqual(Right | Hmd, EditorGazeAim.Reconcile(Fps, 0), "nothing anywhere: right hand");
        Assert.AreEqual(Left | Right | Hmd, EditorGazeAim.Reconcile(Left | Right, Right | Hmd), "Shift+Space: both, as the raw simulator does");
    }

    [Test]
    public void TargetHand_DefaultsRight_AndSwitches()
    {
        var go = new GameObject("right");
        var gaze = go.AddComponent<EditorGazeAim>();
        Assert.AreEqual(EditorGazeAim.SimulatorTargetMask, gaze.CurrentTargetMask, "right hand by default: trigger/grip are the hero loop");
        gaze.TargetLeft();
        Assert.AreEqual(EditorGazeAim.SimulatorTargetMaskLeft, gaze.CurrentTargetMask);
        gaze.TargetRight();
        Assert.AreEqual(EditorGazeAim.SimulatorTargetMask, gaze.CurrentTargetMask);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void StepRoll_NoScroll_NoChange()
    {
        Assert.AreEqual(40f, EditorGazeAim.StepRoll(40f, 0f, 15f), 1e-4f);
    }

    [Test]
    public void MirrorForLeftHand_FlipsXOnly()
    {
        var m = EditorGazeAim.MirrorForLeftHand(new Vector3(0.18f, -0.22f, 0.25f));
        Assert.AreEqual(new Vector3(-0.18f, -0.22f, 0.25f), m);
    }

    [Test]
    public void FollowOnly_LeftInstance_NeverSteersOrTwists()
    {
        var go = new GameObject("left");
        var gaze = go.AddComponent<EditorGazeAim>();
        gaze.ConfigureAsFollower();
        Assert.IsFalse(gaze.steerSimulator, "left hand must not retarget the simulator or buttons would reach both hands");
        Assert.AreEqual(0f, gaze.degreesPerNotch, "scroll twist belongs to the right hand only");
        Assert.Less(gaze.offset.x, 0f, "left hand sits left of the head");
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Converge_RayPointsAtWhatTheEyeSees()
    {
        // Hand 20 cm right / 13 cm down of the eye; the eye looks at a menu 0.8 m ahead.
        var head = new Pose(new Vector3(0f, 1.6f, 0f), Quaternion.identity);
        var hand = EditorGazeAim.Compute(head, new Vector3(0.2f, -0.13f, 0.38f), 0f).position;
        var aim = new Vector3(0f, 1.6f, 0.8f);
        var rot = EditorGazeAim.Converge(hand, aim, head.rotation, 0f);
        var dir = rot * Vector3.forward;
        var toAim = (aim - hand).normalized;
        Assert.Greater(Vector3.Dot(dir, toAim), 0.9999f, "ray lands on the gaze point, no 20 cm parallax");
    }

    [Test]
    public void Converge_KeepsTwistRoll()
    {
        var head = Quaternion.identity;
        var rot = EditorGazeAim.Converge(Vector3.zero, new Vector3(0f, 0f, 2f), head, 30f);
        Assert.AreEqual(30f, Mathf.DeltaAngle(0f, rot.eulerAngles.z), 0.01f, "scroll twist still reads on the controller");
    }

    [Test]
    public void Converge_AimBehindHand_FallsBackToHeadForward()
    {
        var rot = EditorGazeAim.Converge(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0.5f), Quaternion.identity, 0f);
        Assert.Greater(Vector3.Dot(rot * Vector3.forward, Vector3.forward), 0.999f, "never points back at the player");
    }
}
