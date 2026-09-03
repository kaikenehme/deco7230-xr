using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Not assertions — pictures. Renders the room from fixed viewpoints into
/// Builds/snapshots/*.png (gitignored) so the scene can be checked headless.
/// </summary>
public class SnapshotTests
{
    static string Dir => Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/snapshots"));

    [UnitySetUp]
    public IEnumerator LoadRoom()
    {
        SceneManager.LoadScene("Room");
        yield return null; yield return null; yield return null;   // presets applied, Start() everywhere
        Directory.CreateDirectory(Dir);
    }

    static void Shoot(string name, Vector3 pos, Vector3 lookAt, float fov = 70f, int w = 1280, int h = 720)
    {
        var go = new GameObject("__snap");
        var cam = go.AddComponent<Camera>();
        cam.fieldOfView = fov; cam.nearClipPlane = 0.05f; cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.Skybox;
        go.transform.position = pos;
        go.transform.LookAt(lookAt);
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        var prev = RenderTexture.active; RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        RenderTexture.active = prev;
        File.WriteAllBytes(Path.Combine(Dir, name + ".png"), tex.EncodeToPNG());
        Object.Destroy(tex); cam.targetTexture = null; rt.Release(); Object.Destroy(go);
    }

    static readonly Vector3 Eye = new(0.3f, 1.5f, 0.8f);   // rig start, standing height

    [UnityTest]
    public IEnumerator Snapshots()
    {
        var tod = Object.FindObjectsByType<TimeOfDayController>(FindObjectsSortMode.None).Single();
        var applier = Object.FindObjectsByType<PresetApplier>(FindObjectsSortMode.None).Single();

        yield return new WaitForSeconds(0.6f);   // mid-pulse
        Shoot("00_onboarding_pulse", Eye, new Vector3(0f, 1.2f, -3.5f));
        yield return new WaitForSeconds(2.2f);   // pulses over, labels still up
        Shoot("01_start_1000_toward_window", Eye, new Vector3(0f, 1.2f, -3.5f));
        Shoot("02_wallW_frames", Eye, new Vector3(-4.5f, 1.4f, 0.8f));
        Shoot("03_lamp_and_clock", new Vector3(1.5f, 1.5f, -1.0f), new Vector3(1.5f, 1.2f, -3.5f), 80f);
        Shoot("04_overview_from_door", new Vector3(2.5f, 2.3f, 3.2f), new Vector3(-0.5f, 0.6f, -1.0f), 80f);

        foreach (var (i, tag) in new[] { (0, "0700"), (2, "1300"), (4, "1900"), (5, "2200") })
        {
            tod.Apply(i); yield return null; yield return null;
            Shoot($"05_time_{tag}", Eye, new Vector3(0f, 1.2f, -3.5f));
            if (i == 2) Shoot("05_time_1300_floor_patch", new Vector3(0.8f, 2.6f, -0.2f), new Vector3(0.8f, 0f, -2.6f), 75f);
        }
        tod.Apply(1); yield return null;

        for (int p = 1; p < applier.presets.Length; p++)
        {
            applier.Apply(p); yield return null; yield return null;
            Shoot($"06_preset_{p}_{applier.presets[p].displayName}", new Vector3(2.5f, 2.3f, 3.2f), new Vector3(-0.5f, 0.6f, -1.0f), 80f);
        }
        applier.Apply(0); yield return null; yield return null;

        // A fake hand near the kept sofa and floor: the affordance tab + a held sample with its thread.
        var hand = new GameObject("FakeHand");
        hand.AddComponent<SphereCollider>().isTrigger = true;
        var mark = hand.AddComponent<MarkTool>();
        var sofa = Surface.All.First(s => s != null && s.GetComponent<FurnitureSlot>() != null && s.State == SurfaceState.Keep);
        var sofaTop = sofa.WorldBounds;
        hand.transform.position = new Vector3(sofaTop.center.x, sofaTop.max.y + 0.12f, sofaTop.center.z + 0.2f);
        yield return null; yield return null;
        Shoot("07_affordance_sofa", new Vector3(sofaTop.center.x + 0.9f, sofaTop.max.y + 0.9f, sofaTop.center.z + 1.4f), hand.transform.position, 50f);

        hand.transform.position = new Vector3(0.5f, 0.15f, -0.5f);
        yield return null; yield return null;
        Shoot("08_affordance_floor", new Vector3(0.5f, 1.2f, 0.6f), hand.transform.position, 50f);

        var puller = Object.FindObjectsByType<SamplePuller>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
        var sampleGo = Object.Instantiate(puller.samplePrefab, new Vector3(-1.0f, 1.2f, -1.0f), Quaternion.identity);
        sampleGo.GetComponent<Sample>().Init(sofa);
        yield return null; yield return null;
        Shoot("09_sample_thread", new Vector3(0.2f, 1.5f, 0.4f), sampleGo.transform.position, 50f);
        Object.Destroy(sampleGo); Object.Destroy(hand);
        yield return null;
        Assert.IsTrue(Directory.GetFiles(Dir, "*.png").Length >= 10);
    }
}
