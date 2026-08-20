using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// On-controller scheme controls: primary button saves the current scheme,
/// secondary button cycles through saved schemes. Physical controller inputs,
/// not floating VR menus — the no-menus rule holds.
/// </summary>
public class SchemeCycler : MonoBehaviour
{
    public InputActionProperty saveAction;
    public InputActionProperty cycleAction;
    public SchemeManager manager;

    int current = -1;

    void OnEnable()
    {
        saveAction.action?.Enable();
        cycleAction.action?.Enable();
    }

    void Update()
    {
        if (manager == null) return;
        if (saveAction.action != null && saveAction.action.WasPressedThisFrame())
            current = manager.SaveScheme();
        if (cycleAction.action != null && cycleAction.action.WasPressedThisFrame() && manager.Count > 0)
        {
            current = (current + 1) % manager.Count;
            manager.ApplyScheme(current);
        }
    }
}
