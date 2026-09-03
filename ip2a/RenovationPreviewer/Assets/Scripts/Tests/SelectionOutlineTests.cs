using NUnit.Framework;
using UnityEngine;

public class SelectionOutlineTests
{
    GameObject slot;

    GameObject Visual(int parts)
    {
        var v = new GameObject("Visual");
        v.transform.SetParent(slot.transform, false);
        for (int i = 0; i < parts; i++)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.SetParent(v.transform, false);
            Object.DestroyImmediate(c.GetComponent<Collider>());
        }
        return v;
    }

    [SetUp] public void Setup() { slot = new GameObject("Slot"); }
    [TearDown] public void Cleanup() { Object.DestroyImmediate(slot); }

    [Test]
    public void Show_CreatesShellPerRenderer_WithFrontCull()
    {
        Visual(2);
        var o = slot.AddComponent<SelectionOutline>();
        o.Show();
        Assert.AreEqual(2, o.ShellCount);
        var shell = slot.GetComponentInChildren<OutlineShellTag>();
        Assert.IsNotNull(shell);
        Assert.AreEqual(1f, shell.GetComponent<MeshRenderer>().sharedMaterial.GetFloat("_Cull"), "front faces culled");
        Assert.AreEqual(SelectionOutline.ShellScale, shell.transform.localScale.x, 1e-4f);
    }

    [Test]
    public void Hide_RemovesShells()
    {
        Visual(1);
        var o = slot.AddComponent<SelectionOutline>();
        o.Show(); o.Hide();
        Assert.AreEqual(0, o.ShellCount);
        Assert.IsNull(slot.GetComponentInChildren<OutlineShellTag>());
        Assert.IsFalse(o.IsShown);
    }

    [Test]
    public void Rebuild_AfterSwap_MatchesNewVisual_AndNeverOutlinesAnOutline()
    {
        var v = Visual(1);
        var o = slot.AddComponent<SelectionOutline>();
        o.Show();
        Object.DestroyImmediate(v);
        Visual(3);
        o.Rebuild();
        Assert.AreEqual(3, o.ShellCount);
        o.Rebuild();
        Assert.AreEqual(3, o.ShellCount, "shells are not re-outlined");
    }
}
