using CosmereCore.Needs;
using CosmereCore.Utils;
using CosmereFramework.Extensions;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Extensions;
using CosmereScadrial.Things;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CosmereScadrial.Utils;

public static class AllomancyUtility {
    /**
     * Based on a FULL mistborn burning every metal being capped around 5 BEUs
     * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
     */
    public const float BeuPerMetalUnit = 0.3125f;

    public static void AddMetalReserve(Pawn pawn, MetallicArtsMetalDef metal, float amount) {
        pawn.GetComp<MetalReserves>().AddReserve(metal, amount);
    }

    public static AcceptanceReport CanUseMetal(Pawn pawn, MetallicArtsMetalDef metal) {
        if (metal.Equals(MetallicArtsMetalDefOf.Lerasium) && pawn.IsMistborn()) {
            return new AcceptanceReport("CS_AlreadyMistborn".Translate(pawn.Named("PAWN")));
        }

        if (metal.godMetal || pawn.IsMistborn() || pawn.IsMisting(metal)) {
            return true;
        }

        return new AcceptanceReport("CS_CannotUseMetal".Translate(pawn.Named("PAWN"), metal.Named("METAL")));
    }

    public static float GetReservePercent(Pawn pawn, MetallicArtsMetalDef metal) {
        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        if (comp == null || !comp.TryGetReserve(metal, out float value)) {
            return 0f;
        }

        return value / MetalReserves.MaxAmount;
    }

    /// <summary>
    ///     Tries to burn the specified metal to generate the required BEUs of investiture.
    /// </summary>
    /// <param name="pawn">The pawn performing the burn.</param>
    /// <param name="metal">The metal to burn.</param>
    /// <param name="requiredBeUs">The amount of Investiture to generate.</param>
    /// <returns>True if successful; false if not enough metal is available.</returns>
    public static bool TryBurnMetalForInvestiture(Pawn pawn, MetallicArtsMetalDef metal, float requiredBeUs) {
        float metalNeeded = GetMetalNeededForBeu(requiredBeUs);
        MetalReserves? comp = pawn.GetComp<MetalReserves>();

        if (!comp.CanLowerReserve(metal, metalNeeded)) {
            return false;
        }

        comp.LowerReserve(metal, metalNeeded);

        Investiture? investitureNeed = pawn.needs.TryGetNeed<Investiture>();
        if (investitureNeed == null) throw new MissingReferenceException("Pawn is missing the Investiture need.");

        investitureNeed.CurLevel = Mathf.Min(
            investitureNeed.CurLevel + requiredBeUs,
            investitureNeed.MaxLevel
        );

        return true;
    }

    public static SurgeChargeHediff? GetSurgeBurn(Pawn pawn) {
        return pawn.health.hediffSet.TryGetHediff(out SurgeChargeHediff hediff) ? hediff : null;
    }

    public static float GetMetalNeededForBeu(float requiredBeu) {
        return requiredBeu / BeuPerMetalUnit;
    }

    public static void TryPawnConsumeVialWithMetal(Pawn pawn, MetallicArtsMetalDef metal) {
        if (pawn.CurJobDef?.defName == RimWorld.JobDefOf.Ingest.defName &&
            pawn.CurJob.targetA.Thing is AllomanticVial) {
            return;
        }

        if (metal.IsOneOf(MetallicArtsMetalDefOf.Duralumin, MetallicArtsMetalDefOf.Nicrosil)) return;
        if (pawn.IsAsleep()) return;
        if (InvestitureDetector.IsShielded(pawn)) return;

        AllomanticVial? vial = pawn.GetVial(metal);
        if (vial == null || vial.stackCount == 0) return;

        Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, vial);
        job.count = 1;
        job.ingestTotalCount = true;
        pawn.jobs.InterruptJobWith(job, JobTag.SatisfyingNeeds);
    }

    public static bool PawnHasVialForMetal(Pawn pawn, MetallicArtsMetalDef metal) {
        if (metal.IsOneOf(MetallicArtsMetalDefOf.Duralumin, MetallicArtsMetalDefOf.Nicrosil)) return false;
        if (pawn.IsAsleep()) return false;
        if (InvestitureDetector.IsShielded(pawn)) return false;

        return pawn.GetVial(metal)?.stackCount != 0;
    }

    public static bool IsBurning(Pawn pawn, MetallicArtsMetalDef metal) {
        return pawn.GetComp<MetalBurning>()?.IsBurning(metal) ?? false;
    }
}