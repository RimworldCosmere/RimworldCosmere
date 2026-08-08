using System.Collections.Generic;
using Cosmere.System.Scadrial.Grid;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

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

    /// <summary>How big a stack one throw lands.</summary>
    public int lumpsPerThrow = 4;

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
                Logger.Error($"{parentDef.defName} lists thrown metal {metals[i]}, which is not a loaded ThingDef.");
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
    private static List<IntVec3>? offsets;

    private float[]? remainder;
    private float throwRemainder;
    private int throwsMade;

    public CompProperties_AshVent Props => (CompProperties_AshVent)props;

    /// <summary>Every cell inside the plume radius, built once and shared by every vent.</summary>
    private static List<IntVec3> Offsets {
        get {
            if (offsets != null) return offsets;

            offsets = new List<IntVec3>();
            for (int x = -AshPlume.RadiusCells; x <= AshPlume.RadiusCells; x++) {
                for (int z = -AshPlume.RadiusCells; z <= AshPlume.RadiusCells; z++) {
                    if (x * x + z * z <= AshPlume.RadiusCells * AshPlume.RadiusCells) {
                        offsets.Add(new IntVec3(x, 0, z));
                    }
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

        // Unsaved, every load hands the vent back a fresh empty bank and pushes the next throw a
        // full cycle out. The ordinal rides along or a save scum reshuffles which metal comes up.
        Scribe_Values.Look(ref throwRemainder, "ashVentThrowRemainder");
        Scribe_Values.Look(ref throwsMade, "ashVentThrowsMade");

        if (Scribe.mode == LoadSaveMode.LoadingVars) remainder = AshPlume.RestoreBank(banked, Offsets.Count);
    }

    /// <summary>
    ///     Adds this cycle's share to every cell in range. Returns whether anything changed, so
    ///     the tracker only dirties the mesh when it must.
    /// </summary>
    public bool ContributeToGrid(AshGrid grid, float dayFraction) {
        Verse.Map? map = parent.Map;
        if (map == null) return false;

        float millimetres = Props.millimetresPerDay * dayFraction;

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

        Map.AshDepthTracker? tracker = map.GetComponent<Map.AshDepthTracker>();
        if (tracker != null && ClearOwnMouth(map, grid, tracker)) changed = true;

        ThrowMetal(dayFraction);
        return changed;
    }

    /// <summary>
    ///     Clears the vent's own cells and thins the drift back in around them. They take the
    ///     plume's peak weight, so left alone the thing blowing the ash out disappears under it.
    /// </summary>
    private bool ClearOwnMouth(Verse.Map map, AshGrid grid, Map.AshDepthTracker tracker) {
        // Position is the low corner of a 2x2 and CenterCell hands back the high one, so neither
        // is the footprint. OccupiedRect is.
        CellRect mouth = parent.OccupiedRect();
        CellRect reach = mouth.ExpandedBy(Mathf.CeilToInt(Props.clearRadius));
        CellIndices indices = map.cellIndices;
        AshBuriedCells buried = tracker.Buried;
        bool changed = false;

        for (int x = reach.minX; x <= reach.maxX; x++) {
            for (int z = reach.minZ; z <= reach.maxZ; z++) {
                IntVec3 cell = new IntVec3(x, 0, z);
                if (!cell.InBounds(map)) continue;

                // Straight-line distance out of the rect, so the falloff rounds off the corners
                // instead of ringing the mouth in another square.
                int dx = x < mouth.minX ? mouth.minX - x : x > mouth.maxX ? x - mouth.maxX : 0;
                int dz = z < mouth.minZ ? mouth.minZ - z : z > mouth.maxZ ? z - mouth.maxZ : 0;
                float distance = Mathf.Sqrt(dx * dx + dz * dz);

                int index = indices.CellToIndex(cell);
                int depth = grid.GetDepthMm(index);
                int allowed = AshPlume.AllowedDepthMm(depth, distance, Props.clearRadius);

                // Under a whole unit is not removable, so bailing here is what stops an already
                // thinned cell reporting a change every cycle and dirtying the mesh forever.
                if (depth - allowed < AshGrid.UnitMm) continue;

                grid.RemoveDepthMm(index, depth - allowed);

                // Every cell whose depth moved, not just the ones that reach zero. A cell dropping
                // from 1200 to 400 stops being buried and the set has to hear about it.
                buried.Set(index, AshDepthMath.IsBuried(grid.GetDepthMm(index), buried.IsBuried(index)));
                changed = true;
            }
        }

        return changed;
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

    /// <summary>Lands one stack of one metal near the vent. False when no cell in range took it.</summary>
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

        // Direct, not Near. Near spirals outward past the checks below and can settle the lump on
        // a buried cell, and it logs an error rather than failing quietly when it runs out of room.
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

        // TryFindRandomCellNear clamps its square to map.Size, one past the last valid index, so
        // InBounds has to come before anything that reads a grid.
        bool Valid(IntVec3 candidate) {
            if (!candidate.InBounds(map)) return false;
            if (!candidate.Standable(map)) return false;

            int index = indices.CellToIndex(candidate);
            return !AshDepthMath.IsBuried(tracker.Grid.GetDepthMm(index), tracker.Buried.IsBuried(index));
        }

        return CellFinder.TryFindRandomCellNear(parent.Position, map, Props.throwRadius, Valid, out cell);
    }
}
