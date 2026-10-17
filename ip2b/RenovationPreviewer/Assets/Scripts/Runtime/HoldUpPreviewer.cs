using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The sole coupling point between samples and surfaces (spec §8).
/// While held: nearest Change surface in range previews the sample's colour;
/// leaving range reverts (a preview must never stick — spec §9); release in
/// range commits; release mid-air discards. The colour is the FanDeck pick (IP2b; twist-to-tune is gone).
/// </summary>
[RequireComponent(typeof(Sample), typeof(XRGrabInteractable))]
public class HoldUpPreviewer : MonoBehaviour
{
    public const float PreviewRadius = 0.45f;

    Sample sample;
    XRGrabInteractable grab;
    Surface previewing;
    Transform holdingHand;

    void Awake()
    {
        sample = GetComponent<Sample>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        holdingHand = args.interactorObject.GetAttachTransform(grab);
    }

    void Update()
    {
        if (holdingHand == null) return;

        Surface nearest = null;
        float best = PreviewRadius;
        foreach (var s in Surface.All)
        {
            if (s == null || !s.IsPaintTarget) continue;
            float d = Vector3.Distance(s.ClosestPoint(transform.position), transform.position);
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
