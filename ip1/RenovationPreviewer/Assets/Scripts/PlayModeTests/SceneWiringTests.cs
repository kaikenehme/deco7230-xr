using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Loads Room.unity and asserts the SceneBuilder wiring is complete — the
/// closest headless equivalent of opening the scene and checking inspectors.
/// </summary>
public class SceneWiringTests
{
    [UnitySetUp]
    public IEnumerator LoadRoom()
    {
        SceneManager.LoadScene("Room");
        yield return null; // let Awake/OnEnable run
    }

    [UnityTest]
    public IEnumerator Room_HasNineSurfaces_TwoKept()
    {
        yield return null;
        var surfaces = Surface.All.Where(s => s != null).ToList();
        Assert.AreEqual(9, surfaces.Count, "Floor, 4 walls, Ceiling, Door, Trim, Sofa");
        Assert.AreEqual(2, surfaces.Count(s => s.State == SurfaceState.Keep), "Floor + Sofa kept");
    }

    // Controller GOs are deactivated by XRI's Input Modality Manager when no
    // XR device is present (always true headless), so searches must include
    // inactive objects.
    [UnityTest]
    public IEnumerator BothControllers_HaveMarkAndPullTools()
    {
        yield return null;
        var marks = Object.FindObjectsByType<MarkTool>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(2, marks.Length, "MarkTool on both controllers");

        var pullers = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.AreEqual(2, pullers.Length, "SamplePuller on both controllers");
        foreach (var p in pullers)
        {
            Assert.IsNotNull(p.samplePrefab, $"{p.name}: sample prefab wired");
            Assert.IsNotNull(p.interactor, $"{p.name}: interactor wired");
            Assert.IsNotNull(p.triggerAction.action, $"{p.name}: trigger action wired");
        }
    }

    [UnityTest]
    public IEnumerator Lamp_And_SchemeCycler_AreWired()
    {
        yield return null;
        var lamp = Object.FindObjectsByType<LampController>(FindObjectsSortMode.None).Single();
        Assert.IsNotNull(lamp.sun, "lamp: sun wired");
        Assert.IsNotNull(lamp.bulb, "lamp: bulb wired");

        var cycler = Object.FindObjectsByType<SchemeCycler>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsNotNull(cycler.manager, "cycler: SchemeManager wired");
    }

    [UnityTest]
    public IEnumerator SamplePrefab_PulledSample_PreviewsAndCommits()
    {
        yield return null;
        var floor = Surface.All.Single(s => s != null && s.name == "Floor");
        var wall = Surface.All.Single(s => s != null && s.name == "Wall_N");
        var prefab = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].samplePrefab;

        // Spawn a sample as SamplePuller would, hold it "near" Wall_N by parking
        // it against the wall, and drive Update manually via frames.
        var go = Object.Instantiate(prefab, wall.transform.position, Quaternion.identity);
        var sample = go.GetComponent<Sample>();
        sample.Init(floor);
        Assert.AreEqual(7, sample.Palette.Length);

        // HoldUpPreviewer only runs while grabbed; simulate its core contract directly:
        wall.Preview(sample.CurrentColor);
        Assert.IsTrue(wall.IsPreviewing);
        wall.Commit(sample.CurrentColor);
        Assert.IsFalse(wall.IsPreviewing);
        Assert.AreEqual(sample.CurrentColor, wall.CommittedColor);

        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Menu_IsOnLeftController_WithCatalogueAndEventSystem()
    {
        yield return null;
        var menu = Object.FindObjectsByType<ControllerMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.IsTrue(menu.transform.parent != null && menu.transform.parent.name == "Left Controller", "menu parented to Left Controller");
        Assert.IsNotNull(menu.catalogue, "catalogue wired");
        Assert.Greater(menu.catalogue.paints.Count, 0);
        Assert.Greater(menu.catalogue.materials.Count, 0);
        Assert.Greater(menu.catalogue.furniture.Count, 0);
        Assert.IsNotNull(menu.head, "head wired");
        Assert.IsNotNull(Object.FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>(), "XRUIInputModule present");
        var relay = Object.FindObjectsByType<MenuSelectRelay>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreSame(menu, relay.menu);
        Assert.IsNotNull(relay.selectAction.action);
    }

    [UnityTest]
    public IEnumerator EverySurface_HasMenuTargetAndKind()
    {
        yield return null;
        foreach (var s in Surface.All.Where(s => s != null))
        {
            Assert.IsNotNull(s.GetComponent<MenuTarget>(), $"{s.name}: MenuTarget");
            if (s.name != "Sofa") Assert.AreNotEqual(SurfaceKind.None, s.Kind, $"{s.name}: kind set");
        }
        var sofa = Surface.All.Single(s => s != null && s.name == "Sofa");
        Assert.IsNotNull(sofa.GetComponent<FurnitureSlot>(), "sofa is a furniture slot");
        Assert.IsNotNull(sofa.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(), "sofa grabbable");
    }

    [UnityTest]
    public IEnumerator RayFeedback_IsOnRightController()
    {
        yield return null;
        var fb = Object.FindObjectsByType<RayFeedback>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        Assert.AreEqual("Right Controller", fb.transform.name);
        Assert.IsNotNull(fb.menu, "menu wired");
        Assert.IsNotNull(fb.rayOrigin, "ray origin wired");
        Assert.IsNotNull(fb.emissionCarrier, "emission carrier material wired");
        Assert.IsTrue(fb.emissionCarrier.IsKeywordEnabled("_EMISSION"));
    }

    [UnityTest]
    public IEnumerator Room_IsRoomWByRoomD_AndFloorTeleports()
    {
        yield return null;
        var floor = Surface.All.Single(s => s != null && s.name == "Floor");
        Assert.AreEqual(RoomSpec.W, floor.transform.localScale.x, 0.001f);
        Assert.AreEqual(RoomSpec.D, floor.transform.localScale.z, 0.001f);
        var tele = floor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
        Assert.IsNotNull(tele, "floor is a teleport area");
        Assert.AreEqual(1 << RoomSpec.TeleportLayer, tele.interactionLayers.value, "teleport layer only");
        var rig = GameObject.Find("XR Origin (XR Rig)");
        Assert.AreEqual(180f, rig.transform.eulerAngles.y, 0.5f, "rig faces the sofa");
    }
}
