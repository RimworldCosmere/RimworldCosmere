using System.Collections.Generic;
using Cosmere.System.Scadrial.Grid;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Thing;

public class CompProperties_AshVent : CompProperties {
    /// <summary>Millimetres this vent puts on its own cell in a day, before wind and falloff.</summary>
    public float millimetresPerDay = 140f;

    /// <summary>0 to 1. How hard the plume leans downwind.</summary>
    public float skew = 0.55f;

    /// <summary>How far past its own cells the vent thins the drift back in, in cells.</summary>
    public float clearRadius = 3f;

    /// <summary>Throws a day. One lump every two days at the default.</summary>
    public float throwsPerDay = 0.5f;

    /// <summary>How big a stack one throw lands. Small on purpose - an eruption throws 25 times.</summary>
    public int lumpsPerThrow = 1;

    /// <summary>How far from the vent a lump can land, in cells.</summary>
    public int throwRadius = 6;

    /// <summary>What the vent brings up with the ash.</summary>
    public List<string> metals = ["Cadmium", "Chromium", "Nicrosil", "Duralumin"];

    private List<ThingDef> resolvedMetals = [];

    /// <summary><see cref="metals"/> looked up once at load rather than once per throw.</summary>
    public List<ThingDef> ResolvedMetals => resolvedMetals;

    public CompProperties_AshVent() {
        compClass = typeof(CompAshVent);
    }

    public override void ResolveReferences(ThingDef parentDef) {
        base.ResolveReferences(parentDef);

        resolvedMetals = new List<ThingDef>(metals.Count);
        for (int i = 0; i < metals.Count; i++) {
            ThingDef? metal = DefDatabase<ThingDef>.GetNamedSilentFail(metals[i]);
            if (metal == null) {
                Log.Error($"{parentDef.defName} lists thrown metal {metals[i]}, which is not a loaded ThingDef.");
                continue;
            }

            resolvedMetals.Add(metal);
        }
    }
}

/// <summary>
///     A vent breathing ash onto the ground around it, and throwing up metal with it. The offsets
///     are walked once per sweep cycle, so the cost is a bounded local write and never a map scan.
/// </summary>
public class CompAshVent : ThingComp {
    private static Dictionary<TerrainDef, TerrainDef>? ladder;
    private static List<IntVec3>? offsets;

    private bool contained;
    private float[]? remainder;
    private float roomRemainder;
    private float soilDays;
    private float soilRadius;
    private float throwRemainder;
    private int throwsMade;

    public CompProperties_AshVent Props => (CompProperties_AshVent)props;

    /// <summary>
    ///     Whether the mouth is breathing into a sealed room instead of over the map. The tracker
    ///     reads it to work out how much of the map's severity this vent has stopped paying for.
    /// </summary>
    public bool Contained => contained;

    /// <summary>
    ///     The chain, resolved against the def database once. A rung whose terrain no loaded mod
    ///     declares is dropped rather than throwing, so one missing def costs one rung.
    /// </summary>
    private static Dictionary<TerrainDef, TerrainDef> Ladder {
        get {
            if (ladder != null) return ladder;

            ladder = new Dictionary<TerrainDef, TerrainDef>(AshVentSoilLadder.Rungs.Count);
            foreach (KeyValuePair<string, string> rung in AshVentSoilLadder.Rungs) {
                TerrainDef? from = DefDatabase<TerrainDef>.GetNamedSilentFail(rung.Key);
                TerrainDef? to = DefDatabase<TerrainDef>.GetNamedSilentFail(rung.Value);

                if (from == null || to == null) {
                    Log.Warn($"Ash vent soil ladder drops {rung.Key} to {rung.Value}: one is not loaded.");
                    continue;
                }

                ladder[from] = to;
            }

            return ladder;
        }
    }

    /// <summary>Every cell inside the plume radius, built once and shared by every vent.</summary>
    private static List<IntVec3> Offsets {
        get {
            if (offsets != null) return offsets;

            offsets = new List<IntVec3>();
            for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
                for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                    if (AshPlume.InRange(x, z)) offsets.Add(new IntVec3(x, 0, z));
                }
            }

            return offsets;
        }
    }

    /// <summary>Carried sub-unit remainder per offset cell, banked so no fraction is ever lost.</summary>
    private float[] Remainder => remainder ??= new float[Offsets.Count];

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
        parent.Map?.GetComponent<Map.AshDepthTracker>()?.RegisterVent(this);
    }

    public override void PostDeSpawn(Verse.Map map, DestroyMode mode) {
        base.PostDeSpawn(map, mode);
        map.GetComponent<Map.AshDepthTracker>()?.DeregisterVent(this);
    }

    public override void PostExposeData() {
        base.PostExposeData();

        List<float>? banked = null;
        if (Scribe.mode == LoadSaveMode.Saving && remainder != null) banked = [..remainder];

        Scribe_Collections.Look(ref banked, "ashVentRemainder", LookMode.Value);

        // unsaved: next throw loses a cycle and a reload could reshuffle which metal comes up next
        Scribe_Values.Look(ref roomRemainder, "ashVentRoomRemainder");
        Scribe_Values.Look(ref throwRemainder, "ashVentThrowRemainder");
        Scribe_Values.Look(ref throwsMade, "ashVentThrowsMade");

        // unsaved, a sealed map would show full severity for as long as the player stays paused on load
        Scribe_Values.Look(ref contained, "ashVentContained");

        // the spread front - the terrain grid alone can't say how far it reached, so this saves it
        Scribe_Values.Look(ref soilRadius, "ashVentSoilRadius");

        // the rung clock - terrain alone can't say when a cell last climbed, so this saves the delay
        Scribe_Values.Look(ref soilDays, "ashVentSoilDays");

        if (Scribe.mode == LoadSaveMode.LoadingVars) remainder = AshPlume.RestoreBank(banked, Offsets.Count);
    }

    /// <summary>
    ///     Puts this cycle's share on the ground - into the room when the mouth is roofed, over the
    ///     map when it is not. Returns whether anything changed, so the mesh only dirties when it must.
    /// </summary>
    public bool ContributeToGrid(AshGrid grid, float dayFraction) {
        Verse.Map? map = parent.Map;
        if (map == null) return false;

        Map.AshDepthTracker? tracker = map.GetComponent<Map.AshDepthTracker>();
        float millimetres = Props.millimetresPerDay * dayFraction;
        bool changed;

        // roofed mouth fills the room instead of the map; needs the tracker for the buried set
        if (tracker != null && SealedRoom(map, grid) is { } room) {
            // Same branch, same fact: the mass stops reaching the map, so the map stops paying.
            contained = true;
            changed = FillRoom(map, grid, tracker, room, millimetres);
        } else {
            contained = false;
            changed = Drift(map, grid, millimetres);

            // Sealed in it stops sweeping too, or a box the size of its mouth cancels the plume.
            if (tracker != null && ClearOwnMouth(map, grid, tracker)) changed = true;

            // Same branch, same reason: a capped vent feeds no ground outside the box it is in.
            if (tracker != null) SpreadSoil(map, tracker, dayFraction);
        }

        ThrowMetal(dayFraction);
        return changed;
    }

    /// <summary>Spreads the cycle's share downwind across open ground, thinning with distance.</summary>
    private bool Drift(Verse.Map map, AshGrid grid, float millimetres) {
        // One slowly drifting heading for the whole map, shared across vents.
        float angle = Mathf.Sin(Find.TickManager.TicksGame / 5200f) * Mathf.PI;
        float headingX = Mathf.Cos(angle);
        float headingZ = Mathf.Sin(angle);
        float skew = Props.skew * Mathf.Clamp01(map.windManager.WindSpeed);

        // An even-sized rect has no centre cell; CellRect.CenterCell hands back its high corner.
        IntVec3 centre = parent.Position;
        CellIndices indices = map.cellIndices;
        List<IntVec3> all = Offsets;
        float[] carried = Remainder;
        bool changed = false;

        for (int i = 0; i < all.Count; i++) {
            IntVec3 cell = centre + all[i];
            if (!cell.InBounds(map)) continue;
            if (!grid.CanHaveAsh(cell)) continue;

            float weight = AshPlume.Weight(all[i].x, all[i].z, headingX, headingZ, skew);
            if (weight <= 0f) continue;

            int deposit = AshPlume.Bank(ref carried[i], millimetres * weight, AshGrid.UnitMm);
            if (deposit <= 0) continue;
            if (grid.AddDepthMm(indices.CellToIndex(cell), deposit) > 0) changed = true;
        }

        return changed;
    }

    /// <summary>
    ///     The room a capped vent breathes into, or null while it still has sky. Roofed is asked
    ///     through CanHaveAsh rather than the roof grid, so the two never disagree about a cell.
    /// </summary>
    private Verse.Room? SealedRoom(Verse.Map map, AshGrid grid) {
        // Every cell of the mouth, not any: leave one square open and the plume finds its way out.
        foreach (IntVec3 cell in parent.OccupiedRect()) {
            if (grid.CanHaveAsh(cell)) return null;
        }

        // RoomAt can return null before the region grid rebuilds; the vent just drifts that cycle instead
        Verse.Room? room = parent.Position.GetRoom(map);
        if (room == null) return null;

        return AshRoomFill.HoldsThePlume(room.CellCount, room.PsychologicallyOutdoors) ? room : null;
    }

    /// <summary>
    ///     Puts the whole plume's mass into the room. Bypasses CanHaveAsh deliberately - the roof
    ///     is why this runs, not a reason to skip the cell - and refreshes each cell it moves.
    /// </summary>
    private bool FillRoom(
        Verse.Map map, AshGrid grid, Map.AshDepthTracker tracker, Verse.Room room, float millimetres
    ) {
        float share = AshRoomFill.PerCellMm(millimetres, room.CellCount);
        int deposit = AshPlume.Bank(ref roomRemainder, share, AshGrid.UnitMm);
        if (deposit <= 0) return false;

        CellIndices indices = map.cellIndices;
        AshBuriedCells buried = tracker.Buried;
        bool changed = false;

        foreach (IntVec3 cell in room.Cells) {
            int index = indices.CellToIndex(cell);
            if (grid.AddDepthMm(index, deposit) <= 0) continue;

            // mark buried now - a room fills fast enough that the sweep's 64-tick catch-up would be visible
            buried.Set(index, AshDepthMath.IsBuried(grid.GetDepthMm(index), buried.IsBuried(index)));
            changed = true;
        }

        return changed;
    }

    /// <summary>
    ///     Clears the vent's own cells and thins the drift back in around them, out as far as the
    ///     ground it has warmed. Left alone the thing blowing the ash out disappears under it, and
    ///     the soil it is feeding stalls under ash terrain before the outer front ever ripens.
    /// </summary>
    private bool ClearOwnMouth(Verse.Map map, AshGrid grid, Map.AshDepthTracker tracker) {
        // OccupiedRect, not Position or CenterCell - neither one is the actual footprint
        CellRect mouth = parent.OccupiedRect();

        // last cycle's front - SpreadSoil runs after this, and a 64-tick lag is nothing over 8 days
        float radius = Mathf.Max(Props.clearRadius, soilRadius);
        CellRect reach = mouth.ExpandedBy(Mathf.CeilToInt(radius));
        CellIndices indices = map.cellIndices;
        AshBuriedCells buried = tracker.Buried;
        bool changed = false;

        for (int x = reach.minX; x <= reach.maxX; x++) {
            for (int z = reach.minZ; z <= reach.maxZ; z++) {
                IntVec3 cell = new IntVec3(x, 0, z);
                if (!cell.InBounds(map)) continue;

                // straight-line distance out of the rect, so falloff rounds the corners, not another square
                int dx = x < mouth.minX ? mouth.minX - x : x > mouth.maxX ? x - mouth.maxX : 0;
                int dz = z < mouth.minZ ? mouth.minZ - z : z > mouth.maxZ ? z - mouth.maxZ : 0;
                float distance = Mathf.Sqrt(dx * dx + dz * dz);

                int index = indices.CellToIndex(cell);
                int depth = grid.GetDepthMm(index);
                int allowed = AshPlume.AllowedDepthMm(depth, distance, radius);

                // under a whole unit is not removable - bailing stops an already-thinned cell dirtying the mesh forever
                if (depth - allowed < AshGrid.UnitMm) continue;

                grid.RemoveDepthMm(index, depth - allowed);

                // every cell whose depth moved, not just ones that hit zero - 1200 to 400 also stops being buried
                buried.Set(index, AshDepthMath.IsBuried(grid.GetDepthMm(index), buried.IsBuried(index)));
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    ///     Walks the front out a little further and carries the ground behind it up a rung where
    ///     one is due. Rationed the way the terrain sweep rations its own, so six vents cannot
    ///     fire hundreds of writes a tick.
    /// </summary>
    private void SpreadSoil(Verse.Map map, Map.AshDepthTracker tracker, float dayFraction) {
        soilRadius = AshVentSoilSpread.Advance(
            soilRadius, Props.clearRadius, Scadrial.Mod.ventSoilReach, dayFraction
        );

        float daysBefore = soilDays;
        soilDays += dayFraction;

        CellRect mouth = parent.OccupiedRect();
        CellRect reach = mouth.ExpandedBy(Mathf.CeilToInt(soilRadius));
        int budget = AshDepthMath.TerrainChangesPerSweep;

        for (int x = reach.minX; x <= reach.maxX; x++) {
            for (int z = reach.minZ; z <= reach.maxZ; z++) {
                if (budget <= 0) return;

                IntVec3 cell = new IntVec3(x, 0, z);
                if (!cell.InBounds(map)) continue;

                // out of the rect, not a centre - rounds the corners, not a square; squared to skip the Sqrt
                int dx = x < mouth.minX ? mouth.minX - x : x > mouth.maxX ? x - mouth.maxX : 0;
                int dz = z < mouth.minZ ? mouth.minZ - z : z > mouth.maxZ ? z - mouth.maxZ : 0;
                int distanceSquared = (dx * dx) + (dz * dz);
                if (!AshVentSoilSpread.Reaches(distanceSquared, soilRadius)) continue;

                if (ClimbOneRung(map, tracker, cell, distanceSquared, daysBefore)) budget--;
            }
        }
    }

    /// <summary>
    ///     Carries one cell up the chain when its dwell on the rung below is done. Terrain only -
    ///     it moves no ash, so the buried set is owed nothing here.
    /// </summary>
    private bool ClimbOneRung(
        Verse.Map map, Map.AshDepthTracker tracker, IntVec3 cell, int distanceSquared, float daysBefore
    ) {
        int index = map.cellIndices.CellToIndex(cell);

        // the feather caps nothing at the front, only inside the clearing - ask the grid, not the feather
        if (AshDepthMath.ShouldSwapToAshTerrain(tracker.Grid.GetDepthMm(index), false)) return false;

        // ash over a remembered original - climbing would strand that memory, leave it to the sweep
        if (tracker.TerrainMemory.IsSwapped(index)) return false;

        TerrainDef current = map.terrainGrid.TerrainAt(index);

        // same ground the ash swap accepts - a colony floor stays theirs, water and rock don't feed
        if (current.temporary || !current.natural || current.IsWater) return false;
        if (current.passability == Traversability.Impassable) return false;

        // no rung above stone, ice, or unknown terrain - the top of the chain has nowhere left to go
        if (!Ladder.TryGetValue(current, out TerrainDef? next)) return false;

        if (!AshVentSoilLadder.RungIsDue(
                cell.x, cell.z, Mathf.Sqrt(distanceSquared), Props.clearRadius, daysBefore, soilDays
            )) {
            return false;
        }

        map.terrainGrid.SetTerrain(cell, next);
        return true;
    }

    /// <summary>
    ///     Banks this cycle's share of a throw and lands whatever it has earned. Called from the
    ///     plume so it sits behind the tracker's era gate rather than carrying a second one.
    /// </summary>
    private void ThrowMetal(float dayFraction) {
        int due = AshMetalThrow.Bank(ref throwRemainder, Props.throwsPerDay, dayFraction);
        for (int i = 0; i < due; i++) {
            ThrowOnce();
        }
    }

    /// <summary>
    ///     Lands one stack of one metal near the vent. False when no cell in range took it. Carries
    ///     no era check of its own, so every caller but the dev action has to bring one.
    /// </summary>
    public bool ThrowOnce() {
        Verse.Map? map = parent.Map;
        if (map == null) return false;

        Map.AshDepthTracker? tracker = map.GetComponent<Map.AshDepthTracker>();
        if (tracker == null) return false;

        List<ThingDef> metals = Props.ResolvedMetals;
        if (metals.Count == 0) return false;

        ThingDef metal = metals[AshMetalThrow.MetalIndex(throwsMade, parent.thingIDNumber, metals.Count)];
        throwsMade++;

        if (!TryFindLandingCell(map, tracker, out IntVec3 cell)) return false;

        Verse.Thing lump = ThingMaker.MakeThing(metal);
        lump.stackCount = Props.lumpsPerThrow;

        // Direct, not Near - Near spirals past the checks below and can land the lump on a buried cell
        if (!GenPlace.TryPlaceThing(lump, cell, map, ThingPlaceMode.Direct)) return false;

        tracker.NotifyMetalThrown(cell);
        return true;
    }

    /// <summary>
    ///     Standable ground in range the ash has not already swallowed. A lump landing on a buried
    ///     cell would be invisible and unhaulable the instant it existed.
    /// </summary>
    private bool TryFindLandingCell(Verse.Map map, Map.AshDepthTracker tracker, out IntVec3 cell) {
        CellIndices indices = map.cellIndices;

        // InBounds must come first - TryFindRandomCellNear clamps to map.Size, one past the last valid index
        bool Valid(IntVec3 candidate) {
            if (!candidate.InBounds(map)) return false;
            if (!candidate.Standable(map)) return false;

            int index = indices.CellToIndex(candidate);
            return !AshDepthMath.IsBuried(tracker.Grid.GetDepthMm(index), tracker.Buried.IsBuried(index));
        }

        return CellFinder.TryFindRandomCellNear(parent.Position, map, Props.throwRadius, Valid, out cell);
    }
}
