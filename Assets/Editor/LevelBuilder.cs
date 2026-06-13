using UnityEngine;
using UnityEditor;

// ─────────────────────────────────────────────────────────────────────────────
// LevelBuilder — asymmetric multi-route terrain, Ramp-style block stacking
//
// Premises:
//   • Player jump ~1.5 → steps ≤1.2 climbable, walls ≥2.6 block movement
//   • Patrol lanes ≥4 wide on the ground
//   • Mounds are pieced from many varied blocks: notched corners, mixed
//     footprints (1.7~3.1), per-block height noise → hand-stacked look
//   • Door/Window pieces appear ONLY inside WallLine composites (flush,
//     zone material applied) — never free-standing
//   • Walls meet at perpendicular corners (L/T shapes), not parallel pairs
//   • Seeded RNG → reproducible
//
// Materials: Z1 walls=PolygonStarter_01  Z2=_04  Z3=_03; props/env default.
// ─────────────────────────────────────────────────────────────────────────────
public static class LevelBuilder
{
    const string PS = "Assets/Synty/PolygonStarter/Prefabs/";
    const string PG = "Assets/Synty/PolygonGeneric/Prefabs/Props/";
    const string EV = "Assets/Synty/PolygonGeneric/Prefabs/Environment/";

    const string BLOCK  = PS + "SM_PolygonPrototype_Buildings_Block_1x1_01P.prefab";
    const string STAIR3 = PS + "SM_PolygonPrototype_Buildings_Stairs_1x3_01P.prefab";
    const string WDOOR  = PS + "SM_PolygonPrototype_Buildings_WallDoor_2x3_01P.prefab";
    const string WWIN   = PS + "SM_PolygonPrototype_Buildings_WallWindow_2x3_01P.prefab";
    const string COL    = PS + "SM_PolygonPrototype_Buildings_Column_2x3_01P.prefab";
    const string ARCH1  = EV + "SM_Gen_Env_Cliff_Arch_01.prefab";
    const string ARCH2  = EV + "SM_Gen_Env_Cliff_Arch_02.prefab";

    const string MAT_Z1 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_01.mat";
    const string MAT_Z2 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_04.mat";
    const string MAT_Z3 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_03.mat";

    const float WALL_H = 3f;
    static System.Random rng;
    static float J(float a) => (float)(rng.NextDouble() * 2 - 1) * a;
    static bool Chance(double c) => rng.NextDouble() < c;

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 1 (blue) — open intro, corner-based walls
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 1 (Blue)")]
    static void BuildZone1()
    {
        rng = new System.Random(11);
        var p = GetOrCreate("Z1_Built");
        var m = Mat(MAT_Z1);
        float y = 0.2f;

        // — South: diagonal door-wall with a perpendicular return at its east end
        WallLine(new Vector3(1, y, -57), -25, m, p, "b4", "d", "b3");
        Wall(new Vector3(9.2f, y, -63), 65, new Vector3(5, 2.4f, 0.5f), m, p); // L return
        Wall(new Vector3(1.8f, y + WALL_H, -56.6f), -25, new Vector3(2.5f, 0.7f, 0.5f), m, p);

        // — East: true L-corner complex (window wall + perpendicular tall return)
        WallLine(new Vector3(14, y, -54), 30, m, p, "b4", "w", "b3");
        Wall(new Vector3(23.5f, y, -47), 120, new Vector3(8, WALL_H, 0.5f), m, p);   // ⊥ return
        Wall(new Vector3(20, y, -42), 120, new Vector3(3, 1.6f, 0.6f), m, p);        // broken cont.
        // low jump spur off the corner (cut the corner by jumping)
        Wall(new Vector3(17, y, -44), 30, new Vector3(4, 1.1f, 0.6f), m, p);

        // — NE LOOKOUT MOUND v2 (notched, mixed block sizes, top ~3.0)
        Mound(new Vector3(22, y, -34), 8, 3, 4, 1.0f, 0.65f, m, p);
        // summit: two unequal slabs + short corner parapet (L)
        Wall(new Vector3(24, y + 2.95f, -27.5f), 8, new Vector3(4.5f, 0.5f, 2.5f), m, p);
        Wall(new Vector3(27.5f, y + 2.95f, -28), 8, new Vector3(2.5f, 0.7f, 2.0f), m, p);
        Wall(new Vector3(25, y + 3.45f, -26.6f), 8, new Vector3(5, 0.6f, 0.4f), m, p);
        Wall(new Vector3(22.6f, y + 3.45f, -28.4f), 98, new Vector3(3, 0.6f, 0.4f), m, p);

        // — West ruins cluster: angled stubs + rubble (kept asymmetric)
        Wall(new Vector3(-26, y, -47), -28, new Vector3(4, 2.4f, 0.5f), m, p);
        Wall(new Vector3(-23, y, -44), -28, new Vector3(2.5f, 1.6f, 0.5f), m, p);
        Wall(new Vector3(-27, y, -43),  62, new Vector3(3, 1.0f, 0.5f), m, p);   // ⊥ stub
        Wall(new Vector3(-24, y, -45.5f), 40, new Vector3(1.2f, 0.6f, 1.2f), m, p);
        Wall(new Vector3(-25.5f, y, -41), 15, new Vector3(0.9f, 0.4f, 0.9f), m, p);

        // — West route: T-shape — window wall with a perpendicular middle stub
        WallLine(new Vector3(-28, y, -34), -35, m, p, "b3", "w", "b3");
        Wall(new Vector3(-22, y, -36), 55, new Vector3(4.5f, 2.2f, 0.5f), m, p);  // T stem
        Wall(new Vector3(-17, y, -30), -35, new Vector3(4, 1.1f, 0.5f), m, p);    // low spur

        // — Props
        Put(PG + "SM_Gen_Prop_Crate_01.prefab",        new Vector3( 14, y, -59), 15, p);
        Put(PG + "SM_Gen_Prop_Crate_02.prefab",        new Vector3( 16, y, -58),  0, p);
        Put(PG + "SM_Gen_Prop_Crate_03.prefab",        new Vector3( 15, y+1, -58.5f), 30, p);
        Put(PG + "SM_Gen_Prop_Barrel_Wood_01.prefab",  new Vector3( 23, y, -58), 20, p);
        Put(PG + "SM_Gen_Prop_Sack_Stack_01.prefab",   new Vector3(-23, y, -41), 45, p);
        Put(PG + "SM_Gen_Prop_Crate_Preset_01.prefab", new Vector3(-26, y, -57), 30, p);
        Put(EV + "SM_Gen_Env_Rock_03.prefab",          new Vector3(-27, y, -64), 40, p);
        Put(EV + "SM_Gen_Env_Rock_07.prefab",          new Vector3( 28, y, -60), 90, p);
        Put(EV + "SM_Gen_Env_Bush_Large_01.prefab",    new Vector3( 11, y, -46),  0, p);
        Put(EV + "SM_Gen_Env_Bush_Large_02.prefab",    new Vector3(-12, y, -60), 60, p);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 2 (yellow) — corner-based maze + heavy elevation
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 2 (Yellow)")]
    static void BuildZone2()
    {
        rng = new System.Random(22);
        var p = GetOrCreate("Z2_Built");
        var m = Mat(MAT_Z2);
        float y = 0.3f;

        // ── ENTRY: L-complex west + angled stub east; the way in is the
        //    perpendicular slot between them (no parallel pair)
        WallLine(new Vector3(-28, y, -21), 0, m, p, "b3", "d", "b6");
        Wall(new Vector3(-15, y, -17.5f), 90, new Vector3(7, WALL_H, 0.5f), m, p); // ⊥ return north
        Wall(new Vector3(-20, y + WALL_H, -21), 0, new Vector3(3, 0.8f, 0.5f), m, p);
        WallLine(new Vector3(4, y, -19), -24, m, p, "b3", "w", "b3");
        Wall(new Vector3(13.4f, y, -23.5f), 66, new Vector3(5, 2.4f, 0.5f), m, p); // ⊥ return south

        // ── NW GRAND PLATEAU v2 (notched 5×4, mixed sizes, top ~3.3)
        Mound(new Vector3(-34, y, -16), 0, 4, 5, 1.0f, 0.6f, m, p);
        // L-shaped two-piece deck + corner parapet with window
        Wall(new Vector3(-31, y + 3.3f, -5.5f), 0, new Vector3(6, 0.5f, 3.0f), m, p);
        Wall(new Vector3(-27.5f, y + 3.3f, -7),  0, new Vector3(3, 0.65f, 4.5f), m, p);
        Wall(new Vector3(-31, y + 3.8f, -4.2f), 0, new Vector3(6, 0.8f, 0.4f), m, p);
        Wall(new Vector3(-26.4f, y + 3.8f, -6.5f), 90, new Vector3(4, 0.8f, 0.4f), m, p);

        // ── CORRIDOR: plateau forms the west side; east side is one L-wall
        //    (no second parallel wall)
        WallLine(new Vector3(-19, y, -15), 90, m, p, "b4", "w", "b3");
        WallLine(new Vector3(-19, y, -4), 0, m, p, "b2", "d", "b3");   // ⊥ turn east at the top
        Put(COL, new Vector3(-19, y, -15.4f), 0, p);
        Wall(new Vector3(-11, y, -1), 145, new Vector3(4, 1.1f, 0.55f), m, p); // low diagonal spur

        // ── CENTRE: zigzag walls with perpendicular pocket stubs
        WallLine(new Vector3(-9, y, -10), -38, m, p, "b4", "w", "b4");
        Wall(new Vector3(-3.5f, y, -14.5f), 52, new Vector3(4, 2.2f, 0.5f), m, p); // ⊥ pocket
        Wall(new Vector3( 5, y, -8),  32, new Vector3(10, 3.6f, 0.5f), m, p);
        Wall(new Vector3( 9, y + 3.6f, -6), 32, new Vector3(4, 0.7f, 0.5f), m, p);
        Wall(new Vector3( 1.5f, y, -4), 122, new Vector3(5, 2.6f, 0.5f), m, p);    // ⊥ off zigzag
        Put(COL, new Vector3(-8.5f, y, -10.5f), 0, p);
        // centre knoll (two unequal blocks)
        Wall(new Vector3(0, y, -13), 18, new Vector3(2.0f, 1.1f, 2.0f), m, p);
        Wall(new Vector3(2, y, -12), 18, new Vector3(2.6f, 1.9f, 2.2f), m, p);

        // ── EAST elevated route: hops → ridge → gap jump → climbing ridge
        Wall(new Vector3(17, y, -17), 0, new Vector3(1.8f, 1.1f, 1.8f), m, p);
        Wall(new Vector3(19, y, -15), 0, new Vector3(1.8f, 2.2f, 1.8f), m, p);
        Wall(new Vector3(23, y, -10), -12, new Vector3(1.6f, 2.2f, 9f), m, p);
        Wall(new Vector3(25, y, -2),  8, new Vector3(1.6f, 2.9f, 7f), m, p);
        Wall(new Vector3(24, y, 2.5f), 0, new Vector3(1.8f, 1.8f, 1.8f), m, p);
        Wall(new Vector3(22, y, 4),    0, new Vector3(1.8f, 0.9f, 1.8f), m, p);
        // guard: L-corner instead of straight twin — wall + perpendicular return
        WallLine(new Vector3(28.5f, y, -14), 90, m, p, "b3", "w", "b3");
        Wall(new Vector3(25, y, -14.2f), 0, new Vector3(7, 2.6f, 0.5f), m, p);     // ⊥ base

        // ── SE STEPPED MOUND v2 (notched 3×3, top ~2.3)
        Mound(new Vector3(12, y, -26), -6, 3, 3, 1.0f, 0.65f, m, p);
        Wall(new Vector3(14, y + 2.3f, -21),   -6, new Vector3(3.5f, 0.5f, 2.2f), m, p);
        Wall(new Vector3(16.5f, y + 2.3f, -21.6f), -6, new Vector3(2.2f, 0.65f, 1.6f), m, p);

        // ── EXIT: T-complex — main line, perpendicular return south at its east
        //    end, and a free corner stub NE; exit slot is the ⊥ gap
        WallLine(new Vector3(-22, y, 3.5f), -4, m, p, "b4", "d", "b6", "w", "b3");
        Wall(new Vector3(-1.5f, y, 0.5f), 86, new Vector3(6, WALL_H, 0.5f), m, p); // ⊥ return
        Wall(new Vector3(-9, y + WALL_H, 3.2f), -4, new Vector3(4, 0.9f, 0.5f), m, p);
        WallLine(new Vector3(8, y, 6), 14, m, p, "b3", "w", "b2");
        Wall(new Vector3(16.8f, y, 3), 104, new Vector3(5, 2.4f, 0.5f), m, p);     // ⊥ return

        // ── Props
        Put(PG + "SM_Gen_Prop_Crate_01.prefab",        new Vector3(-26, y, -18), 20, p);
        Put(PG + "SM_Gen_Prop_Crate_02.prefab",        new Vector3(-24.5f, y, -17), 0, p);
        Put(PG + "SM_Gen_Prop_Barrel_Wood_01.prefab",  new Vector3( 13, y, -5),  45, p);
        Put(PG + "SM_Gen_Prop_Sack_Stack_02.prefab",   new Vector3( 25, y, -19), 10, p);
        Put(PG + "SM_Gen_Prop_Crate_Preset_01.prefab", new Vector3( -7, y, -16), 30, p);
        Put(PG + "SM_Gen_Prop_Barrel_Wood_03.prefab",  new Vector3( -1, y, -6),   0, p);
        Put(PG + "SM_Gen_Prop_Crate_03.prefab",        new Vector3(-29, y + 3.8f, -6), 25, p);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 3 (red) — layered towers, arch terrain, stair-linked sky platforms
    // No free-standing door/window: openings only inside WallLine parapets.
    // Cliff arches + stairs build a third, higher route over the towers.
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 3 (Red)")]
    static void BuildZone3()
    {
        rng = new System.Random(33);
        var p = GetOrCreate("Z3_Built");
        var m = Mat(MAT_Z3);
        float y = 0.2f;

        // ── Ground funnel walls (composites with embedded openings)
        WallLine(new Vector3(-19, y, 16), 25, m, p, "b3", "w", "b4");
        WallLine(new Vector3( 9,  y, 17), -30, m, p, "b3", "d", "b2");

        // ── TIER 1 (west, top 3.2): three offset layers + annex
        Wall(new Vector3(-10, y,        26),   0, new Vector3(13, 1.2f, 9.5f), m, p);
        Wall(new Vector3(-13, y,        22.5f),-4, new Vector3(6,  1.2f, 4f),  m, p);
        Wall(new Vector3(-9,  y + 1.2f, 26.5f), 3, new Vector3(11, 1.1f, 8f),  m, p);
        Wall(new Vector3(-8,  y + 2.3f, 27),    0, new Vector3(9.5f, 0.9f, 7f), m, p);
        // main stairs (south, 2-wide)
        PutS(STAIR3, new Vector3(-11, y, 18.5f), 0, new Vector3(2, 1, 1.17f), null, p);
        PutS(STAIR3, new Vector3( -9, y, 18.5f), 0, new Vector3(2, 1, 1.17f), null, p);
        // east-face jump steps
        Wall(new Vector3(-0.5f, y, 23), 15, new Vector3(1.7f, 1.1f, 1.7f), m, p);
        Wall(new Vector3(-1.5f, y, 26), -10, new Vector3(1.7f, 2.2f, 1.7f), m, p);
        // top cover + north parapet (door embedded in the wall line)
        Wall(new Vector3(-12, 3.4f, 28.5f), 12, new Vector3(1.5f, 1.0f, 1.5f), m, p);
        Wall(new Vector3(-6,  3.4f, 25),   -8, new Vector3(1.2f, 0.7f, 1.8f), m, p);
        WallLine(new Vector3(-14.5f, 3.4f, 30.4f), 0, m, p, "b4", "d", "b3");

        // ── ARCH PASSAGE between T1 and T2: cliff arch spans the gap;
        //    a stair climbs from T1 top onto the arch back → sky route start
        Put(ARCH1, new Vector3(0, y, 31), 90, p);
        PutS(STAIR3, new Vector3(-3.5f, 3.4f, 31), 90, new Vector3(2, 1, 0.9f), null, p);
        FloatBlock(new Vector3(0.5f, 6.6f, 31), 5, 2.6f, m, p);   // arch-top landing

        // ── TIER 2 (east, top 6.2): four offset layers + annex
        Wall(new Vector3(8,  y,        36),    0, new Vector3(11, 1.6f, 8.5f), m, p);
        Wall(new Vector3(11, y,        32),   12, new Vector3(5,  1.6f, 4f),   m, p);
        Wall(new Vector3(8,  y + 1.6f, 36.5f),-3, new Vector3(9.5f, 1.6f, 7.5f), m, p);
        Wall(new Vector3(7,  y + 3.2f, 36),    2, new Vector3(8.5f, 1.5f, 6.5f), m, p);
        Wall(new Vector3(7.5f, y + 4.7f, 36.5f), 0, new Vector3(7.5f, 1.5f, 6f), m, p);
        // SE ground hop chain (5 steps to T2)
        Wall(new Vector3(16, y, 29),  20, new Vector3(1.7f, 1.2f, 1.7f), m, p);
        Wall(new Vector3(17, y, 32), -15, new Vector3(1.7f, 2.4f, 1.7f), m, p);
        Wall(new Vector3(16, y, 35),  10, new Vector3(1.7f, 3.6f, 1.7f), m, p);
        Wall(new Vector3(15, y, 38),   0, new Vector3(1.7f, 4.8f, 1.7f), m, p);
        Wall(new Vector3(13.5f, y, 40), -8, new Vector3(1.7f, 5.9f, 1.7f), m, p);
        // top cover + west parapet (door embedded)
        Wall(new Vector3(10, 6.4f, 38.5f), 25, new Vector3(1.6f, 1.0f, 1.6f), m, p);
        Wall(new Vector3(5,  6.4f, 34),   -12, new Vector3(1.3f, 0.6f, 1.3f), m, p);
        WallLine(new Vector3(3.7f, 6.4f, 39.8f), -90, m, p, "b2", "d", "b2");

        // ── SKY ROUTE (level 3): from arch-top landing (6.6) climb floating
        //    stair-linked platforms over the towers → summit approach at 9.2
        FloatBlock(new Vector3(-2, 7.6f, 34), -10, 2.4f, m, p);
        PutS(STAIR3, new Vector3(-1, 7.7f, 36.5f), 90, new Vector3(1.6f, 0.55f, 0.8f), null, p);
        FloatBlock(new Vector3(2, 9.3f, 39), 15, 2.7f, m, p);
        FloatBlock(new Vector3(5, 10.2f, 43), -5, 2.2f, m, p);   // highest point, drop to T3

        // ── SECOND ARCH (SE): terrain feature sheltering the hop chain
        Put(ARCH2, new Vector3(19, y, 35), -70, p);

        // ── TIER 3 (centre-north, top 9.2): five-layer tower
        Wall(new Vector3(0,  y,        47),   0, new Vector3(10, 2.0f, 6.5f), m, p);
        Wall(new Vector3(-4, y,        44),  -8, new Vector3(4,  2.0f, 3f),   m, p);
        Wall(new Vector3(0,  y + 2.0f, 47.5f), 4, new Vector3(9,  2.0f, 5.5f), m, p);
        Wall(new Vector3(0.5f, y + 4.0f, 47), -2, new Vector3(8,  2.0f, 5f),  m, p);
        Wall(new Vector3(0,  y + 6.0f, 47.5f), 0, new Vector3(7,  1.6f, 4.5f), m, p);
        Wall(new Vector3(0,  y + 7.6f, 47),    3, new Vector3(6,  1.6f, 4f),  m, p);
        // final stair from T2 top
        PutS(STAIR3, new Vector3(4, 6.4f, 42.5f), 0, new Vector3(2, 1, 0.95f), null, p);
        // summit parapet (door embedded in the line) + columns
        WallLine(new Vector3(-3.5f, 9.2f, 49.4f), 0, m, p, "b2", "d", "b2");
        Put(COL, new Vector3(-3.8f, 9.2f, 49.1f), 0, p);
        Put(COL, new Vector3( 3.8f, 9.2f, 49.1f), 0, p);

        // ── Ground stepped mounds v2 (vantage points)
        Mound(new Vector3(-24, y, 24), -8, 2, 2, 1.0f, 0.7f, m, p);
        Wall(new Vector3(-22.5f, y + 1.7f, 27.5f), -8, new Vector3(3, 0.5f, 2), m, p);
        Mound(new Vector3(19, y, 20), 14, 2, 2, 1.1f, 0.7f, m, p);

        // ── Props
        Put(PG + "SM_Gen_Prop_Crate_01.prefab", new Vector3( 21, y, 26), 35, p);
        Put(PG + "SM_Gen_Prop_Crate_02.prefab", new Vector3(-20, y, 31), 10, p);
        Put(EV + "SM_Gen_Env_Rock_04.prefab",   new Vector3(-23, y, 19), 70, p);
        Put(EV + "SM_Gen_Env_Rock_06.prefab",   new Vector3( 22, y, 43), 140, p);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    // Ramp-style stepped mound v2 — irregular by design:
    //   • corner cells have 45% chance to be skipped (notched outline)
    //   • per-cell footprint varies 1.7~3.1 (some blocks much bigger)
    //   • height = row rise + noise; 15% of cells get an extra ±0.4 bump
    static void Mound(Vector3 origin, float rotY, int cols, int rows,
                      float h0, float hStep, Material m, GameObject parent)
    {
        var rot = Quaternion.Euler(0, rotY, 0);
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            bool corner = (r == 0 || r == rows - 1) && (c == 0 || c == cols - 1);
            if (corner && Chance(0.45)) continue;                 // notch

            float w = 1.7f + (float)rng.NextDouble() * 1.4f;      // 1.7~3.1
            float d = 1.7f + (float)rng.NextDouble() * 1.4f;
            float h = h0 + r * hStep + J(0.15f);
            if (Chance(0.15)) h += Chance(0.5) ? 0.4f : -0.4f;    // odd bump/dip
            if (h < 0.5f) h = 0.5f;

            var local = new Vector3(c * 2f + J(0.25f), 0, r * 2f + J(0.25f));
            Wall(origin + rot * local, rotY + J(7f), new Vector3(w, h, d), m, parent);
        }
    }

    // Floating platform: slab + thin support pillar to the ground
    static void FloatBlock(Vector3 topPos, float rotY, float size, Material m, GameObject parent)
    {
        Wall(new Vector3(topPos.x, topPos.y - 0.5f, topPos.z), rotY,
             new Vector3(size, 0.5f, size), m, parent);
        Wall(new Vector3(topPos.x, 0.2f, topPos.z), rotY,
             new Vector3(0.6f, topPos.y - 0.7f, 0.6f), m, parent);
    }

    // Composite wall line: "bN"=block, "d"=WallDoor, "w"=WallWindow, "gN"=gap.
    // Door/window pieces inherit the zone material → no blue stragglers.
    static void WallLine(Vector3 origin, float rotY, Material m, GameObject parent,
                         params string[] segs)
    {
        Vector3 dir = Quaternion.Euler(0, rotY, 0) * Vector3.right;
        float cursor = 0f;
        foreach (var s in segs)
        {
            if (s[0] == 'b')
            {
                float len = float.Parse(s.Substring(1));
                Wall(origin + dir * (cursor + len / 2f), rotY,
                     new Vector3(len, WALL_H, 0.5f), m, parent);
                cursor += len;
            }
            else if (s[0] == 'd' || s[0] == 'w')
            {
                PutS(s[0] == 'd' ? WDOOR : WWIN,
                     origin + dir * (cursor + 1f), rotY, Vector3.one, m, parent);
                cursor += 2f;
            }
            else if (s[0] == 'g')
                cursor += float.Parse(s.Substring(1));
        }
    }

    static void Wall(Vector3 pos, float rotY, Vector3 scale, Material mat, GameObject parent)
        => PutS(BLOCK, pos, rotY, scale, mat, parent);

    static void Put(string path, Vector3 pos, float rotY, GameObject parent)
        => PutS(path, pos, rotY, Vector3.one, null, parent);

    static void PutS(string path, Vector3 pos, float rotY, Vector3 scale,
                     Material mat, GameObject parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { Debug.LogWarning($"Missing: {path}"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        go.transform.SetParent(parent.transform);
        go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, rotY, 0));
        go.transform.localScale = scale;
        if (mat != null)
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = mat;
        Undo.RegisterCreatedObjectUndo(go, "Place " + System.IO.Path.GetFileName(path));
    }

    static Material Mat(string path)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null) Debug.LogWarning($"[LevelBuilder] Material missing: {path}");
        return m;
    }

    static GameObject GetOrCreate(string name)
    {
        var e = GameObject.Find(name);
        if (e != null) return e;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Level Builder/Dump Scene Hierarchy")]
    static void DumpHierarchy()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            DumpRecursive(root.transform, 0, sb);
        string path = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.dataPath), "SceneHierarchy.txt");
        System.IO.File.WriteAllText(path, sb.ToString());
        Debug.Log($"[LevelBuilder] Hierarchy written to {path}");
    }

    static void DumpRecursive(Transform t, int depth, System.Text.StringBuilder sb)
    {
        var pos = t.position; var s = t.lossyScale;
        sb.Append(new string(' ', depth * 2)).Append(t.name)
          .Append($"  pos({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
        if (s != Vector3.one) sb.Append($"  scale({s.x:F2}, {s.y:F2}, {s.z:F2})");
        sb.AppendLine();
        foreach (Transform c in t) DumpRecursive(c, depth + 1, sb);
    }
}
