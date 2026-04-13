using System.Reflection;
using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch]
public static class RaidDefenseTrackingPatch {
    static MethodBase TargetMethod() {
        return typeof(Lord).GetMethod("Cleanup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [HarmonyPrefix]
    private static void Prefix(Lord __instance) {
        if (__instance.Map == null) return;
        if (__instance.faction == null || !__instance.faction.HostileTo(Faction.OfPlayer)) return;

        LordJob lordJob = __instance.LordJob;
        if (lordJob is not LordJob_AssaultColony and not LordJob_AssaultThings) return;

        List<Pawn> colonists = __instance.Map.mapPawns.FreeColonistsSpawned;
        int downedCount = 0;
        int standingCount = 0;

        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist.Downed) {
                downedCount++;
            } else if (!colonist.Dead) {
                standingCount++;
            }
        }

        for (int i = 0; i < colonists.Count; i++) {
            Pawn colonist = colonists[i];
            if (colonist.Dead || colonist.Downed) continue;

            Surgebinder? surgebinder = colonist.genes?.GetFirstGeneOfType<Surgebinder>();
            if (surgebinder == null) continue;

            if (colonist.Drafted || colonist.CurJobDef == RimWorld.JobDefOf.AttackMelee || colonist.CurJobDef == RimWorld.JobDefOf.AttackStatic) {
                colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_RaidsDefended, 1);
            }

            if (standingCount <= 2 && downedCount >= 2) {
                colonist.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_LastStanding, 1);
            }
        }
    }
}
