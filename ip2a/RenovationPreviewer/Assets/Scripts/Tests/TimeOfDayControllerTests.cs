using NUnit.Framework;
using UnityEngine;

public class TimeOfDayControllerTests
{
    GameObject go; Light sun; TimeOfDayController tod;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("Managers");
        sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        tod = go.AddComponent<TimeOfDayController>();
        tod.sun = sun;
    }

    [TearDown]
    public void Cleanup() { Object.DestroyImmediate(sun.gameObject); Object.DestroyImmediate(go); }

    [Test]
    public void Apply_Night_DisablesSun()
    {
        tod.Apply(5);
        Assert.IsFalse(sun.enabled);
        Assert.AreEqual(5, tod.Index);
    }

    [Test]
    public void Apply_Noon_SetsRotationColourIntensity()
    {
        tod.Apply(2);
        var s = TimeOfDay.Stops[2];
        Assert.IsTrue(sun.enabled);
        Assert.AreEqual(s.sunColor, sun.color);
        Assert.AreEqual(s.sunIntensity, sun.intensity, 1e-4f);
        Assert.Less(Quaternion.Angle(TimeOfDay.SunRotation(s), sun.transform.rotation), 0.01f);
    }

    [Test]
    public void Apply_RaisesChanged_AndNextWraps()
    {
        int got = -1;
        tod.Changed += i => got = i;
        tod.Apply(5);
        Assert.AreEqual(5, got);
        tod.Next();
        Assert.AreEqual(0, tod.Index);
        Assert.AreEqual(0, got);
    }

    [Test] public void Default_IsTenOClock() => Assert.AreEqual(10, tod.Current.hour);
}
