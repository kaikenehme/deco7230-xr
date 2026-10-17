using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Facilitator control for the IP2a protocol: Task 1 starts with a sample already in
/// the participant's hand so everyone reaches the constrained set (Evaluation 1 §05).
/// Holding the left X button for a second spawns a floor sample into the right hand.
/// Works on device, so no keyboard is needed.
/// </summary>
public class FacilitatorSpawn : MonoBehaviour
{
    public const float HoldSeconds = 1f;

    public InputActionProperty spawnAction;
    public SamplePuller puller;

    float held;
    bool fired;

    public static bool ShouldFire(float heldSeconds, bool alreadyFired) => heldSeconds >= HoldSeconds && !alreadyFired;

    void OnEnable() => spawnAction.action?.Enable();

    void Update()
    {
        if (spawnAction.action == null || !spawnAction.action.IsPressed()) { held = 0f; fired = false; return; }
        held += Time.deltaTime;
        if (!ShouldFire(held, fired)) return;
        fired = true;
        var source = Surface.All.FirstOrDefault(s => s != null && s.State == SurfaceState.Keep && s.Kind == SurfaceKind.Floor)
                     ?? Surface.All.FirstOrDefault(s => s != null && s.State == SurfaceState.Keep);
        if (source != null && puller != null) puller.SpawnFrom(source, transform.position);
    }
}
