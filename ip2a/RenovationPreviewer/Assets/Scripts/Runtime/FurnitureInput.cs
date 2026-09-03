using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Reads both thumbsticks every frame for FurnitureSlot (rotate while held). Lives on the rig root.</summary>
public class FurnitureInput : MonoBehaviour
{
    public InputActionProperty leftStick, rightStick;

    void OnEnable() { leftStick.action?.Enable(); rightStick.action?.Enable(); }

    void Update()
    {
        FurnitureSlot.LeftStick = leftStick.action != null ? leftStick.action.ReadValue<Vector2>() : Vector2.zero;
        FurnitureSlot.RightStick = rightStick.action != null ? rightStick.action.ReadValue<Vector2>() : Vector2.zero;
    }
}
