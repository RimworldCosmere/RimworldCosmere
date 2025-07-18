using HarmonyLib;
using Verse;

namespace CosmereRoshar;

[HarmonyPatch(typeof(Pawn), "Kill")]
public static class Patch_Pawn_Kill {
    private static void Prefix(Pawn __instance, DamageInfo? dinfo) {
        if (__instance != null && __instance.RaceProps.Humanlike) {
            CompAbilityEffect_SpawnEquipment abilityComp =
                __instance.GetAbilityComp<CompAbilityEffect_SpawnEquipment>(
                    CosmereRosharDefs.whtwl_SummonShardblade.defName
                );
            if (abilityComp == null) return;
            if (abilityComp.bladeObject != null) {
                {
                    CompShardblade shardBladeComp = abilityComp.bladeObject.GetComp<CompShardblade>();
                    if (shardBladeComp != null) {
                        shardBladeComp.summon();
                        shardBladeComp.dismissBlade(__instance);
                        shardBladeComp.summon();
                        shardBladeComp.dismissBlade(__instance);
                        shardBladeComp.summon();
                        shardBladeComp.severBond(__instance);
                        shardBladeComp.dismissBlade(__instance);
                    }
                }
            }
        }
    }
}