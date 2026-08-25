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
}
