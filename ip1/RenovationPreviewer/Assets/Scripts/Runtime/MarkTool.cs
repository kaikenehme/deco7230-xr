using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sweep-to-mark: brushing a controller across a surface while holding grip
/// toggles its Keep/Change state. Grip gate + speed threshold answer open Q3
/// (accidental-sweep ambiguity); per-surface debounce stops double toggles.
/// Also doubles as the "controller cue" collider other diegetic props detect.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MarkTool : MonoBehaviour
{
    public InputActionProperty gripAction;

    const float SpeedThreshold = 0.6f, HoldTime = 0.15f, Debounce = 0.5f;

    Vector3 lastPos;
    float speed, sweepTimer;
    Surface candidate;
    float lastToggleTime = -10f;

    void Awake()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void OnEnable() => gripAction.action?.Enable();

    void Update()
    {
        speed = Mathf.Lerp(speed,
            (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 1e-4f), 0.5f);
        lastPos = transform.position;
    }

    void OnTriggerStay(Collider other)
    {
        var surf = other.GetComponent<Surface>();
        if (surf == null) return;

        bool grip = gripAction.action != null && gripAction.action.ReadValue<float>() > 0.5f;
        if (!grip || speed < SpeedThreshold) { sweepTimer = 0f; candidate = null; return; }

        if (candidate != surf) { candidate = surf; sweepTimer = 0f; }
        sweepTimer += Time.deltaTime;

        if (sweepTimer >= HoldTime && Time.time - lastToggleTime > Debounce)
        {
            surf.ToggleState();
            lastToggleTime = Time.time;
            sweepTimer = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (candidate != null && other.GetComponent<Surface>() == candidate)
        {
            candidate = null;
            sweepTimer = 0f;
        }
    }
}
