using UnityEngine;

/// <summary>Procedural picture for a preset frame: wall band, accent stripe, floor band. No art assets.</summary>
public static class PresetCard
{
    public static Texture2D Render(RoomPreset p, int size = 256)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = $"Card_{p.displayName}", filterMode = FilterMode.Bilinear };
        int floorTop = (int)(size * 0.38f), accentTop = (int)(size * 0.62f), accentBottom = (int)(size * 0.54f);
        for (int y = 0; y < size; y++)
        {
            Color c = y < floorTop ? p.floorSwatch : (y >= accentBottom && y < accentTop ? p.accentSwatch : p.wallSwatch);
            for (int x = 0; x < size; x++) tex.SetPixel(x, y, c);
        }
        // a sofa-ish block on the floor so the card reads as a room, not a flag
        var sofa = Color.Lerp(p.accentSwatch, Color.black, 0.35f);
        for (int y = (int)(size * 0.30f); y < (int)(size * 0.46f); y++)
            for (int x = (int)(size * 0.22f); x < (int)(size * 0.62f); x++) tex.SetPixel(x, y, sofa);
        tex.Apply();
        return tex;
    }
}
