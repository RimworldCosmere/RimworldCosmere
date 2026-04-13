using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(AttackTargetFinder), "IsAutoTargetable")]
public static class InvisibilityTargetingPatch {
    private static void Postfix(IAttackTarget target, ref bool __result) {
        if (!__result) return;

        if (target.Thing is Pawn targetPawn && Invisibility.InvisiblePawns.Contains(targetPawn)) {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
public static class InvisibilityBreakOnAttackPatch {
    private static void Prefix(Verb __instance) {
        if (__instance.CasterPawn == null) return;

        Pawn casterPawn = __instance.CasterPawn;
        if (!Invisibility.InvisiblePawns.Contains(casterPawn)) return;

        RimWorld.Ability? invisAbility = casterPawn.abilities?.GetAbility(
            AbilityDefOf.Cosmere_Roshar_Ability_Invisibility
        );
        if (invisAbility is Invisibility { status.isActive: true } invis) {
            invis.UpdateStatus(Active.Off);
        }
    }
}
