using System;
using Cosmere.Core;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class Atium : HediffWithComps {
    private const int TicksPerDay = GenDate.TicksPerDay;
    private const float AgeTicksPerGameTick = 43200f;

    // How young atium will take a pawn. Flat rather than read off the race, since
    // life stages are flagged loosely enough that teenagers count as adult.
    private const int MinAgeYears = 18;

    // Held in ticks and compared as a long. Float loses whole ticks past about
    // sixteen million, and this floor is sixty-five million, so a float compare
    // lands either side of it and the dial never sees itself arrive.
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
            Logger.Error("CS_Error_MissingRequirement".Translate("Atium", "the Atium gene"));
            pawn.health.RemoveHediff(this);
        }
    }

    // Hands back the share of this interval's draw that bought no years, so a
    // pawn arriving at the floor is not charged for the part that did nothing.
    private void RefundUnused(float fraction) {
        Feruchemist? gene = atium;
        if (gene == null || fraction <= 0f) return;

        float refund = Feruchemist.AmountPerSecond * Severity * fraction;
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

        // Age has a floor. A dial left pushed against it spends charge, and now the
        // metalmind itself, to move a number that cannot move - so park it rather
        // than let the pawn burn an implant away for nothing.
        float direction = isStoring ? +1f : -1f;
        float severityFactor = CompoundedTap.Scale(def, Severity) / 5.0f;
        long requested = (long)(delta * AgeTicksPerGameTick * severityFactor);
        long current = pawn.ageTracker.AgeBiologicalTicks;

        if (direction < 0f) {
            // Ordinary aging ticks the pawn back up between rare ticks, so they sit a
            // few hundred ticks above the floor rather than on it. Asking whether they
            // have arrived never answers yes; ask how much is left to shed instead.
            long available = current - MinAgeTicks;
            if (available <= 0L) {
                ParkDial();

                return;
            }

            long applied = Math.Min(requested, available);
            pawn.ageTracker.AgeBiologicalTicks = current - applied;

            // The draw only paid for the years actually shed, so hand back the rest
            // and stop: the pawn is at the floor and further tapping buys nothing.
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
