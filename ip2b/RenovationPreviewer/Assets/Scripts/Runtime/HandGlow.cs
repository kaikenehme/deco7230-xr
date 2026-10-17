using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The controller itself glows amber while it is close enough to a kept surface to
/// pull from it — the hand-side half of the first-contact affordance (the surface-side
/// half is PullAffordance). Reuses HoverGlow so the look matches the ray feedback.
/// </summary>
public class HandGlow : MonoBehaviour
{
    public Color glow = RayFeedback.HoverGlowColor;
    public bool IsGlowing { get; private set; }
    public readonly HoverGlow hover = new();

    MeshRenderer[] renderers;
    readonly List<(Renderer, Color)> targets = new();

    void Awake() => renderers = GetComponentsInChildren<MeshRenderer>(true);
    void OnDisable() { hover.ClearAll(); IsGlowing = false; }

    void Update()
    {
        if (renderers == null) return;
        IsGlowing = PullAffordance.NearestPullable(transform.position, out _) != null;
        targets.Clear();
        if (IsGlowing) foreach (var r in renderers) targets.Add((r, glow));
        hover.Set(targets);
    }
}
