using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Comp.WeaponsAndArmor;
using HarmonyLib;
using Verse;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class PatchPawnKill {
    private static void Prefix(Pawn? instance, DamageInfo? dinfo) {
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