using System.Collections.Generic;
using UnityEngine;

public enum ReleaseOutcome { CommitHoldUp, Toss, Discard }

/// <summary>
/// IP2b N5: what letting go of a chip means. Previewing a surface always commits by hold-up
/// (Review Focus #2: never twice, however fast the hand moves); otherwise a fast release
/// throws it; a slow release in mid-air discards it, as before.
/// </summary>
public static class TossDetector
{
    public const float MinSpeed = 1.5f;   // m/s — a deliberate flick, above hand tremor and a gentle let-go
    public const int Window = 5;          // frames of hand history used for the release velocity

    /// <summary>Average velocity across the whole window (first to last sample), so one jittery frame can't decide a throw.</summary>
    public static Vector3 Velocity(IReadOnlyList<Vector3> positions, IReadOnlyList<float> times)
    {
        if (positions == null || times == null || positions.Count < 2 || times.Count < 2) return Vector3.zero;
        int last = Mathf.Min(positions.Count, times.Count) - 1;
        float dt = times[last] - times[0];
        return dt > 1e-5f ? (positions[last] - positions[0]) / dt : Vector3.zero;
    }

    /// <summary>canceled = the select was cut short (sample replaced by a new pull, room switched): never commit or throw.</summary>
    public static ReleaseOutcome Decide(bool previewing, Vector3 velocity, bool canceled = false)
    {
        if (canceled) return ReleaseOutcome.Discard;
        if (previewing) return ReleaseOutcome.CommitHoldUp;
        return velocity.magnitude >= MinSpeed ? ReleaseOutcome.Toss : ReleaseOutcome.Discard;
    }
}
