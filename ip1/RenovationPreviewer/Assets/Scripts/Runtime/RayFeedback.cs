using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emission glow on whatever the right-hand ray is pointing at (and on the menu's
/// current target). Pure state machine — RayFeedback drives it every frame.
/// </summary>
public class HoverGlow
{
    public static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    public const string Keyword = "_EMISSION";

    readonly Dictionary<Renderer, Material> lit = new();

    public IReadOnlyCollection<Renderer> Lit => lit.Keys;

    /// <summary>Glow these renderers; anything previously lit but absent here is cleared.</summary>
    public void Set(IEnumerable<(Renderer rend, Color glow)> targets)
    {
        var keep = new HashSet<Renderer>();
        foreach (var (rend, glow) in targets)
        {
            if (rend == null) continue;
            keep.Add(rend);
            var m = Mat(rend);
            if (m == null) continue;
            m.EnableKeyword(Keyword);
            m.SetColor(EmissionId, glow);
            lit[rend] = m;
        }
        var stale = new List<Renderer>();
        foreach (var kv in lit) if (!keep.Contains(kv.Key)) stale.Add(kv.Key);
        foreach (var r in stale) Clear(r);
    }

    public void ClearAll()
    {
        foreach (var r in new List<Renderer>(lit.Keys)) Clear(r);
    }

    void Clear(Renderer r)
    {
        if (r != null)
        {
            var m = Mat(r);
            if (m != null) { m.SetColor(EmissionId, Color.black); m.DisableKeyword(Keyword); }
        }
        lit.Remove(r);
    }

    static Material Mat(Renderer r) => Application.isPlaying ? r.material : r.sharedMaterial;
}

/// <summary>
/// Where am I pointing, and is it selectable? Ring reticle at the ray hit
/// (amber over a MenuTarget, white otherwise), soft glow on the hovered target,
/// steady glow on the menu's current target. Right controller only.
/// </summary>
public class RayFeedback : MonoBehaviour
{
    public Transform rayOrigin;         // NearFar interactor transform
    public ControllerMenu menu;
    public Material emissionCarrier;    // any URP/Lit material with _EMISSION on, so the shader variant ships
    public float maxDistance = 6f;
    public float reticleSize = 0.04f;
    public Transform ignoreRoot;        // the XR Origin

    public static readonly Color HoverGlowColor = new Color(0.949f, 0.702f, 0.239f, 1f) * 0.35f;
    public static readonly Color CurrentGlowColor = new Color(0.949f, 0.702f, 0.239f, 1f) * 0.18f;

    public MenuTarget Hovered { get; private set; }
    public readonly HoverGlow glow = new();

    SpriteRenderer reticle;
    Transform head;

    void Awake()
    {
        var go = new GameObject("Reticle");
        go.transform.SetParent(transform, false);
        reticle = go.AddComponent<SpriteRenderer>();
        reticle.sprite = UiKit.Ring;
        reticle.color = Color.white;
        reticle.enabled = false;
        go.transform.localScale = Vector3.one * reticleSize;
    }

    /// <summary>Rotation that faces the ring along -normal; safe when the normal is collinear with world-up.</summary>
    public static Quaternion ReticleRotation(Vector3 normal)
    {
        var n = -normal;
        var up = Mathf.Abs(Vector3.Dot(n, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
        return Quaternion.LookRotation(n, up);
    }

    void OnDisable() { glow.ClearAll(); if (reticle != null) reticle.enabled = false; Hovered = null; }

    void Update()
    {
        if (head == null && Camera.main != null) head = Camera.main.transform;
        var origin = rayOrigin != null ? rayOrigin : transform;
        MenuTarget hit = null;
        if (RayUtil.TryHit(origin.position, origin.forward, maxDistance, ignoreRoot, out var h))
        {
            bool onPanel = h.collider.CompareTag("MenuPanel");
            hit = onPanel ? null : h.collider.GetComponentInParent<MenuTarget>();
            reticle.enabled = !onPanel;
            reticle.transform.position = h.point + h.normal * 0.004f;
            // Floor/ceiling hits make the normal collinear with world-up; LookRotation then degenerates
            // (NaN rotation → NaN bounds → "Invalid AABB" spam). Pick a safe up vector.
            reticle.transform.rotation = ReticleRotation(h.normal);
            reticle.color = hit != null ? UiKit.Accent : new Color(1f, 1f, 1f, 0.8f);
        }
        else reticle.enabled = false;
        Hovered = hit;

        var targets = new List<(Renderer, Color)>();
        if (hit != null) foreach (var r in hit.GetComponentsInChildren<Renderer>()) targets.Add((r, HoverGlowColor));
        var cur = menu != null && menu.IsOpen ? menu.Current : null;
        if (cur != null && cur != hit) foreach (var r in cur.GetComponentsInChildren<Renderer>()) targets.Add((r, CurrentGlowColor));
        glow.Set(targets);
    }
}
