using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RayFeedbackTests
{
    static Renderer Cube()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        return go.GetComponent<Renderer>();
    }

    [TearDown]
    public void Cleanup()
    {
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)) Object.DestroyImmediate(r.gameObject);
    }

    [Test]
    public void Set_LightsTargets_AndClearsStale()
    {
        var a = Cube(); var b = Cube();
        var glow = new HoverGlow();
        glow.Set(new List<(Renderer, Color)> { (a, Color.red) });
        Assert.IsTrue(a.sharedMaterial.IsKeywordEnabled(HoverGlow.Keyword));
        Assert.AreEqual(Color.red, a.sharedMaterial.GetColor(HoverGlow.EmissionId));

        glow.Set(new List<(Renderer, Color)> { (b, Color.green) });
        Assert.IsFalse(a.sharedMaterial.IsKeywordEnabled(HoverGlow.Keyword), "a cleared");
        Assert.AreEqual(Color.black, a.sharedMaterial.GetColor(HoverGlow.EmissionId));
        Assert.IsTrue(b.sharedMaterial.IsKeywordEnabled(HoverGlow.Keyword));
        Assert.AreEqual(1, glow.Lit.Count);
    }

    [Test]
    public void ClearAll_RestoresEverything()
    {
        var a = Cube();
        var glow = new HoverGlow();
        glow.Set(new List<(Renderer, Color)> { (a, Color.red) });
        glow.ClearAll();
        Assert.IsFalse(a.sharedMaterial.IsKeywordEnabled(HoverGlow.Keyword));
        Assert.AreEqual(0, glow.Lit.Count);
    }

    [Test]
    public void Set_IgnoresDestroyedRenderers()
    {
        var a = Cube();
        var glow = new HoverGlow();
        glow.Set(new List<(Renderer, Color)> { (a, Color.red) });
        Object.DestroyImmediate(a.gameObject);
        Assert.DoesNotThrow(() => glow.Set(new List<(Renderer, Color)>()));
        Assert.AreEqual(0, glow.Lit.Count);
    }

    [Test]
    public void UiKit_SpritesAreGenerated()
    {
        Assert.IsNotNull(UiKit.Rounded);
        Assert.IsNotNull(UiKit.Ring);
        Assert.Greater(UiKit.Rounded.border.x, 0, "rounded sprite is 9-sliced");
    }

    [Test]
    public void ReticleRotation_IsFiniteOnFloorAndWall()
    {
        foreach (var normal in new[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.right, new Vector3(0.001f, 1f, 0f).normalized })
        {
            var q = RayFeedback.ReticleRotation(normal);
            Assert.IsFalse(float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w), $"NaN for normal {normal}");
            var fwd = q * Vector3.forward;
            Assert.AreEqual(1f, Vector3.Dot(fwd, -normal), 1e-3f, $"faces along -normal for {normal}");
        }
    }
}
