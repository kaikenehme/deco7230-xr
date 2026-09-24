using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// Editor-only desktop controls for testing without a headset (IP2a is run in the editor).
/// The cursor stays free: the right hand's ray points at whatever is under it (EditorGazeAim
/// reads CursorRay), so the head, and the menu riding the left hand, hold still while you aim.
/// Hold the right mouse button and drag to look (the cursor hides and locks while you do, so a
/// turn is not cut short at the screen edge); WASD walks, clamped inside the room.
/// Buttons still go through the XR Device Simulator (click = trigger, G = grip, B/N, T/Y).
/// WASD stops reaching the simulated thumbsticks (it pushed them forward = teleport aim); a held
/// piece turns with Q/E instead of the thumbstick.
/// A click on a touch-only prop (preset frame, lamp, clock) touches it, since the hands can't
/// reach a wall here. The controller menu comes in closer while desktop mode is on.
/// F4 hands everything back to the raw simulator. Inert on device.
/// </summary>
public class EditorDesktopRig : MonoBehaviour
{
    public Transform head;              // Main Camera
    public RectTransform menu;          // ControllerMenu on the left hand
    public float lookDegreesPerPixel = 0.3f;
    public float walkSpeed = 1.6f;
    public float wallMargin = 0.35f;
    public bool active = true;

    public const float PitchLimit = 80f, TouchReach = 12f;

    /// <summary>Menu offset from the left hand in desktop mode: ~0.45 m ahead of the eye, just left of centre
    /// (the device offset, 0.4 m out from the hand, puts it ~0.8 m away here, too small to read).</summary>
    public static readonly Vector3 MenuOffset = new(0.15f, 0.13f, 0.07f);

    /// <summary>The live instance while desktop mode is on, else null. EditorGazeAim and SimulatorCursorLock consult it.</summary>
    public static EditorDesktopRig Current { get; private set; }

    TrackedPoseDriver headDriver;
    float yaw, pitch;
    Vector3 deviceMenuOffset;
    bool haveDeviceMenuOffset;

    /// <summary>World ray from the camera through the mouse cursor.</summary>
    public bool TryCursorRay(out Ray ray)
    {
        ray = default;
        var cam = head != null ? head.GetComponent<Camera>() : null;
        var mouse = Mouse.current;
        if (cam == null || mouse == null) return false;
        ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        return true;
    }

    /// <summary>Q/E as a thumbstick x for turning a held piece: Q = left (−1), E = right (+1).</summary>
    public static float TurnKeys(bool q, bool e) => (e ? 1f : 0f) - (q ? 1f : 0f);

    /// <summary>Yaw/pitch after a mouse drag; pitch clamped so the view never flips.</summary>
    public static Vector2 Look(Vector2 yawPitch, Vector2 mouseDelta, float degreesPerPixel) =>
        new(yawPitch.x + mouseDelta.x * degreesPerPixel,
            Mathf.Clamp(yawPitch.y - mouseDelta.y * degreesPerPixel, -PitchLimit, PitchLimit));

    /// <summary>Rig position after one frame of WASD, along the flat view direction, kept inside the room.</summary>
    public static Vector3 Walk(Vector3 pos, float viewYaw, Vector2 input, float speed, float dt, float halfW, float halfD, float margin)
    {
        var rot = Quaternion.Euler(0f, viewYaw, 0f);
        var step = rot * new Vector3(input.x, 0f, input.y);
        if (step.sqrMagnitude > 1f) step.Normalize();
        var p = pos + step * speed * dt;
        return new Vector3(Mathf.Clamp(p.x, -halfW + margin, halfW - margin), pos.y, Mathf.Clamp(p.z, -halfD + margin, halfD - margin));
    }

    void Awake()
    {
        if (!Application.isEditor) { enabled = false; return; }
        if (head != null) headDriver = head.GetComponent<TrackedPoseDriver>();
    }

    void OnEnable() => Apply();
    void OnDisable() { active = false; Apply(); }

    void Apply()
    {
        if (headDriver != null) headDriver.enabled = !active;
        if (menu != null)
        {
            if (!haveDeviceMenuOffset) { deviceMenuOffset = menu.anchoredPosition3D; haveDeviceMenuOffset = true; }
            menu.anchoredPosition3D = active ? MenuOffset : deviceMenuOffset;
        }
        if (active)
        {
            Current = this;
            var e = head != null ? head.eulerAngles : Vector3.zero;
            yaw = e.y; pitch = Mathf.DeltaAngle(0f, e.x);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (Current == this) Current = null;
            var sim = XRDeviceSimulator.instance;
            if (sim != null) sim.axis2DTargets = XRDeviceSimulator.Axis2DTargets.Primary2DAxis;   // F4: raw simulator gets its thumbsticks back
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.f4Key.wasPressedThisFrame) { active = !active; Apply(); }
        if (!active || head == null) return;
        // The simulator copies WASD into the controllers' primary2DAxis; thumbstick-forward is XRI's teleport aim.
        // Every frame, since the simulator has its own toggle key for this.
        var sim = XRDeviceSimulator.instance;
        if (sim != null && sim.axis2DTargets != XRDeviceSimulator.Axis2DTargets.None) sim.axis2DTargets = XRDeviceSimulator.Axis2DTargets.None;

        var mouse = Mouse.current;
        bool looking = mouse != null && mouse.rightButton.isPressed;
        var wantLock = looking ? CursorLockMode.Locked : CursorLockMode.None;
        if (Cursor.lockState != wantLock) { Cursor.lockState = wantLock; Cursor.visible = !looking; }
        if (looking)
        {
            var yp = Look(new Vector2(yaw, pitch), mouse.delta.ReadValue(), lookDegreesPerPixel);
            yaw = yp.x; pitch = yp.y;
        }
        head.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (!looking && mouse != null && mouse.leftButton.wasPressedThisFrame && TryCursorRay(out var ray))
            RayUtil.PickTouchable(ray, transform, TouchReach)?.Touch();

        if (kb != null)
        {
            var input = new Vector2((kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0), (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0));
            if (input != Vector2.zero)
            {
                // Move the rig; the camera rides it. Clamp on the head's floor point so you never walk into a wall.
                var headFloor = new Vector3(head.position.x, 0f, head.position.z);
                var next = Walk(headFloor, yaw, input, walkSpeed, Time.deltaTime, RoomSpec.W / 2f, RoomSpec.D / 2f, wallMargin);
                transform.position += next - headFloor;
            }
        }
    }
}
