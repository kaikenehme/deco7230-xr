using UnityEngine;

/// <summary>
/// Diegetic light control: touching the lamp cycles Warm -> Cool -> Daylight.
/// Three states, not on/off, because "does this green survive a warm bulb at
/// 9pm?" is the question worth asking (concept spec §7).
/// </summary>
public class LampController : MonoBehaviour
{
    public enum LightState { Warm, Cool, Daylight }

    public Light sun;
    public Light bulb;
    public LightState Current { get; private set; } = LightState.Daylight;

    const float Debounce = 0.6f;
    float lastTouch = -10f;

    public static LightState Next(LightState s) => (LightState)(((int)s + 1) % 3);

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() == null) return; // only controller cues count
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        Apply(Next(Current));
    }

    public void Apply(LightState s)
    {
        Current = s;
        switch (s)
        {
            case LightState.Warm:
                Set(sun, new Color(1f, 0.83f, 0.66f), 0.15f);
                Set(bulb, new Color(1f, 0.75f, 0.45f), 1.6f);
                RenderSettings.ambientLight = new Color(0.35f, 0.30f, 0.25f);
                break;
            case LightState.Cool:
                Set(sun, new Color(0.75f, 0.86f, 1f), 0.4f);
                Set(bulb, new Color(0.85f, 0.92f, 1f), 1.0f);
                RenderSettings.ambientLight = new Color(0.30f, 0.35f, 0.40f);
                break;
            case LightState.Daylight:
                Set(sun, new Color(1f, 0.96f, 0.89f), 1.0f);
                Set(bulb, Color.black, 0f);
                RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.45f);
                break;
        }
    }

    static void Set(Light l, Color c, float intensity)
    {
        if (l == null) return;
        l.color = c;
        l.intensity = intensity;
    }
}
