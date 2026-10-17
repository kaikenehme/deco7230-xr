using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The only writer of the sun, ambient light, skybox and environment GI. The lamp owns
/// its bulb and nothing else, so the two never fight. Starts at 10:00. The wall clock
/// calls Next(); the editor also takes F5–F10 for the facilitator (1/2/3 belong to the
/// XR Device Simulator).
/// </summary>
public class TimeOfDayController : MonoBehaviour
{
    public Light sun;
    public Material sky;
    public int Index { get; private set; } = 1;
    public TimeOfDay.Stop Current => TimeOfDay.Stops[Index];
    public event Action<int> Changed;

    static readonly int ExposureId = Shader.PropertyToID("_Exposure");
    static readonly int TintId = Shader.PropertyToID("_SkyTint");

    void Awake()
    {
        // Instance the sky so play-mode edits never write into the asset.
        if (Application.isPlaying && sky != null) { sky = new Material(sky); RenderSettings.skybox = sky; }
    }

    void Start() => Apply(Index);

    public void Next() => Apply(TimeOfDay.Next(Index));

    public void Apply(int i)
    {
        Index = Mathf.Clamp(i, 0, TimeOfDay.Stops.Length - 1);
        var s = TimeOfDay.Stops[Index];
        if (sun != null)
        {
            sun.transform.rotation = TimeOfDay.SunRotation(s);
            sun.color = s.sunColor;
            sun.intensity = s.sunIntensity;
            sun.enabled = s.SunOn;
        }
        if (Application.isPlaying)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = s.ambient;
            if (sky != null) { sky.SetFloat(ExposureId, s.skyExposure); sky.SetColor(TintId, s.skyTint); }
            DynamicGI.UpdateEnvironment();
        }
        Changed?.Invoke(Index);
    }

#if UNITY_EDITOR
    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.f5Key.wasPressedThisFrame) Apply(0);
        else if (kb.f6Key.wasPressedThisFrame) Apply(1);
        else if (kb.f7Key.wasPressedThisFrame) Apply(2);
        else if (kb.f8Key.wasPressedThisFrame) Apply(3);
        else if (kb.f9Key.wasPressedThisFrame) Apply(4);
        else if (kb.f10Key.wasPressedThisFrame) Apply(5);
    }
#endif
}
