using CosmereCore.Need;
using CosmereCore.Util;
using CosmereFramework.Extension;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Allomancy.Hediff;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Util;

public static class AllomancyUtility {
    /**
     * Based on a FULL mistborn burning every metal being capped around 5 BEUs
     * 5 BEU / 16 metals / 100 units per metal ends up with this conversion rate
     */
    public const float BeuPerMetalUnit = 0.3125f;

    /// <summary>
    ///     Tries to burn the specified metal to generate the required BEUs of investiture.
    /// </summary>
    /// <param name="pawn">The pawn performing the burn.</param>
    /// <param name="metal">The metal to burn.</param>
    /// <param name="requiredBeUs">The amount of Investiture to generate.</param>
    /// <returns>True if successful; false if not enough metal is available.</returns>
    public static bool TryBurnMetalForInvestiture(Pawn pawn, MetallicArtsMetalDef metal, float requiredBeUs) {
        float metalNeeded = GetMetalNeededForBeu(requiredBeUs);
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        if (gene == null) return false;

        if (!gene.CanLowerReserve(metalNeeded)) {
            return false;
        }

        gene.RemoveFromReserve(metalNeeded);

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

    /**
     * @todo Convert to extension
     */
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