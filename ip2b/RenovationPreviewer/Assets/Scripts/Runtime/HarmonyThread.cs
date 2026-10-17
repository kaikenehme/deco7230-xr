using UnityEngine;

/// <summary>
/// Makes the constraint explain itself (Evaluation 1 §05, A2 risk flag): a thin thread
/// runs from the held sample back to the kept surface it was pulled from, current colour
/// at the sample end, source colour at the surface end, and a small swatch of the
/// source colour rides on the sample. Constraint that is visibly doing something reads
/// as help; constraint that is silent reads as fewer options.
/// </summary>
[RequireComponent(typeof(Sample))]
public class HarmonyThread : MonoBehaviour
{
    public const float Width = 0.012f, SwatchSize = 0.035f;

    Sample sample;
    LineRenderer line;
    SpriteRenderer swatch;

    public static (Vector3 from, Vector3 to) Endpoints(Vector3 samplePos, Surface source) =>
        (samplePos, source != null ? source.ClosestPoint(samplePos) : samplePos);

    public static (Color start, Color end) Colours(Sample s) => (s.CurrentColor, s.BaseColor);

    void Awake()
    {
        sample = GetComponent<Sample>();
        if (!Application.isPlaying) return;
        var lineGo = new GameObject("Thread");
        lineGo.transform.SetParent(transform, false);
        line = lineGo.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = line.endWidth = Width;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.enabled = false;

        var swGo = new GameObject("SourceSwatch");
        swGo.transform.SetParent(transform, false);
        swGo.transform.localPosition = new Vector3(0f, 0.9f, 0f);   // just above the sample sphere (local units: sample is 0.08 m)
        swatch = swGo.AddComponent<SpriteRenderer>();
        swatch.sprite = UiKit.Circle;
    }

    void LateUpdate()
    {
        if (line == null || sample == null) return;
        var src = sample.SourceSurface;
        if (src == null) { line.enabled = false; if (swatch != null) swatch.enabled = false; return; }
        var (from, to) = Endpoints(transform.position, src);
        line.enabled = true;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        var (start, end) = Colours(sample);
        line.startColor = start;
        line.endColor = end;
        if (swatch != null)
        {
            swatch.enabled = true;
            swatch.color = end;
            // counter the sample's scale so the swatch stays 3 cm
            var s = transform.lossyScale.x;
            swatch.transform.localScale = Vector3.one * (s > 1e-4f ? SwatchSize / s : 1f);
            if (Camera.main != null) swatch.transform.rotation = Quaternion.LookRotation(swatch.transform.position - Camera.main.transform.position);
        }
    }
}
