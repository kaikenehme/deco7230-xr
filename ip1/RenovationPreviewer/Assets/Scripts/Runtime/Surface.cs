using System.Collections.Generic;
using UnityEngine;

public enum SurfaceState { Keep, Change }

/// <summary>
/// State machine for one renovatable surface. Holds Keep/Change state and the
/// committed vs previewed colour AND material. Never knows samples or menus
/// exist (concept spec §8). ExecuteAlways so the static registry works in
/// EditMode tests and scene tooling; renderer writes are guarded to play mode.
/// </summary>
[ExecuteAlways]
public class Surface : MonoBehaviour
{
    public static readonly List<Surface> All = new();

    [SerializeField] SurfaceState state = SurfaceState.Change;
    [SerializeField] SurfaceKind kind = SurfaceKind.None;

    public SurfaceState State => state;
    public SurfaceKind Kind => kind;

    public Color CommittedColor { get; private set; } = Color.white;
    public Color DisplayColor { get; private set; } = Color.white;
    /// <summary>Material committed via the menu; null = whatever the scene shipped with.</summary>
    public Material CommittedMaterial { get; private set; }
    public Material DisplayMaterial { get; private set; }
    public bool IsPreviewing { get; private set; }

    Renderer rend;
    Material baseMaterial;   // the material the scene shipped with, for revert-to-original

    void Awake() => RebindRenderer();

    /// <summary>Re-read the renderer (root or first child). Call after swapping the visual.</summary>
    public void RebindRenderer()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
        {
            baseMaterial = rend.sharedMaterial;
            CommittedColor = DisplayColor = rend.sharedMaterial.color;
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

    // ---- colour ----
    public void Preview(Color c)
    {
        if (state == SurfaceState.Keep) return; // kept surfaces are sources, never targets
        IsPreviewing = true;
        ApplyColor(c);
    }

    public void Commit(Color c)
    {
        if (state == SurfaceState.Keep) return;
        CommittedColor = c;
        IsPreviewing = false;
        ApplyColor(c);
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

    /// <summary>A preview must never stick: restore committed material then committed colour.</summary>
    public void Revert()
    {
        IsPreviewing = false;
        ApplyMaterial(CommittedMaterial);
        ApplyColor(CommittedColor);
    }

    void ApplyColor(Color c)
    {
        DisplayColor = c;
        if (Application.isPlaying && rend != null)
            rend.material.color = c;
    }

    void ApplyMaterial(Material m)
    {
        DisplayMaterial = m;
        if (!Application.isPlaying || rend == null) return;
        var src = m != null ? m : baseMaterial;
        if (src == null) return;
        // Instance so tint edits never write into the shared asset.
        var inst = new Material(src) { color = DisplayColor };
        // Catalogue materials store tiles-per-metre in mainTextureScale; a primitive cube's
        // UVs span 0..1 per face, so multiply by the face size (two largest cube axes).
        if (m != null)
        {
            var sc = transform.lossyScale;
            float a = Mathf.Max(sc.x, sc.y, sc.z);
            float c = Mathf.Min(sc.x, sc.y, sc.z);
            float b = sc.x + sc.y + sc.z - a - c;
            inst.mainTextureScale = new Vector2(src.mainTextureScale.x * a, src.mainTextureScale.y * b);
        }
        rend.material = inst;
    }
}
