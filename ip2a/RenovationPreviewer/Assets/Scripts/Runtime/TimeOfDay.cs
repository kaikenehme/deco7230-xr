using UnityEngine;

/// <summary>
/// Six discrete times of day (Q7: are three lighting states enough? now testable).
/// Pure data + math; TimeOfDayController applies a stop to the scene.
/// Azimuth 0 = light travels +Z, i.e. in through the window on Wall_S; elevation is the
/// sun's height above the horizon. A hemisphere-accurate sun path is not modelled.
/// </summary>
public static class TimeOfDay
{
    public struct Stop
    {
        public int hour;
        public float elevation, azimuth;
        public Color sunColor; public float sunIntensity;
        public Color ambient;
        public float skyExposure; public Color skyTint;
        public bool SunOn => sunIntensity > 0f;
    }

    public static readonly Stop[] Stops =
    {
        new() { hour = 7,  elevation = 12f,  azimuth = -65f, sunColor = new(1f, 0.80f, 0.60f), sunIntensity = 0.7f, ambient = new(0.28f, 0.27f, 0.30f), skyExposure = 0.9f,  skyTint = new(0.60f, 0.45f, 0.40f) },
        new() { hour = 10, elevation = 40f,  azimuth = -35f, sunColor = new(1f, 0.96f, 0.90f), sunIntensity = 1.1f, ambient = new(0.42f, 0.43f, 0.45f), skyExposure = 1.2f,  skyTint = new(0.50f, 0.50f, 0.50f) },
        new() { hour = 13, elevation = 62f,  azimuth = 0f,   sunColor = new(1f, 0.98f, 0.95f), sunIntensity = 1.3f, ambient = new(0.48f, 0.48f, 0.50f), skyExposure = 1.3f,  skyTint = new(0.50f, 0.50f, 0.50f) },
        new() { hour = 16, elevation = 38f,  azimuth = 40f,  sunColor = new(1f, 0.92f, 0.80f), sunIntensity = 1.0f, ambient = new(0.40f, 0.39f, 0.40f), skyExposure = 1.1f,  skyTint = new(0.50f, 0.50f, 0.50f) },
        new() { hour = 19, elevation = 6f,   azimuth = 75f,  sunColor = new(1f, 0.55f, 0.30f), sunIntensity = 0.5f, ambient = new(0.20f, 0.16f, 0.18f), skyExposure = 0.6f,  skyTint = new(0.70f, 0.35f, 0.25f) },
        new() { hour = 22, elevation = -20f, azimuth = 90f,  sunColor = Color.black,           sunIntensity = 0f,   ambient = new(0.06f, 0.07f, 0.10f), skyExposure = 0.05f, skyTint = new(0.20f, 0.25f, 0.40f) },
    };

    public static int Next(int i) => (i + 1) % Stops.Length;
    public static Quaternion SunRotation(Stop s) => Quaternion.Euler(s.elevation, s.azimuth, 0f);
    /// <summary>Clockwise hour-hand angle from 12 o'clock, degrees.</summary>
    public static float HourHandAngle(int hour) => (hour % 12) * 30f;
}
