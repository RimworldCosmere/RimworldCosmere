using System.Collections.Generic;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Comp.Thing;

public class CompProperties_AshGas : CompProperties {
    /// <summary>Cells past the vent's own footprint that the gas still reaches.</summary>
    public float radius = 5f;

    /// <summary>
    ///     Raw exposure standing on the mouth itself, before any mask. How fast that turns into
    ///     severity belongs to AshLungMath - one knob for the reach, one for the curve, never two.
    /// </summary>
    public float exposureAtMouth = 1f;

    public CompProperties_AshGas() {
        compClass = typeof(CompAshGas);
    }
}

/// <summary>
///     What the vent breathes out. One sweep per map walks the spawned pawns rather than each vent
///     sweeping its own cells, so overlapping vents can never dose or relieve the same pawn twice.
/// </summary>
public class CompAshGas : ThingComp {
    private static HediffDef? ashLung;
    private static StatDef? filtration;
    private static bool defsMissing;

    private CellRect mouth;
    private CompHeatPusher? heat;

    public CompProperties_AshGas Props => (CompProperties_AshGas)props;

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);

        // Derived from a building that cannot move, so it is rebuilt on every spawn and never saved.
        mouth = parent.OccupiedRect();
        heat = parent.GetComp<CompHeatPusher>();
        parent.Map?.GetComponent<Map.AshDepthTracker>()?.RegisterGas(this);
    }

    public override void PostDeSpawn(Verse.Map map, DestroyMode mode) {
        base.PostDeSpawn(map, mode);
        map.GetComponent<Map.AshDepthTracker>()?.DeregisterGas(this);
    }

    /// <summary>Raw exposure on a cell before the pawn's mask. Zero outside the radius.</summary>
    public float RawExposureAt(IntVec3 cell) {
        float distance = AshGasFalloff.DistanceToMouth(cell.x, cell.z, mouth.minX, mouth.minZ, mouth.maxX, mouth.maxZ);

        return AshGasFalloff.Exposure(distance, Props.radius, Props.exposureAtMouth);
    }

    /// <summary>
    ///     One pass over the map's pawns. Driven by the tracker's stripe clock and deliberately
    ///     outside its era gate: the gas stops at the Catacendre, ash lung still has to recede.
    /// </summary>
    public static void Exhale(Verse.Map map, List<CompAshGas> vents, float hours) {
        if (!TryResolveDefs()) return;

        bool breathing = AshEra.CanAccumulate(map);
        for (int i = 0; i < vents.Count; i++) {
            CompHeatPusher? pusher = vents[i].heat;
            if (pusher != null) pusher.enabled = breathing;
        }

        IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            if (!pawn.RaceProps.IsFlesh) continue;

            float raw = breathing ? RawExposureFor(pawn.Position, vents) : 0f;

            pawn.health.hediffSet.TryGetHediff(ashLung, out Verse.Hediff? lung);
            if (raw <= 0f && lung == null) continue;

            // Zero exposure means zero effective exposure whatever the mask is worth, so a pawn
            // only out here to recede never pays for the stat read.
            float worn = raw > 0f ? pawn.GetStatValue(filtration) : 0f;
            float delta = AshLungMath.SeverityDeltaPerHour(raw, worn) * hours;

            if (lung == null && delta <= 0f) continue;

            // Vanilla drops the hediff itself once severity reaches zero, so recede needs no undo.
            lung ??= pawn.health.GetOrAddHediff(ashLung);
            lung.Severity += delta;
        }
    }

    /// <summary>Worst exposure any one vent puts on the cell, not the sum of all of them.</summary>
    private static float RawExposureFor(IntVec3 cell, List<CompAshGas> vents) {
        float worst = 0f;

        for (int i = 0; i < vents.Count; i++) {
            float here = vents[i].RawExposureAt(cell);
            if (here > worst) worst = here;
            if (worst >= 1f) break;
        }

        return worst;
    }

    /// <summary>
    ///     The comp lives in Core and both defs ship with Scadrial, so a Core-only load resolves
    ///     neither. Latching the failure keeps that from throwing once a stripe cycle forever.
    /// </summary>
    private static bool TryResolveDefs() {
        if (defsMissing) return false;
        if (ashLung != null && filtration != null) return true;

        ashLung = DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_AshLung");
        filtration = DefDatabase<StatDef>.GetNamedSilentFail("Cosmere_Scadrial_Stat_AshFiltration");
        if (ashLung != null && filtration != null) return true;

        Logger.Error(
            "Ash gas is registered but Cosmere_Scadrial_Hediff_AshLung or Cosmere_Scadrial_Stat_AshFiltration " +
            "is not loaded. The vent will radiate heat and throw metal but nobody will ever choke on it."
        );
        defsMissing = true;

        return false;
    }
}
