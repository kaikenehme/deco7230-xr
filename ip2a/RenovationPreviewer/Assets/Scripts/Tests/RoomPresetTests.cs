using NUnit.Framework;
using UnityEngine;

public class RoomPresetTests
{
    [Test]
    public void Card_Render_Is256AndUsesSwatches()
    {
        var p = ScriptableObject.CreateInstance<RoomPreset>();
        p.displayName = "T"; p.floorSwatch = Color.red; p.wallSwatch = Color.green; p.accentSwatch = Color.blue;
        var tex = PresetCard.Render(p);
        Assert.AreEqual(256, tex.width); Assert.AreEqual(256, tex.height);
        Assert.AreEqual(Color.red, tex.GetPixel(10, 10), "floor at the bottom");
        Assert.AreEqual(Color.green, tex.GetPixel(10, 245), "wall at the top");
        Assert.AreEqual(Color.blue, tex.GetPixel(10, 148), "accent stripe");
        Object.DestroyImmediate(tex); Object.DestroyImmediate(p);
    }
}
