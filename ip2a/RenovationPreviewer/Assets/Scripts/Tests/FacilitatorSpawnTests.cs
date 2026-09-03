using NUnit.Framework;

public class FacilitatorSpawnTests
{
    [Test] public void ShouldFire_UnderThreshold_False() => Assert.IsFalse(FacilitatorSpawn.ShouldFire(FacilitatorSpawn.HoldSeconds - 0.1f, false));
    [Test] public void ShouldFire_AtThreshold_True() => Assert.IsTrue(FacilitatorSpawn.ShouldFire(FacilitatorSpawn.HoldSeconds, false));
    [Test] public void ShouldFire_AlreadyFired_False() => Assert.IsFalse(FacilitatorSpawn.ShouldFire(5f, true));
}
