using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// On-controller scheme controls: primary button saves the current scheme,
/// secondary button cycles through saved schemes. Physical controller inputs,
/// not floating VR menus — the no-menus rule holds.
/// Right B also closes the controller menu (MenuSelectRelay); while the menu is
/// open this component yields, and runs before the relay so it sees it open.
/// </summary>
[DefaultExecutionOrder(-10)]
public class SchemeCycler : MonoBehaviour
{
    public InputActionProperty saveAction;
    public InputActionProperty cycleAction;
    public SchemeManager manager;
    public ControllerMenu menu;

    public static bool ShouldCycle(bool pressed, int schemeCount, bool menuOpen) => pressed && schemeCount > 0 && !menuOpen;

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
        bool pressed = cycleAction.action != null && cycleAction.action.WasPressedThisFrame();
        if (ShouldCycle(pressed, manager.Count, menu != null && menu.IsOpen))
        {
            current = (current + 1) % manager.Count;
            manager.ApplyScheme(current);
        }
    }
}
