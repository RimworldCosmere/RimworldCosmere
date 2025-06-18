using System.Collections.Generic;
using CosmereCore.Needs;
using CosmereCore.Utils;
using CosmereResources.Defs;
using CosmereResources.ModExtensions;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Utils;

public static class AllomancyUtility {
    /**
     * Based on a FULL mistborn burning every metal being capped around 5 BEUs
     * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
     */
    public const float BEU_PER_METAL_UNIT = 0.3125f;

    public static void AddMetalReserve(Pawn pawn, MetallicArtsMetalDef metal, float amount) {
        pawn.GetComp<MetalReserves>().AddReserve(metal, amount);
    }

    public static bool CanUseMetal(Pawn pawn, MetallicArtsMetalDef metal) {
        if (metal.Equals(MetallicArtsMetalDefOf.Lerasium) && IsMistborn(pawn)) return false;

        return metal.godMetal || IsMistborn(pawn) || IsMisting(pawn, metal);
    }

    public static bool IsMistborn(Pawn pawn) {
        return pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Mistborn);
    }

    public static bool IsMisting(Pawn pawn, MetallicArtsMetalDef metal) {
        return pawn.genes.HasActiveGene(DefDatabase<GeneDef>.GetNamed($"Cosmere_Misting_{metal.defName}"));
    }

    public static float GetReservePercent(Pawn pawn, MetallicArtsMetalDef metal) {
        MetalReserves? comp = pawn.GetComp<MetalReserves>();
        if (comp == null || !comp.TryGetReserve(metal, out float value)) {
            return 0f;
        }

        return value / MetalReserves.MAX_AMOUNT;
    }

    /// <summary>
    ///     Tries to burn the specified metal to generate the required BEUs of investiture.
    /// </summary>
    /// <param name="pawn">The pawn performing the burn.</param>
    /// <param name="metal">The metal to burn.</param>
    /// <param name="requiredBEUs">The amount of Investiture to generate.</param>
    /// <returns>True if successful; false if not enough metal is available.</returns>
    public static bool TryBurnMetalForInvestiture(Pawn pawn, MetallicArtsMetalDef metal, float requiredBEUs) {
        float metalNeeded = GetMetalNeededForBeu(requiredBEUs);
        MetalReserves? comp = pawn.GetComp<MetalReserves>();

        if (!comp.CanLowerReserve(metal, metalNeeded)) {
            return false;
        }

        comp.LowerReserve(metal, metalNeeded);

        Investiture? investitureNeed = pawn.needs.TryGetNeed<Investiture>();
        if (investitureNeed == null) throw new MissingReferenceException("Pawn is missing the Investiture need.");

        investitureNeed.CurLevel = Mathf.Min(
            investitureNeed.CurLevel + requiredBEUs,
            investitureNeed.MaxLevel
        );

        return true;
    }

    public static SurgeChargeHediff GetSurgeBurn(Pawn pawn) {
        SurgeChargeHediff hediff = null;
        pawn.health?.hediffSet.TryGetHediff(out hediff);

        return hediff;
    }

    public static float GetMetalNeededForBeu(float requiredBeu) {
        return requiredBeu / BEU_PER_METAL_UNIT;
    }

    public static bool PawnConsumeVialWithMetal(Pawn pawn, MetallicArtsMetalDef metal,
        bool allowMultiVial = false) {
        if (metal.Equals(MetallicArtsMetalDefOf.Duralumin) || metal.Equals(MetallicArtsMetalDefOf.Nicrosil)) {
            return false;
        }

        if (PawnUtility.IsAsleep(pawn)) return false;
        if (pawn?.inventory?.innerContainer == null) return false;

        foreach (Thing? vial in pawn.inventory.innerContainer) {
            List<MetalDef?> metals = vial.def.GetModExtension<MetalsLinked>()?.Metals ?? [];

            if (metals.Count > 1 && !allowMultiVial) continue;

            if (!metals.Contains(metal) || vial.stackCount <= 0) continue;

            if (Mathf.Approximately(vial.Ingested(pawn, 1f), 0f)) continue;
            Log.Warning($"{pawn.NameShortColored} is auto-ingesting {vial.LabelShort} for {metal}.");
            return true;
        }

        return false;
    }

    public static bool PawnHasVialForMetal(Pawn pawn, MetallicArtsMetalDef metal, bool allowMultiVial = false) {
        if (metal.Equals(MetallicArtsMetalDefOf.Duralumin) || metal.Equals(MetallicArtsMetalDefOf.Nicrosil)) {
            return false;
        }

        if (InvestitureDetector.IsShielded(pawn)) return false;

        if (pawn?.inventory?.innerContainer == null) return false;

        foreach (Thing? vial in pawn.inventory.innerContainer) {
            List<MetalDef?> metals = vial.def.GetModExtension<MetalsLinked>()?.Metals ?? [];

            if (metals.Count > 1 && !allowMultiVial) continue;

            if (metals.Contains(metal) && vial.stackCount > 0) return true;
        }

        return false;
    }

    public static bool IsBurning(Pawn pawn, MetallicArtsMetalDef metal) {
        MetalBurning? burning = pawn.GetComp<MetalBurning>();

        return burning?.IsBurning(metal) ?? false;
    }
}