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
        Assert.AreSame(p, EditorDesktopRig.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_SolidChildOfProp_Found()
    {
        var root = new GameObject("PresetFrame"); made.Add(root);
        root.transform.position = new Vector3(0f, 0f, 3f);
        var p = root.AddComponent<Prop>();
        Box("Backing", new Vector3(0f, 0f, 3f), trigger: false, parent: root.transform);
        Physics.SyncTransforms();
        Assert.AreSame(p, EditorDesktopRig.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_WallInFront_Blocks()
    {
        Box("Wall", new Vector3(0f, 0f, 2f), trigger: false);
        Box("Lamp", new Vector3(0f, 0f, 4f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.IsNull(EditorDesktopRig.PickTouchable(Forward, null, 10f));
    }

    [Test]
    public void Pick_RigAndStrayTriggers_SkippedNotBlocking()
    {
        var rig = new GameObject("Rig"); made.Add(rig);
        Box("Hand", new Vector3(0f, 0f, 1f), trigger: false, parent: rig.transform);
        Box("Zone", new Vector3(0f, 0f, 2f), trigger: true);
        var p = Box("Clock", new Vector3(0f, 0f, 4f), trigger: true).AddComponent<Prop>();
        Physics.SyncTransforms();
        Assert.AreSame(p, EditorDesktopRig.PickTouchable(Forward, rig.transform, 10f));
    }
}
