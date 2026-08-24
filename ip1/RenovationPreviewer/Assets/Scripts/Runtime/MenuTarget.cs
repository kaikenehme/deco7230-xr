using UnityEngine;

/// <summary>
/// Marks something the right-hand ray can select for the controller menu and
/// says which tabs apply. Pure lookup — no UI, no input.
/// </summary>
public class MenuTarget : MonoBehaviour
{
    public Surface Surface { get; private set; }
    public FurnitureSlot Slot { get; private set; }

    void Awake()
    {
        Surface = GetComponent<Surface>();
        Slot = GetComponent<FurnitureSlot>();
    }

    public MenuTab Tabs
    {
        get
        {
            if (Slot == null) Slot = GetComponent<FurnitureSlot>();
            if (Surface == null) Surface = GetComponent<Surface>();
            if (Slot != null) return MenuTab.Swap | MenuTab.Remove;
            if (Surface == null) return MenuTab.None;
            if (Surface.State == SurfaceState.Keep) return MenuTab.KeepPrompt;
            var tabs = MenuTab.Paint;
            if (Surface.Kind is SurfaceKind.Floor or SurfaceKind.Wall or SurfaceKind.Ceiling) tabs |= MenuTab.Material;
            if (Surface.Kind == SurfaceKind.Floor) tabs |= MenuTab.Furniture;
            return tabs;
        }
    }

    public string DisplayName => gameObject.name.Replace('_', ' ');

    void OnDisable()
    {
        // A target vanishing mid-preview must never leave a preview stuck.
        if (Surface != null && Surface.IsPreviewing) Surface.Revert();
    }
}
