using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// ─────────────────────────────────────────────────────────────────────────────
// ScenePlacer
//
//  Zone 1  Z: -70 → -25   Blue floor only  (objects placed by hand)
//  Zone 2  Z: -25 → +10   Yellow floor only
//  Zone 3  Z: +10 → +45   Red floor + platform structures
//  Trees   ellipse border
//
//  Floor tile: Floor_5x5 at scale (2,1,2) = 10×10 world units each
//  Tiles extend beyond zone edges so they overlap the irregular mountain base.
//
//  MAT_BLUE   = PolygonStarter_01
//  MAT_YELLOW = PolygonStarter_04
//  MAT_RED    = PolygonStarter_03  (was red in Zone 2 before swap)
// ─────────────────────────────────────────────────────────────────────────────
public static class ScenePlacer
{
    const string PS  = "Assets/Synty/PolygonStarter/Prefabs/";
    const string ENV = "Assets/Synty/PolygonGeneric/Prefabs/Environment/";

    const string MAT_BLUE   = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_01.mat";
    const string MAT_YELLOW = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_04.mat";
    const string MAT_RED    = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_03.mat";

    const string FLOOR_5X5 = PS + "SM_PolygonPrototype_Buildings_Floor_5x5_01P.prefab";

    static readonly Vector2 EC = new Vector2(0f, -12f);
    static readonly float   RX = 42f, RZ = 66f;

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Scene Placer/Place All Zones")]
    static void PlaceAll()
    {
        PlaceZone1(); PlaceZone2(); PlaceZone3(); PlaceBorderTrees();
        Debug.Log("[ScenePlacer] Done — re-bake NavMesh.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 1 — Blue floor only
    // X: -50 → +50 (11 tiles),  Z: -70 → -25 (6 rows)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Scene Placer/1. Zone 1 — Blue Floor")]
    static void PlaceZone1()
    {
        var p   = GetOrCreate("Zone1_Open");
        var mat = LoadMat(MAT_BLUE);
        TileFloor(mat, p,
            xStart: -50f, xCount: 11, // X: -50 to +50
            zStart: -70f, zCount:  6, // Z: -70 to -20 (slightly over -25 edge)
            step: 10f);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 2 — Yellow floor only
    // X: -50 → +50 (11 tiles),  Z: -25 → +10 (4 rows, edge overlap on both sides)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Scene Placer/2. Zone 2 — Yellow Floor")]
    static void PlaceZone2()
    {
        var p   = GetOrCreate("Zone2_Route");
        var mat = LoadMat(MAT_YELLOW);
        TileFloor(mat, p,
            xStart: -50f, xCount: 11,
            zStart: -25f, zCount:  4, // Z: -25 to +15 (slightly over +10 edge)
            step: 10f);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 3 — Red floor + platform structures
    // Floor: X: -55 → +55 (12 tiles), Z: +10 → +50 (5 rows)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Scene Placer/3. Zone 3 — Red Floor + Platforms")]
    static void PlaceZone3()
    {
        var p   = GetOrCreate("Zone3_Platform");
        var mat = LoadMat(MAT_RED);

        // ── Red floor (extends wide to overlap mountains) ─────────────────
        TileFloor(mat, p,
            xStart: -55f, xCount: 12, // X: -55 to +55
            zStart:  10f, zCount:  5, // Z: +10 to +50
            step: 10f);

        // ── Ground approach (Y=0, Z=10–20) ───────────────────────────────
        PlaceEnv("SM_Gen_Env_Cliff_Arch_01.prefab",  new Vector3(  0, 0, 11), 180f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_01.prefab",  new Vector3(-32, 0, 14),  90f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_01.prefab",  new Vector3( 32, 0, 14), 270f, p);
        PlaceEnv("SM_Gen_Env_Cliff_01.prefab",       new Vector3(-44, 0, 16), 160f, p);
        PlaceEnv("SM_Gen_Env_Cliff_01.prefab",       new Vector3( 44, 0, 16),  20f, p);

        // ── TIER 1  (Y=3, Z=21–30) ───────────────────────────────────────
        for (int xi = 0; xi < 6; xi++)
        for (int zi = 0; zi < 2; zi++)
            PlaceStruct(FLOOR_5X5,
                        new Vector3(-12.5f + xi * 5f, 3f, 21f + zi * 5f),
                        0f, Vector3.one, p);

        // Stairs ground → Tier 1 (4-wide)
        for (int xi = 0; xi < 4; xi++)
            PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Stairs_1x3_01P.prefab",
                        new Vector3(-6f + xi * 4f, 0f, 18f), 0f, Vector3.one, p);

        // Side ramps
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Ramp_25_1x1_01P.prefab",
                    new Vector3(-20f, 0f, 21f), 270f, new Vector3(4, 3, 6), p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Ramp_25_1x1_01P.prefab",
                    new Vector3( 20f, 0f, 21f),  90f, new Vector3(4, 3, 6), p);

        // Tier 1 north wall: Wall–Wall–WallDoor–Wall–Wall–WallDoor–Wall–Wall
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-14f, 3f, 31f), 3, new Vector3(2,0,0), 0f, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_WallDoor_2x3_01P.prefab",
                    new Vector3(-8f, 3f, 31f), 0f, Vector3.one, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-6f, 3f, 31f), 3, new Vector3(2,0,0), 0f, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_WallDoor_2x3_01P.prefab",
                    new Vector3( 0f, 3f, 31f), 0f, Vector3.one, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3( 2f, 3f, 31f), 3, new Vector3(2,0,0), 0f, p);

        // Tier 1 side walls (partial)
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-16f, 3f, 21f), 2, new Vector3(0,0,5), 90f, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3( 16f, 3f, 21f), 2, new Vector3(0,0,5), 270f, p);

        // Tier 1 cliff flanks
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_03.prefab",   new Vector3(-38, 0, 24), 100f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_05.prefab",   new Vector3( 36, 0, 26),  80f, p);
        PlaceEnv("SM_Gen_Env_Cliff_Pillar_01.prefab", new Vector3(-30, 0, 22),   0f, p);
        PlaceEnv("SM_Gen_Env_Cliff_Pillar_01.prefab", new Vector3( 30, 0, 22),  45f, p);

        // ── TIER 2  (Y=6, Z=33–40) ───────────────────────────────────────
        for (int xi = 0; xi < 4; xi++)
        for (int zi = 0; zi < 2; zi++)
            PlaceStruct(FLOOR_5X5,
                        new Vector3(-7.5f + xi * 5f, 6f, 33f + zi * 5f),
                        0f, Vector3.one, p);

        // Stairs Tier 1 → Tier 2
        for (int xi = 0; xi < 2; xi++)
            PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Stairs_1x3_01P.prefab",
                        new Vector3(-2f + xi * 4f, 3f, 31f), 0f, Vector3.one, p);

        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Ramp_45_1x1_01P.prefab",
                    new Vector3(-14f, 3f, 34f), 270f, new Vector3(3, 3, 3), p);

        // Tier 2 north wall: Wall–WallDoor–Wall–Wall–WallDoor–Wall
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-10f, 6f, 43f), 2, new Vector3(2,0,0), 0f, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_WallDoor_2x3_01P.prefab",
                    new Vector3(-6f, 6f, 43f), 0f, Vector3.one, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-4f, 6f, 43f), 2, new Vector3(2,0,0), 0f, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_WallDoor_2x3_01P.prefab",
                    new Vector3( 0f, 6f, 43f), 0f, Vector3.one, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3( 2f, 6f, 43f), 2, new Vector3(2,0,0), 0f, p);

        // Corner columns anchor wall ends
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab",
                    new Vector3(-12f, 6f, 43f), 0f, Vector3.one, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab",
                    new Vector3(  6f, 6f, 43f), 0f, Vector3.one, p);

        // Tier 2 side walls
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-10f, 6f, 33f), 2, new Vector3(0,0,5), 90f, p);
        PlaceStruct(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                    new Vector3(10f, 6f, 33f), 270f, Vector3.one, p);

        // Tier 2 cliff flanks
        PlaceEnv("SM_Gen_Env_Cliff_02.prefab",      new Vector3(-34, 0, 36), 120f, p);
        PlaceEnv("SM_Gen_Env_Cliff_03.prefab",      new Vector3( 32, 0, 36),  60f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_06.prefab", new Vector3(-44, 0, 40), 100f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_07.prefab", new Vector3( 42, 0, 40),  80f, p);

        // ── TIER 3  (Y=9, Z=45–50) — summit ─────────────────────────────
        for (int xi = 0; xi < 3; xi++)
            PlaceStruct(FLOOR_5X5,
                        new Vector3(-5f + xi * 5f, 9f, 46f), 0f, Vector3.one, p);

        // Stairs Tier 2 → Tier 3
        for (int xi = 0; xi < 2; xi++)
            PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Stairs_1x1_01P.prefab",
                        new Vector3(-2f + xi * 4f, 6f, 44f), 0f, new Vector3(2, 3, 3), p);

        // Summit walls
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-6f, 9f, 51f), 4, new Vector3(2,0,0), 0f, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3(-8f, 9f, 46f), 3, new Vector3(0,0,2), 90f, p);
        WallRow(PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab",
                new Vector3( 8f, 9f, 46f), 3, new Vector3(0,0,2), 270f, p);

        // Summit corner columns
        foreach (var c in new Vector3[]
            { new(-8,9,46), new(8,9,46), new(-8,9,51), new(8,9,51) })
            PlaceStruct(PS + "SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab",
                        c, 0f, Vector3.one, p);

        // Summit cliff backdrop
        PlaceEnv("SM_Gen_Env_Cliff_04.prefab",        new Vector3(-28, 0, 46), 130f, p);
        PlaceEnv("SM_Gen_Env_Cliff_04.prefab",        new Vector3( 28, 0, 46),  50f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_04.prefab",   new Vector3(-18, 0, 50), 150f, p);
        PlaceEnv("SM_Gen_Env_Dirt_Cliff_04.prefab",   new Vector3( 18, 0, 50),  30f, p);
        PlaceEnv("SM_Gen_Env_Cliff_Pillar_01.prefab", new Vector3(-12, 6, 48),   0f, p);
        PlaceEnv("SM_Gen_Env_Cliff_Pillar_01.prefab", new Vector3( 12, 6, 48),  90f, p);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // BORDER TREES
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Scene Placer/4. Border Trees")]
    static void PlaceBorderTrees()
    {
        var parent = GetOrCreate("Trees");
        string[] broad = { "SM_Gen_Env_Tree_01.prefab", "SM_Gen_Env_Tree_02.prefab", "SM_Gen_Env_Tree_03.prefab" };
        string[] pine  = { "SM_Gen_Env_Tree_Pine_01.prefab", "SM_Gen_Env_Tree_Pine_02.prefab", "SM_Gen_Env_Tree_Pine_03.prefab" };
        string[] dead  = { "SM_Gen_Env_Tree_Dead_01.prefab", "SM_Gen_Env_Tree_Dead_02.prefab" };
        string[] under = { "SM_Gen_Env_Bush_Large_01.prefab", "SM_Gen_Env_Bush_Large_02.prefab",
                           "SM_Gen_Env_Shrub_01.prefab", "SM_Gen_Env_Shrub_02.prefab" };
        var rng = new System.Random(42);
        PlaceEllipseRing(parent, broad, pine, dead, under, EC, RX,     RZ,     64, 4f, 1.0f, 2.0f, rng);
        PlaceEllipseRing(parent, broad, pine, dead, under, EC, RX-7f, RZ-7f,  36, 3f, 0.6f, 1.2f, rng);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    // Tile Floor_5x5 at scale (2,1,2) = 10×10 world units
    // xStart/zStart = center of first tile
    static void TileFloor(Material mat, GameObject parent,
                          float xStart, int xCount,
                          float zStart, int zCount,
                          float step)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(FLOOR_5X5);
        if (asset == null) { Debug.LogWarning($"Missing floor prefab: {FLOOR_5X5}"); return; }
        var scale = new Vector3(2, 1, 2);
        for (int xi = 0; xi < xCount; xi++)
        for (int zi = 0; zi < zCount; zi++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.transform.SetParent(parent.transform);
            go.transform.position = new Vector3(xStart + xi * step, 0f, zStart + zi * step);
            go.transform.localScale = scale;
            if (mat != null)
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                    r.sharedMaterial = mat;
            Undo.RegisterCreatedObjectUndo(go, "Floor");
        }
    }

    static Material LoadMat(string path)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) Debug.LogWarning($"[ScenePlacer] Material not found: {path}");
        return m;
    }

    static void PlaceStruct(string fullPath, Vector3 pos, float rotY, Vector3 scale, GameObject parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        if (asset == null) { Debug.LogWarning($"Missing: {fullPath}"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(parent.transform);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, rotY, 0));
        go.transform.localScale = scale;
        Undo.RegisterCreatedObjectUndo(go, "Struct");
    }

    static void PlaceStruct(string fullPath, Vector3 pos, float rotY, GameObject parent)
        => PlaceStruct(fullPath, pos, rotY, Vector3.one, parent);

    static void PlaceEnv(string filename, Vector3 pos, float rotY, GameObject parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ENV + filename);
        if (asset == null) { Debug.LogWarning($"Missing ENV: {filename}"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(parent.transform);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, rotY, 0));
        Undo.RegisterCreatedObjectUndo(go, "Env");
    }

    static void WallRow(string fullPath, Vector3 start, int count,
                        Vector3 step, float rotY, GameObject parent)
    {
        for (int i = 0; i < count; i++)
            PlaceStruct(fullPath, start + step * i, rotY, parent);
    }

    static void PlaceEllipseRing(GameObject parent,
        string[] broad, string[] pine, string[] dead, string[] under,
        Vector2 center, float rx, float rz, int count,
        float jitter, float sMin, float sMax, System.Random rng)
    {
        for (int i = 0; i < count; i++)
        {
            float a  = (float)i / count * Mathf.PI * 2f;
            float bx = center.x + rx * Mathf.Cos(a);
            float bz = center.y + rz * Mathf.Sin(a);
            float px = bx + (float)(rng.NextDouble() - 0.5) * jitter * 2f;
            float pz = bz + (float)(rng.NextDouble() - 0.5) * jitter * 2f;
            double roll = rng.NextDouble();
            string name = roll < 0.06 ? dead[rng.Next(dead.Length)]
                        : bz > center.y ? (roll < 0.55 ? pine[rng.Next(pine.Length)] : broad[rng.Next(broad.Length)])
                        : (roll < 0.65 ? broad[rng.Next(broad.Length)] : pine[rng.Next(pine.Length)]);
            SpawnTree(ENV + name, new Vector3(px, 0, pz), parent, sMin, sMax, rng);
            if (rng.NextDouble() < 0.4)
                SpawnTree(ENV + under[rng.Next(under.Length)],
                          new Vector3(px + (float)(rng.NextDouble()-0.5)*3f, 0,
                                      pz + (float)(rng.NextDouble()-0.5)*3f),
                          parent, 0.6f, 1.2f, rng);
        }
    }

    static void SpawnTree(string path, Vector3 pos, GameObject parent,
                          float sMin, float sMax, System.Random rng)
    {
        var pf = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (pf == null) { Debug.LogWarning($"Missing tree: {path}"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(pf);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0, (float)(rng.NextDouble() * 360f), 0);
        go.transform.localScale = Vector3.one * (sMin + (float)(rng.NextDouble() * (sMax - sMin)));
        Undo.RegisterCreatedObjectUndo(go, "Tree");
    }

    static GameObject GetOrCreate(string name)
    {
        var e = GameObject.Find(name);
        if (e != null) return e;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Parent");
        return go;
    }
}
