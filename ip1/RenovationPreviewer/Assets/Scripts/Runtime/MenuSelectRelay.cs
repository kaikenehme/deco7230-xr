using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Right controller: trigger → physics ray from the controller → MenuTarget → menu.
/// Hitting the menu panel itself (tag MenuPanel) is ignored so UI clicks never
/// select the wall behind the canvas. Trigger on nothing closes the menu.
/// Left controller secondary button also closes.
/// </summary>
public class MenuSelectRelay : MonoBehaviour
{
    public InputActionProperty selectAction;
    public InputActionProperty closeAction;
    public Transform rayOrigin;      // defaults to this transform
    public ControllerMenu menu;
    public float maxDistance = 6f;
    public Transform ignoreRoot;     // the XR Origin — never select your own body/controllers

    void OnEnable() { selectAction.action?.Enable(); closeAction.action?.Enable(); }

    void Update()
    {
        if (menu == null) return;
        if (closeAction.action != null && closeAction.action.WasPressedThisFrame()) { menu.Hide(); return; }
        if (selectAction.action == null || !selectAction.action.WasPressedThisFrame()) return;

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
