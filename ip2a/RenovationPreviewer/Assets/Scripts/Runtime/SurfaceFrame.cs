using UnityEngine;

/// <summary>
/// Hover highlight for a paintable surface: an amber rectangle drawn on the face the
/// ray hit, instead of lighting the whole wall (IP2a feel pass, 22 Sep: "frame, not the
/// whole object" — the same ask P1/P3 made for furniture in IP1, see SelectionOutline).
/// The rectangle is the surface's world bounds projected on that face, so a compound
/// surface such as the window wall gets one frame.
/// </summary>
[RequireComponent(typeof(Surface))]
public class SurfaceFrame : MonoBehaviour
{
    public const float Width = 0.02f;
    public const float Inset = 0.03f;
    public const float Lift = 0.005f;

    public bool IsShown { get; private set; }

    Surface surface;
    LineRenderer line;

    /// <summary>
    /// Four corners of the bounds face that points along <paramref name="normal"/> (snapped to
    /// its dominant axis), pulled in by <paramref name="inset"/> and pushed out by <paramref name="lift"/>.
    /// </summary>
    public static Vector3[] Corners(Bounds b, Vector3 normal, float inset, float lift)
    {
        int axis = 0;
        for (int i = 1; i < 3; i++) if (Mathf.Abs(normal[i]) > Mathf.Abs(normal[axis])) axis = i;
        float sign = Mathf.Sign(normal[axis]);
        int u = (axis + 1) % 3, v = (axis + 2) % 3;

        var centre = b.center;
        centre[axis] += sign * (b.extents[axis] + lift);
        float eu = Mathf.Max(0f, b.extents[u] - inset), ev = Mathf.Max(0f, b.extents[v] - inset);

        var c = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            var p = centre;
            p[u] += (i == 0 || i == 3) ? -eu : eu;
            p[v] += (i < 2) ? -ev : ev;
            c[i] = p;
        }
        return c;
    }

    public LineRenderer EnsureLine()
    {
        if (line != null) return line;
        var go = new GameObject("SurfaceFrame");
        go.transform.SetParent(transform, false);
        line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = line.endWidth = Width;
        line.numCornerVertices = 2;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = UiKit.Accent;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.enabled = false;
        return line;
    }

    Vector3 lastNormal = Vector3.up;

    public void Show(Vector3 hitNormal)
    {
        if (surface == null) surface = GetComponent<Surface>();
        lastNormal = hitNormal;
        var l = EnsureLine();
        l.SetPositions(Corners(surface.WorldBounds, hitNormal, Inset, Lift));
        l.enabled = true;
        IsShown = true;
    }

    /// <summary>Keep framing the face last hit — the menu is open on this surface and the ray has moved on.</summary>
    public void Show() => Show(lastNormal);

    public void Hide()
    {
        if (line != null) line.enabled = false;
        IsShown = false;
    }

    void OnDisable() => Hide();
}
