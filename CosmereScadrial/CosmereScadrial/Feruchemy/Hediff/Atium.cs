using System;
using System.Collections.Generic;
using CosmereResources;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Feruchemy.Hediff;

public class Atium : HediffWithComps {
    private const int TicksPerDay = GenDate.TicksPerDay;
    public const int MinAgeYears = 21;
    private const float AgeTicksPerGameTick = 43200f; // 5 years per hour at severity 5

    private static readonly List<HediffDef> AgeConditions = new List<HediffDef> {
        DefDatabase<HediffDef>.GetNamed("BadBack"),
        DefDatabase<HediffDef>.GetNamed("Frail"),
        DefDatabase<HediffDef>.GetNamed("Cataract"),
        DefDatabase<HediffDef>.GetNamed("Blindness"),
        DefDatabase<HediffDef>.GetNamed("HearingLoss"),
        DefDatabase<HediffDef>.GetNamed("Dementia"),
        DefDatabase<HediffDef>.GetNamed("Alzheimers"),
        DefDatabase<HediffDef>.GetNamed("HeartArteryBlockage"),
        DefDatabase<HediffDef>.GetNamed("Carcinoma"),
        DefDatabase<HediffDef>.GetNamed("OrganDecay"),
    };

    protected bool isTapping => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_TapAtium);
    protected bool isStoring => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_StoreAtium);
    protected Feruchemist? atium => pawn.genes.GetFeruchemicGeneForMetal(MetalDefOf.Atium);

    public override void PostMake() {
        base.PostMake();

        if (atium == null) throw new Exception("Atium can't be on a pawn that doesnt have the Atium gene");
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!isTapping && !isStoring) return;

        // Positive for storing (age increases), negative for tapping (age decreases)
        float direction = isStoring ? +1f : -1f;
        // 5.0 severity = 1 year per in-game hour = 8640 biological age ticks per game tick
        float severityFactor = Severity / 5.0f;
        long ageDeltaTicks = (long)(direction * delta * AgeTicksPerGameTick * severityFactor);
        long newBiologicalAge = pawn.ageTracker.AgeBiologicalTicks + ageDeltaTicks;

        pawn.ageTracker.AgeBiologicalTicks = (long)Mathf.Max(newBiologicalAge, MinAgeYears * TicksPerDay);

        float ageYears = pawn.ageTracker.AgeBiologicalYearsFloat;
        if (!isTapping ||
            !(ageYears <= 55f) ||
            !pawn.IsHashIntervalTick(GenTicks.TickLongInterval, delta) && Rand.Chance(1 / 100f)) return;

        // Remove age-related Hediffs
        AgeConditions.Shuffle();
        foreach (HediffDef def in AgeConditions) {
            Verse.Hediff h = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (h == null) continue;
            pawn.health.RemoveHediff(h);
            return;
        }
    }
}