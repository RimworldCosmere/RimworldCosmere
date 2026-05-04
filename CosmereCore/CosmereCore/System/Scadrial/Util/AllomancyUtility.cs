using RimWorld;
using UnityEngine;
using Verse;
using Cosmere.System.Scadrial.Allomancy.Hediff;

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

    public static float GetCoppercloudStrength(Map map) {
        float totalReduction = 0f;
        int smokerCount = 0;

        List<Pawn> colonists = map.mapPawns.FreeColonists;
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

        return Mathf.Min(totalReduction, 0.25f);
    }
}