using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Right controller: trigger → physics ray from the controller → MenuTarget → menu.
/// Hitting the menu panel itself (tag MenuPanel) is ignored so UI clicks never
/// select the wall behind the canvas. Trigger on nothing closes the menu.
/// Left Y, right B (same hand that opened it) and, in the editor, Backspace close.
/// </summary>
public class MenuSelectRelay : MonoBehaviour
{
    public InputActionProperty selectAction;
    public InputActionProperty closeAction;          // left Y
    public InputActionProperty closeSameHandAction;  // right B
    public Transform rayOrigin;      // defaults to this transform
    public ControllerMenu menu;
    public float maxDistance = 6f;
    public Transform ignoreRoot;     // the XR Origin — never select your own body/controllers
    public SamplePuller puller;      // same controller; when it can pull, trigger means pull, not menu

    void OnEnable() { selectAction.action?.Enable(); closeAction.action?.Enable(); closeSameHandAction.action?.Enable(); }

    bool ClosePressed()
    {
        if (closeAction.action != null && closeAction.action.WasPressedThisFrame()) return true;
        if (closeSameHandAction.action != null && closeSameHandAction.action.WasPressedThisFrame()) return true;
#if UNITY_EDITOR
        var kb = Keyboard.current;
        if (kb != null && kb.backspaceKey.wasPressedThisFrame) return true;
#endif
        return false;
    }

    void Update()
    {
        if (menu == null) return;
        if (ClosePressed()) { menu.Hide(); return; }
        if (selectAction.action == null || !selectAction.action.WasPressedThisFrame()) return;
        if (puller != null && puller.CanPull) return;

        var origin = rayOrigin != null ? rayOrigin : transform;
        if (!RayUtil.TryHit(origin.position, origin.forward, maxDistance, ignoreRoot, out var hit))
        {
            menu.Hide();
            return;
        }
        if (hit.collider.CompareTag("MenuPanel")) return;   // clicking the UI, not the room

        var target = hit.collider.GetComponentInParent<MenuTarget>();
        if (target == null) { menu.Hide(); return; }
        if (target.Surface != null && target.Surface.Kind == SurfaceKind.Floor)
            menu.SpawnPoint = hit.point;
        menu.Show(target);
    }
}
