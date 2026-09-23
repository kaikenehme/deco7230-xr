using NUnit.Framework;
using UnityEngine;

public class FurnitureSlotTests
{
    static FurnitureOption Option(string id, float height = 0.8f)
    {
        var prefab = new GameObject(id);
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.transform.SetParent(prefab.transform);
        mesh.transform.localPosition = new Vector3(0, height / 2f, 0);
        mesh.transform.localScale = new Vector3(0.6f, height, 0.6f);
        return new FurnitureOption { name = id, sourceId = id, prefab = prefab, category = FurnitureCategory.Seating };
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var slot in Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None))
            Object.DestroyImmediate(slot.gameObject);
        foreach (var s in Surface.All.ToArray())
            if (s != null) Object.DestroyImmediate(s.gameObject);
    }

    [Test]
    public void Swap_ReplacesVisual_KeepsPose()
    {
        var root = new GameObject("Sofa");
        root.transform.position = new Vector3(-1.2f, 0, -0.9f);
        root.transform.rotation = Quaternion.Euler(0, 90, 0);
        var slot = root.AddComponent<FurnitureSlot>();
        var first = Option("A"); var second = Option("B");

        slot.Swap(first);
        var firstVisual = slot.Visual;
        slot.Swap(second);

        Assert.AreSame(second, slot.Current);
        Assert.IsTrue(firstVisual == null, "old visual destroyed");
        Assert.AreEqual("B", slot.Visual.name);
        Assert.AreEqual(root.transform, slot.Visual.transform.parent);
        Assert.AreEqual(Vector3.zero, slot.Visual.transform.localPosition);
        Assert.AreEqual(new Vector3(-1.2f, 0, -0.9f), root.transform.position);
        Assert.AreEqual(90f, root.transform.eulerAngles.y, 0.01f);
    }

    [Test]
    public void Swap_RefitsColliderAndRebindsSurface()
    {
        var root = new GameObject("Sofa");
        var surf = root.AddComponent<Surface>();
        surf.SetState(SurfaceState.Keep);
        var col = root.AddComponent<BoxCollider>();
        var slot = root.AddComponent<FurnitureSlot>();

        slot.Swap(Option("Tall", height: 1.2f));

        Assert.AreEqual(1.2f, col.size.y, 0.01f, "collider fits new visual");
        Assert.AreEqual(0.6f, col.center.y, 0.01f);
        Assert.AreEqual(surf.CommittedColor, slot.Visual.GetComponentInChildren<Renderer>().sharedMaterial.color, "surface re-bound to new renderer");
    }

    [Test]
    public void Remove_DestroysVisual_KeepsSlot()
    {
        var root = new GameObject("Sofa");
        var slot = root.AddComponent<FurnitureSlot>();
        slot.Swap(Option("A"));
        slot.Remove();
        Assert.IsNull(slot.Current);
        Assert.IsTrue(slot.Visual == null);
        Assert.IsTrue(root != null && slot != null, "slot survives");
    }

    [Test]
    public void Spawn_ClampsToFloorBounds_AndSitsOnFloor()
    {
        var floorBounds = new Bounds(Vector3.zero, new Vector3(4, 0.1f, 3));
        var slot = FurnitureSlot.Spawn(Option("A"), new Vector3(5f, 0.7f, -9f), floorBounds);
        Assert.AreEqual(2f - FurnitureSlot.FloorMargin, slot.transform.position.x, 0.001f);
        Assert.AreEqual(-1.5f + FurnitureSlot.FloorMargin, slot.transform.position.z, 0.001f);
        Assert.AreEqual(0f, slot.transform.position.y, 0.001f);
        Assert.IsNotNull(slot.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>());
        Assert.IsTrue(slot.GetComponent<Rigidbody>().isKinematic);
        Assert.IsNotNull(slot.GetComponent<MenuTarget>());
    }

    [Test]
    public void Swap_OnSceneBuiltSlot_RemovesPreExistingParts()
    {
        var root = new GameObject("Sofa");
        for (int i = 0; i < 4; i++)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = "Part" + i;
            part.transform.SetParent(root.transform);
        }
        var slot = root.AddComponent<FurnitureSlot>();
        slot.Swap(Option("New"));
        Assert.AreEqual(1, root.transform.childCount, "old parts gone, one visual left");
        Assert.AreEqual("New", root.transform.GetChild(0).name);
        slot.Remove();
        Assert.AreEqual(0, root.transform.childCount);
    }

    [Test]
    public void FloorPointOnRay_HitsPlaneY0()
    {
        var p = FurnitureSlot.FloorPointOnRay(new Vector3(0f, 1f, 0f), new Vector3(0f, -1f, 1f).normalized);
        Assert.AreEqual(0f, p.y, 1e-4f);
        Assert.AreEqual(1f, p.z, 1e-3f, "45° down from 1 m lands 1 m ahead");
    }

    [Test]
    public void FloorPointOnRay_UpwardRay_StopsAtMaxReach()
    {
        var p = FurnitureSlot.FloorPointOnRay(new Vector3(0f, 1f, 0f), new Vector3(0f, 0.5f, 1f).normalized, 2f);
        Assert.AreEqual(0f, p.y, 1e-4f);
        Assert.AreEqual(2f, p.z, 1e-3f, "level or upward ray = the far end of reach, not a point near the hand");
    }

    [Test]
    public void FloorPointOnRay_ShallowRay_CappedAtMaxReach()
    {
        // 1.4 m hand height, 5° down: the floor is 16 m away — cap it so the piece never shoots off.
        var dir = Quaternion.Euler(5f, 0f, 0f) * Vector3.forward;
        var p = FurnitureSlot.FloorPointOnRay(new Vector3(0f, 1.4f, 0f), dir, 6f);
        Assert.AreEqual(6f, p.z, 1e-3f);
    }

    [Test]
    public void FloorPointOnRay_JustBelowAndJustAboveLevel_Continuous()
    {
        var o = new Vector3(0f, 1.4f, 0f);
        var below = FurnitureSlot.FloorPointOnRay(o, Quaternion.Euler(0.5f, 0f, 0f) * Vector3.forward, 6f);
        var above = FurnitureSlot.FloorPointOnRay(o, Quaternion.Euler(-0.5f, 0f, 0f) * Vector3.forward, 6f);
        Assert.Less(Vector3.Distance(below, above), 0.01f, "crossing the horizon must not make the piece jump");
    }

    [Test]
    public void GrabOffset_KeepsPieceWhereItWas_ThenMovesWithTarget()
    {
        var piece = new Vector3(0.6f, 0f, -1.3f);
        var rayFloor = new Vector3(1.14f, 0f, -2.47f);   // where the ray behind the chair met the floor (probe, 23 Sep)
        var offset = FurnitureSlot.GrabOffset(piece, rayFloor);
        Assert.Less(Vector3.Distance(piece, rayFloor + offset), 1e-4f, "grab = no jump");
        Assert.Less(Vector3.Distance(piece + new Vector3(1f, 0f, 0.6f), rayFloor + new Vector3(1f, 0f, 0.6f) + offset), 1e-4f);
        Assert.AreEqual(0f, offset.y);
    }

    [Test] public void YawStep_BelowDeadzone_IsZero() => Assert.AreEqual(0f, FurnitureSlot.YawStep(new Vector2(0.2f, 0.9f), 1f));
    [Test] public void YawStep_FullDeflection_Is90DegPerSec() => Assert.AreEqual(FurnitureSlot.RotateDegPerSec, FurnitureSlot.YawStep(Vector2.right, 1f), 1e-4f);

    [Test]
    public void Spawn_WithYawAndKeep_AddsKeepSurfaceAndSampleColor()
    {
        var bounds = new Bounds(Vector3.zero, new Vector3(9, 0.1f, 7));
        var slot = FurnitureSlot.Spawn(Option("Sofa"), new Vector3(-2f, 0f, -2f), 90f, bounds, true, Color.cyan, SlotOrigin.Preset);
        Assert.AreEqual(90f, slot.transform.eulerAngles.y, 0.01f);
        var surf = slot.GetComponent<Surface>();
        Assert.IsNotNull(surf, "kept furniture is a sample source");
        Assert.AreEqual(SurfaceState.Keep, surf.State);
        Assert.AreEqual(Color.cyan, surf.SampleColor);
        Assert.IsNotNull(slot.GetComponent<PullAffordance>());
        Assert.AreEqual(SlotOrigin.Preset, slot.Origin);
    }

    [Test]
    public void Spawn_DisablesXriPoseTracking_AndDefaultsToUser()
    {
        var slot = FurnitureSlot.Spawn(Option("A"), Vector3.zero, new Bounds(Vector3.zero, new Vector3(4, 0.1f, 3)));
        var grab = slot.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        Assert.IsFalse(grab.trackPosition, "we place it at the ray, XRI must not float it");
        Assert.IsFalse(grab.trackRotation, "we yaw it from the stick, XRI must not pitch it");
        Assert.AreEqual(SlotOrigin.User, slot.Origin);
        Assert.IsFalse(slot.IsHeld);
    }
}
