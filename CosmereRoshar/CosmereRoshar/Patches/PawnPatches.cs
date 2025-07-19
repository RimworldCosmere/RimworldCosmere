using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Extensions;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn))]
public static class PatchPawnKill {
    [HarmonyPatch(nameof(Pawn.Kill))]
    [HarmonyPrefix]
    private static void PrefixKill(Pawn? instance, DamageInfo? dinfo) {
        if (instance == null || !instance.RaceProps.Humanlike) return;

        SpawnEquipment? abilityComp =
            instance.GetAbilityComp<SpawnEquipment>(CosmereRosharDefs.WhtwlSummonShardblade.defName);

        if (!(abilityComp?.bladeObject?.TryGetComp(out ShardBlade blade) ?? false)) return;
        blade.Summon();
        blade.DismissBlade(instance);
        blade.Summon();
        blade.DismissBlade(instance);
        blade.Summon();
        blade.SeverBond(instance);
        blade.DismissBlade(instance);
    }
}