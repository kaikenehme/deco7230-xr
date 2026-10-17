using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IP2b N2: the pulled sample opens in the hand like a paint fan deck — seven cards on an arc
/// above it, all visible at once, lightest on the left. Picking a card makes it the sample's
/// colour (the one hold-up previews and commits). Replaces twist-to-tune. Still a held object,
/// not a menu. No text on the cards (text readability principle).
/// </summary>
[RequireComponent(typeof(Sample))]
public class FanDeck : MonoBehaviour
{
    public const float Radius = 0.16f, SpreadDeg = 120f;   // ~5.6 cm between card centres: 5 cm cards just touch
    static readonly Vector3 ChipSize = new(0.05f, 0.08f, 0.004f);
    const float SelectedScale = 1.3f, SelectedLift = 0.02f;

    public int Selected { get; private set; } = -1;
    public IReadOnlyList<FanChip> Chips => chips;

    readonly List<FanChip> chips = new();
    Sample sample;
    Transform fan;
    Renderer sampleRenderer;

    /// <summary>Card i of count on a fan arc in fan space (metres): middle card straight up.</summary>
    public static Vector3 ChipLocalPosition(int i, int count)
    {
        float a = (count <= 1 ? 0f : Mathf.Lerp(-SpreadDeg / 2f, SpreadDeg / 2f, i / (float)(count - 1))) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(a) * Radius, Mathf.Cos(a) * Radius, 0f);
    }

    public void Build(Color[] palette)
    {
        sample = GetComponent<Sample>();
        sampleRenderer = GetComponent<Renderer>();
        if (fan != null) DestroyImmediate(fan.gameObject);
        chips.Clear();

        fan = new GameObject("Fan").transform;
        fan.SetParent(transform, false);
        var ls = transform.lossyScale;   // the sample sphere is scaled; the fan works in metres
        fan.localScale = new Vector3(1f / Mathf.Max(ls.x, 1e-4f), 1f / Mathf.Max(ls.y, 1e-4f), 1f / Mathf.Max(ls.z, 1e-4f));

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        for (int i = 0; i < palette.Length; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Chip_{i}";
            go.transform.SetParent(fan, false);
            go.transform.localPosition = ChipLocalPosition(i, palette.Length);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Lerp(-SpreadDeg / 2f, SpreadDeg / 2f, palette.Length <= 1 ? 0.5f : i / (float)(palette.Length - 1)));
            go.transform.localScale = ChipSize;
            var col = go.GetComponent<BoxCollider>(); col.isTrigger = true;
            var r = go.GetComponent<Renderer>();
            if (shader != null) r.sharedMaterial = new Material(shader) { color = palette[i] };
            var chip = go.AddComponent<FanChip>();
            chip.Index = i; chip.Deck = this; chip.Colour = palette[i];
            chips.Add(chip);
        }
        Selected = -1;
        Pick(2);
    }

    public void Pick(int i)
    {
        if (i < 0 || i >= chips.Count) return;
        if (Selected >= 0 && Selected < chips.Count) Raise(chips[Selected], false);
        Selected = i;
        Raise(chips[i], true);
        if (sample != null && sample.Palette != null && i < sample.Palette.Length)
        {
            sample.CurrentColor = sample.Palette[i];
            if (sampleRenderer != null && Application.isPlaying) sampleRenderer.material.color = sample.CurrentColor;
        }
    }

    void Raise(FanChip c, bool on)
    {
        var basePos = ChipLocalPosition(c.Index, chips.Count);
        c.transform.localPosition = on ? basePos + basePos.normalized * SelectedLift : basePos;
        c.transform.localScale = on ? ChipSize * SelectedScale : ChipSize;
    }

    void LateUpdate()
    {
        // Cards face the viewer so all seven read at a glance.
        var cam = Camera.main;
        if (fan == null || cam == null) return;
        var away = fan.position - cam.transform.position;
        if (away.sqrMagnitude > 1e-6f) fan.rotation = Quaternion.LookRotation(away.normalized, Vector3.up);
    }
}
