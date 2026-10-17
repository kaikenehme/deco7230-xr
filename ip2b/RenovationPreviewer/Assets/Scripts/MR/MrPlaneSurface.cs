using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// On the AR plane prefab: once the plane is classified as a wall or floor, it becomes a
/// paintable Surface. Invisible until painted; a preview shows at 60 %, a commit at 80 %, so
/// the real wall still reads through the paint (passthrough colour is not accurate — the
/// showcase asks "would you use this at home?", not "is this the right colour?").
/// </summary>
[RequireComponent(typeof(ARPlane))]
public class MrPlaneSurface : MonoBehaviour
{
    public const float PreviewAlpha = 0.6f, CommittedAlpha = 0.8f;
    const float ClassifyTimeout = 3f;

    ARPlane plane;
    Surface surface;
    Renderer rend;
    float born;
    bool committedLogged;

    public static float AlphaFor(bool previewing, bool committed) => previewing ? PreviewAlpha : committed ? CommittedAlpha : 0f;

    void Awake() { plane = GetComponent<ARPlane>(); rend = GetComponent<Renderer>(); born = Time.time; }

    void Update()
    {
        if (surface != null || plane == null) return;
        var kind = MrModeController.KindFor(plane.classifications);
        if (kind == SurfaceKind.None)
        {
            if (Time.time - born > ClassifyTimeout) enabled = false;   // a table, ceiling, door… stays unpaintable
            return;
        }
        surface = gameObject.AddComponent<Surface>();
        surface.SetState(SurfaceState.Change);
        surface.SetKind(kind);
        surface.RebindRenderer();
        gameObject.name = $"MR_{kind}_{plane.trackableId}";
    }

    void LateUpdate()
    {
        if (surface == null || rend == null) return;
        var c = rend.material.color;
        c.a = AlphaFor(surface.IsPreviewing, surface.HasUserColour);
        rend.material.color = c;
        if (surface.HasUserColour && !committedLogged) { committedLogged = true; Debug.Log("[IP2b] mr_commit " + surface.Kind); }
    }
}
