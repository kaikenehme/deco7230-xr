using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// IP2b N7 — MR showcase. Touch the "Your room" frame: the virtual room hides, passthrough
/// shows the real one, and detected walls and floor become paintable Surfaces (MrPlaneSurface),
/// so the same pull → fan → hold-up/toss loop works on real walls. Touch again for VR.
/// VR is the default; nothing in Runtime references this assembly (delete MR/ to remove it).
/// </summary>
public class MrModeController : MonoBehaviour, ITouchable
{
    const float Debounce = 0.5f;
    /// <summary>Meta's runtime permission for Space Setup data; without it no planes arrive (meta-openxr scene-setup.md).</summary>
    public const string ScenePermission = "com.oculus.permission.USE_SCENE";

    public PresetApplier presets;
    public GameObject roomShell;                 // everything virtual that hides in MR
    public Camera xrCamera;
    public ARSession session;
    public ARCameraManager cameraManager;        // enabling it turns Meta passthrough on
    public ARPlaneManager planeManager;
    public Transform head;                       // the frame floats in front of this in MR

    public bool IsOn { get; private set; }

    readonly List<GameObject> hiddenSlots = new();
    CameraClearFlags oldFlags; Color oldBackground;
    Vector3 homePos; Quaternion homeRot; bool homeKnown;
    float lastTouch = -10f;

    /// <summary>Real planes that take paint: walls and floor only (tables, ceilings, doors, invisible walls ignored).</summary>
    public static SurfaceKind KindFor(PlaneClassifications c)
    {
        if ((c & PlaneClassifications.WallFace) != 0) return SurfaceKind.Wall;
        if ((c & PlaneClassifications.Floor) != 0) return SurfaceKind.Floor;
        return SurfaceKind.None;
    }

    void OnEnable() { if (presets != null) presets.BeforeApply += TurnOff; }
    void OnDisable() { if (presets != null) presets.BeforeApply -= TurnOff; }
    void TurnOff() => SetMr(false);

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        SetMr(!IsOn);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() != null) Touch();
    }

    public void SetMr(bool on)
    {
        if (on == IsOn) return;
        IsOn = on;
        if (!homeKnown) { homePos = transform.position; homeRot = transform.rotation; homeKnown = true; }

        if (on)
        {
            hiddenSlots.Clear();
            foreach (var s in FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None)) { hiddenSlots.Add(s.gameObject); s.gameObject.SetActive(false); }
            if (roomShell != null) roomShell.SetActive(false);
            if (xrCamera != null)
            {
                oldFlags = xrCamera.clearFlags; oldBackground = xrCamera.backgroundColor;
                xrCamera.clearFlags = CameraClearFlags.SolidColor;
                xrCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);   // alpha 0 = passthrough shows through
            }
            FloatInFront();
        }
        else
        {
            foreach (var g in hiddenSlots) if (g != null) g.SetActive(true);
            hiddenSlots.Clear();
            if (roomShell != null) roomShell.SetActive(true);
            if (xrCamera != null) { xrCamera.clearFlags = oldFlags; xrCamera.backgroundColor = oldBackground; }
            transform.SetPositionAndRotation(homePos, homeRot);
        }

        if (session != null) session.enabled = on;
        if (cameraManager != null) cameraManager.enabled = on;
        if (planeManager != null)
        {
            planeManager.enabled = on && HasScenePermission();   // Update enables it once the permission arrives
            foreach (var p in planeManager.trackables) p.gameObject.SetActive(on);
        }
        if (on && !HasScenePermission()) RequestScenePermission();
        Debug.Log($"[IP2b] mr {(on ? "on" : "off")}");
    }

    /// <summary>In the real room the frame's wall may not exist: float it 0.9 m in front of the user, at chest height.</summary>
    void FloatInFront()
    {
        if (head == null) return;
        var fwd = head.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.forward;
        fwd.Normalize();
        var pos = head.position + fwd * 0.9f; pos.y = Mathf.Max(1.2f, head.position.y - 0.3f);
        // Frames are built with the picture on local +X: point +X back at the user (review I3).
        transform.SetPositionAndRotation(pos, Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(0f, 90f, 0f));
    }

    void Start() => RequestScenePermission();   // ask at launch, so the system dialog never interrupts a session

    static bool HasScenePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(ScenePermission);
#else
        return true;
#endif
    }

    static void RequestScenePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(ScenePermission))
            UnityEngine.Android.Permission.RequestUserPermission(ScenePermission);
#endif
    }

    void Update()
    {
        if (IsOn && planeManager != null && !planeManager.enabled && HasScenePermission()) planeManager.enabled = true;
#if UNITY_EDITOR
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.mKey.wasPressedThisFrame) Touch();   // facilitator / desktop dry run
#endif
    }
}
