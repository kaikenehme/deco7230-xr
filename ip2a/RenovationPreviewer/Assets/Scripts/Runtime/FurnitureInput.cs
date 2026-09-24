using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Reads both thumbsticks every frame for FurnitureSlot (rotate while held); Q/E stand in for them in desktop mode. Lives on the rig root.</summary>
public class FurnitureInput : MonoBehaviour
{
    public InputActionProperty leftStick, rightStick;

    void OnEnable() { leftStick.action?.Enable(); rightStick.action?.Enable(); }

    void Update()
    {
        FurnitureSlot.LeftStick = leftStick.action != null ? leftStick.action.ReadValue<Vector2>() : Vector2.zero;
        FurnitureSlot.RightStick = rightStick.action != null ? rightStick.action.ReadValue<Vector2>() : Vector2.zero;
        // Desktop mode: WASD no longer drives the sticks, so Q/E turn a held piece.
        var kb = Keyboard.current;
        if (EditorDesktopRig.Current != null && kb != null)
        {
            float turn = EditorDesktopRig.TurnKeys(kb.qKey.isPressed, kb.eKey.isPressed);
            if (turn != 0f) { FurnitureSlot.LeftStick.x = turn; FurnitureSlot.RightStick.x = turn; }
        }
    }
}
