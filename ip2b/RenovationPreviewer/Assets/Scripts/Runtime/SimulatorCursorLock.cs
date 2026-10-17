#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor-only: locks the mouse cursor as soon as play mode starts so XR Device
/// Simulator look and clicks stay inside the Game view. Esc (or the simulator's
/// backslash toggle) releases it; clicking back into the Game view re-locks.
/// Compiled out of device builds entirely.
/// </summary>
public class SimulatorCursorLock : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject(nameof(SimulatorCursorLock)) { hideFlags = HideFlags.HideAndDontSave };
        DontDestroyOnLoad(go);
        go.AddComponent<SimulatorCursorLock>();
    }

    void Start()
    {
        if (EditorDesktopRig.Current == null) Cursor.lockState = CursorLockMode.Locked;   // desktop mode keeps the cursor free
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (EditorDesktopRig.Current == null && mouse != null && Cursor.lockState == CursorLockMode.None && mouse.leftButton.wasPressedThisFrame)
            Cursor.lockState = CursorLockMode.Locked;
    }
}
#endif
