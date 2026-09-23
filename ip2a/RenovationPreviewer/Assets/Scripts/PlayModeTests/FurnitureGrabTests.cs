using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Furniture grab end to end in the real Room scene: the right NearFarInteractor aimed
/// at a preset chair must see it as a valid target, a select must mark it held, and
/// re-aiming the ray at another floor point must carry the chair there.
/// </summary>
public class FurnitureGrabTests
{
    [UnitySetUp]
    public IEnumerator LoadRoom()
    {
        SceneManager.LoadScene("Room");
        yield return null; yield return null; yield return null;
    }

    static GameObject RightController()
    {
        // No XR device headless → the modality manager leaves the controllers inactive; wake the right one.
        var rig = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        Assert.IsNotNull(rig, "XR Origin");
        var right = rig.transform.Find("Camera Offset/Right Controller")?.gameObject;
        Assert.IsNotNull(right, "Right Controller");
        right.SetActive(true);
        return right;
    }

    static void Pose(Transform controller, Vector3 from, Vector3 at)
    {
        foreach (var g in controller.GetComponents<EditorGazeAim>()) g.enabled = false;
        var tpd = controller.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        if (tpd != null) tpd.enabled = false;
        controller.SetPositionAndRotation(from, Quaternion.LookRotation(at - from));
    }

    [UnityTest]
    public IEnumerator AimedAtChair_GrabThenReaim_ChairFollowsRay()
    {
        var slots = Object.FindObjectsByType<FurnitureSlot>(FindObjectsSortMode.None);
        var chair = slots.FirstOrDefault(s => s.name.ToLower().Contains("chair"));
        Assert.IsNotNull(chair, "a chair in preset 0: " + string.Join(",", slots.Select(s => s.name)));
        var controller = RightController().transform;
        var nf = controller.GetComponentInChildren<NearFarInteractor>();
        Assert.IsNotNull(nf, "right NearFarInteractor");

        var col = chair.GetComponent<BoxCollider>();
        var aimAt = col.bounds.center;
        var from = aimAt + (new Vector3(0f, 1.3f, 0f) - new Vector3(aimAt.x, 0f, aimAt.z)).normalized * 2.5f;
        from.y = 1.3f;
        Pose(controller, from, aimAt);
        for (int i = 0; i < 5; i++) yield return null;

        var targets = new List<IXRInteractable>();
        nf.GetValidTargets(targets);
        Debug.Log($"[GrabProbe] chair={chair.name} col={col.bounds} from={from} targets=[{string.Join(",", targets.Select(t => (t as Component)?.name))}] enabled={nf.enabled} layers={nf.interactionLayers.value}");
        var grab = chair.GetComponent<XRGrabInteractable>();
        Debug.Log($"[GrabProbe] grab enabled={grab.enabled} layers={grab.interactionLayers.value} colliders={grab.colliders.Count} trackPos={grab.trackPosition}");
        CollectionAssert.Contains(targets, grab, "ray on the chair → chair is a valid select target");

        var before = chair.transform.position;
        nf.interactionManager.SelectEnter((IXRSelectInteractor)nf, (IXRSelectInteractable)grab);
        yield return null; yield return null;
        Debug.Log($"[GrabProbe] after select: IsHeld={chair.IsHeld} isSelecting={nf.hasSelection} region={nf.selectionRegion.Value} pos={chair.transform.position}");
        Assert.IsTrue(chair.IsHeld, "held after select");
        Assert.Less(Vector3.Distance(chair.transform.position, before), 0.05f, "grabbing must not move the chair (was a 1.3 m jump to where the ray met the floor behind it)");

        // Re-aim: the ray's floor point moves by delta → the chair moves by the same delta.
        var oldFloor = FurnitureSlot.FloorPointOnRay(from, (aimAt - from).normalized);
        // Toward the room centre, so the clamp at the walls never decides the result.
        var newAim = aimAt + new Vector3(-Mathf.Sign(aimAt.x) * 0.8f, 0f, -Mathf.Sign(aimAt.z) * 0.5f);
        Pose(controller, from, newAim);
        for (int i = 0; i < 5; i++) yield return null;
        var delta = FurnitureSlot.FloorPointOnRay(from, (newAim - from).normalized) - oldFloor;
        var want = before + delta;
        Debug.Log($"[GrabProbe] after re-aim: pos={chair.transform.position} want≈{want} held={chair.IsHeld} region={nf.selectionRegion.Value}");
        Assert.Less(Vector3.Distance(chair.transform.position, want), 0.05f, "chair follows the ray, keeping its grab offset");

        nf.interactionManager.SelectExit((IXRSelectInteractor)nf, (IXRSelectInteractable)grab);
        yield return null;
        Assert.IsFalse(chair.IsHeld);
        Assert.AreEqual(0f, chair.transform.position.y, 1e-4f, "released flat on the floor");
    }
}
