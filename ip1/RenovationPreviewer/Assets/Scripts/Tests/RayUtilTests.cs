using NUnit.Framework;
using UnityEngine;

public class RayUtilTests
{
    GameObject rig, body, wall;

    [SetUp]
    public void Setup()
    {
        rig = new GameObject("Rig");
        body = GameObject.CreatePrimitive(PrimitiveType.Capsule);   // stands in for the CharacterController
        body.transform.SetParent(rig.transform); body.transform.position = new Vector3(0, 1, 0.5f);
        wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = new Vector3(0, 1, 3f); wall.transform.localScale = new Vector3(4, 3, 0.1f);
        Physics.SyncTransforms();
    }

    [TearDown]
    public void Cleanup() { Object.DestroyImmediate(rig); Object.DestroyImmediate(wall); }

    [Test]
    public void SkipsRigColliders_HitsWallBehind()
    {
        Assert.IsTrue(RayUtil.TryHit(new Vector3(0, 1, -0.5f), Vector3.forward, 10f, rig.transform, out var hit));
        Assert.AreEqual(wall, hit.collider.gameObject);
    }

    [Test]
    public void WithoutIgnore_HitsBodyFirst()
    {
        Assert.IsTrue(RayUtil.TryHit(new Vector3(0, 1, -0.5f), Vector3.forward, 10f, null, out var hit));
        Assert.AreEqual(body, hit.collider.gameObject);
    }

    [Test]
    public void NothingInRange_ReturnsFalse()
    {
        Assert.IsFalse(RayUtil.TryHit(new Vector3(0, 1, -0.5f), Vector3.back, 10f, rig.transform, out _));
    }
}
