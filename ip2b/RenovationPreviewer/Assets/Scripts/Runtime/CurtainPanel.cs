using UnityEngine;

/// <summary>On each curtain panel: a controller cue brushing it touches the curtains (device path).</summary>
public class CurtainPanel : MonoBehaviour
{
    public CurtainController controller;

    void OnTriggerEnter(Collider other)
    {
        if (controller != null && other.GetComponent<MarkTool>() != null) controller.Touch();
    }
}
