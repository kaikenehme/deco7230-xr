using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The sole coupling point between samples and surfaces (spec §8).
/// While held: nearest Change surface in range previews the sample's colour;
/// leaving range reverts (a preview must never stick — spec §9); release in
/// range commits; release mid-air discards. Also feeds controller roll to the
/// HarmonyTuner so twist-to-tune works while holding up.
/// </summary>
[RequireComponent(typeof(Sample), typeof(XRGrabInteractable))]
public class HoldUpPreviewer : MonoBehaviour
{
    public const float PreviewRadius = 0.45f;

    Sample sample;
    HarmonyTuner tuner;
    XRGrabInteractable grab;
    Surface previewing;
    Transform holdingHand;
    float grabRollRef;

    void Awake()
    {
        sample = GetComponent<Sample>();
        tuner = GetComponent<HarmonyTuner>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        holdingHand = args.interactorObject.GetAttachTransform(grab);
        grabRollRef = holdingHand.eulerAngles.z;
    }

    void Update()
    {
        if (holdingHand == null) return;

        if (tuner != null)
            tuner.Tick(Mathf.DeltaAngle(grabRollRef, holdingHand.eulerAngles.z));

        Surface nearest = null;
        float best = PreviewRadius;
        foreach (var s in Surface.All)
        {
            if (s == null || s.State != SurfaceState.Change) continue;
            var col = s.GetComponent<Collider>();
            if (col == null) continue;
            float d = Vector3.Distance(col.ClosestPoint(transform.position), transform.position);
            if (d < best) { best = d; nearest = s; }
        }

        if (nearest != previewing)
        {
            if (previewing != null) previewing.Revert();
            previewing = nearest;
        }
        if (previewing != null) previewing.Preview(sample.CurrentColor);
    }

    void OnReleased(SelectExitEventArgs args)
    {
        holdingHand = null;
        if (previewing != null)
        {
            previewing.Commit(sample.CurrentColor);
            previewing = null;
        }
        Destroy(gameObject); // mid-air release = discard; either way the sample is gone
    }
}
