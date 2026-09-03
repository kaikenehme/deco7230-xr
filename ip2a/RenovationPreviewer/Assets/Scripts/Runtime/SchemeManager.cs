using System.Collections.Generic;
using UnityEngine;

/// <summary>What a Change surface looked like when a scheme was saved.</summary>
public struct SurfaceLook
{
    public Color color;
    public Material material;   // null = the material the scene shipped with
    public bool userColour;     // was the colour an explicit paint choice? (tint rule)
}

/// <summary>
/// Snapshot / restore of all Change-surface committed looks (material + colour).
/// Up to 3 schemes, oldest slot overwritten on overflow (concept spec §8).
/// </summary>
public class SchemeManager : MonoBehaviour
{
    public const int MaxSchemes = 3;

    readonly List<Dictionary<Surface, SurfaceLook>> schemes = new();
    int nextSlot;

    public int Count => schemes.Count;

    public int SaveScheme()
    {
        var snapshot = new Dictionary<Surface, SurfaceLook>();
        foreach (var s in Surface.All)
            if (s != null && s.State == SurfaceState.Change)
                snapshot[s] = new SurfaceLook { color = s.CommittedColor, material = s.CommittedMaterial, userColour = s.HasUserColour };

        int slot = nextSlot;
        if (schemes.Count < MaxSchemes) schemes.Add(snapshot);
        else schemes[slot] = snapshot;
        nextSlot = (slot + 1) % MaxSchemes;
        return slot;
    }

    public void ApplyScheme(int slot)
    {
        if (slot < 0 || slot >= schemes.Count) return;
        foreach (var kv in schemes[slot])
        {
            if (kv.Key == null) continue;
            kv.Key.Restore(kv.Value.color, kv.Value.material, kv.Value.userColour);
        }
    }
}
