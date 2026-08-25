using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot, idempotent importer. Pulls CC0 furniture (Poly Haven glTF 1k),
/// CC0 PBR materials (ambientCG 1K-JPG) and Dulux colour data, writes them under
/// Assets/Catalogue, builds URP materials + furniture prefabs, and fills
/// Catalogue.asset. Re-running skips files already on disk.
/// </summary>
public static class CatalogueImporter
{
    const string Root = "Assets/Catalogue";
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(5) };

    // ---- what we bundle (ids are the sources' own asset ids) ----
    static readonly (string id, string label, FurnitureCategory cat)[] Furniture =
    {
        ("Sofa_01", "Sofa (grey)", FurnitureCategory.Seating),
        ("sofa_02", "Sofa (leather)", FurnitureCategory.Seating),
        ("ArmChair_01", "Armchair", FurnitureCategory.Seating),
        ("mid_century_lounge_chair", "Lounge chair", FurnitureCategory.Seating),
        ("modern_coffee_table_01", "Coffee table", FurnitureCategory.Table),
        ("coffee_table_round_01", "Round coffee table", FurnitureCategory.Table),
        ("side_table_01", "Side table", FurnitureCategory.Table),
        ("painted_wooden_shelves", "Shelves", FurnitureCategory.Storage),
    };

    // tile = metres per texture repeat (ambientCG textures are authored at roughly 1–2 m per repeat)
    static readonly (string id, string label, SurfaceKind targets, float tile)[] Materials =
    {
        ("Tiles040", "Stone tiles", SurfaceKind.Floor, 2.0f),
        ("Tiles107", "Hex tiles", SurfaceKind.Floor | SurfaceKind.Wall, 1.0f),
        ("Tiles133A", "Terrazzo tiles", SurfaceKind.Floor, 2.0f),
        ("Marble012", "Marble", SurfaceKind.Floor | SurfaceKind.Wall, 2.0f),
        ("WoodFloor051", "Oak boards", SurfaceKind.Floor, 2.0f),
        ("WoodFloor043", "Dark timber", SurfaceKind.Floor, 2.0f),
        ("Carpet016", "Carpet", SurfaceKind.Floor, 1.0f),
        ("Plaster001", "Plaster", SurfaceKind.Wall | SurfaceKind.Ceiling, 2.0f),
        ("PaintedPlaster017", "Painted plaster", SurfaceKind.Wall | SurfaceKind.Ceiling, 2.0f),
        ("Concrete034", "Concrete", SurfaceKind.Wall | SurfaceKind.Floor, 2.0f),
    };

    // 24 Dulux AU names; any missing from the dataset are topped up by hue spread.
    static readonly string[] PaintNames =
    {
        "Whisper White", "Antique White U.S.A.", "Highgate", "Grey Pebble", "Clay Pipe", "Sandy Day",
        "Beige Royal", "Warm Neutral", "Silkwort", "Tranquil Retreat", "Powder Blue", "Mustard Seed",
        "Berry Crush", "Domino", "Black", "Natural White", "Lexicon", "Vivid White", "Monument",
        "Wombat", "Deep Ocean", "Teal Waters", "Wild Sage", "Terracotta",
    };

    [MenuItem("Renovation/Import Catalogue")]
    public static void Import()
    {
        Directory.CreateDirectory(Root);
        var credits = new StringBuilder("# Catalogue credits\n\nAll 3D models and textures are CC0 (public domain). Colour data is public Dulux Australia colour information used for display names only.\n\n");
        var cat = AssetDatabase.LoadAssetAtPath<Catalogue>($"{Root}/Catalogue.asset");
        if (cat == null)
        {
            cat = ScriptableObject.CreateInstance<Catalogue>();
            AssetDatabase.CreateAsset(cat, $"{Root}/Catalogue.asset");
        }
        cat.paints.Clear(); cat.materials.Clear(); cat.furniture.Clear();

        try
        {
            ImportPaints(cat, credits);
            ImportMaterials(cat, credits);
            ImportFurniture(cat, credits);
        }
        finally
        {
            File.WriteAllText($"{Root}/CREDITS.md", credits.ToString());
            EditorUtility.SetDirty(cat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
        Debug.Log($"CatalogueImporter: {cat.paints.Count} paints, {cat.materials.Count} materials, {cat.furniture.Count} furniture");
    }

    // ------------------------------------------------------------------ paints
    [Serializable] class DuluxColour { public string code; public string name; public int red, green, blue, lrv; public string url; }
    [Serializable] class DuluxFile { public int count; public List<DuluxColour> colours; }

    static void ImportPaints(Catalogue cat, StringBuilder credits)
    {
        var dir = $"{Root}/Paints"; Directory.CreateDirectory(dir);
        var path = $"{dir}/dulux.json";
        if (!File.Exists(path))
            File.WriteAllBytes(path, Get("https://raw.githubusercontent.com/shanmoorthy/dulux-paint-colour-data/master/data/colours.json"));
        var all = JsonUtility.FromJson<DuluxFile>(File.ReadAllText(path)).colours;

        var picked = new List<DuluxColour>();
        foreach (var n in PaintNames)
        {
            var c = all.FirstOrDefault(x => x.name == n);
            if (c != null && picked.All(p => p.code != c.code)) picked.Add(c);
        }
        // top up to 24 with a hue spread of mid-LRV colours
        var pool = all.Where(x => x.lrv >= 20 && x.lrv <= 75 && picked.All(p => p.code != x.code))
                      .OrderBy(x => Hue(x)).ToList();
        for (int i = 0; picked.Count < 24 && pool.Count > 0; i++)
            picked.Add(pool[(int)((long)i * pool.Count / 12) % pool.Count]);

        foreach (var c in picked)
            cat.paints.Add(new PaintOption { name = c.name, code = c.code, color = new Color(c.red / 255f, c.green / 255f, c.blue / 255f) });
        credits.AppendLine($"## Paint\n- Dulux Australia colour names/RGB via https://github.com/shanmoorthy/dulux-paint-colour-data ({picked.Count} colours)\n");
    }

    static float Hue(DuluxColour c) { Color.RGBToHSV(new Color(c.red / 255f, c.green / 255f, c.blue / 255f), out var h, out _, out _); return h; }

    // --------------------------------------------------------------- materials
    static void ImportMaterials(Catalogue cat, StringBuilder credits)
    {
        credits.AppendLine("## Materials (ambientCG, CC0)");
        var litShader = Shader.Find("Universal Render Pipeline/Lit");
        for (int i = 0; i < Materials.Length; i++)
        {
            var (id, label, targets, tile) = Materials[i];
            EditorUtility.DisplayProgressBar("Catalogue", $"Material {id}", (float)i / Materials.Length);
            var dir = $"{Root}/Materials/{id}"; Directory.CreateDirectory(dir);
            var colorPath = $"{dir}/{id}_1K-JPG_Color.jpg";
            var normalPath = $"{dir}/{id}_1K-JPG_NormalGL.jpg";
            if (!File.Exists(colorPath))
            {
                var zip = Get($"https://ambientcg.com/get?file={id}_1K-JPG.zip");
                using var arc = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
                foreach (var e in arc.Entries)
                    if (e.Name.EndsWith("_Color.jpg") || e.Name.EndsWith("_NormalGL.jpg"))
                        e.ExtractToFile($"{dir}/{e.Name}", true);
            }
            AssetDatabase.ImportAsset(colorPath); AssetDatabase.ImportAsset(normalPath);
            MarkNormal(normalPath);

            var matPath = $"{dir}/{id}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(litShader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.shader = litShader;
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath));
            mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            mat.EnableKeyword("_NORMALMAP");
            mat.color = Color.white;
            mat.SetFloat("_Smoothness", 0.35f);
            // Cubes are 1 unit; scale is applied per surface size at runtime by the shader tiling below.
            mat.mainTextureScale = new Vector2(1f / tile, 1f / tile);
            EditorUtility.SetDirty(mat);

            cat.materials.Add(new MaterialOption { name = label, sourceId = id, material = mat, targets = targets });
            credits.AppendLine($"- {id} — https://ambientcg.com/view?id={id}");
        }
        credits.AppendLine();
    }

    static void MarkNormal(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null || imp.textureType == TextureImporterType.NormalMap) return;
        imp.textureType = TextureImporterType.NormalMap;
        imp.SaveAndReimport();
    }

    // --------------------------------------------------------------- furniture
    static void ImportFurniture(Catalogue cat, StringBuilder credits)
    {
        credits.AppendLine("## Furniture (Poly Haven, CC0)");
        Directory.CreateDirectory($"{Root}/Prefabs");
        for (int i = 0; i < Furniture.Length; i++)
        {
            var (id, label, category) = Furniture[i];
            EditorUtility.DisplayProgressBar("Catalogue", $"Furniture {id}", (float)i / Furniture.Length);
            try
            {
                var dir = $"{Root}/Furniture/{id}"; Directory.CreateDirectory(dir);
                var gltfPath = $"{dir}/{id}.gltf";
                if (!File.Exists(gltfPath))
                {
                    // files/<id> → { gltf: { "1k": { gltf: { url, include: { "<rel>": { url } } } } } }
                    var files = JObject.Parse(Encoding.UTF8.GetString(Get($"https://api.polyhaven.com/files/{id}")));
                    var gltf = files["gltf"]["1k"]["gltf"];
                    File.WriteAllBytes(gltfPath, Get((string)gltf["url"]));
                    foreach (var kv in (JObject)gltf["include"])
                    {
                        var target = $"{dir}/{kv.Key}";
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        if (!File.Exists(target)) File.WriteAllBytes(target, Get((string)kv.Value["url"]));
                    }
                }
                var prefabPath = $"{Root}/Prefabs/{id}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    if (!File.Exists(gltfPath)) throw new Exception("no prefab and no glTF on disk — delete the folder and re-run");
                    AssetDatabase.ImportAsset(gltfPath, ImportAssetOptions.ForceSynchronousImport);
                    var model = AssetDatabase.LoadAssetAtPath<GameObject>(gltfPath);
                    if (model == null) { Debug.LogWarning($"CatalogueImporter: glTF import produced no GameObject for {id}"); continue; }
                    prefab = BakePrefab(id, model, dir, prefabPath);
                    // The glTF ScriptedImporter is only needed once; baked assets need no importer at reload/build.
                    AssetDatabase.DeleteAsset(gltfPath);
                    foreach (var bin in Directory.GetFiles(dir, "*.bin")) AssetDatabase.DeleteAsset(bin.Replace('\\', '/'));
                }
                cat.furniture.Add(new FurnitureOption { name = label, sourceId = id, prefab = prefab, category = category });
                credits.AppendLine($"- {id} — https://polyhaven.com/a/{id}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"CatalogueImporter: skipped {id}: {ex.Message}");
            }
        }
        credits.AppendLine();
    }

    /// <summary>
    /// Copy meshes + materials out of the glTFast-imported model into standalone assets
    /// (Mesh .asset, URP/Lit .mat reusing the downloaded jpgs) and save a prefab whose
    /// pivot sits at the floor. Nothing in the prefab references the .gltf afterwards.
    /// </summary>
    static GameObject BakePrefab(string id, GameObject model, string dir, string prefabPath)
    {
        var meshDir = $"{Root}/Meshes"; Directory.CreateDirectory(meshDir);
        var matDir = $"{Root}/Materials/Furniture"; Directory.CreateDirectory(matDir);
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        var src = (GameObject)PrefabUtility.InstantiatePrefab(model);
        var wrapper = new GameObject(id);
        int mi = 0;
        foreach (var mf in src.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            var mesh = UnityEngine.Object.Instantiate(mf.sharedMesh);
            mesh.name = $"{id}_{mi}";
            var meshPath = $"{meshDir}/{id}_{mi}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);

            var go = new GameObject(mf.gameObject.name);
            go.transform.SetParent(wrapper.transform, false);
            go.transform.position = mf.transform.position;
            go.transform.rotation = mf.transform.rotation;
            go.transform.localScale = mf.transform.lossyScale;
            go.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            var mr = go.AddComponent<MeshRenderer>();
            var srcMr = mf.GetComponent<MeshRenderer>();
            var mats = new List<Material>();
            var srcMats = srcMr != null ? srcMr.sharedMaterials : new Material[0];
            for (int k = 0; k < Mathf.Max(1, srcMats.Length); k++)
            {
                var sm = k < srcMats.Length ? srcMats[k] : null;
                var matPath = $"{matDir}/{id}_{mi}_{k}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null) { mat = new Material(lit); AssetDatabase.CreateAsset(mat, matPath); }
                mat.shader = lit;
                Texture baseTex = null, normalTex = null; Color tint = Color.white;
                if (sm != null)
                {
                    baseTex = sm.HasProperty("baseColorTexture") ? sm.GetTexture("baseColorTexture") : sm.mainTexture;
                    normalTex = sm.HasProperty("normalTexture") ? sm.GetTexture("normalTexture") : null;
                    if (sm.HasProperty("baseColorFactor")) tint = sm.GetColor("baseColorFactor");
                }
                // Fallback: the jpgs Poly Haven shipped alongside the model.
                if (baseTex == null) baseTex = FindTexture(dir, "_diff_");
                if (normalTex == null) normalTex = FindTexture(dir, "_nor_gl_");
                mat.SetTexture("_BaseMap", baseTex);
                if (normalTex != null) { mat.SetTexture("_BumpMap", normalTex); mat.EnableKeyword("_NORMALMAP"); }
                mat.color = tint;
                mat.SetFloat("_Smoothness", 0.3f);
                EditorUtility.SetDirty(mat);
                mats.Add(mat);
            }
            mr.sharedMaterials = mats.ToArray();
            mi++;
        }
        UnityEngine.Object.DestroyImmediate(src);

        // Pivot at the floor, centred in XZ.
        var rends = wrapper.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            var b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
            var shift = new Vector3(-b.center.x, -b.min.y, -b.center.z);
            foreach (Transform c in wrapper.transform) c.position += shift;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath);
        UnityEngine.Object.DestroyImmediate(wrapper);
        return prefab;
    }

    static Texture2D FindTexture(string dir, string tag)
    {
        var texDir = Path.Combine(dir, "textures");
        if (!Directory.Exists(texDir)) return null;
        foreach (var f in Directory.GetFiles(texDir, "*.jpg"))
            if (f.Contains(tag))
            {
                var ap = f.Replace('\\', '/');
                if (tag.Contains("nor")) MarkNormal(ap);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(ap);
            }
        return null;
    }

    static byte[] Get(string url)
    {
        var res = Http.GetAsync(url).GetAwaiter().GetResult();
        res.EnsureSuccessStatusCode();
        return res.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
    }
}
