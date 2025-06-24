using System;
using System.Linq;
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
using PawnUtility = CosmereFramework.Utils.PawnUtility;

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
        if (metal.Equals(MetallicArtsMetalDefOf.Lerasium) && IsMistborn(pawn)) {
            return new AcceptanceReport("CS_AlreadyMistborn".Translate(pawn.Named("PAWN")));
        }

        if (metal.godMetal || IsMistborn(pawn) || IsMisting(pawn, metal)) {
            return true;
        }

        return new AcceptanceReport("CS_CannotUseMetal".Translate(pawn.Named("PAWN"), metal.Named("METAL")));
    }

    [Obsolete("Use pawn.IsMistborn() instead.")]
    public static bool IsMistborn(Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn);
    }

    [Obsolete("Use pawn.IsMisting() instead.")]
    public static bool IsMisting(Pawn pawn, MetallicArtsMetalDef metal) {
        return pawn.genes.HasActiveGene(GeneDefOf.GetMistingGeneForMetal(metal));
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
        if (PawnUtility.IsAsleep(pawn)) return;
        if (InvestitureDetector.IsShielded(pawn)) return;

        AllomanticVial? vial = pawn.GetVials(metal).FirstOrDefault();
        if (vial == null || vial?.stackCount == 0) return;

        Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, vial);
        job.count = 1;
        job.ingestTotalCount = true;
        pawn.jobs.InterruptJobWith(job, JobTag.SatisfyingNeeds);
    }

    public static bool PawnHasVialForMetal(Pawn pawn, MetallicArtsMetalDef metal) {
        if (metal.IsOneOf(MetallicArtsMetalDefOf.Duralumin, MetallicArtsMetalDefOf.Nicrosil)) return false;
        if (PawnUtility.IsAsleep(pawn)) return false;
        if (InvestitureDetector.IsShielded(pawn)) return false;

        return pawn.GetVials(metal).FirstOrDefault()?.stackCount != 0;
    }

    public static bool IsBurning(Pawn pawn, MetallicArtsMetalDef metal) {
        return pawn.GetComp<MetalBurning>()?.IsBurning(metal) ?? false;
    }
}