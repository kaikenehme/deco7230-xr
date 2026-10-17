using UnityEngine;

/// <summary>
/// IP2b N4: touch either curtain to draw both across the window; daylight fades with them
/// over FadeSeconds (the "brightness changes" principle — never snap a headset's whole view
/// dark). Lamp and pendant keep the closed room lit, which also shows how colours read at night.
/// </summary>
public class CurtainController : MonoBehaviour, ITouchable
{
    public const float FadeSeconds = 1.0f;
    /// <summary>Daylight left when fully closed: linen lets a little through.</summary>
    public const float ClosedDaylight = 0.04f;
    /// <summary>Closed panel centre's distance from the window centre (m); 0.97 m panels stretched to cover a 2.2 m window.</summary>
    public const float ClosedOffset = 0.55f, ClosedWidthScale = 1.2f;
    const float Debounce = 0.5f;

    public Transform left, right;
    public float windowCentreX;
    public TimeOfDayController timeOfDay;

    public bool IsClosed => !targetOpen && Openness <= 0f;
    public float Openness { get; private set; } = 1f;

    bool targetOpen = true;
    float leftOpenX, rightOpenX, lastTouch = -10f;
    bool captured;

    public static float StepOpenness(float current, bool targetOpen, float dt) =>
        Mathf.MoveTowards(current, targetOpen ? 1f : 0f, dt / FadeSeconds);

    /// <summary>Panel centre x for an openness (1 = open where it was built, 0 = drawn to the window centre).</summary>
    public static float PanelX(float openX, float windowCentreX, int side, float openness) =>
        Mathf.Lerp(windowCentreX + side * ClosedOffset, openX, openness);

    void Capture()
    {
        if (captured) return;
        if (left != null) leftOpenX = left.position.x;
        if (right != null) rightOpenX = right.position.x;
        captured = true;
    }

    void Awake() => Capture();

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        SetOpen(!targetOpen);
    }

    public void SetOpen(bool open, bool instant = false)
    {
        Capture();
        targetOpen = open;
        if (instant) { Openness = open ? 1f : 0f; Pose(); }
    }

    void Update()
    {
        float next = StepOpenness(Openness, targetOpen, Time.deltaTime);
        if (next == Openness && (timeOfDay == null || Mathf.Approximately(timeOfDay.DaylightScale, Openness))) return;
        Openness = next;
        Pose();
    }

    void Pose()
    {
        float s = Mathf.Lerp(ClosedWidthScale, 1f, Openness);
        if (left != null) { var p = left.position; p.x = PanelX(leftOpenX, windowCentreX, -1, Openness); left.position = p; left.localScale = new Vector3(s, 1f, 1f); }
        if (right != null) { var p = right.position; p.x = PanelX(rightOpenX, windowCentreX, +1, Openness); right.position = p; right.localScale = new Vector3(s, 1f, 1f); }
        if (timeOfDay != null) timeOfDay.DaylightScale = Openness;
    }
}
