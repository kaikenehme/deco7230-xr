using UnityEngine;

/// <summary>
/// Diegetic time-of-day control: touching the clock advances to the next stop and the
/// hour hand shows it. Same touch-with-controller pattern as the lamp. The clock's
/// front faces +Z (into the room from Wall_S), so clockwise is a negative Z rotation.
/// </summary>
public class WallClock : MonoBehaviour, ITouchable
{
    public TimeOfDayController controller;
    public Transform hourHand, minuteHand;
    public Renderer face;

    const float Debounce = 0.6f;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    float lastTouch = -10f;

    public static Quaternion HandRotation(int hour) => Quaternion.Euler(0f, 0f, -TimeOfDay.HourHandAngle(hour));

    void OnEnable()
    {
        if (controller == null) return;
        controller.Changed += Show;
        Show(controller.Index);
    }

    void OnDisable() { if (controller != null) controller.Changed -= Show; }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() != null) Touch(); // only controller cues count
    }

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        if (controller != null) controller.Next();
    }

    public void Show(int stopIndex)
    {
        var stop = TimeOfDay.Stops[Mathf.Clamp(stopIndex, 0, TimeOfDay.Stops.Length - 1)];
        if (hourHand != null) hourHand.localRotation = HandRotation(stop.hour);
        if (minuteHand != null) minuteHand.localRotation = Quaternion.identity;   // every stop is on the hour
        if (face != null && Application.isPlaying)
        {
            var m = face.material;
            m.EnableKeyword("_EMISSION");
            m.SetColor(EmissionId, stop.SunOn ? Color.black : new Color(0.30f, 0.30f, 0.25f));   // readable at night
        }
    }
}
