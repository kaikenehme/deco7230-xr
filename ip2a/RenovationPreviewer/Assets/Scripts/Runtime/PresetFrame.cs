using UnityEngine;

/// <summary>
/// A framed picture on the wall; touching it dresses the room in that preset. The
/// current preset's frame glows amber. Same touch-with-controller pattern as the lamp.
/// </summary>
public class PresetFrame : MonoBehaviour, ITouchable
{
    public PresetApplier applier;
    public int index;
    public Renderer frame;

    const float Debounce = 1.0f;
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    float lastTouch = -10f;

    void OnEnable()
    {
        if (applier == null) return;
        applier.Applied += OnApplied;
        OnApplied(applier.Current);
    }

    void OnDisable() { if (applier != null) applier.Applied -= OnApplied; }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MarkTool>() != null) Touch(); // only controller cues count
    }

    public void Touch()
    {
        if (Time.time - lastTouch < Debounce) return;
        lastTouch = Time.time;
        if (applier != null) applier.Apply(index);
    }

    public bool IsCurrent => applier != null && applier.Current == index;

    void OnApplied(int current)
    {
        if (frame == null || !Application.isPlaying) return;
        var m = frame.material;
        m.EnableKeyword("_EMISSION");
        m.SetColor(EmissionId, current == index ? UiKit.Accent * 0.5f : Color.black);
    }
}
