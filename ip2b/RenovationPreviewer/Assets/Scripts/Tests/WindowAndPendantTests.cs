using NUnit.Framework;
using UnityEngine;

/// <summary>Kaike's lap, 17 Oct: the garden outside ignored the clock; the ceiling pendant couldn't be touched.</summary>
public class WindowAndPendantTests
{
    static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

    [Test]
    public void WindowTint_NightIsDark_NoonIsFull()
    {
        Assert.Less(Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[5])), 0.2f, "22:00");
        Assert.Greater(Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[2])), 0.95f, "13:00");
    }

    [Test]
    public void WindowTint_DuskIsWarmerThanNoon()
    {
        var dusk = TimeOfDay.WindowTint(TimeOfDay.Stops[4]);
        Assert.Greater(dusk.r, dusk.b + 0.15f, "19:00 reads orange");
        Assert.Less(Lum(dusk), Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[2])));
    }

    [Test]
    public void WindowTint_FollowsLightOrderThroughTheDay()
    {
        // morning brighter than night, dusk brighter than night
        Assert.Greater(Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[0])), Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[5])));
        Assert.Greater(Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[4])), Lum(TimeOfDay.WindowTint(TimeOfDay.Stops[5])));
    }

    [Test]
    public void Pendant_TouchTogglesLight()
    {
        var go = new GameObject("Pendant");
        var light = new GameObject("PendantLight").AddComponent<Light>();
        light.transform.SetParent(go.transform);
        var p = go.AddComponent<PendantLight>();
        p.bulb = light;
        Assert.IsTrue(p.IsOn, "starts on");
        p.Toggle();
        Assert.IsFalse(p.IsOn); Assert.IsFalse(light.enabled);
        p.Toggle();
        Assert.IsTrue(p.IsOn); Assert.IsTrue(light.enabled);
        Object.DestroyImmediate(go);
    }
}
