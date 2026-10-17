using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Ten seconds of in-world onboarding on entry (Evaluation 1 §05: diegetic is not the
/// same as discoverable). IP2b: nothing is "staying", so no labels — the room's hero piece
/// (sofa / bed) pulses twice so people know they can touch it, then the lamp pulses.
/// No panels, no menus. Runs once the first preset has been applied.
/// </summary>
public class OnboardingSequence : MonoBehaviour
{
    public const float Duration = 10f, PulseSeconds = 1.2f;
    public PresetApplier presets;
    public LampController lamp;
    public Transform head;

    public bool Running { get; private set; }
    public bool Done { get; private set; }

    readonly HoverGlow glow = new();

    /// <summary>The largest furniture piece by footprint (sofa in Living, bed in Bedroom); null if none.</summary>
    public static Surface Hero(IEnumerable<Surface> all) =>
        all.Where(s => s != null && s.GetComponent<FurnitureSlot>() != null)
           .OrderByDescending(s => { var b = s.WorldBounds; return b.size.x * b.size.z; })
           .FirstOrDefault();

    void OnEnable()
    {
        if (presets != null && presets.Current < 0) presets.Applied += OnFirstApplied;
        else StartCoroutine(Run());
    }

    void OnFirstApplied(int _)
    {
        presets.Applied -= OnFirstApplied;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (Running || Done) yield break;
        Running = true;
        var hero = Hero(Surface.All);
        float t0 = Time.time;
        for (int pulse = 0; pulse < 2 && hero != null; pulse++)
        {
            for (float t = 0f; t < PulseSeconds; t += Time.deltaTime)
            {
                float k = 0.5f - 0.5f * Mathf.Cos(t / PulseSeconds * 2f * Mathf.PI);   // 0 → 1 → 0
                var list = new List<(Renderer, Color)>();
                if (hero != null) foreach (var r in hero.Renderers) list.Add((r, RayFeedback.HoverGlowColor * (0.1f + 0.5f * k)));
                glow.Set(list);
                yield return null;
            }
        }
        glow.ClearAll();
        if (lamp != null) lamp.Pulse(3f);
        while (Time.time - t0 < Duration) yield return null;
        Running = false;
        Done = true;
    }
}
