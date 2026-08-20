using NUnit.Framework;

public class HarmonyTunerTests
{
    [Test] public void NeutralRoll_IsFirstIndex() => Assert.AreEqual(0, HarmonyTuner.RollToIndex(-90f, 7));
    [Test] public void FullRoll_IsLastIndex() => Assert.AreEqual(6, HarmonyTuner.RollToIndex(90f, 7));
    [Test] public void MidRoll_IsMiddleIndex() => Assert.AreEqual(3, HarmonyTuner.RollToIndex(0f, 7));
    [Test] public void OverRotation_Clamps() => Assert.AreEqual(6, HarmonyTuner.RollToIndex(400f, 7));
    [Test] public void UnderRotation_Clamps() => Assert.AreEqual(0, HarmonyTuner.RollToIndex(-400f, 7));
}
