using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
public static class KillViolationPatch {
    private static void Prefix(
        Pawn __instance,
        DamageInfo? dinfo,
        out (bool downed, bool friendly, bool colonist, bool fleeing) __state
    ) {
        __state = (
            __instance.Downed,
            dinfo.HasValue && dinfo.Value.Instigator is Pawn killer && !__instance.HostileTo(killer),
            __instance.Faction == Faction.OfPlayer,
            __instance.CurJobDef == RimWorld.JobDefOf.FleeAndCower
        );
    }

    private static void Postfix(
        Pawn __instance,
        DamageInfo? dinfo,
        (bool downed, bool friendly, bool colonist, bool fleeing) __state
    ) {
        if (!__instance.RaceProps.Humanlike) return;
        if (!dinfo.HasValue) return;

        Pawn? killer = dinfo.Value.Instigator as Pawn;
        if (killer == null) return;

        Surgebinder? surgebinder = ViolationUtility.GetSurgebinder(killer);
        if (surgebinder == null) return;

        string orderName = surgebinder.radiantOrderDef.defName;
        bool killerInBerserk = killer.InMentalState && killer.MentalStateDef == MentalStateDefOf.Berserk;

        switch (orderName) {
            case "Windrunner":
                if (__state.colonist || __state.friendly) {
                    ViolationUtility.ApplyViolation(killer, 0.6f, "killing a friendly");
                } else if (__state.downed || __state.fleeing) {
                    ViolationUtility.ApplyViolation(killer, 0.3f, "killing a defenseless enemy");
                }

                break;

            case "Dustbringer":
                if (__state.colonist && killerInBerserk) {
                    ViolationUtility.ApplyViolation(killer, 0.6f, "killing a colonist in berserk rage");
                }

                break;
        }
    }
}

[HarmonyPatch(typeof(DamageWorker), nameof(DamageWorker.Apply))]
public static class FriendlyFireViolationPatch {
    private static void Postfix(DamageWorker __instance, DamageInfo dinfo, Verse.Thing victim) {
        if (victim is not Pawn targetPawn) return;
        if (!targetPawn.RaceProps.Humanlike) return;

        Pawn? attacker = dinfo.Instigator as Pawn;
        if (attacker == null) return;
        if (attacker == targetPawn) return;

        Surgebinder? surgebinder = ViolationUtility.GetSurgebinder(attacker);
        if (surgebinder == null) return;

        bool targetIsFriendly = !targetPawn.HostileTo(attacker);
        if (!targetIsFriendly) return;

        string orderName = surgebinder.radiantOrderDef.defName;
        if (orderName == "Dustbringer") {
            ViolationUtility.ApplyViolation(attacker, 0.3f, "attacking a friendly");
        }

        if (orderName == "Bondsmith" &&
            targetPawn.Faction != null &&
            !targetPawn.Faction.IsPlayer &&
            targetPawn.Faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally) {
            ViolationUtility.ApplyViolation(attacker, 0.6f, "attacking an allied faction member");
        }
    }
}