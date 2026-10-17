using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Pull: with a kept surface in reach (touching it, or within the affordance radius
/// where its tab is showing), pressing trigger spawns a sample already held by that
/// controller. Pulling from a Change surface is refused — only kept things are
/// sources; this IS the concept (spec §9). The sample spawns at the tab so it
/// visibly peels off the surface into the hand (XRI eases it in).
/// </summary>
public class SamplePuller : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public GameObject samplePrefab;
    public XRBaseInteractor interactor;

    Surface touching;

    public Surface Touching => touching;
    /// <summary>The kept surface this hand could pull from right now, else null.</summary>
    public Surface Pullable { get; private set; }
    public bool CanPull => Pullable != null;

    void OnEnable() => triggerAction.action?.Enable();

    void OnTriggerEnter(Collider other)
    {
        var s = other.GetComponentInParent<Surface>();
        if (s != null) touching = s;
    }

    void OnTriggerExit(Collider other)
    {
        if (touching != null && other.GetComponentInParent<Surface>() == touching) touching = null;
    }

    void Update()
    {
        Pullable = touching != null && touching.State == SurfaceState.Keep
            ? touching
            : PullAffordance.NearestPullable(transform.position, out _);
        if (Pullable == null || samplePrefab == null || interactor == null) return;
        if (triggerAction.action == null || !triggerAction.action.WasPressedThisFrame()) return;

        var aff = Pullable.GetComponent<PullAffordance>();
        var at = aff != null && aff.IsVisible ? aff.TabPosition : transform.position;
        SpawnFrom(Pullable, at);
        if (aff != null) aff.Peel(Time.time);
    }

    /// <summary>Spawn a sample of source at a world point and put it straight into this hand.</summary>
    public Sample SpawnFrom(Surface source, Vector3 at)
    {
        if (source == null || samplePrefab == null || interactor == null) return null;
        var go = Instantiate(samplePrefab, at, Quaternion.identity);
        var sample = go.GetComponent<Sample>();
        sample.Init(source);
        var grab = go.GetComponent<XRGrabInteractable>();
        interactor.interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
        return sample;
    }
}
