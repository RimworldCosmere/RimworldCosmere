using Cosmere.System.Scadrial.Utility;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch;

[HarmonyPatch(typeof(IncidentWorker_Raid), "PostProcessSpawnedPawns")]
public static class PatchRaidPostProcess {
    [HarmonyPostfix]
    public static void Postfix(IncidentParms parms, List<Pawn> pawns) {
        Map? map = parms.target as Map;
        if (map == null) return;

        float copperStrength = AllomancyUtility.GetCoppercloudStrength(map);
        if (copperStrength <= 0f) return;

        float confusionChance = Mathf.Clamp(copperStrength * 2f, 0.15f, 0.6f);
        int confusedCount = 0;

        for (int i = 0; i < pawns.Count; i++) {
            Pawn raider = pawns[i];
            if (raider.health == null) continue;
            if (!Rand.Chance(confusionChance)) continue;

            Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Cosmere_Scadrial_Hediff_CopperConfusion, raider);
            raider.health.AddHediff(hediff);
            confusedCount++;
        }

        int reductionPercent = Mathf.RoundToInt(copperStrength * 100f);
        string message = confusedCount > 0
            ? "Cosmere_Scadrial_CopperRaidReduction_WithConfusion".Translate(reductionPercent, confusedCount)
            : "Cosmere_Scadrial_CopperRaidReduction".Translate(reductionPercent);
        Messages.Message(message, MessageTypeDefOf.PositiveEvent, false);
    }
}