using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Ten seconds of in-world onboarding on entry (Evaluation 1 §05: diegetic is not the
/// same as discoverable). The kept surfaces pulse amber twice under a floating
/// "staying" label, then the lamp pulses. No panels, no menus. Runs once the first
/// preset has been applied so the kept furniture exists.
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
    readonly List<TextMesh> labels = new();

    public static IEnumerable<Surface> Targets(IEnumerable<Surface> all) =>
        all.Where(s => s != null && s.State == SurfaceState.Keep);

    /// <summary>Where the "staying" label floats: just above the piece.</summary>
    public static Vector3 LabelAnchor(Bounds b, Vector3 head) => new(b.center.x, b.max.y + 0.25f, b.center.z);

    /// <summary>Kept furniture gets a "staying" label; the floor only pulses (a label for it floats mid-room).</summary>
    public static bool Labelled(SurfaceKind kind) => kind != SurfaceKind.Floor;

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
        var targets = Targets(Surface.All).ToList();
        foreach (var s in targets) if (Labelled(s.Kind)) labels.Add(MakeLabel(s));
        Face();

        float t0 = Time.time;
        for (int pulse = 0; pulse < 2; pulse++)
        {
            for (float t = 0f; t < PulseSeconds; t += Time.deltaTime)
            {
                float k = 0.5f - 0.5f * Mathf.Cos(t / PulseSeconds * 2f * Mathf.PI);   // 0 → 1 → 0
                var list = new List<(Renderer, Color)>();
                foreach (var s in targets) if (s != null) foreach (var r in s.Renderers) list.Add((r, RayFeedback.HoverGlowColor * (0.1f + 0.5f * k)));   // a glow, not a floodlight: the floor is 63 m²
                glow.Set(list);
                Face();
                yield return null;
            }
        }
        glow.ClearAll();
        if (lamp != null) lamp.Pulse(3f);

        while (Time.time - t0 < Duration)
        {
            Face();
            yield return null;
        }
        foreach (var l in labels) if (l != null) Destroy(l.gameObject);
        labels.Clear();
        Running = false;
        Done = true;
    }

    /// <summary>TextMesh reads correctly when its +Z points away from the viewer.</summary>
    void Face()
    {
        if (head == null) return;
        foreach (var l in labels)
        {
            if (l == null) continue;
            var away = l.transform.position - head.position; away.y = 0f;
            if (away.sqrMagnitude > 1e-4f) l.transform.rotation = Quaternion.LookRotation(away.normalized, Vector3.up);
        }
    }

    TextMesh MakeLabel(Surface s)
    {
        var go = new GameObject($"Staying ({s.name})");
        var headPos = head != null ? head.position : Vector3.up * 1.6f;
        go.transform.position = LabelAnchor(s.WorldBounds, headPos);
        var tm = go.AddComponent<TextMesh>();
        tm.text = "staying";
        tm.font = UiKit.Font;
        tm.fontSize = 64;
        tm.characterSize = 0.04f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = UiKit.Accent;
        go.GetComponent<MeshRenderer>().sharedMaterial = UiKit.Font.material;
        return tm;
    }
}
