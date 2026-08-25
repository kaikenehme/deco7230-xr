using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// One piece of movable furniture. The slot GameObject is the stable identity
/// (pose, collider, grab, Surface for sample-pulling); the visual is a child that
/// Swap() replaces. Remove() leaves the empty slot so nothing else dangles.
/// </summary>
public class FurnitureSlot : MonoBehaviour
{
    public const float FloorMargin = 0.3f;

    public FurnitureOption Current { get; private set; }
    public GameObject Visual { get; private set; }

    Bounds? floorBounds;

    public void Swap(FurnitureOption option)
    {
        ClearChildren();   // every child is visual — the scene-built sofa's parts included
        Current = option;
        if (option == null || option.prefab == null) { Visual = null; return; }

        Visual = Instantiate(option.prefab, transform);
        Visual.name = option.prefab.name;
        Visual.transform.localPosition = Vector3.zero;
        Visual.transform.localRotation = Quaternion.identity;
        Visual.transform.localScale = Vector3.one;
        // Visual meshes must not carry colliders of their own — the slot's box is the only one.
        foreach (var c in Visual.GetComponentsInChildren<Collider>()) DestroyNow(c);

        FitCollider();
        var surf = GetComponent<Surface>();
        if (surf != null) surf.RebindRenderer();
    }

    public void Remove()
    {
        ClearChildren();
        Visual = null;
        Current = null;
    }

    void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            c.SetParent(null, false);   // immediate: childCount/bounds are right this frame
            DestroyNow(c.gameObject);
        }
        Visual = null;
    }

    /// <summary>Create a new grabbable slot on the floor at floorPoint, clamped inside floorBounds.</summary>
    public static FurnitureSlot Spawn(FurnitureOption option, Vector3 floorPoint, Bounds floorBounds)
    {
        var go = new GameObject($"Furniture_{option.sourceId}");
        var slot = go.AddComponent<FurnitureSlot>();
        slot.floorBounds = floorBounds;
        go.transform.position = slot.Clamp(floorPoint);

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        go.AddComponent<BoxCollider>();
        var grab = go.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        grab.selectExited.AddListener(slot.OnReleased);
        go.AddComponent<MenuTarget>();

        slot.Swap(option);
        return slot;
    }

    /// <summary>Wire the release snap on a slot that was built by SceneBuilder (sofa).</summary>
    public void BindGrab(Bounds bounds)
    {
        floorBounds = bounds;
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null) grab.selectExited.AddListener(OnReleased);
    }

    void OnReleased(SelectExitEventArgs _)
    {
        // Snap: flat on the floor, upright, inside the room.
        var p = Clamp(transform.position);
        transform.position = new Vector3(p.x, 0f, p.z);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    Vector3 Clamp(Vector3 p)
    {
        if (floorBounds == null) return new Vector3(p.x, 0f, p.z);
        var b = floorBounds.Value;
        return new Vector3(
            Mathf.Clamp(p.x, b.min.x + FloorMargin, b.max.x - FloorMargin),
            0f,
            Mathf.Clamp(p.z, b.min.z + FloorMargin, b.max.z - FloorMargin));
    }

    void FitCollider()
    {
        var col = GetComponent<BoxCollider>();
        if (col == null || Visual == null) return;
        var rends = Visual.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;
        var b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        col.center = transform.InverseTransformPoint(b.center);
        var s = transform.lossyScale;
        col.size = new Vector3(b.size.x / s.x, b.size.y / s.y, b.size.z / s.z);
    }

    static void DestroyNow(Object o)
    {
        if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
    }
}
