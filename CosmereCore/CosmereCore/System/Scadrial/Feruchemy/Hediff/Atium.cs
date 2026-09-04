using System;
using Cosmere.Core;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Atium : HediffWithComps {
    private const int TicksPerDay = GenDate.TicksPerDay;
    private const float AgeTicksPerGameTick = 43200f;

    /// How young atium will take a pawn. Flat rather than read off the race, since
    /// life stages are flagged loosely enough that teenagers count as adult.
    private const int MinAgeYears = 18;

    /// Held as a long, not float: float loses whole ticks past ~16M ticks, and this
    /// floor sits at 65M, so a float compare would never see it arrive.
    private const long MinAgeTicks = MinAgeYears * (long)GenDate.TicksPerYear;

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

    private bool isTapping => CompoundedTap.IsTap(def, HediffDefOf.Cosmere_Scadrial_Hediff_TapAtium);

    private bool isStoring => def.Equals(HediffDefOf.Cosmere_Scadrial_Hediff_StoreAtium);

    private Feruchemist? atium => pawn.genes?.GetFeruchemicGeneForMetal(MetalDefOf.Atium);

    public override void PostMake() {
        base.PostMake();

        if (atium == null) {
            Log.Error("CS_Error_MissingRequirement".Translate("Atium", "the Atium gene"));
            pawn.health.RemoveHediff(this);
        }
    }

    /// Hands back the share of this interval's draw that bought no years, so a
    /// pawn arriving at the floor is not charged for the part that did nothing.
    private void RefundUnused(float fraction) {
        Feruchemist? gene = atium;
        if (gene == null || fraction <= 0f) return;

        float refund = FeruchemyRate.PerSecond(Severity, gene.RateMultiplier, gene.Efficiency) * fraction;
        if (refund <= 0f) return;

        if (CompoundedTap.IsCompounded(def)) gene.AddCompoundedToStore(refund);
        else gene.AddToStore(refund);
    }

    // Stops whichever dial is drawing on this metal, leaving the other alone.
    private void ParkDial() {
        Feruchemist? gene = atium;
        if (gene == null) return;

        if (CompoundedTap.IsCompounded(def)) {
            gene.compoundedTargetValue = Feruchemist.IdleTarget;
            gene.compounding = false;
        } else {
            gene.targetValue = Feruchemist.IdleTarget;
        }

        Messages.Message(
            "CS_Feruchemy_AtiumFloorReached".Translate(pawn.Named("PAWN")),
            pawn,
            MessageTypeDefOf.NeutralEvent
        );
    }

    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) return;

        if (!isTapping && !isStoring) return;

        // age has a floor; a dial pushed against it burns charge and the metalmind for nothing, so park it.
        float direction = isStoring ? +1f : -1f;
        float severityFactor = CompoundedTap.Scale(def, Severity) / 5.0f;
        long requested = (long)(delta * AgeTicksPerGameTick * severityFactor);
        long current = pawn.ageTracker.AgeBiologicalTicks;

        if (direction < 0f) {
            // natural aging ticks them back above the floor between rare ticks; check ticks left, not equality.
            long available = current - MinAgeTicks;
            if (available <= 0L) {
                ParkDial();

                return;
            }

            long applied = Math.Min(requested, available);
            pawn.ageTracker.AgeBiologicalTicks = current - applied;

            // only charge for years actually shed; refund the rest and park, the floor buys nothing further.
            if (applied < requested) {
                RefundUnused(1f - applied / (float)requested);
                ParkDial();
            }
        } else {
            pawn.ageTracker.AgeBiologicalTicks = current + requested;
        }

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
