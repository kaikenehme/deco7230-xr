using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clear outline for furniture instead of a tinted frame (IP1: P1 and P3 asked for it).
/// Inverted hull: one slightly enlarged, front-culled, unlit copy of each mesh renderer
/// under the visual; only the rim survives the depth test. Shown while the ray hovers
/// the slot and while it is held.
/// </summary>
public class SelectionOutline : MonoBehaviour
{
    public const float ShellScale = 1.03f;
    /// <summary>URP/Unlit, amber, Cull Front. Assigned from the scene by RenovationConfig; a fallback is built if absent.</summary>
    public static Material ShellMaterial;

    public bool IsShown { get; private set; }
    public int ShellCount => shells.Count;

    readonly List<GameObject> shells = new();

    public static Material Fallback()
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "OutlineShell (runtime)" };
        ConfigureShellMaterial(m);
        return m;
    }

    public static void ConfigureShellMaterial(Material m)
    {
        m.color = UiKit.Accent;
        m.SetFloat("_Cull", 1f);   // front faces culled → only the rim shows
    }

    public void Show()
    {
        if (IsShown) return;
        IsShown = true;
        Rebuild();
    }

    public void Hide()
    {
        IsShown = false;
        Clear();
    }

    /// <summary>Call after the visual changes (FurnitureSlot.Swap). No-op while hidden.</summary>
    public void Rebuild()
    {
        Clear();
        if (!IsShown) return;
        var mat = ShellMaterial != null ? ShellMaterial : (ShellMaterial = Fallback());
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            if (mr.transform == transform || mr.GetComponent<OutlineShellTag>() != null) continue;
            var mf = mr.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            var shell = new GameObject("OutlineShell");
            shell.AddComponent<OutlineShellTag>();
            shell.transform.SetParent(mr.transform, false);
            shell.transform.localScale = Vector3.one * ShellScale;
            shell.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var r = shell.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shells.Add(shell);
        }
    }

    void Clear()
    {
        foreach (var s in shells) if (s != null) { if (Application.isPlaying) Destroy(s); else DestroyImmediate(s); }
        shells.Clear();
    }

    void OnDestroy() => Clear();
}

/// <summary>Marks shell objects so Rebuild never outlines an outline.</summary>
public class OutlineShellTag : MonoBehaviour { }
