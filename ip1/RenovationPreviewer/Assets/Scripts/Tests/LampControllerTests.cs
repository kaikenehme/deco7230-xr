using NUnit.Framework;

public class LampControllerTests
{
    [Test] public void Warm_CyclesToCool() => Assert.AreEqual(LampController.LightState.Cool, LampController.Next(LampController.LightState.Warm));
    [Test] public void Cool_CyclesToDaylight() => Assert.AreEqual(LampController.LightState.Daylight, LampController.Next(LampController.LightState.Cool));
    [Test] public void Daylight_CyclesToWarm() => Assert.AreEqual(LampController.LightState.Warm, LampController.Next(LampController.LightState.Daylight));
}
