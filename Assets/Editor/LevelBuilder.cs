using UnityEngine;
using UnityEditor;

// ─────────────────────────────────────────────────────────────────────────────
// LevelBuilder — asymmetric multi-route terrain, Ramp-style block stacking
//
// Premises:
//   • Player jump ~1.5 → steps ≤1.2 climbable, walls ≥2.6 block movement
//   • Patrol lanes ≥4 wide on the ground
//   • Mounds/plateaus are PIECED from many 2×2-ish blocks (like the player's
//     Ramp: ~40 blocks, per-block height jitter) — never one stretched cube
//   • Door/Window (2w×3h) always embedded flush inside height-3 wall lines
//   • Seeded RNG → jitter is random-looking but reproducible per re-run
//
// Materials: Z1 walls=PolygonStarter_01  Z2=_04  Z3=_03; props/rocks default.
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

    const string MAT_Z1 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_01.mat";
    const string MAT_Z2 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_04.mat";
    const string MAT_Z3 = "Assets/Synty/PolygonStarter/Materials/PolygonStarter_03.mat";

    const float WALL_H = 3f;
    static System.Random rng;
    static float J(float a) => (float)(rng.NextDouble() * 2 - 1) * a;   // jitter

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 1 (blue) — open intro  (~45 pieces)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 1 (Blue)")]
    static void BuildZone1()
    {
        rng = new System.Random(11);
        var p = GetOrCreate("Z1_Built");
        var m = Mat(MAT_Z1);
        float y = 0.2f;

        // — South diagonal wall: block-DOOR-block, uneven crown
        WallLine(new Vector3(1, y, -57), -25, m, p, "b4", "d", "b3");
        Wall(new Vector3(1.8f, y + WALL_H, -56.6f), -25, new Vector3(2.5f, 0.7f, 0.5f), m, p);

        // — East route: angled wall with WINDOW + L-corner of two heights
        WallLine(new Vector3(16, y, -55), 30, m, p, "b5", "w", "b4");
        Wall(new Vector3(26, y, -45), 80, new Vector3(7, WALL_H, 0.5f), m, p);
        Wall(new Vector3(24, y, -40), -10, new Vector3(4, 2.2f, 0.6f), m, p);

        // — East jump shortcut
        Wall(new Vector3(15, y, -37), 60, new Vector3(5, 1.1f, 0.6f), m, p);
        Wall(new Vector3(13, y, -40), 20, new Vector3(1.5f, 0.6f, 1.5f), m, p);

        // — NE LOOKOUT MOUND (Ramp-style, 4 rows × 3 cols rising south→north)
        //   rows: 1.0 → 1.6 → 2.2 → 2.8, walk/jump up from the south face
        Mound(new Vector3(22, y, -34), 8, 3, new float[] { 1.0f, 1.6f, 2.2f, 2.8f }, m, p);
        // summit slab pair (two different sizes pieced together) + parapet
        Wall(new Vector3(24, y + 2.8f, -27.5f), 8, new Vector3(4.5f, 0.5f, 2.5f), m, p);
        Wall(new Vector3(27.5f, y + 2.8f, -28),  8, new Vector3(2.5f, 0.7f, 2.0f), m, p);
        Wall(new Vector3(25, y + 3.3f, -26.6f), 8, new Vector3(5, 0.6f, 0.4f), m, p);

        // — West ruins cluster: three broken wall stubs + fallen blocks
        Wall(new Vector3(-26, y, -47), -28, new Vector3(4, 2.4f, 0.5f), m, p);
        Wall(new Vector3(-23, y, -44), -28, new Vector3(2.5f, 1.6f, 0.5f), m, p);
        Wall(new Vector3(-27, y, -43),  62, new Vector3(3, 1.0f, 0.5f), m, p);
        Wall(new Vector3(-24, y, -45.5f), 40, new Vector3(1.2f, 0.6f, 1.2f), m, p); // rubble
        Wall(new Vector3(-25.5f, y, -41), 15, new Vector3(0.9f, 0.4f, 0.9f), m, p); // rubble

        // — West route: angled window wall + low stub
        WallLine(new Vector3(-28, y, -34), -35, m, p, "b3", "w", "b3");
        Wall(new Vector3(-17, y, -30), -35, new Vector3(4, 1.1f, 0.5f), m, p);

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
    // ZONE 2 (yellow) — three lanes + heavy elevation  (~80 pieces)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 2 (Yellow)")]
    static void BuildZone2()
    {
        rng = new System.Random(22);
        var p = GetOrCreate("Z2_Built");
        var m = Mat(MAT_Z2);
        float y = 0.3f;

        // ── ENTRY composite line + jagged crowns
        WallLine(new Vector3(-30, y, -21.5f), 4, m, p, "b6", "d", "b5", "w", "b6");
        Wall(new Vector3(-22, y + WALL_H, -21), 4, new Vector3(3, 0.8f, 0.5f), m, p);
        Wall(new Vector3(-3,  y + WALL_H, -19.8f), 4, new Vector3(2, 0.6f, 0.5f), m, p);
        WallLine(new Vector3(3, y, -19), -20, m, p, "b4", "w", "b3");

        // ── NW GRAND PLATEAU (Ramp-style 5 rows × 4 cols, top ~3.4)
        //   climbs from the south: 1.0 → 1.6 → 2.2 → 2.8 → 3.4
        Mound(new Vector3(-34, y, -16), 0, 4, new float[] { 1.0f, 1.6f, 2.2f, 2.8f, 3.4f }, m, p);
        // summit: two slabs of different sizes pieced into an L-shaped deck
        Wall(new Vector3(-31, y + 3.4f, -5.5f), 0, new Vector3(6, 0.5f, 3.0f), m, p);
        Wall(new Vector3(-27.5f, y + 3.4f, -7),  0, new Vector3(3, 0.65f, 4.5f), m, p);
        // parapets on two edges + an embedded WINDOW overlooking centre lane
        Wall(new Vector3(-31, y + 3.9f, -4.2f), 0, new Vector3(6, 0.8f, 0.4f), m, p);
        PutS(WWIN, new Vector3(-26.2f, y + 3.4f, -6), 90, Vector3.one, m, p);

        // ── WEST corridor (between plateau and centre): composite, bends east
        WallLine(new Vector3(-19, y, -16), 90, m, p, "b4", "w", "b4");
        WallLine(new Vector3(-23, y, -3),   0, m, p, "b2", "d", "b4");
        Put(COL, new Vector3(-19, y, -16.5f), 0, p);
        Wall(new Vector3(-12, y, 0), 90, new Vector3(4, 1.1f, 0.55f), m, p);

        // ── CENTRE zigzag: three angled walls, varied heights + crowns
        WallLine(new Vector3(-9, y, -10), -38, m, p, "b4", "w", "b4");
        Wall(new Vector3( 5, y, -8),  32, new Vector3(10, 3.6f, 0.5f), m, p);
        Wall(new Vector3( 9, y + 3.6f, -6), 32, new Vector3(4, 0.7f, 0.5f), m, p);
        Wall(new Vector3(-3, y, -2), -25, new Vector3(7, 2.6f, 0.5f), m, p);
        Put(COL, new Vector3(-8.5f, y, -10.5f), 0, p);
        Put(COL, new Vector3( 10f,  y, -4.5f), 0, p);
        // centre pocket: small stepped knoll (2 blocks, vantage / coin spot)
        Wall(new Vector3(0, y, -13), 18, new Vector3(2.0f, 1.1f, 2.0f), m, p);
        Wall(new Vector3(2, y, -12), 18, new Vector3(2.4f, 1.9f, 2.4f), m, p);

        // ── EAST elevated speed route, EXTENDED: hops → ridge → GAP JUMP → ridge2
        Wall(new Vector3(17, y, -17), 0, new Vector3(1.8f, 1.1f, 1.8f), m, p);
        Wall(new Vector3(19, y, -15), 0, new Vector3(1.8f, 2.2f, 1.8f), m, p);
        // ridge 1 (h2.2, runnable top)
        Wall(new Vector3(23, y, -10), -12, new Vector3(1.6f, 2.2f, 9f), m, p);
        // ~1.8 air gap (jumpable), then ridge 2 climbs to 2.9
        Wall(new Vector3(25, y, -2),  8, new Vector3(1.6f, 2.9f, 7f), m, p);
        // descent blocks at the far end
        Wall(new Vector3(24, y, 2.5f), 0, new Vector3(1.8f, 1.8f, 1.8f), m, p);
        Wall(new Vector3(22, y, 4),    0, new Vector3(1.8f, 0.9f, 1.8f), m, p);
        // guard walls beside the ridges (ground route must detour)
        WallLine(new Vector3(28.5f, y, -14), 90, m, p, "b3", "w", "b4");
        Wall(new Vector3(31, y, -2), 90, new Vector3(6, 2.6f, 0.5f), m, p);

        // ── SE STEPPED MOUND (smaller sibling of NW plateau, top 2.2)
        Mound(new Vector3(12, y, -25), -6, 3, new float[] { 1.0f, 1.6f, 2.2f }, m, p);
        // two-piece cap
        Wall(new Vector3(14, y + 2.2f, -20),   -6, new Vector3(3.5f, 0.5f, 2.2f), m, p);
        Wall(new Vector3(16.5f, y + 2.2f, -20.6f), -6, new Vector3(2.2f, 0.65f, 1.6f), m, p);

        // ── EXIT composite line: door west, double window east, jagged top
        WallLine(new Vector3(-22, y, 3.5f), -4, m, p, "b5", "d", "b8", "w", "w", "b4");
        Wall(new Vector3(-9, y + WALL_H, 3),   -4, new Vector3(4, 0.9f, 0.5f), m, p);
        Wall(new Vector3(14, y + WALL_H, 1.6f), -4, new Vector3(3, 0.6f, 0.5f), m, p);
        WallLine(new Vector3(17, y, 2), 14, m, p, "b4", "w", "b2");

        // ── Props
        Put(PG + "SM_Gen_Prop_Crate_01.prefab",        new Vector3(-26, y, -18), 20, p);
        Put(PG + "SM_Gen_Prop_Crate_02.prefab",        new Vector3(-24.5f, y, -17), 0, p);
        Put(PG + "SM_Gen_Prop_Barrel_Wood_01.prefab",  new Vector3( 13, y, -5),  45, p);
        Put(PG + "SM_Gen_Prop_Sack_Stack_02.prefab",   new Vector3( 25, y, -19), 10, p);
        Put(PG + "SM_Gen_Prop_Crate_Preset_01.prefab", new Vector3( -7, y, -16), 30, p);
        Put(PG + "SM_Gen_Prop_Barrel_Wood_03.prefab",  new Vector3( -1, y, -6),   0, p);
        Put(PG + "SM_Gen_Prop_Crate_03.prefab",        new Vector3(-29, y + 3.9f, -6), 25, p); // plateau loot
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ZONE 3 (red) — layered towers + floating platform chains  (~90 pieces)
    //
    //   Each tier is built from stacked layers of different-sized slabs
    //   (no single stretched cube). Stairs link tiers; floating block chains
    //   offer asymmetric jump routes between/around them.
    //     T1 top 3.2 (west) → T2 top 6.2 (east) → T3 top 9.2 (centre-north)
    // ═════════════════════════════════════════════════════════════════════════
    [MenuItem("Tools/Level Builder/Zone 3 (Red)")]
    static void BuildZone3()
    {
        rng = new System.Random(33);
        var p = GetOrCreate("Z3_Built");
        var m = Mat(MAT_Z3);
        float y = 0.2f;

        // ── Ground funnel walls toward the arch
        WallLine(new Vector3(-19, y, 16), 25, m, p, "b3", "w", "b4");
        WallLine(new Vector3( 9,  y, 17), -30, m, p, "b3", "d", "b2");

        // ── TIER 1 (west, top 3.2): three stacked layers, offset footprints
        Wall(new Vector3(-10, y,        26),   0, new Vector3(13, 1.2f, 9.5f), m, p); // L0 base
        Wall(new Vector3(-13, y,        22.5f),-4, new Vector3(6,  1.2f, 4f),  m, p); // L0 annex
        Wall(new Vector3(-9,  y + 1.2f, 26.5f), 3, new Vector3(11, 1.1f, 8f),  m, p); // L1
        Wall(new Vector3(-8,  y + 2.3f, 27),    0, new Vector3(9.5f, 0.9f, 7f), m, p); // L2 top
        // façade flush on the south face
        Put(WDOOR, new Vector3(-13, y, 21.7f), 0, p);
        Put(WWIN,  new Vector3( -6, y, 21.25f), 0, p);
        // main stairs (south, 2-wide)
        PutS(STAIR3, new Vector3(-11, y, 18.5f), 0, new Vector3(2, 1, 1.17f), null, p);
        PutS(STAIR3, new Vector3( -9, y, 18.5f), 0, new Vector3(2, 1, 1.17f), null, p);
        // east-face jump steps (1.1 → 2.2)
        Wall(new Vector3(-0.5f, y, 23), 15, new Vector3(1.7f, 1.1f, 1.7f), m, p);
        Wall(new Vector3(-1.5f, y, 26), -10, new Vector3(1.7f, 2.2f, 1.7f), m, p);
        // top cover blocks + north parapet line with door silhouette
        Wall(new Vector3(-12, 3.4f, 28.5f), 12, new Vector3(1.5f, 1.0f, 1.5f), m, p);
        Wall(new Vector3(-6,  3.4f, 25),   -8, new Vector3(1.2f, 0.7f, 1.8f), m, p);
        WallLine(new Vector3(-14.5f, 3.4f, 30.4f), 0, m, p, "b4", "d", "b3");

        // ── FLOATING CHAIN A: T1 top → air hops (NW, over the gap) → T2
        //   3.2 → 4.3 → 5.3 → 6.2, blocks float on tall thin pillars
        FloatBlock(new Vector3(-3, 4.3f, 30), 10, 2.6f, m, p);
        FloatBlock(new Vector3(-1, 5.3f, 33),-15, 2.4f, m, p);
        FloatBlock(new Vector3( 1, 6.2f, 35),  5, 2.8f, m, p);

        // ── TIER 2 (east, top 6.2): four stacked layers
        Wall(new Vector3(8,  y,        36),    0, new Vector3(11, 1.6f, 8.5f), m, p); // L0
        Wall(new Vector3(11, y,        32),   12, new Vector3(5,  1.6f, 4f),   m, p); // L0 annex
        Wall(new Vector3(8,  y + 1.6f, 36.5f),-3, new Vector3(9.5f, 1.6f, 7.5f), m, p); // L1
        Wall(new Vector3(7,  y + 3.2f, 36),    2, new Vector3(8.5f, 1.5f, 6.5f), m, p); // L2
        Wall(new Vector3(7.5f, y + 4.7f, 36.5f), 0, new Vector3(7.5f, 1.5f, 6f), m, p); // L3 top
        // west-face window strip (two heights — flush decoration)
        Put(WWIN, new Vector3(2.55f, y + 1.6f, 34), 90, p);
        Put(WWIN, new Vector3(2.85f, y + 3.2f, 38), 90, p);
        // stairs T1→T2 through the gap
        PutS(STAIR3, new Vector3(-2, 3.4f, 32.5f), 90, new Vector3(2, 1, 0.95f), null, p);
        // SE ground hop chain (1.2 steps, risky fast route to T2)
        Wall(new Vector3(16, y, 29),  20, new Vector3(1.7f, 1.2f, 1.7f), m, p);
        Wall(new Vector3(17, y, 32), -15, new Vector3(1.7f, 2.4f, 1.7f), m, p);
        Wall(new Vector3(16, y, 35),  10, new Vector3(1.7f, 3.6f, 1.7f), m, p);
        Wall(new Vector3(15, y, 38),   0, new Vector3(1.7f, 4.8f, 1.7f), m, p);
        Wall(new Vector3(13.5f, y, 40), -8, new Vector3(1.7f, 5.9f, 1.7f), m, p);
        // top cover + west parapet with door frame at stair arrival
        Wall(new Vector3(10, 6.4f, 38.5f), 25, new Vector3(1.6f, 1.0f, 1.6f), m, p);
        Wall(new Vector3(5,  6.4f, 34),   -12, new Vector3(1.3f, 0.6f, 1.3f), m, p);
        WallLine(new Vector3(3.7f, 6.4f, 39.8f), -90, m, p, "b2", "d", "b2");

        // ── FLOATING CHAIN B: T2 top → air hops (E side, curling north) → T3
        //   6.2 → 7.3 → 8.3 → 9.2
        FloatBlock(new Vector3(12, 7.3f, 41), -10, 2.5f, m, p);
        FloatBlock(new Vector3(10, 8.3f, 44),  15, 2.7f, m, p);
        FloatBlock(new Vector3( 7, 9.2f, 46),  -5, 2.4f, m, p);

        // ── TIER 3 (centre-north, top 9.2): five-layer tower
        Wall(new Vector3(0,  y,        47),   0, new Vector3(10, 2.0f, 6.5f), m, p); // L0
        Wall(new Vector3(-4, y,        44),  -8, new Vector3(4,  2.0f, 3f),   m, p); // L0 annex
        Wall(new Vector3(0,  y + 2.0f, 47.5f), 4, new Vector3(9,  2.0f, 5.5f), m, p); // L1
        Wall(new Vector3(0.5f, y + 4.0f, 47), -2, new Vector3(8,  2.0f, 5f),  m, p); // L2
        Wall(new Vector3(0,  y + 6.0f, 47.5f), 0, new Vector3(7,  1.6f, 4.5f), m, p); // L3
        Wall(new Vector3(0,  y + 7.6f, 47),    3, new Vector3(6,  1.6f, 4f),  m, p); // L4 top
        // south façade: stacked windows (tower face)
        Put(WWIN, new Vector3(-2, y + 2.0f, 44.7f), 0, p);
        Put(WWIN, new Vector3( 1, y + 4.0f, 44.5f), 0, p);
        Put(WWIN, new Vector3(-1, y + 6.0f, 45.2f), 0, p);
        // final stair from T2 top
        PutS(STAIR3, new Vector3(4, 6.4f, 42.5f), 0, new Vector3(2, 1, 0.95f), null, p);
        // summit parapet with embedded door + columns
        WallLine(new Vector3(-3.5f, 9.2f, 49.4f), 0, m, p, "b2", "d", "b2");
        Put(COL, new Vector3(-3.8f, 9.2f, 49.1f), 0, p);
        Put(COL, new Vector3( 3.8f, 9.2f, 49.1f), 0, p);

        // ── Ground stepped mounds (Ramp-style, vantage points)
        Mound(new Vector3(-24, y, 24), -8, 2, new float[] { 1.0f, 1.7f }, m, p);
        Wall(new Vector3(-22.5f, y + 1.7f, 27.5f), -8, new Vector3(3, 0.5f, 2), m, p);
        Mound(new Vector3(19, y, 20), 14, 2, new float[] { 1.1f, 1.8f }, m, p);

        // ── Props
        Put(PG + "SM_Gen_Prop_Crate_01.prefab", new Vector3( 21, y, 26), 35, p);
        Put(PG + "SM_Gen_Prop_Crate_02.prefab", new Vector3(-20, y, 31), 10, p);
        Put(EV + "SM_Gen_Env_Rock_04.prefab",   new Vector3(-23, y, 19), 70, p);
        Put(EV + "SM_Gen_Env_Rock_06.prefab",   new Vector3( 22, y, 43), 140, p);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    // Ramp-style stepped mound: rows of 2×2 blocks rising along +Z (local),
    // per-block height/rotation jitter for a hand-stacked look.
    // origin = south-west corner at floor level.
    static void Mound(Vector3 origin, float rotY, int cols, float[] rowHeights,
                      Material m, GameObject parent)
    {
        var rot = Quaternion.Euler(0, rotY, 0);
        for (int r = 0; r < rowHeights.Length; r++)
        for (int c = 0; c < cols; c++)
        {
            float h = rowHeights[r] + J(0.12f);
            var local = new Vector3(c * 2f + J(0.06f), 0, r * 2f + J(0.06f));
            Wall(origin + rot * local, rotY + J(5f),
                 new Vector3(2f + J(0.1f), h, 2f + J(0.1f)), m, parent);
        }
    }

    // Floating platform: slab on a thin tall pillar (reads as built, not glitch)
    static void FloatBlock(Vector3 topPos, float rotY, float size, Material m, GameObject parent)
    {
        // slab (walkable top at topPos.y)
        Wall(new Vector3(topPos.x, topPos.y - 0.5f, topPos.z), rotY,
             new Vector3(size, 0.5f, size), m, parent);
        // supporting pillar down to the ground
        Wall(new Vector3(topPos.x, 0.2f, topPos.z), rotY,
             new Vector3(0.6f, topPos.y - 0.7f, 0.6f), m, parent);
    }

    // Composite wall line: "bN"=block N long, "d"=WallDoor, "w"=WallWindow, "gN"=gap
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
