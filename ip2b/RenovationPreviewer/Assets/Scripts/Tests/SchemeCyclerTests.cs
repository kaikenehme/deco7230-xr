using NUnit.Framework;

public class SchemeCyclerTests
{
    [Test] public void ShouldCycle_Pressed_WithSchemes_MenuClosed_True() => Assert.IsTrue(SchemeCycler.ShouldCycle(true, 2, false));
    [Test] public void ShouldCycle_MenuOpen_False_SameButtonClosesTheMenu() => Assert.IsFalse(SchemeCycler.ShouldCycle(true, 2, true));
    [Test] public void ShouldCycle_NoSchemes_False() => Assert.IsFalse(SchemeCycler.ShouldCycle(true, 0, false));
    [Test] public void ShouldCycle_NotPressed_False() => Assert.IsFalse(SchemeCycler.ShouldCycle(false, 2, false));
}
