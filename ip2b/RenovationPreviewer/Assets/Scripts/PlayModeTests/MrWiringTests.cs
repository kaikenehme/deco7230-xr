using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>IP2b N7: the MR showcase is wired, off by default, and switching it hides/restores the virtual room.</summary>
public class MrWiringTests
{
    [UnitySetUp]
    public IEnumerator LoadRoom() { SceneManager.LoadScene("Room"); yield return null; yield return null; }

    [UnityTest]
    public IEnumerator YourRoomFrame_WiredAndOffByDefault()
    {
        var mr = Object.FindAnyObjectByType<MrModeController>();
        Assert.IsNotNull(mr, "'Your room' frame present");
        Assert.IsFalse(mr.IsOn, "VR is the default");
        Assert.IsNotNull(mr.presets); Assert.IsNotNull(mr.roomShell); Assert.IsNotNull(mr.xrCamera);
        Assert.IsNotNull(mr.cameraManager); Assert.IsNotNull(mr.planeManager); Assert.IsNotNull(mr.session);
        Assert.IsFalse(mr.cameraManager.enabled, "no passthrough in VR");
        Assert.IsFalse(mr.planeManager.enabled, "no plane detection in VR");
        Assert.IsFalse(mr.session.enabled, "no AR session in VR");
        Assert.Greater(mr.transform.position.z, RoomSpec.D / 2f - 0.2f, "on Wall_N");
        yield break;
    }

    [UnityTest]
    public IEnumerator Shell_HoldsTheRoom_NotTheRigOrManagers()
    {
        var shell = GameObject.Find("RoomShell");
        Assert.IsNotNull(shell);
        foreach (var n in new[] { "Floor", "Wall_N", "Curtains", "Beam", "WindowView", "ArcLamp" })
            Assert.IsTrue(GameObject.Find(n).transform.IsChildOf(shell.transform), n + " under RoomShell");
        Assert.IsFalse(GameObject.Find("Managers").transform.IsChildOf(shell.transform));
        Assert.IsFalse(GameObject.Find("XR Origin (XR Rig)").transform.IsChildOf(shell.transform));
        yield break;
    }

    [UnityTest]
    public IEnumerator SetMr_HidesRoomAndFurniture_ThenRestores()
    {
        var mr = Object.FindAnyObjectByType<MrModeController>();
        int pieces = Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Length;
        Assert.Greater(pieces, 0);
        LogAssert.ignoreFailingMessages = true;   // no AR provider in the editor: AR Foundation may complain when enabled
        mr.SetMr(true);
        yield return null;
        Assert.IsFalse(mr.roomShell.activeSelf);
        Assert.AreEqual(0, Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Length, "furniture hidden");
        Assert.IsFalse(Surface.All.Any(s => s != null && s.name == "Wall_N"), "virtual walls are not paint targets in MR");
        mr.SetMr(false);
        yield return null;
        LogAssert.ignoreFailingMessages = false;
        Assert.IsTrue(mr.roomShell.activeSelf);
        Assert.AreEqual(pieces, Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Length, "furniture back");
    }
}
