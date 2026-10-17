using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// The sole coupling point between samples and surfaces (spec §8).
/// While held: nearest Change surface in range previews the sample's colour;
/// leaving range reverts (a preview must never stick — spec §9); release in
/// range commits; a fast release away from any surface throws it (TossDetector, N5); a slow release mid-air discards. The colour is the FanDeck pick (IP2b; twist-to-tune is gone).
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

    // Last few positions/times of the held chip, for the release velocity (N5 toss).
    readonly System.Collections.Generic.List<Vector3> trail = new();
    readonly System.Collections.Generic.List<float> trailT = new();

    void OnGrabbed(SelectEnterEventArgs args)
    {
        holdingHand = args.interactorObject.GetAttachTransform(grab);
        trail.Clear(); trailT.Clear();
    }

    void Update()
    {
        if (holdingHand == null) return;

        trail.Add(transform.position); trailT.Add(Time.time);
        if (trail.Count > TossDetector.Window) { trail.RemoveAt(0); trailT.RemoveAt(0); }

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
        switch (TossDetector.Decide(previewing != null, TossDetector.Velocity(trail, trailT)))
        {
            case ReleaseOutcome.CommitHoldUp:
                previewing.Commit(sample.CurrentColor);
                Debug.Log("[IP2b] commit holdup " + previewing.name);
                break;
            case ReleaseOutcome.Toss:
                TossedChip.Launch(transform.position, TossDetector.Velocity(trail, trailT), sample.CurrentColor);
                break;
        }
        previewing = null;
        Destroy(gameObject); // the sample is gone either way: committed, thrown, or discarded mid-air
    }
}
