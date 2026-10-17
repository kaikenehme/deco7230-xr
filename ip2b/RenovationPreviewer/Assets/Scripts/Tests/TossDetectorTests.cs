using NUnit.Framework;
using UnityEngine;

public class TossDetectorTests
{
    [Test] public void Previewing_AlwaysHoldUp_EvenWhenFast() => Assert.AreEqual(ReleaseOutcome.CommitHoldUp, TossDetector.Decide(true, new Vector3(0, 0, 5)));   // Review Focus #2
    [Test] public void FastAway_Tosses() => Assert.AreEqual(ReleaseOutcome.Toss, TossDetector.Decide(false, new Vector3(0, 0, 2)));
    [Test] public void Slow_Discards() => Assert.AreEqual(ReleaseOutcome.Discard, TossDetector.Decide(false, new Vector3(0, 0, 1)));
    [Test] public void Threshold_IsInclusive() => Assert.AreEqual(ReleaseOutcome.Toss, TossDetector.Decide(false, new Vector3(TossDetector.MinSpeed, 0, 0)));

    [Test]
    public void Velocity_FromSamples()
    {
        var p = new[] { Vector3.zero, new Vector3(0, 0, 0.1f) };
        var t = new[] { 0f, 0.05f };
        Assert.AreEqual(2f, TossDetector.Velocity(p, t).z, 1e-4f);
    }

    [Test]
    public void Velocity_UsesWholeWindow_NotJustLastFrame()
    {
        // a jittery last frame must not decide the throw
        var p = new[] { Vector3.zero, new Vector3(0, 0, 0.1f), new Vector3(0, 0, 0.2f), new Vector3(0, 0, 0.2f) };
        var t = new[] { 0f, 0.05f, 0.1f, 0.1139f };
        Assert.AreEqual(0.2f / 0.1139f, TossDetector.Velocity(p, t).z, 1e-3f);
    }

    [Test]
    public void Velocity_TooFewSamplesOrZeroTime_IsZero()
    {
        Assert.AreEqual(Vector3.zero, TossDetector.Velocity(new[] { Vector3.one }, new[] { 0f }));
        Assert.AreEqual(Vector3.zero, TossDetector.Velocity(new[] { Vector3.zero, Vector3.one }, new[] { 1f, 1f }));
    }
}
