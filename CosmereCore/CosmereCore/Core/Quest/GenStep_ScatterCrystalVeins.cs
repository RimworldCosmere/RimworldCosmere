using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest;

/// <summary>
///     Grows one large connected mineral field with a ragged, organic outline: a thick trunk
///     that branches and tapers, so the mass reads as something that spread through the ground
///     rather than a circle someone stamped there.
///     <para>
///         Branches are swathes, not lines. A one-cell branch renders as a fence; thickness is
///         what makes it read as ore at map zoom. Radius drops by one each generation, so the
///         middle is a solid field and the edges break up into fingers.
///     </para>
///     <para>
///         Sixteen sets the geometry - branches fork at a sixteenth of a turn - and forcedLumpSize
///         is a hard cap on cells placed, so the payout is exact no matter how the shape falls.
///     </para>
/// </summary>
public class GenStep_ScatterCrystalVeins : GenStep_ScatterLumpsMineable {
    private const float Sixteenth = 360f / 16f;

    /// <summary>Cells a branch walks before it forks.</summary>
    public int branchLength = 18;

    /// <summary>How far a branch may wander off true, in degrees, per step.</summary>
    public float jitter = 12f;

    public int maxDepth = 4;

    /// <summary>Half-width of a trunk. Each generation is one thinner, to a minimum of one.</summary>
    public int trunkRadius = 5;

    public int trunks = 5;

    public override int SeedPart => 161616161;

    /// <summary>
    ///     Grows the field from the middle of the map rather than a scattered point. The whole
    ///     site exists for this one feature, so it should be what the player lands next to, and
    ///     centring it also keeps it clear of the map edge where branches get clipped.
    /// </summary>
    public override void Generate(Verse.Map map, GenStepParams parms) {
        ScatterAt(map.Center, map, parms);
    }

    protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1) {
        ThingDef? thingDef = ChooseThingDef();
        if (thingDef == null) return;

        int budget = forcedLumpSize > 0 ? forcedLumpSize : 16;
        List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        HashSet<IntVec3> placed = new HashSet<IntVec3>();

        // Breadth-first so every arm grows evenly and the budget runs out at the tips rather
        // than after one arm has eaten it all.
        Queue<(IntVec3 from, float angle, int depth)> queue = new Queue<(IntVec3, float, int)>();

        float spread = 360f / trunks;
        float offset = Rand.Range(0f, spread);
        for (int i = 0; i < trunks; i++) {
            queue.Enqueue((c, offset + i * spread, 0));
        }

        while (queue.Count > 0 && placed.Count < budget) {
            (IntVec3 from, float angle, int depth) = queue.Dequeue();
            if (depth > maxDepth) continue;

            int radius = Mathf.Max(trunkRadius - depth, 1);
            IntVec3 tip = from;
            float heading = angle;

            for (int step = 0; step < branchLength && placed.Count < budget; step++) {
                heading += Rand.Range(-jitter, jitter);
                float radians = heading * Mathf.Deg2Rad;

                IntVec3 next = new IntVec3(
                    tip.x + Mathf.RoundToInt(Mathf.Cos(radians)),
                    0,
                    tip.z + Mathf.RoundToInt(Mathf.Sin(radians))
                );

                if (!next.InBounds(map)) break;

                tip = next;
                Daub(tip, radius, map, thingDef, usedRects, placed, budget);
            }

            queue.Enqueue((tip, angle - Sixteenth, depth + 1));
            queue.Enqueue((tip, angle + Sixteenth, depth + 1));
        }

        Logger.Verbose(
            $"GenStep_ScatterCrystalVeins: field at {c} placed {placed.Count}/{budget} cells of {thingDef.defName}."
        );

        if (placed.Count == 0) return;

        // Publish the field's bounds the way GenStep_PreciousLump does, so anything ordered
        // later that anchors on the objective - the garrison - lands on the crystal.
        int minX = int.MaxValue, minZ = int.MaxValue, maxX = int.MinValue, maxZ = int.MinValue;
        foreach (IntVec3 cell in placed) {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.z < minZ) minZ = cell.z;
            if (cell.z > maxZ) maxZ = cell.z;
        }

        MapGenerator.SetVar("RectOfInterest", CellRect.FromLimits(minX, minZ, maxX, maxZ));
    }

    /// <summary>Fills a rough disc, with the rim thinned out so the outline stays ragged.</summary>
    private static void Daub(
        IntVec3 centre,
        int radius,
        Map map,
        ThingDef thingDef,
        List<CellRect> usedRects,
        HashSet<IntVec3> placed,
        int budget
    ) {
        int sqr = radius * radius;
        for (int dx = -radius; dx <= radius; dx++) {
            for (int dz = -radius; dz <= radius; dz++) {
                if (placed.Count >= budget) return;

                int distance = dx * dx + dz * dz;
                if (distance > sqr) continue;

                // Rim cells land only most of the time, which is what keeps the edge broken up
                // instead of a run of clean arcs.
                if (distance > sqr - radius && !Rand.Chance(0.55f)) continue;

                TryPlace(new IntVec3(centre.x + dx, 0, centre.z + dz), map, thingDef, usedRects, placed);
            }
        }
    }

    private static bool TryPlace(
        IntVec3 cell,
        Map map,
        ThingDef thingDef,
        List<CellRect> usedRects,
        HashSet<IntVec3> placed
    ) {
        if (placed.Contains(cell) || !cell.InBounds(map)) return false;

        for (int i = 0; i < usedRects.Count; i++) {
            if (usedRects[i].Contains(cell)) return false;
        }

        // Deliberately not restricted to existing rock: a field this size leaves any natural
        // patch within a few cells, and requiring rock threw away all but a fraction of it.
        if (cell.GetTerrain(map).IsWater) return false;

        // Skip occupied cells rather than clearing them. Destroying rock mid-generation kicks
        // off a roof-collapse check, and each collapse destroys more rock - over thousands of
        // cells that cascade recurses deep enough to overflow the stack.
        if (cell.GetEdifice(map) != null) return false;

        GenSpawn.Spawn(thingDef, cell, map);
        placed.Add(cell);
        return true;
    }
}
