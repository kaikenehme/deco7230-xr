using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Writes the three IP2b RoomPreset assets (Living, Bedroom, Empty) (+ card textures and frame materials) from code,
/// resolving materials, paints and furniture through Catalogue.asset. Re-runnable.
/// Menu: Renovation → Build Presets. Headless: -executeMethod PresetAuthoring.Build.
/// </summary>
public static class PresetAuthoring
{
    const string Root = "Assets/Presets";
    public static readonly string[] Names = { "Living", "Bedroom", "Empty" };
    static readonly string[] Walls = { "Wall_N", "Wall_S", "Wall_E", "Wall_W" };
    static readonly string[] Trims = { "Door", "Trim", "WindowFrame" };

    [MenuItem("Renovation/Build Presets")]
    public static void Build()
    {
        var cat = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        if (cat == null) { Debug.LogError("PresetAuthoring: run Renovation → Import Catalogue first"); return; }
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "Presets");

        // IP2b layouts for the 5.5 x 5 m room (x east, z north; yaw = the way the piece faces, 0 = +Z).
        // Kept clear (SceneWiringTests): start spot (0.9, 1.3), preset frames on Wall_N (x -1.4..0.5),
        // door (NE), arc lamp (SW corner). Mock-ups: ip2b/art/room_v2_*.png, ip2b/art/bedroom_v1.png.

        // ---- Living: warm contemporary (references A + F) ----
        var living = Preset(cat, "Living", "Living");
        Floor(cat, living, "WoodFloor051");
        Paint(cat, living, Walls, "Sandy Day");   // the mock-up's warm white (#DCD1BF)
        Paint(cat, living, new[] { "Ceiling" }, "Whisper White");
        Paint(cat, living, Trims, "Antique White U.S.A.");
        Place(cat, living, "curved_sofa", -2.12f, 0.2f, 90f);         // back to Wall_W, under the prints
        Place(cat, living, "oval_marble_table", -0.9f, 0.2f, 90f);                // long axis along z, on the rug
        Place(cat, living, "tv_unit", 2.53f, 0.2f, -90f);                         // against Wall_E, facing the sofa
        Place(cat, living, "round_armchair", 0.9f, -1.6f, -45f);                  // by the window, angled to the table
        Place(cat, living, "pouf", -0.9f, -1.25f, 0f);
        Place(cat, living, "pouf", 2.05f, -1.65f, 0f);
        Place(cat, living, "monstera_stand", -2.3f, 2.05f, 0f);                   // NW corner
        Swatches(cat, living, "WoodFloor051", "Sandy Day", "round_armchair");

        // ---- Bedroom: mid-century — walnut, teal wall behind the bed, mustard duvet ----
        var bed = Preset(cat, "Bedroom", "Bedroom");
        Floor(cat, bed, "WoodFloor043");
        Paint(cat, bed, new[] { "Wall_N", "Wall_S", "Wall_E" }, "Antique White U.S.A.");
        PaintHex(p: bed, surfaces: new[] { "Wall_W" }, hex: "#2F6B6A", userColour: true);   // mid-century teal (no teal in the 24 Dulux set)
        Paint(cat, bed, new[] { "Ceiling" }, "Whisper White");
        Paint(cat, bed, Trims, "Antique White U.S.A.");
        Place(cat, bed, "bed_platform", -1.75f, 0.2f, 90f);           // headboard to Wall_W
        Place(cat, bed, "bed_nightstand_lamp", -2.45f, -1.25f, 90f);
        Place(cat, bed, "bed_nightstand", -2.45f, 1.65f, 90f);
        Place(cat, bed, "bed_vanity", 2.38f, 0.0f, -90f);                         // against Wall_E
        Place(cat, bed, "mid_century_lounge_chair", 1.0f, -1.6f, -45f);           // reading spot by the window
        Place(cat, bed, "monstera_stand", 2.3f, -2.05f, 0f);
        Place(cat, bed, "potted_plant_01", 2.35f, 1.15f, 0f);
        Swatches(cat, bed, "WoodFloor043", "Antique White U.S.A.", "bed_platform");

        // ---- Empty room (20 Oct, replaces the MR showcase): bare white walls, plain light boards, nothing else ----
        var empty = Preset(cat, "Empty", "Empty room");
        empty.bare = true;
        Floor(cat, empty, "WoodFloor051");
        Paint(cat, empty, Walls, "Whisper White");
        Paint(cat, empty, new[] { "Ceiling" }, "Whisper White");
        Paint(cat, empty, Trims, "Antique White U.S.A.");
        Swatches(cat, empty, "WoodFloor051", "Whisper White", "Whisper White");

        foreach (var p in new[] { living, bed, empty }) { WriteCard(p); EditorUtility.SetDirty(p); }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        foreach (var old in new[] { "Scandi", "MidCentury" })   // retired in IP2b
        {
            AssetDatabase.DeleteAsset(AssetPath(old));
            AssetDatabase.DeleteAsset(CardMaterialPath(old));
            AssetDatabase.DeleteAsset($"{Root}/Card_{old}.png");
        }
        Debug.Log("PresetAuthoring: 3 presets written (Living, Bedroom, Empty)");
    }

    public static string AssetPath(string name) => $"{Root}/{name}.asset";
    public static string CardMaterialPath(string name) => $"{Root}/Card_{name}.mat";

    static RoomPreset Preset(Catalogue cat, string fileName, string display)
    {
        var path = AssetPath(fileName);
        var p = AssetDatabase.LoadAssetAtPath<RoomPreset>(path);
        if (p == null) { p = ScriptableObject.CreateInstance<RoomPreset>(); AssetDatabase.CreateAsset(p, path); }
        p.displayName = display;
        p.looks.Clear(); p.furniture.Clear();
        return p;
    }

    static void Floor(Catalogue cat, RoomPreset p, string materialId)
    {
        var m = cat.Material(materialId);
        if (m == null) { Debug.LogWarning($"PresetAuthoring: material {materialId} missing"); return; }
        p.looks.Add(new SurfaceLookSpec { surfaceName = "Floor", state = SurfaceState.Change,   // IP2b N3: the floor takes paint
            material = m.material, color = Color.white, sampleColor = m.sampleColor });
    }

    static void Paint(Catalogue cat, RoomPreset p, IEnumerable<string> surfaces, string paintName, bool userColour = false)
    {
        var c = PaintColour(cat, paintName);
        foreach (var s in surfaces)
            p.looks.Add(new SurfaceLookSpec { surfaceName = s, state = SurfaceState.Change, material = null, color = c, userColour = userColour, sampleColor = c });
    }

    static void PaintHex(RoomPreset p, IEnumerable<string> surfaces, string hex, bool userColour = false)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        foreach (var s in surfaces)
            p.looks.Add(new SurfaceLookSpec { surfaceName = s, state = SurfaceState.Change, material = null, color = c, userColour = userColour, sampleColor = c });
    }

    static void Place(Catalogue cat, RoomPreset p, string id, float x, float z, float yaw, bool keep = false)
    {
        var f = cat.Furniture(id);
        if (f == null) Debug.LogWarning($"PresetAuthoring: furniture {id} not in the catalogue — placement kept, applier will skip it");
        p.furniture.Add(new PlacementSpec { sourceId = id, position = new Vector3(x, 0f, z), yaw = yaw, keep = keep, sampleColor = f != null ? f.sampleColor : Color.grey });
    }

    static void Swatches(Catalogue cat, RoomPreset p, string floorMat, string wallPaint, string accent)
    {
        p.floorSwatch = cat.Material(floorMat)?.sampleColor ?? Color.grey;
        p.wallSwatch = PaintColour(cat, wallPaint);
        var f = cat.Furniture(accent);
        p.accentSwatch = f != null ? f.sampleColor : PaintColour(cat, accent);
    }

    static Color PaintColour(Catalogue cat, string name)
    {
        var paint = cat.Paint(name);
        if (paint != null) return paint.color;
        Debug.LogWarning($"PresetAuthoring: paint '{name}' not in the catalogue, using a fallback");
        return name switch
        {
            "Terracotta" => new Color(0.78f, 0.45f, 0.32f),
            "Powder Blue" => new Color(0.68f, 0.78f, 0.86f),
            "Tranquil Retreat" => new Color(0.84f, 0.86f, 0.85f),
            _ => new Color(0.95f, 0.94f, 0.91f),
        };
    }

    static void WriteCard(RoomPreset p)
    {
        var name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(p));
        var pngPath = $"{Root}/Card_{name}.png";
        var tex = PresetCard.Render(p);
        File.WriteAllBytes(pngPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(pngPath);
        var imp = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (imp != null && imp.mipmapEnabled) { imp.mipmapEnabled = false; imp.SaveAndReimport(); }

        var matPath = CardMaterialPath(name);
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(mat, matPath); }
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
        EditorUtility.SetDirty(mat);
    }
}
