using UnityEngine;

/// <summary>On each curtain panel: a controller cue brushing it touches the curtains (device path).</summary>
public class CurtainPanel : MonoBehaviour
{
    public CurtainController controller;

    /// <summary>Only an empty hand's controller cue draws the curtains (review I4: holding a chip by the window flipped them).</summary>
    public static bool Accepts(bool isCue, bool handHoldingSample) => isCue && !handHoldingSample;

    void OnTriggerEnter(Collider other)
    {
        if (controller == null) return;
        var puller = other.GetComponentInParent<SamplePuller>();
        if (puller == null) puller = other.GetComponentInChildren<SamplePuller>();
        if (Accepts(other.GetComponent<MarkTool>() != null, puller != null && puller.Held != null)) controller.Touch();
    }
}
