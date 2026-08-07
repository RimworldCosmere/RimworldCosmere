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

    public CompProperties_AshVent() {
        compClass = typeof(CompAshVent);
    }
}

/// <summary>
///     A vent breathing ash onto the ground around it. The offsets are walked once per full sweep
///     cycle, so the cost is a bounded local write and never a scan of the map.
/// </summary>
public class CompAshVent : ThingComp {
    private static List<IntVec3>? offsets;

    private float accrued;

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
        Scribe_Values.Look(ref accrued, "ashVentAccrued");
    }

    /// <summary>
    ///     Adds this cycle's share to every cell in range. Returns whether anything changed, so
    ///     the tracker only dirties the mesh when it must.
    /// </summary>
    public bool ContributeToGrid(AshGrid grid, float dayFraction) {
        Verse.Map? map = parent.Map;
        if (map == null) return false;

        accrued += Props.millimetresPerDay * dayFraction;
        if (accrued < 1f) return false;

        float millimetres = accrued;
        accrued = 0f;

        // One slowly drifting heading for the whole map, so drifts share a downwind side rather
        // than each vent picking its own direction.
        float angle = Mathf.Sin(Find.TickManager.TicksGame / 5200f) * Mathf.PI;
        float headingX = Mathf.Cos(angle);
        float headingZ = Mathf.Sin(angle);
        float skew = Props.skew * Mathf.Clamp01(map.windManager.WindSpeed);

        IntVec3 centre = parent.Position;
        CellIndices indices = map.cellIndices;
        List<IntVec3> all = Offsets;
        bool changed = false;

        for (int i = 0; i < all.Count; i++) {
            IntVec3 cell = centre + all[i];
            if (!cell.InBounds(map)) continue;
            if (!grid.CanHaveAsh(cell)) continue;

            float weight = AshPlume.Weight(all[i].x, all[i].z, headingX, headingZ, skew);
            if (weight <= 0f) continue;

            int add = (int)(millimetres * weight);
            if (add <= 0) continue;
            if (grid.AddDepthMm(indices.CellToIndex(cell), add) > 0) changed = true;
        }

        return changed;
    }
}
