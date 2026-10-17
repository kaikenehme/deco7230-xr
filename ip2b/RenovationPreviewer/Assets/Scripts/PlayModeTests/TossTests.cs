using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>IP2b N5: a thrown chip commits once to the first wall/floor it hits; furniture never takes paint.</summary>
public class TossTests
{
    [UnitySetUp]
    public IEnumerator LoadRoom() { SceneManager.LoadScene("Room"); yield return null; yield return null; }

    [UnityTest]
    public IEnumerator TossedChip_CommitsOnceToWall()
    {
        var wall = Surface.All.Single(s => s != null && s.name == "Wall_E");
        var c = new Color(0.2f, 0.5f, 0.4f);
        LogAssert.Expect(LogType.Log, new Regex(@"^\[IP2b\] commit toss Wall_E$"));
        var chip = TossedChip.Launch(new Vector3(1.0f, 1.5f, -0.9f), new Vector3(4f, 0.5f, 0f), c);   // toward Wall_E, clear of the TV unit
        yield return new WaitForSeconds(1.0f);
        Assert.IsTrue(wall.HasUserColour, "wall painted");
        Assert.AreEqual(c.r, wall.CommittedColor.r, 1e-3f);
        Assert.IsTrue(chip == null, "chip gone");
        LogAssert.NoUnexpectedReceived();
    }

    [UnityTest]
    public IEnumerator TossedChip_IgnoresFurniture()
    {
        var sofa = Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None).Single(s => s.name.Contains("curved_sofa")).GetComponent<Surface>();
        var before = sofa.SampleColor;
        var target = sofa.WorldBounds.center;
        var from = target + new Vector3(1.5f, 0.6f, 0f);
        var chip = TossedChip.Launch(from, (target - from).normalized * 5f, Color.red);
        yield return new WaitForSeconds(1.0f);
        Assert.IsFalse(sofa.HasUserColour, "furniture is pull-only");
        Assert.AreEqual(before, sofa.SampleColor);
        Assert.IsTrue(chip == null, "chip gone after hitting the sofa");
    }
}
