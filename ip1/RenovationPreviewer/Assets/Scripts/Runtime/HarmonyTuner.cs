using UnityEngine;

/// <summary>
/// Twist-to-tune: maps controller roll (degrees, relative to grab pose) onto an
/// index in the sample's harmony palette and keeps the sample's colour current.
/// </summary>
[RequireComponent(typeof(Sample))]
public class HarmonyTuner : MonoBehaviour
{
    Sample sample;
    Renderer rend;

    void Awake()
    {
        sample = GetComponent<Sample>();
        rend = GetComponent<Renderer>();
    }

    public static int RollToIndex(float rollDegrees, int paletteSize)
    {
        float t = Mathf.InverseLerp(-90f, 90f, Mathf.Clamp(rollDegrees, -90f, 90f));
        return Mathf.Clamp(Mathf.RoundToInt(t * (paletteSize - 1)), 0, paletteSize - 1);
    }

    public void Tick(float rollDegrees)
    {
        if (sample == null || sample.Palette == null || sample.Palette.Length == 0) return;
        var c = sample.Palette[RollToIndex(rollDegrees, sample.Palette.Length)];
        if (c == sample.CurrentColor) return;
        sample.CurrentColor = c;
        if (rend != null) rend.material.color = c;
    }
}
