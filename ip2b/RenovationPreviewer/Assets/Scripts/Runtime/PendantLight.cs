using UnityEngine;

/// <summary>
/// The woven ceiling pendant is a switch (Kaike's lap, 17 Oct): touch it with a controller,
/// point + trigger, or click in desktop mode to turn its warm light on or off. With the
/// curtains shut it is one of the two lights left, so it's worth being able to test without it.
/// </summary>
public class PendantLight : MonoBehaviour, ITouchable
{
    const float Debounce = 0.6f;

    public Light bulb;
    public bool IsOn { get; private set; } = true;
    float lastTouch = -10f;

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        Toggle();
    }

    public void Toggle()
    {
        IsOn = !IsOn;
        if (bulb != null) bulb.enabled = IsOn;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() != null) Touch();
    }
}
