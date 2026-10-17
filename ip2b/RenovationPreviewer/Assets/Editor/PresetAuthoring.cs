using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Writes the three RoomPreset assets (+ card textures and frame materials) from code,
/// resolving materials, paints and furniture through Catalogue.asset. Re-runnable.
/// Menu: Renovation → Build Presets. Headless: -executeMethod PresetAuthoring.Build.
/// </summary>
public static class PresetAuthoring
{
    const string Root = "Assets/Presets";
    public static readonly string[] Names = { "Scandi", "MidCentury", "Bedroom" };
    static readonly string[] Walls = { "Wall_N", "Wall_S", "Wall_E", "Wall_W" };
    static readonly string[] Trims = { "Door", "Trim", "WindowFrame" };

    [MenuItem("Renovation/Build Presets")]
    public static void Build()
    {
        var cat = AssetDatabase.LoadAssetAtPath<Catalogue>("Assets/Catalogue/Catalogue.asset");
        if (cat == null) { Debug.LogError("PresetAuthoring: run Renovation → Import Catalogue first"); return; }
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "Presets");

        // ---- Scandi: light oak kept, white walls, grey sofa ----
        var scandi = Preset(cat, "Scandi", "Scandi");
        Floor(cat, scandi, "WoodFloor051");
        Paint(cat, scandi, Walls, "Whisper White");
        Paint(cat, scandi, new[] { "Ceiling" }, "Vivid White");
        Paint(cat, scandi, Trims, "Vivid White");
        // Layout (all three): 9 x 7 m room, lounge on the window wall, reading nook by the lamp (SE),
        // storage on the walls, plants in corners. Coffee table 40 cm off the sofa, 76 cm+ walkways,
        // Wall_W (preset frames), the start spot, the door and the lamp kept clear (SceneWiringTests).
        Place(cat, scandi, "Sofa_01", -1.4f, -3.07f, 0f, keep: true);            // under the clock
        Place(cat, scandi, "coffee_table_round_01", -1.4f, -1.67f, 0f);
        Place(cat, scandi, "modern_arm_chair_01", 0.45f, -1.85f, -105f);        // pair angled in on the table
        Place(cat, scandi, "ArmChair_01", -3.2f, -1.85f, 105f);
        Place(cat, scandi, "side_table_01", -2.65f, -3.15f, 0f);
        Place(cat, scandi, "potted_plant_01", -3.95f, -3.0f, 0f);
        Place(cat, scandi, "GreenChair_01", 3.1f, -2.1f, -45f);                 // reading nook, lamp at its shoulder
        Place(cat, scandi, "side_table_tall_01", 3.85f, -1.75f, 0f);
        Place(cat, scandi, "Shelf_01", 4.2f, 0.3f, -90f);                       // bookcase on Wall_E
        Place(cat, scandi, "round_wooden_table_01", -2.0f, 2.1f, 0f);           // small table + two chairs, north side
        Place(cat, scandi, "GreenChair_01", -2.0f, 1.05f, 0f);
        Place(cat, scandi, "GreenChair_01", -0.95f, 2.1f, -90f);
        Place(cat, scandi, "painted_wooden_shelves", -1.2f, 3.2f, 180f);
        Swatches(cat, scandi, "WoodFloor051", "Whisper White", "Sofa_01");

        // ---- Mid-century: walnut kept, terracotta accent wall behind the sofa ----
        var mid = Preset(cat, "MidCentury", "Mid-century");
        Floor(cat, mid, "WoodFloor043");
        Paint(cat, mid, new[] { "Wall_N", "Wall_E", "Wall_W" }, "Antique White U.S.A.");
        Paint(cat, mid, new[] { "Wall_S" }, "Terracotta", userColour: true);
        Paint(cat, mid, new[] { "Ceiling" }, "Antique White U.S.A.");
        Paint(cat, mid, Trims, "Natural White");
        Place(cat, mid, "sofa_02", -1.4f, -2.99f, 0f, keep: true);
        Place(cat, mid, "modern_coffee_table_01", -1.4f, -1.86f, 0f);
        Place(cat, mid, "mid_century_lounge_chair", 0.5f, -1.9f, -110f);
        Place(cat, mid, "modern_arm_chair_01", -3.2f, -1.85f, 105f);
        Place(cat, mid, "side_table_tall_01", -2.65f, -3.15f, 0f);
        Place(cat, mid, "potted_plant_01", -3.95f, -3.0f, 0f);
        Place(cat, mid, "ArmChair_01", 3.0f, -2.0f, -45f);                      // reading nook
        Place(cat, mid, "side_table_01", 3.85f, -1.55f, -90f);
        Place(cat, mid, "wooden_display_shelves_01", 4.2f, 0.3f, -90f);
        Place(cat, mid, "ClassicConsole_01", -1.4f, 3.15f, 180f);               // console on Wall_N, vase beside
        Place(cat, mid, "ceramic_vase_01", -0.35f, 3.2f, 0f);
        Place(cat, mid, "potted_plant_01", -4.0f, 3.15f, 0f);
        Swatches(cat, mid, "WoodFloor043", "Antique White U.S.A.", "Terracotta");

        // ---- Bedroom: carpet and bed kept, warm walls, soft blue-grey behind the bed ----
        // Headboard on the window wall under the clock (where the living rooms keep the sofa), so the
        // kept bed is in view from the start spot. Nightstands either side, bench at the foot.
        var bed = Preset(cat, "Bedroom", "Bedroom");
        Floor(cat, bed, "Carpet016");
        Paint(cat, bed, new[] { "Wall_N", "Wall_E", "Wall_W" }, "Warm Neutral");
        Paint(cat, bed, new[] { "Wall_S" }, "Tranquil Retreat", userColour: true);
        Paint(cat, bed, new[] { "Ceiling" }, "Vivid White");
        Paint(cat, bed, Trims, "Vivid White");
        Place(cat, bed, "GothicBed_01", -1.4f, -2.45f, 0f, keep: true);         // 1.5 x 2.0 m, head to Wall_S
        Place(cat, bed, "ClassicNightstand_01", -2.5f, -3.26f, 0f);
        Place(cat, bed, "ClassicNightstand_01", -0.3f, -3.26f, 0f);
        Place(cat, bed, "painted_wooden_bench", -1.4f, -1.05f, 0f);              // foot of the bed
        Place(cat, bed, "GreenChair_01", 3.1f, -2.1f, -45f);                     // reading corner, lamp at its shoulder
        Place(cat, bed, "side_table_tall_01", 3.85f, -1.75f, 0f);
        Place(cat, bed, "drawer_cabinet", 4.22f, 0.4f, -90f);                    // chest of drawers on Wall_E
        Place(cat, bed, "painted_wooden_cabinet", -1.5f, 3.15f, 180f);          // wardrobe on Wall_N
        Place(cat, bed, "painted_wooden_shelves", 0.6f, 3.2f, 180f);
        Place(cat, bed, "potted_plant_01", -3.95f, -3.0f, 0f);
        Place(cat, bed, "potted_plant_04", 4.05f, 2.1f, 0f);
        Swatches(cat, bed, "Carpet016", "Warm Neutral", "GothicBed_01");

        foreach (var p in new[] { scandi, mid, bed }) { WriteCard(p); EditorUtility.SetDirty(p); }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PresetAuthoring: 3 presets written");
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
        p.looks.Add(new SurfaceLookSpec { surfaceName = "Floor", state = SurfaceState.Keep, material = m.material, color = Color.white, sampleColor = m.sampleColor });
    }

    static void Paint(Catalogue cat, RoomPreset p, IEnumerable<string> surfaces, string paintName, bool userColour = false)
    {
        var c = PaintColour(cat, paintName);
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
