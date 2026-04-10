using Cosmere.Core;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Atium : HediffWithComps {
    private const int TicksPerDay = GenDate.TicksPerDay;
    private const int MinAgeYears = 21;
    private const float AgeTicksPerGameTick = 43200f;

    private static readonly string[] AgeConditionNames = [
        "BadBack", "Frail", "Cataract", "Blindness", "HearingLoss",
        "Dementia", "Alzheimers", "HeartArteryBlockage", "Carcinoma", "OrganDecay",
    ];

    private static List<HediffDef>? cachedAgeConditions;
    private static List<HediffDef> AgeConditions {
        get {
            if (cachedAgeConditions != null) return cachedAgeConditions;
            cachedAgeConditions = [];
            for (int i = 0; i < AgeConditionNames.Length; i++) {
                HediffDef? def = DefDatabase<HediffDef>.GetNamedSilentFail(AgeConditionNames[i]);
                if (def != null) cachedAgeConditions.Add(def);
            }
            return cachedAgeConditions;
        }
    }

    private bool isTapping => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_TapAtium);
    private bool isStoring => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_StoreAtium);
    private Feruchemist? atium => pawn.genes?.GetFeruchemicGeneForMetal(MetalDefOf.Atium);

    public override void PostMake() {
        base.PostMake();

        if (atium == null) {
            Logger.Error("CS_Error_MissingRequirement".Translate("Atium", "the Atium gene"));
            pawn.health.RemoveHediff(this);
        }
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!isTapping && !isStoring) return;

        float direction = isStoring ? +1f : -1f;
        float severityFactor = Severity / 5.0f;
        long ageDeltaTicks = (long)(direction * delta * AgeTicksPerGameTick * severityFactor);
        long newBiologicalAge = pawn.ageTracker.AgeBiologicalTicks + ageDeltaTicks;

        pawn.ageTracker.AgeBiologicalTicks = (long)Mathf.Max(newBiologicalAge, MinAgeYears * GenDate.TicksPerYear);

        float ageYears = pawn.ageTracker.AgeBiologicalYearsFloat;
        if (!isTapping ||
            !(ageYears <= 55f) ||
            !pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta) && Rand.Chance(1 / 100f)) {
            return;
        }

        AgeConditions.Shuffle();
        foreach (HediffDef conditionDef in AgeConditions) {
            Verse.Hediff h = pawn.health.hediffSet.GetFirstHediffOfDef(conditionDef);
            if (h == null) continue;
            pawn.health.RemoveHediff(h);
            return;
        }
    }
}
