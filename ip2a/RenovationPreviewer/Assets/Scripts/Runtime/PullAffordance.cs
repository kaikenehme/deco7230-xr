using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// First-contact feedback on kept surfaces. IP1 finding P-a: 0/5 pulled a sample, yet
/// all five touched a kept surface during free look and got nothing back. Now a swatch
/// tab in the surface's sample colour appears at the point nearest an approaching hand,
/// lifts when the hand is within touch range, and hides briefly after a pull so the
/// sample visibly leaves it. State lives in Tick() (pure, testable); the visual is an
/// unparented sprite because surfaces are scaled cubes and a child would inherit the squash.
/// </summary>
[RequireComponent(typeof(Surface))]
public class PullAffordance : MonoBehaviour
{
    public const float ShowRadius = 0.35f, TouchRadius = 0.10f, TabSize = 0.06f;
    public const float SurfaceOffset = 0.01f, LiftOffset = 0.03f, PeelHide = 0.5f;

    Surface surface;
    public Surface Surface => surface != null ? surface : (surface = GetComponent<Surface>());   // lazy: Awake doesn't run in EditMode
    public bool IsVisible { get; private set; }
    public bool IsLifted { get; private set; }
    public Vector3 TabPosition { get; private set; }
    public Vector3 TabNormal { get; private set; } = Vector3.up;
    public Color TabColor => Surface != null ? Surface.SampleColor : Color.white;

    float hiddenUntil = -1f;
    Transform tab;
    SpriteRenderer disc;

    void Awake()
    {
        if (Application.isPlaying) BuildTab();
    }

    void OnDisable() { IsVisible = false; IsLifted = false; if (tab != null) tab.gameObject.SetActive(false); }
    void OnDestroy() { if (tab != null) Destroy(tab.gameObject); }

    void BuildTab()
    {
        var go = new GameObject($"PullTab ({name})");
        tab = go.transform;
        disc = go.AddComponent<SpriteRenderer>();
        disc.sprite = UiKit.Circle;
        var ringGo = new GameObject("Ring");
        ringGo.transform.SetParent(tab, false);
        ringGo.transform.localPosition = new Vector3(0f, 0f, -0.001f);
        var ring = ringGo.AddComponent<SpriteRenderer>();
        ring.sprite = UiKit.Ring;
        ring.color = Color.white;
        go.SetActive(false);
    }

    /// <summary>The kept surface a hand at p could pull from: nearest within ShowRadius, else null.</summary>
    public static Surface NearestPullable(Vector3 p, out float distance)
    {
        Surface best = null; distance = ShowRadius;
        foreach (var s in Surface.All)
        {
            if (s == null || s.State != SurfaceState.Keep) continue;
            float d = Vector3.Distance(s.ClosestPoint(p), p);
            if (d < distance) { distance = d; best = s; }
        }
        return best;
    }

    /// <summary>Pure state update. now = Time.time, passed in so tests own the clock.</summary>
    public void Tick(IReadOnlyList<Vector3> hands, float now)
    {
        IsVisible = false; IsLifted = false;
        if (Surface == null || Surface.State != SurfaceState.Keep || now < hiddenUntil || hands == null) return;

        float best = ShowRadius; Vector3 bestPt = default, bestHand = default; bool any = false;
        foreach (var h in hands)
        {
            var q = Surface.ClosestPoint(h);
            float d = Vector3.Distance(q, h);
            if (d < best) { best = d; bestPt = q; bestHand = h; any = true; }
        }
        if (!any) return;

        IsVisible = true;
        IsLifted = best < TouchRadius;
        var n = bestHand - bestPt;
        if (n.sqrMagnitude > 1e-6f) TabNormal = n.normalized;
        TabPosition = bestPt + TabNormal * (IsLifted ? LiftOffset : SurfaceOffset);
    }

    /// <summary>A sample just left this tab: hide it for a beat so the pull reads as "taken".</summary>
    public void Peel(float now) { hiddenUntil = now + PeelHide; IsVisible = false; IsLifted = false; }

    void Update()
    {
        if (!Application.isPlaying) return;
        Tick(HandPositions(), Time.time);
        if (tab == null) return;
        tab.gameObject.SetActive(IsVisible);
        if (!IsVisible) return;
        tab.position = TabPosition;
        tab.rotation = RayFeedback.ReticleRotation(TabNormal);
        tab.localScale = Vector3.one * (IsLifted ? TabSize * 1.25f : TabSize);
        disc.color = TabColor;
    }

    static readonly List<Vector3> handBuf = new();
    static IReadOnlyList<Vector3> HandPositions()
    {
        handBuf.Clear();
        foreach (var m in MarkTool.All) if (m != null && m.isActiveAndEnabled) handBuf.Add(m.transform.position);
        return handBuf;
    }
}
