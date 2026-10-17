using NUnit.Framework;
using UnityEngine;

public class EditorDesktopRigTests
{
    [Test]
    public void Look_DragRight_TurnsRight_DragUp_LooksUp()
    {
        var yp = EditorDesktopRig.Look(Vector2.zero, new Vector2(100f, 50f), 0.1f);
        Assert.AreEqual(10f, yp.x, 1e-4f);
        Assert.AreEqual(-5f, yp.y, 1e-4f, "Unity pitch: negative x-rotation looks up");
    }

    [Test]
    public void Look_PitchClamped()
    {
        var yp = EditorDesktopRig.Look(Vector2.zero, new Vector2(0f, -5000f), 0.1f);
        Assert.AreEqual(EditorDesktopRig.PitchLimit, yp.y, 1e-4f);
    }

    [Test]
    public void Walk_W_MovesAlongView_ClampedAtWall()
    {
        var p = EditorDesktopRig.Walk(Vector3.zero, 90f, new Vector2(0f, 1f), 2f, 0.5f, 4.5f, 3.5f, 0.35f);
        Assert.AreEqual(1f, p.x, 1e-4f, "facing +x, W walks +x");
        Assert.AreEqual(0f, p.z, 1e-4f);
        var far = EditorDesktopRig.Walk(new Vector3(4f, 0f, 0f), 90f, new Vector2(0f, 1f), 2f, 1f, 4.5f, 3.5f, 0.35f);
        Assert.AreEqual(4.5f - 0.35f, far.x, 1e-4f, "stops short of Wall_E");
    }

    [Test]
    public void Walk_Diagonal_NotFaster()
    {
        var p = EditorDesktopRig.Walk(Vector3.zero, 0f, new Vector2(1f, 1f), 1f, 1f, 9f, 9f, 0f);
        Assert.AreEqual(1f, new Vector2(p.x, p.z).magnitude, 1e-4f);
    }

    [Test]
    public void TurnKeys_Q_Left_E_Right_BothCancel()
    {
        Assert.AreEqual(0f, EditorDesktopRig.TurnKeys(false, false));
        Assert.AreEqual(-1f, EditorDesktopRig.TurnKeys(true, false), "Q turns anticlockwise, like the stick pushed left");
        Assert.AreEqual(1f, EditorDesktopRig.TurnKeys(false, true));
        Assert.AreEqual(0f, EditorDesktopRig.TurnKeys(true, true));
        Assert.Greater(Mathf.Abs(EditorDesktopRig.TurnKeys(false, true)), FurnitureSlot.StickDeadzone, "clears the stick deadzone");
    }
}
