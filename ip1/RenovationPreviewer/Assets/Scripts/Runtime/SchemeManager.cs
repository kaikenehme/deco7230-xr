using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Snapshot / restore of all Change-surface committed colours. Up to 3 schemes,
/// oldest slot overwritten on overflow (concept spec §8).
/// </summary>
public class SchemeManager : MonoBehaviour
{
    public const int MaxSchemes = 3;

    readonly List<Dictionary<Surface, Color>> schemes = new();
    int nextSlot;

    public int Count => schemes.Count;

    public int SaveScheme()
    {
        var snapshot = new Dictionary<Surface, Color>();
        foreach (var s in Surface.All)
            if (s != null && s.State == SurfaceState.Change)
                snapshot[s] = s.CommittedColor;

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
            if (kv.Key != null) kv.Key.Commit(kv.Value);
    }
}
