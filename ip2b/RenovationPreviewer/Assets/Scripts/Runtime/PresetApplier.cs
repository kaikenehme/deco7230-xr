using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Dresses the room in one of the presets: clears every non-scene furniture slot, gives
/// each shell surface its baseline look by name, spawns the preset's furniture. Preset 0
/// applies at Start. Frames on the wall (PresetFrame) call Apply; the editor also takes
/// F1–F3 for the facilitator. A bare preset (the Empty room) hides the room-level dressing
/// and resets the time of day to the start stop; other presets show the dressing and keep the time.
/// </summary>
public class PresetApplier : MonoBehaviour
{
    public Catalogue catalogue;
    public RoomPreset[] presets;
    public Bounds floorBounds;
    /// <summary>Room-level dressing built by SceneBuilder (rug, prints, arc lamp): hidden in a bare preset.</summary>
    public GameObject[] dressing = Array.Empty<GameObject>();
    public TimeOfDayController timeOfDay;

    public int Current { get; private set; } = -1;
    public event Action<int> Applied;

    void Start() => Apply(0);

    public static IEnumerable<FurnitureSlot> SlotsToClear(IEnumerable<FurnitureSlot> all) =>
        all.Where(s => s != null && s.Origin != SlotOrigin.Scene);

    public void Apply(int i)
    {
        if (presets == null || presets.Length == 0) return;
        i = Mathf.Clamp(i, 0, presets.Length - 1);
        var preset = presets[i];
        if (preset == null) return;

        // Review Focus #4: a new room starts clean — curtains open, no sample/fan left in a hand.
        foreach (var c in FindObjectsByType<CurtainController>(FindObjectsSortMode.None)) c.SetOpen(true, instant: true);
        foreach (var smp in FindObjectsByType<Sample>(FindObjectsSortMode.None))
        {
            smp.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(smp.gameObject); else DestroyImmediate(smp.gameObject);
        }

        foreach (var slot in SlotsToClear(FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None)).ToList())
        {
            slot.gameObject.SetActive(false);   // out of Surface.All and FindObjects this frame
            if (Application.isPlaying) Destroy(slot.gameObject); else DestroyImmediate(slot.gameObject);
        }

        // Empty room: rug, prints and arc lamp go (a hidden rug also leaves Surface.All, so it can't be pulled from).
        if (dressing != null)
            foreach (var d in dressing) if (d != null) d.SetActive(!preset.bare);

        foreach (var look in preset.looks)
        {
            var s = Surface.All.FirstOrDefault(x => x != null && x.name == look.surfaceName);
            if (s == null) { Debug.LogWarning($"PresetApplier: no surface named '{look.surfaceName}'"); continue; }
            s.SetBaseLook(look.material, look.color, look.sampleColor, look.state);
            if (look.userColour && look.state == SurfaceState.Change) s.Commit(look.color);
        }

        // Only the Empty room resets the light: without the arc lamp it would otherwise open in Phase 3's evening.
        if (preset.bare && timeOfDay != null) timeOfDay.Apply(TimeOfDayController.StartIndex);

        foreach (var pl in preset.furniture)
        {
            var opt = catalogue != null ? catalogue.Furniture(pl.sourceId) : null;
            if (opt == null || opt.prefab == null) { Debug.LogWarning($"PresetApplier: '{pl.sourceId}' not in the catalogue, skipped"); continue; }
            FurnitureSlot.Spawn(opt, pl.position, pl.yaw, floorBounds, pl.keep, pl.sampleColor, SlotOrigin.Preset);
        }

        Current = i;
        Applied?.Invoke(i);
    }

#if UNITY_EDITOR
    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.f1Key.wasPressedThisFrame) Apply(0);
        else if (kb.f2Key.wasPressedThisFrame) Apply(1);
        else if (kb.f3Key.wasPressedThisFrame) Apply(2);
    }
#endif
}
