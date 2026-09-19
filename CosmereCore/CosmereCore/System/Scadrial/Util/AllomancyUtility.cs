using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Threat;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

public static class AllomancyUtility {
    public static SurgeChargeHediff? FindSurgeChargeHediff(Pawn pawn) {
        return pawn.health.hediffSet.TryGetHediff(out SurgeChargeHediff hediff) ? hediff : null;
    }

    public static bool HasActiveBronzeSeeker(Map map) {
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            Hediff? bronzeAura =
                pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_BronzeAura);
            if (bronzeAura != null) return true;
        }

        return false;
    }

    public static float GetBronzeSeekerStrength(Map map) {
        float maxStrength = 0f;
        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            Hediff? bronzeAura =
                pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_BronzeAura);
            if (bronzeAura == null) continue;

            float allomanticPower = Mathf.Clamp01(pawn.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower));
            float strength = bronzeAura.Severity * allomanticPower;
            if (strength > maxStrength) maxStrength = strength;
        }

        return maxStrength;
    }

    public static bool CaravanHasActiveBronzeSeeker(List<Pawn> pawns) {
        for (int i = 0; i < pawns.Count; i++) {
            Pawn pawn = pawns[i];
            Hediff? bronzeAura =
                pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_BronzeAura);
            if (bronzeAura != null) return true;
        }

        return false;
    }

    /// <summary>
    ///     How much of this colony the copperclouds are hiding. A cloud only conceals what stands
    ///     inside it, so the full reduction is scaled by the share of colonists actually covered.
    /// </summary>
    public static float GetCoppercloudStrength(Map map) {
        List<Pawn> colonists = map.mapPawns.FreeColonists;

        float totalReduction = 0f;
        int smokerCount = 0;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            Hediff? copperAura =
                pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_CopperAura);
            if (copperAura == null) continue;

            float severity = copperAura.Severity;
            float allomanticPower = Mathf.Clamp01(pawn.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower));
            float reduction = 0.10f * severity * allomanticPower;

            float diminishingFactor = Mathf.Pow(0.5f, smokerCount);
            totalReduction += reduction * diminishingFactor;
            smokerCount++;
        }

        if (smokerCount == 0) return 0f;

        return Mathf.Min(totalReduction, 0.25f) * ShelteredShare(colonists);
    }

    /// <summary>
    ///     The share of colonists standing inside a cloud, asked of Coppercloud so the radius is
    ///     read from the comp that applies it rather than restated here.
    /// </summary>
    private static float ShelteredShare(List<Pawn> colonists) {
        if (colonists.Count == 0) return 0f;

        List<float> covered = [];
        for (int i = 0; i < colonists.Count; i++) {
            covered.Add(Coppercloud.StrengthOver(colonists[i]) > 0f ? 1f : 0f);
        }

        return ScadrialThreat.Coverage(covered);
    }
}
