using System.Collections.Generic;
using UnityEngine;

public enum SurfaceState { Keep, Change }

/// <summary>
/// State machine for one renovatable surface. Holds Keep/Change state and the
/// committed vs previewed colour. Never knows samples exist (concept spec §8).
/// ExecuteAlways so the static registry works in EditMode tests and scene tooling;
/// renderer writes are guarded to play mode.
/// </summary>
[ExecuteAlways]
public class Surface : MonoBehaviour
{
    public static readonly List<Surface> All = new();

    [SerializeField] SurfaceState state = SurfaceState.Change;

    public SurfaceState State => state;
    public Color CommittedColor { get; private set; } = Color.white;
    public Color DisplayColor { get; private set; } = Color.white;
    public bool IsPreviewing { get; private set; }

    Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
            CommittedColor = DisplayColor = rend.sharedMaterial.color;
    }

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable() => All.Remove(this);

    public void SetState(SurfaceState s) => state = s;

    public void ToggleState() =>
        state = state == SurfaceState.Keep ? SurfaceState.Change : SurfaceState.Keep;

    public void Preview(Color c)
    {
        if (state == SurfaceState.Keep) return; // kept surfaces are sources, never targets
        IsPreviewing = true;
        Apply(c);
    }

    public void Commit(Color c)
    {
        if (state == SurfaceState.Keep) return;
        CommittedColor = c;
        IsPreviewing = false;
        Apply(c);
    }

    public void Revert()
    {
        IsPreviewing = false;
        Apply(CommittedColor);
    }

    void Apply(Color c)
    {
        DisplayColor = c;
        if (Application.isPlaying && rend != null)
            rend.material.color = c;
    }
}
