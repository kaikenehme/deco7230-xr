using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>Desktop mode: a click on a touch-only prop (preset frame, lamp, clock) stands in for reaching out to it.</summary>
public class DesktopTouchTests
{
    class Prop : MonoBehaviour, ITouchable { public int touches; public void Touch() => touches++; }

    readonly List<GameObject> made = new();

    GameObject Box(string name, Vector3 pos, bool trigger, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.GetComponent<Collider>().isTrigger = trigger;
        made.Add(go);
        return go;
    }

    [TearDown]
    public void TearDown() { foreach (var g in made) if (g != null) Object.DestroyImmediate(g); made.Clear(); }

    static readonly Ray Forward = new(Vector3.zero, Vector3.forward);

    [Test]
    public void Pick_TriggerOnlyProp_Found()
    {
        var p = Box("Lamp", new Vector3(0f, 0f, 3f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.AreSame(p, RayUtil.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_SolidChildOfProp_Found()
    {
        var root = new GameObject("PresetFrame"); made.Add(root);
        root.transform.position = new Vector3(0f, 0f, 3f);
        var p = root.AddComponent<Prop>();
        Box("Backing", new Vector3(0f, 0f, 3f), trigger: false, parent: root.transform);
        Physics.SyncTransforms();
        Assert.AreSame(p, RayUtil.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_WallInFront_Blocks()
    {
        Box("Wall", new Vector3(0f, 0f, 2f), trigger: false);
        Box("Lamp", new Vector3(0f, 0f, 4f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.IsNull(RayUtil.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_MenuPanelInFront_Blocks()
    {
        // The menu hangs off the left hand (inside the rig) but must still stop a click reaching the frame behind it.
        var rig = new GameObject("Rig"); made.Add(rig);
        var panel = Box("MenuBlocker", new Vector3(0f, 0f, 1f), trigger: false, parent: rig.transform);
        panel.tag = "MenuPanel";
        Box("PresetFrame", new Vector3(0f, 0f, 3f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.IsNull(RayUtil.PickTouchable(Forward, rig.transform, 10f));
    }

    [Test]
    public void Pick_RigAndStrayTriggers_SkippedNotBlocking()
    {
        var rig = new GameObject("Rig"); made.Add(rig);
        Box("Hand", new Vector3(0f, 0f, 1f), trigger: false, parent: rig.transform);
        Box("Zone", new Vector3(0f, 0f, 2f), trigger: true);
        var p = Box("Clock", new Vector3(0f, 0f, 4f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.AreSame(p, RayUtil.PickTouchable(Forward, rig.transform, 10f));
    }

    [Test]
    public void Pick_PropInFrontOfWall_Found_SoTheWallMenuStaysShut()
    {
        // Lamp and clock colliders are triggers; the menu ray skips triggers and used to open the wall behind.
        Box("Wall_S", new Vector3(0f, 0f, 5f), trigger: false);
        var p = Box("WallClock", new Vector3(0f, 0f, 4.8f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.AreSame(p, RayUtil.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Relay_TriggerOnProp_TouchesIt_AndReportsHandled()
    {
        // On the headset, point + trigger at the lamp, clock or a frame presses it (same as a click in desktop mode).
        Box("Wall_S", new Vector3(0f, 0f, 5f), trigger: false);
        var p = Box("Lamp", new Vector3(0f, 0f, 3f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.IsTrue(MenuSelectRelay.TryTouchProp(Forward, null, 10f));
        Assert.AreEqual(1, p.touches);
    }

    [Test]
    public void Relay_TriggerOnWall_NotHandled()
    {
        Box("Wall_S", new Vector3(0f, 0f, 5f), trigger: false);
        Physics.SyncTransforms();
        Assert.IsFalse(MenuSelectRelay.TryTouchProp(Forward, null, 10f));
    }
}
