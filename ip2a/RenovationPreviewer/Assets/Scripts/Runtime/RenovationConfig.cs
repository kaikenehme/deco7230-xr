using UnityEngine;

/// <summary>Scene-side references for statics that need a serialized asset (so shader variants ship in the build).</summary>
public class RenovationConfig : MonoBehaviour
{
    public Material outlineShell;

    void Awake()
    {
        if (outlineShell != null) SelectionOutline.ShellMaterial = outlineShell;
    }
}
