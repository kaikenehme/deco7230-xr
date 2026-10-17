using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class OnboardingSequenceTests
{
    [TearDown]
    public void Cleanup()
    {
        foreach (var s in Surface.All.ToArray()) if (s != null) Object.DestroyImmediate(s.gameObject);
        foreach (var f in Object.FindObjectsByType<FurnitureSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(f.gameObject);
    }

    static Surface Make(SurfaceState st, Vector3 size, bool furniture)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = size;
        if (furniture) go.AddComponent<FurnitureSlot>();
        var s = go.AddComponent<Surface>(); s.SetState(st); return s;
    }

    [Test]
    public void Hero_IsTheLargestFurniturePiece_NotTheRoomOrRug()
    {
        // IP2b: no kept/change split, so onboarding points at one big piece (sofa / bed), then the lamp.
        Make(SurfaceState.Change, new Vector3(5, 0.1f, 5), false);     // floor
        Make(SurfaceState.Keep, new Vector3(3, 0.01f, 2.4f), false);   // rug: a source, but not furniture
        var sofa = Make(SurfaceState.Keep, new Vector3(2.6f, 0.75f, 1.2f), true);
        Make(SurfaceState.Keep, new Vector3(0.6f, 0.3f, 0.6f), true);  // pouf
        Assert.AreSame(sofa, OnboardingSequence.Hero(Surface.All));
    }

    [Test]
    public void Hero_NullWhenNoFurniture()
    {
        Make(SurfaceState.Change, new Vector3(5, 0.1f, 5), false);
        Assert.IsNull(OnboardingSequence.Hero(Surface.All));
    }
}
