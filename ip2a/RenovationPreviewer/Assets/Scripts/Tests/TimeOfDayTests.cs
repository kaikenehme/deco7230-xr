using NUnit.Framework;
using UnityEngine;

public class TimeOfDayTests
{
    [Test]
    public void Stops_AreSix_Chronological()
    {
        Assert.AreEqual(6, TimeOfDay.Stops.Length);
        for (int i = 1; i < TimeOfDay.Stops.Length; i++)
            Assert.Greater(TimeOfDay.Stops[i].hour, TimeOfDay.Stops[i - 1].hour);
    }

    [Test] public void Next_WrapsNightToMorning() => Assert.AreEqual(0, TimeOfDay.Next(TimeOfDay.Stops.Length - 1));
    [Test] public void Night_SunOff() => Assert.IsFalse(TimeOfDay.Stops[5].SunOn);
    [Test] public void Night_StillHasSomeAmbient() => Assert.Greater(TimeOfDay.Stops[5].ambient.maxColorComponent, 0.03f);

    [Test]
    public void Morning_IsLow_AndOffAxis()
    {
        var m = TimeOfDay.Stops[0];
        Assert.Less(m.elevation, 20f);
        Assert.AreNotEqual(0f, m.azimuth);
    }

    [Test]
    public void Noon_IsHighest_AndBrightest()
    {
        float best = -1f; int bi = -1;
        for (int i = 0; i < TimeOfDay.Stops.Length; i++) if (TimeOfDay.Stops[i].elevation > best) { best = TimeOfDay.Stops[i].elevation; bi = i; }
        Assert.AreEqual(13, TimeOfDay.Stops[bi].hour);
        foreach (var s in TimeOfDay.Stops) Assert.LessOrEqual(s.sunIntensity, TimeOfDay.Stops[bi].sunIntensity);
    }

    [Test]
    public void SunRotation_Noon_PointsDownAndIntoTheRoom()
    {
        var f = TimeOfDay.SunRotation(TimeOfDay.Stops[2]) * Vector3.forward;
        Assert.Less(f.y, -0.5f, "light travels downward");
        Assert.Greater(f.z, 0f, "light travels +Z: in through the Wall_S window");
    }

    [Test] public void HourHandAngle_13_Is30() => Assert.AreEqual(30f, TimeOfDay.HourHandAngle(13));
    [Test] public void HourHandAngle_22_Is300() => Assert.AreEqual(300f, TimeOfDay.HourHandAngle(22));
}
