using System.Collections.Generic;
using UnityEngine;

public enum SurfaceState { Keep, Change }

/// <summary>
/// State machine for one renovatable surface. Holds Keep/Change state and the
/// committed vs previewed colour AND material. Never knows samples or menus
/// exist (concept spec §8). ExecuteAlways so the static registry works in
/// EditMode tests and scene tooling; renderer writes are guarded to play mode.
///
/// A surface may be compound: one logical wall made of several child cubes
/// (a wall with a window opening), or a furniture prefab with several meshes.
/// Every renderer/collider under the root belongs to it.
/// </summary>
[ExecuteAlways]
public class Surface : MonoBehaviour
{
    public static readonly List<Surface> All = new();

    [SerializeField] SurfaceState state = SurfaceState.Change;
    [SerializeField] SurfaceKind kind = SurfaceKind.None;
    [SerializeField] Color sampleColor = Color.white;
    [SerializeField] bool hasSampleColor;
    /// <summary>Preset baseline material (catalogue tiles-per-metre convention); null = each renderer's own shipped material.</summary>
    [SerializeField] Material baseOverride;

    public SurfaceState State => state;
    public SurfaceKind Kind => kind;

    public Color CommittedColor { get; private set; } = Color.white;
    public Color DisplayColor { get; private set; } = Color.white;
    /// <summary>Material committed via the menu; null = whatever the scene shipped with.</summary>
    public Material CommittedMaterial { get; private set; }
    public Material DisplayMaterial { get; private set; }
    public bool IsPreviewing { get; private set; }
    /// <summary>True once a paint colour was explicitly chosen. Until then a menu material
    /// shows untinted (white) — CommittedColor still holds the scene's base colour, and
    /// tinting a texture with that base is the "wall turns default blue" bug.</summary>
    public bool HasUserColour { get; private set; }

    /// <summary>The colour a pulled sample starts from. A user paint wins; otherwise the
    /// authored sample colour (needed for textured surfaces whose tint is white); otherwise
    /// the display tint.</summary>
    public Color SampleColor => HasUserColour ? DisplayColor : hasSampleColor ? sampleColor : DisplayColor;

    readonly List<Renderer> renderers = new();
    readonly List<Material> shipped = new();     // each renderer's original material, index-parallel
    readonly List<Collider> colliders = new();

    public IReadOnlyList<Renderer> Renderers => renderers;
    public IReadOnlyList<Collider> Colliders => colliders;

    void Awake() => RebindRenderer();

    /// <summary>Re-read renderers and colliders (root + children). Call after swapping the visual.</summary>
    public void RebindRenderer()
    {
        renderers.Clear(); shipped.Clear(); colliders.Clear();
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
        {
            renderers.Add(r);
            shipped.Add(r.sharedMaterial);
        }
        foreach (var c in GetComponentsInChildren<Collider>())
            if (!c.isTrigger) colliders.Add(c);

        if (renderers.Count > 0 && renderers[0].sharedMaterial != null)
        {
            CommittedColor = DisplayColor = renderers[0].sharedMaterial.color;
            HasUserColour = false;
        }
    }

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable() => All.Remove(this);

    public void SetState(SurfaceState s) => state = s;
    public void SetKind(SurfaceKind k) => kind = k;

    public void ToggleState() =>
        state = state == SurfaceState.Keep ? SurfaceState.Change : SurfaceState.Keep;

    public void SetBaseSampleColor(Color c) { sampleColor = c; hasSampleColor = true; }

    /// <summary>Preset baseline: redefine what "original" means for this surface. Bypasses
    /// the Keep guard on purpose (presets dress kept surfaces too), wipes any user choice, and
    /// applies immediately. baseMat null keeps each renderer's shipped material.</summary>
    public void SetBaseLook(Material baseMat, Color baseColor, Color sample, SurfaceState newState)
    {
        state = newState;
        baseOverride = baseMat;
        CommittedMaterial = null;
        CommittedColor = baseColor;
        HasUserColour = false;
        IsPreviewing = false;
        SetBaseSampleColor(sample);
        ApplyMaterial(null);
        ApplyColor(baseColor, false);
    }

    // ---- geometry ----
    public Bounds WorldBounds
    {
        get
        {
            bool any = false; var b = new Bounds(transform.position, Vector3.zero);
            foreach (var r in renderers) { if (r == null) continue; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
            if (!any) foreach (var c in colliders) { if (c == null) continue; if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds); }
            return b;
        }
    }

    /// <summary>Nearest point on any of this surface's colliders (falls back to the renderer bounds).</summary>
    public Vector3 ClosestPoint(Vector3 p)
    {
        bool any = false; float best = float.MaxValue; var bestPt = p;
        foreach (var c in colliders)
        {
            if (c == null) continue;
            var q = c.ClosestPoint(p);
            float d = (q - p).sqrMagnitude;
            if (d < best) { best = d; bestPt = q; any = true; }
        }
        return any ? bestPt : WorldBounds.ClosestPoint(p);
    }

    // ---- colour ----
    public void Preview(Color c)
    {
        if (state == SurfaceState.Keep) return; // kept surfaces are sources, never targets
        IsPreviewing = true;
        ApplyColor(c, true);
    }

    public void Commit(Color c)
    {
        if (state == SurfaceState.Keep) return;
        CommittedColor = c;
        HasUserColour = true;
        IsPreviewing = false;
        ApplyColor(c, true);
    }

    // ---- material ----
    public void PreviewMaterial(Material m)
    {
        if (state == SurfaceState.Keep) return;
        IsPreviewing = true;
        ApplyMaterial(m);
    }

    public void CommitMaterial(Material m)
    {
        if (state == SurfaceState.Keep) return;
        CommittedMaterial = m;
        IsPreviewing = false;
        ApplyMaterial(m);
    }

    /// <summary>Scheme restore: put back a saved look verbatim, including whether the
    /// colour was a user choice (drives the material tint rule).</summary>
    public void Restore(Color c, Material m, bool userColour)
    {
        if (state == SurfaceState.Keep) return;
        CommittedMaterial = m;
        CommittedColor = c;
        HasUserColour = userColour;
        IsPreviewing = false;
        ApplyMaterial(m);
        ApplyColor(c, userColour);
    }

    /// <summary>A preview must never stick: restore committed material then committed colour.</summary>
    public void Revert()
    {
        IsPreviewing = false;
        ApplyMaterial(CommittedMaterial);
        ApplyColor(CommittedColor, HasUserColour);
    }

    void ApplyColor(Color c, bool userIntent)
    {
        DisplayColor = c;
        if (!Application.isPlaying) return;
        // A catalogue material (menu pick or preset base) shows untinted until a paint was chosen.
        bool tiled = DisplayMaterial != null || baseOverride != null;
        var tint = userIntent || !tiled ? c : Color.white;
        foreach (var r in renderers) if (r != null) r.material.color = tint;
    }

    void ApplyMaterial(Material m)
    {
        DisplayMaterial = m;
        if (!Application.isPlaying) return;
        for (int i = 0; i < renderers.Count; i++)
        {
            var r = renderers[i];
            if (r == null) continue;
            var tiledSrc = m != null ? m : baseOverride;
            var src = tiledSrc != null ? tiledSrc : shipped[i];
            if (src == null) continue;
            // Instance so tint edits never write into the shared asset. A catalogue material
            // shows untinted until a paint colour was explicitly chosen (WYSIWYG with hover).
            var inst = new Material(src) { color = tiledSrc != null && !HasUserColour ? Color.white : DisplayColor };
            // Catalogue materials store tiles-per-metre in mainTextureScale; a primitive cube's
            // UVs span 0..1 per face, so multiply by the face size (two largest cube axes).
            // Shipped prefab materials are UV-mapped and keep their own scale.
            if (tiledSrc != null)
            {
                var sc = r.transform.lossyScale;
                float a = Mathf.Max(sc.x, sc.y, sc.z);
                float cmin = Mathf.Min(sc.x, sc.y, sc.z);
                float b = sc.x + sc.y + sc.z - a - cmin;
                inst.mainTextureScale = new Vector2(src.mainTextureScale.x * a, src.mainTextureScale.y * b);
            }
            r.material = inst;
        }
    }
}
