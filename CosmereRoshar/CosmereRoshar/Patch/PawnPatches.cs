using Cosmere.Roshar.Combat.Abilities.Implementations;
using HarmonyLib;
using Verse;

namespace Cosmere.Roshar.Patches;

[HarmonyPatch(typeof(Pawn))]
public static class PatchPawnKill {
    [HarmonyPatch(nameof(Pawn.Kill))]
    [HarmonyPrefix]
    private static void PrefixKill(Pawn? __instance, DamageInfo? dinfo) {
        if (__instance == null || !__instance.RaceProps.Humanlike) return;

        SpawnEquipment? abilityComp =
            __instance.GetAbilityComp<SpawnEquipment>(Defs.Cosmere_Roshar_SummonShardblade.defName);

        if (!(abilityComp?.bladeObject?.TryGetComp(out ShardBlade blade) ?? false)) return;
        blade.Summon();
        blade.DismissBlade(__instance);
        blade.Summon();
        blade.DismissBlade(__instance);
        blade.Summon();
        blade.SeverBond(__instance);
        blade.DismissBlade(__instance);
    }
}