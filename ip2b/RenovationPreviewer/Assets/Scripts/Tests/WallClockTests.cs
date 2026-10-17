using NUnit.Framework;
using UnityEngine;

public class WallClockTests
{
    [Test]
    public void HandRotation_13_Is30DegreesClockwise()
    {
        var z = WallClock.HandRotation(13).eulerAngles.z;
        Assert.AreEqual(330f, z, 0.01f, "clockwise = negative about +Z, which faces the viewer");
    }

    [Test]
    public void Show_RotatesHourHand_ForEachStop()
    {
        var go = new GameObject("clock");
        var clock = go.AddComponent<WallClock>();
        var hand = new GameObject("hour").transform; hand.SetParent(go.transform, false);
        clock.hourHand = hand;
        for (int i = 0; i < TimeOfDay.Stops.Length; i++)
        {
            clock.Show(i);
            Assert.Less(Quaternion.Angle(WallClock.HandRotation(TimeOfDay.Stops[i].hour), hand.localRotation), 0.01f, $"stop {i}");
        }
        Object.DestroyImmediate(go);
    }
}
