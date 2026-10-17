using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

/// <summary>
/// While any furniture is held, the thumbsticks mean "rotate the piece", so every
/// LocomotionProvider on the rig is switched off. Counted, so two hands holding two
/// pieces release cleanly. XRI's ControllerInputActionManager already suppresses
/// locomotion during a far grab; this also covers near grabs.
/// </summary>
public static class LocomotionLock
{
    public static int Depth { get; private set; }
    static readonly List<LocomotionProvider> disabled = new();

    public static void Acquire()
    {
        if (Depth++ > 0) return;
        disabled.Clear();
        foreach (var p in Object.FindObjectsByType<LocomotionProvider>(FindObjectsSortMode.None))
            if (p.enabled) { p.enabled = false; disabled.Add(p); }
    }

    public static void Release()
    {
        if (Depth == 0) return;
        if (--Depth > 0) return;
        foreach (var p in disabled) if (p != null) p.enabled = true;
        disabled.Clear();
    }

    /// <summary>Tests only.</summary>
    public static void Reset() { Depth = 0; disabled.Clear(); }
}
