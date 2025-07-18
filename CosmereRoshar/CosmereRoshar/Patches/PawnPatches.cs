using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Comps.WeaponsAndArmor;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn), "Kill")]
public static class PatchPawnKill {
    private static void Prefix(Pawn instance, DamageInfo? dinfo) {
        if (instance != null && instance.RaceProps.Humanlike) {
            CompAbilityEffectSpawnEquipment abilityComp =
                instance.GetAbilityComp<CompAbilityEffectSpawnEquipment>(
                    CosmereRosharDefs.WhtwlSummonShardblade.defName
                );
            if (abilityComp == null) return;
            if (abilityComp.bladeObject != null) {
                {
                    CompShardblade shardBladeComp = abilityComp.bladeObject.GetComp<CompShardblade>();
                    if (shardBladeComp != null) {
                        shardBladeComp.Summon();
                        shardBladeComp.DismissBlade(instance);
                        shardBladeComp.Summon();
                        shardBladeComp.DismissBlade(instance);
                        shardBladeComp.Summon();
                        shardBladeComp.SeverBond(instance);
                        shardBladeComp.DismissBlade(instance);
                    }
                }
            }
        }
    }
}