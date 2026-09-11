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
    public void StepRoll_NoScroll_NoChange()
    {
        Assert.AreEqual(40f, EditorGazeAim.StepRoll(40f, 0f, 15f), 1e-4f);
    }
}
