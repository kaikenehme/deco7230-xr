using NUnit.Framework;
using UnityEngine;

public class LampControllerTests
{
    [Test] public void Warm_CyclesToCool() => Assert.AreEqual(LampController.LightState.Cool, LampController.Next(LampController.LightState.Warm));
    [Test] public void Cool_CyclesToOff() => Assert.AreEqual(LampController.LightState.Off, LampController.Next(LampController.LightState.Cool));
    [Test] public void Off_CyclesToWarm() => Assert.AreEqual(LampController.LightState.Warm, LampController.Next(LampController.LightState.Off));

    [Test]
    public void Apply_Off_DisablesBulb()
    {
        var go = new GameObject("lamp");
        var bulb = new GameObject("bulb").AddComponent<Light>();
        var lamp = go.AddComponent<LampController>();
        lamp.bulb = bulb;
        lamp.Apply(LampController.LightState.Off);
        Assert.IsFalse(bulb.enabled);
        Assert.AreEqual(0f, bulb.intensity);
        Object.DestroyImmediate(bulb.gameObject); Object.DestroyImmediate(go);
    }

    [Test]
    public void Apply_Warm_SetsWarmColourAndIntensity()
    {
        var go = new GameObject("lamp");
        var bulb = new GameObject("bulb").AddComponent<Light>();
        var lamp = go.AddComponent<LampController>();
        lamp.bulb = bulb;
        lamp.Apply(LampController.LightState.Off);
        lamp.Apply(LampController.LightState.Warm);
        Assert.IsTrue(bulb.enabled);
        Assert.AreEqual(LampController.WarmColor, bulb.color);
        Assert.Greater(bulb.intensity, 1f);
        Object.DestroyImmediate(bulb.gameObject); Object.DestroyImmediate(go);
    }

    [Test]
    public void Default_IsWarm_AndUntouched()
    {
        var go = new GameObject("lamp");
        var lamp = go.AddComponent<LampController>();
        Assert.AreEqual(LampController.LightState.Warm, lamp.Current);
        Assert.IsFalse(lamp.Touched);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void IdleEmission_StaysWithinRange()
    {
        for (float t = 0f; t < 3f; t += 0.05f)
        {
            float e = LampController.IdleEmission(t);
            Assert.GreaterOrEqual(e, LampController.IdleMin - 1e-4f);
            Assert.LessOrEqual(e, LampController.IdleMax + 1e-4f);
        }
    }
}
