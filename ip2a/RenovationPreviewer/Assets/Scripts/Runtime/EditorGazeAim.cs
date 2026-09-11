using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// Editor-only simulator crutch: the right controller rides the head at a fixed
/// offset so mouse-look aims the ray, like a first-person game. Scroll wheel rolls
/// the controller (twist-to-tune). F4 toggles back to the raw XR Device Simulator.
/// Inert on device: the component disables itself when not in the editor, and the
/// TrackedPoseDriver it pauses is re-enabled on toggle-off or destroy.
///
/// The classic XR Device Simulator only feeds buttons (left click = trigger, G = grip)
/// to a *targeted* controller, and its FPS mode targets none. While active this
/// forces its target to HMD + right controller every frame: mouse and WASD drive the
/// head, buttons reach the right hand, and the pose override below keeps the ray on
/// the gaze. The target field is internal, hence reflection; failure is logged once
/// and the raw simulator keys still work.
/// </summary>
public class EditorGazeAim : MonoBehaviour
{
    public Transform head;
    public TrackedPoseDriver poseDriver;
    public Vector3 offset = new(0.18f, -0.22f, 0.25f);
    public float degreesPerNotch = 15f;
    public bool active = true;

    public float Roll { get; private set; }

    /// <summary>XRDeviceSimulator.TargetedDevices.RightDevice | HMD (internal enum: FPS=1, Left=2, Right=4, HMD=8).</summary>
    public const int SimulatorTargetMask = 4 | 8;
    const int SimulatorTargetFps = 1;
    const string TargetField = "m_TargetedDeviceInput";

    FieldInfo targetField;
    bool targetFieldMissingLogged;
    float savedScrollSensitivity = -1f;

    public static Pose Compute(Pose head, Vector3 offset, float rollDegrees)
    {
        var pos = head.position + head.rotation * offset;
        var rot = head.rotation * Quaternion.AngleAxis(rollDegrees, Vector3.forward);
        return new Pose(pos, rot);
    }

    public static float StepRoll(float current, float scrollNotches, float degreesPerNotch)
    {
        var r = current + scrollNotches * degreesPerNotch;
        return Mathf.DeltaAngle(0f, r);   // wrap to (-180, 180]
    }

    void Awake()
    {
        if (!Application.isEditor) { enabled = false; return; }
        if (poseDriver == null) poseDriver = GetComponent<TrackedPoseDriver>();
    }

    void OnEnable() => ApplyDriverState();
    void OnDisable() { if (poseDriver != null) poseDriver.enabled = true; ReleaseSimulator(); }

    void ApplyDriverState()
    {
        if (poseDriver != null) poseDriver.enabled = !active;
        if (!active) ReleaseSimulator();
    }

    void SteerSimulator()
    {
        var sim = XRDeviceSimulator.instance;
        if (sim == null) return;
        targetField ??= typeof(XRDeviceSimulator).GetField(TargetField, BindingFlags.Instance | BindingFlags.NonPublic);
        if (targetField == null)
        {
            if (!targetFieldMissingLogged) { Debug.LogWarning($"EditorGazeAim: XRDeviceSimulator.{TargetField} not found; press Y in the simulator to get buttons"); targetFieldMissingLogged = true; }
            return;
        }
        targetField.SetValue(sim, System.Enum.ToObject(targetField.FieldType, SimulatorTargetMask));
        if (savedScrollSensitivity < 0f) { savedScrollSensitivity = sim.mouseScrollRotateSensitivity; sim.mouseScrollRotateSensitivity = 0f; }   // scroll is ours: twist, not head roll
    }

    void ReleaseSimulator()
    {
        var sim = XRDeviceSimulator.instance;
        if (sim == null || targetField == null) return;
        targetField.SetValue(sim, System.Enum.ToObject(targetField.FieldType, SimulatorTargetFps));
        if (savedScrollSensitivity >= 0f) { sim.mouseScrollRotateSensitivity = savedScrollSensitivity; savedScrollSensitivity = -1f; }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.f4Key.wasPressedThisFrame) { active = !active; ApplyDriverState(); }
        if (!active) return;
        SteerSimulator();

        var mouse = Mouse.current;
        if (mouse != null)
        {
            // Wheel magnitude differs per OS/mouse (±120, ±1, trackpad fractions); one step per frame of scroll is enough.
            var y = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(y) > 0.01f) Roll = StepRoll(Roll, Mathf.Sign(y), degreesPerNotch);
        }
    }

    void LateUpdate()
    {
        if (!active || head == null) return;
        var p = Compute(new Pose(head.position, head.rotation), offset, Roll);
        transform.SetPositionAndRotation(p.position, p.rotation);
    }
}
