using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Pull: touching a Keep surface and pressing trigger spawns a sample already
/// held by that controller. Pulling from a Change surface is refused — only
/// kept things are sources; this IS the concept (spec §9).
/// </summary>
public class SamplePuller : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public GameObject samplePrefab;
    public XRBaseInteractor interactor;

    Surface touching;

    void OnEnable() => triggerAction.action?.Enable();

    void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponent<Surface>();
        if (s != null) touching = s;
    }

    void OnTriggerExit(Collider other)
    {
        if (touching != null && other.GetComponent<Surface>() == touching) touching = null;
    }

    void Update()
    {
        if (touching == null || samplePrefab == null || interactor == null) return;
        if (triggerAction.action == null || !triggerAction.action.WasPressedThisFrame()) return;
        if (touching.State != SurfaceState.Keep) return;

        var go = Instantiate(samplePrefab, transform.position, Quaternion.identity);
        go.GetComponent<Sample>().Init(touching);
        var grab = go.GetComponent<XRGrabInteractable>();
        interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
    }
}
