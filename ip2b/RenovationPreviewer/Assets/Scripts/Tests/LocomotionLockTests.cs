using NUnit.Framework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class LocomotionLockTests
{
    GameObject go;

    [SetUp] public void Setup() { LocomotionLock.Reset(); go = new GameObject("rig"); }
    [TearDown] public void Cleanup() { LocomotionLock.Reset(); Object.DestroyImmediate(go); }

    [Test]
    public void Acquire_DisablesProviders_Release_Restores()
    {
        var tp = go.AddComponent<TeleportationProvider>();
        Assert.IsTrue(tp.enabled);
        LocomotionLock.Acquire();
        Assert.IsFalse(tp.enabled);
        Assert.AreEqual(1, LocomotionLock.Depth);
        LocomotionLock.Release();
        Assert.IsTrue(tp.enabled);
        Assert.AreEqual(0, LocomotionLock.Depth);
    }

    [Test]
    public void NestedAcquire_ReleasesOnlyWhenAllReleased()
    {
        var tp = go.AddComponent<TeleportationProvider>();
        LocomotionLock.Acquire(); LocomotionLock.Acquire();
        LocomotionLock.Release();
        Assert.IsFalse(tp.enabled, "one hand still holding");
        LocomotionLock.Release();
        Assert.IsTrue(tp.enabled);
    }

    [Test]
    public void Release_WithoutAcquire_IsHarmless()
    {
        LocomotionLock.Release();
        Assert.AreEqual(0, LocomotionLock.Depth);
    }
}
