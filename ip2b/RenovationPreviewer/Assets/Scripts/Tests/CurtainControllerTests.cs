using NUnit.Framework;
using UnityEngine;

public class CurtainControllerTests
{
    [Test]
    public void StepOpenness_TakesFadeSecondsToClose()
    {
        float o = 1f; int frames = 0;
        while (o > 0f && frames < 1000) { o = CurtainController.StepOpenness(o, false, 1 / 72f); frames++; }
        Assert.AreEqual(72, frames, 2);   // FadeSeconds = 1 s at 72 fps
    }

    [Test]
    public void StepOpenness_ReversesMidFade()
    {
        // Review Focus #3: toggled mid-fade → turns around from where it is.
        float o = CurtainController.StepOpenness(1f, false, 0.5f);
        Assert.AreEqual(0.5f, o, 1e-4f);
        o = CurtainController.StepOpenness(o, true, 0.25f);
        Assert.AreEqual(0.75f, o, 1e-4f);
    }

    [Test]
    public void NoSingleFrameJump()
    {
        // "Brightness changes" principle: never snap the whole field of view dark.
        float prev = 1f;
        for (int i = 0; i < 100; i++)
        {
            float o = CurtainController.StepOpenness(prev, false, 1 / 72f);
            Assert.LessOrEqual(prev - o, 1 / 72f / CurtainController.FadeSeconds + 1e-5f);
            prev = o;
        }
    }

    [Test]
    public void DaylightFactor_ClosedIsDimNotBlack()
    {
        Assert.AreEqual(1f, TimeOfDayController.DaylightFactor(1f), 1e-4f);
        Assert.AreEqual(CurtainController.ClosedDaylight, TimeOfDayController.DaylightFactor(0f), 1e-4f);
        Assert.Greater(CurtainController.ClosedDaylight, 0f);
    }

    [Test]
    public void PanelX_MeetsAtWindowCentreWhenClosed()
    {
        Assert.AreEqual(-1.2f, CurtainController.PanelX(-1.2f, 0.3f, -1, 1f), 1e-4f, "open: where it was built");
        Assert.Less(CurtainController.PanelX(-1.2f, 0.3f, -1, 0f), 0.3f, "closed: left panel ends left of centre");
        Assert.Greater(CurtainController.PanelX(-1.2f, 0.3f, -1, 0f), -1.2f);
    }
}
