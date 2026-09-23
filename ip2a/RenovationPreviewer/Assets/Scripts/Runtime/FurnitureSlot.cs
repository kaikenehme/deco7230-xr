using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public enum SlotOrigin { Scene, Preset, User }

/// <summary>
/// One piece of movable furniture. The slot GameObject is the stable identity
/// (pose, collider, grab, Surface for sample-pulling); the visual is a child that
/// Swap() replaces. Remove() leaves the empty slot so nothing else dangles.
///
/// Manipulation (Evaluation 1 §05, P2/P4 "place it, don't drop it"): XRI pose tracking
/// is off; while held the slot follows where the ray meets the floor (far grab) or the
/// hand (near grab), keeping the offset it had when grabbed so it never jumps; the
/// thumbstick of the holding hand yaws it, and locomotion is locked so the stick never
/// turns the player instead. Release snaps it upright inside the room.
/// </summary>
public class FurnitureSlot : MonoBehaviour
{
    public const float FloorMargin = 0.3f;
    public const float RotateDegPerSec = 90f, StickDeadzone = 0.3f;
    /// <summary>Horizontal cap on how far a far grab can push a piece (m). A shallow ray meets the floor
    /// tens of metres out; beyond this it stops, and level/upward rays sit here too, so crossing the horizon never jumps.</summary>
    public const float MaxReach = 6f;

    /// <summary>Thumbstick values, written each frame by FurnitureInput.</summary>
    public static Vector2 LeftStick, RightStick;

    public FurnitureOption Current { get; private set; }
    public GameObject Visual { get; private set; }
    public SlotOrigin Origin { get; set; } = SlotOrigin.User;
    public bool IsHeld { get; private set; }

    Bounds? floorBounds;
    XRGrabInteractable grab;
    IXRSelectInteractor holder;
    NearFarInteractor nearFar;
    SelectionOutline outline;
    float yaw;
    // Piece position minus the ray/hand target at grab time (XZ), so the grab never moves the piece;
    // re-measured whenever the hold switches between near and far.
    Vector3 grabOffset;
    NearFarInteractor.Region? offsetRegion;

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
        var o = GetComponent<SelectionOutline>();
        if (o != null) o.Rebuild();
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

    /// <summary>Create a new grabbable slot on the floor at floorPoint, clamped inside floorBounds (user-added).</summary>
    public static FurnitureSlot Spawn(FurnitureOption option, Vector3 floorPoint, Bounds floorBounds) =>
        Spawn(option, floorPoint, 0f, floorBounds, false, Color.grey, SlotOrigin.User);

    /// <summary>Full form: yaw in degrees; keep = also a Keep sample source with an authored colour.</summary>
    public static FurnitureSlot Spawn(FurnitureOption option, Vector3 floorPoint, float yaw, Bounds floorBounds, bool keep, Color sampleColor, SlotOrigin origin)
    {
        var go = new GameObject($"Furniture_{option.sourceId}");
        var slot = go.AddComponent<FurnitureSlot>();
        slot.floorBounds = floorBounds;
        slot.Origin = origin;
        slot.yaw = yaw;
        go.transform.position = slot.Clamp(floorPoint);
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        go.AddComponent<BoxCollider>();
        var grab = go.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        slot.BindGrab(floorBounds);
        go.AddComponent<MenuTarget>();

        if (keep)
        {
            var surf = go.AddComponent<Surface>();
            surf.SetState(SurfaceState.Keep);
            surf.SetBaseSampleColor(sampleColor);
            go.AddComponent<PullAffordance>();
        }

        slot.Swap(option);
        return slot;
    }

    /// <summary>Wire grab handling on a slot whose XRGrabInteractable already exists (scene-built or Spawn).</summary>
    public void BindGrab(Bounds bounds)
    {
        floorBounds = bounds;
        grab = GetComponent<XRGrabInteractable>();
        if (grab == null) return;
        grab.trackPosition = false;    // we place it (ray → floor), XRI does not float it along the ray
        grab.trackRotation = false;    // we yaw it from the stick, XRI does not pitch it
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
        outline = GetComponent<SelectionOutline>();
        if (outline == null) outline = gameObject.AddComponent<SelectionOutline>();
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        IsHeld = true;
        holder = args.interactorObject;
        nearFar = holder as NearFarInteractor ?? (holder as Component)?.GetComponentInParent<NearFarInteractor>();
        yaw = transform.eulerAngles.y;
        offsetRegion = null;
        LocomotionLock.Acquire();
        if (outline != null) outline.Show();
    }

    void OnReleased(SelectExitEventArgs _)
    {
        IsHeld = false;
        holder = null;
        nearFar = null;
        LocomotionLock.Release();
        if (outline != null) outline.Hide();   // RayFeedback re-shows it next frame if still pointed at
        // Snap: flat on the floor, upright, inside the room.
        var p = Clamp(transform.position);
        transform.position = new Vector3(p.x, 0f, p.z);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    void OnDestroy() { if (IsHeld) LocomotionLock.Release(); }

    void Update()
    {
        if (!IsHeld || holder == null) return;
        Vector3 target;
        var region = nearFar != null ? nearFar.selectionRegion.Value : NearFarInteractor.Region.Near;
        if (region == NearFarInteractor.Region.Far)
        {
            var ray = nearFar.transform;
            target = FloorPointOnRay(ray.position, ray.forward);
        }
        else
        {
            var attach = holder.GetAttachTransform(grab);
            target = attach != null ? attach.position : transform.position;
        }
        if (offsetRegion != region) { grabOffset = GrabOffset(transform.position, target); offsetRegion = region; }
        transform.position = Clamp(target + grabOffset);
        yaw += YawStep(StickFor(holder), Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    static Vector2 StickFor(IXRInteractor interactor) =>
        interactor != null && interactor.handedness == InteractorHandedness.Left ? LeftStick : RightStick;

    /// <summary>Where a ray meets the floor plane (y = 0), at most maxReach away horizontally.
    /// A level or upward ray lands at maxReach, so the piece never vanishes and never jumps back.</summary>
    public static Vector3 FloorPointOnRay(Vector3 origin, Vector3 dir, float maxReach = MaxReach)
    {
        var flat = new Vector3(dir.x, 0f, dir.z);
        var floorOrigin = new Vector3(origin.x, 0f, origin.z);
        if (flat.sqrMagnitude < 1e-8f) return floorOrigin;   // straight down or up
        float along = dir.y < -1e-4f ? -origin.y / dir.y * flat.magnitude : float.PositiveInfinity;
        return floorOrigin + flat.normalized * Mathf.Min(along, maxReach);
    }

    /// <summary>Offset that keeps a piece where it is when the target first takes hold of it (flat, XZ only).</summary>
    public static Vector3 GrabOffset(Vector3 piece, Vector3 target) => new(piece.x - target.x, 0f, piece.z - target.z);

    /// <summary>Degrees of yaw for one frame of stick input; dead zone, stick right = clockwise.</summary>
    public static float YawStep(Vector2 stick, float dt) =>
        Mathf.Abs(stick.x) < StickDeadzone ? 0f : stick.x * RotateDegPerSec * dt;

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
