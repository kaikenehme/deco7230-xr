using UnityEngine;

/// <summary>
/// Diegetic light control: touching the lamp (or its pull cord) cycles Warm → Cool → Off.
/// The bulb is all this owns — daylight now comes through the window from
/// TimeOfDayController, so "does this green survive a warm bulb at 10pm?" is asked by
/// combining the two. The shade pulses until first touch so studio-condition users
/// find it (IP1: found 0/2 in the studio, 2/3 at home).
/// </summary>
public class LampController : MonoBehaviour, ITouchable
{
    public enum LightState { Warm, Cool, Off }

    public Light bulb;
    public Renderer shade;
    public LightState Current { get; private set; } = LightState.Warm;
    public bool Touched { get; private set; }

    public static readonly Color WarmColor = new(1f, 0.75f, 0.45f);
    public static readonly Color CoolColor = new(0.85f, 0.92f, 1f);
    public const float SteadyEmission = 0.25f, IdleMin = 0.15f, IdleMax = 0.5f, IdleHz = 1.2f;

    const float Debounce = 0.6f;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    float lastTouch = -10f;
    float pulseUntil = -1f;

    public static LightState Next(LightState s) => (LightState)(((int)s + 1) % 3);

    public static (Color color, float intensity) Look(LightState s) => s switch
    {
        LightState.Warm => (WarmColor, 1.2f),
        LightState.Cool => (CoolColor, 1.0f),
        _ => (Color.black, 0f),
    };

    /// <summary>Shade emission while idle or pulsing: a slow sine between IdleMin and IdleMax.</summary>
    public static float IdleEmission(float t) =>
        Mathf.Lerp(IdleMin, IdleMax, 0.5f + 0.5f * Mathf.Sin(t * IdleHz * 2f * Mathf.PI));

    void Start() => Apply(Current);

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() != null) Touch(); // only controller cues count
    }

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        Touched = true;
        Apply(Next(Current));
    }

    public void Apply(LightState s)
    {
        Current = s;
        var (c, i) = Look(s);
        if (bulb != null) { bulb.color = c; bulb.intensity = i; bulb.enabled = i > 0f; }
        SetEmission(s == LightState.Off ? 0f : SteadyEmission);
    }

    /// <summary>Onboarding hook: keep the shade pulsing for a while regardless of touches.</summary>
    public void Pulse(float seconds) => pulseUntil = Time.time + seconds;

    void Update()
    {
        if (Current == LightState.Off) return;
        if (!Touched || Time.time < pulseUntil) SetEmission(IdleEmission(Time.time));
    }

    void SetEmission(float k)
    {
        if (shade == null || !Application.isPlaying) return;
        var tint = Look(Current == LightState.Off ? LightState.Warm : Current).color;
        var m = shade.material;
        m.EnableKeyword("_EMISSION");
        m.SetColor(EmissionId, tint * k);
    }
}
