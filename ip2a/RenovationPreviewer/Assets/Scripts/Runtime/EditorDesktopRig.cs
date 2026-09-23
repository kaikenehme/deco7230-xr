using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

/// <summary>
/// Editor-only desktop controls for testing without a headset (IP2a is run in the editor).
/// The cursor stays free: the right hand's ray points at whatever is under it (EditorGazeAim
/// reads CursorRay), so the head, and the menu riding the left hand, hold still while you aim.
/// Hold the right mouse button and drag to look; WASD walks, clamped inside the room.
/// Buttons still go through the XR Device Simulator (click = trigger, G = grip, B/N, T/Y).
/// F4 hands everything back to the raw simulator. Inert on device.
/// </summary>
public class EditorDesktopRig : MonoBehaviour
{
    public Transform head;              // Main Camera
    public float lookDegreesPerPixel = 0.15f;
    public float walkSpeed = 1.6f;
    public float wallMargin = 0.35f;
    public bool active = true;

    public const float PitchLimit = 80f;

    /// <summary>The live instance while desktop mode is on, else null. EditorGazeAim and SimulatorCursorLock consult it.</summary>
    public static EditorDesktopRig Current { get; private set; }

    TrackedPoseDriver headDriver;
    float yaw, pitch;

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
        if (active)
        {
            Current = this;
            var e = head != null ? head.eulerAngles : Vector3.zero;
            yaw = e.y; pitch = Mathf.DeltaAngle(0f, e.x);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Current == this) Current = null;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.f4Key.wasPressedThisFrame) { active = !active; Apply(); }
        if (!active || head == null) return;
        if (Cursor.lockState != CursorLockMode.None) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        var mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.isPressed)
        {
            var yp = Look(new Vector2(yaw, pitch), mouse.delta.ReadValue(), lookDegreesPerPixel);
            yaw = yp.x; pitch = yp.y;
        }
        head.rotation = Quaternion.Euler(pitch, yaw, 0f);

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
