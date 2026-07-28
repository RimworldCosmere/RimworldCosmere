using Concord;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

// Pawn.Kill gets exactly one whole-method Around, so both pre-call-state consumers ride on it:
// the killer-violation checks below and the witness tracking in DeathWitnessTracking.
[Patch]
public abstract class PawnKillPatch : Pawn {
    [Inject(At.Around, nameof(Kill))]
    private void AroundKill(
        DamageInfo? dinfo,
        Verse.Hediff exactCulprit,
        VoidOperation<DamageInfo?, Verse.Hediff> original
    ) {
        Pawn self = this;

        Map? mapHeld = DeathWitnessTracking.CaptureMapHeld(self);
        (bool downed, bool friendly, bool colonist, bool fleeing) state = (
            self.Downed,
            dinfo.HasValue && dinfo.Value.Instigator is Pawn killer && !self.HostileTo(killer),
            self.Faction == Faction.OfPlayer,
            self.CurJobDef == RimWorld.JobDefOf.FleeAndCower
        );

        original.Invoke(dinfo, exactCulprit);

        DeathWitnessTracking.NotifyWitnesses(self, mapHeld);
        ApplyKillViolations(self, dinfo, state);
    }

    private static void ApplyKillViolations(
        Pawn victim,
        DamageInfo? dinfo,
        (bool downed, bool friendly, bool colonist, bool fleeing) state
    ) {
        if (!victim.RaceProps.Humanlike) return;
        if (!dinfo.HasValue) return;

        Pawn? killer = dinfo.Value.Instigator as Pawn;
        if (killer == null) return;

        Surgebinder? surgebinder = ViolationUtility.GetSurgebinder(killer);
        if (surgebinder == null) return;

        RadiantOrderDef orderDef = surgebinder.radiantOrderDef;
        bool killerInBerserk = killer.InMentalState && killer.MentalStateDef == MentalStateDefOf.Berserk;

        if (orderDef == RadiantOrderDefOf.Windrunner) {
            if (state.colonist || state.friendly) {
                ViolationUtility.ApplyViolation(killer, 0.6f, "killing a friendly");
            } else if (state.downed || state.fleeing) {
                ViolationUtility.ApplyViolation(killer, 0.3f, "killing a defenseless enemy");
            }
        } else if (orderDef == RadiantOrderDefOf.Dustbringer) {
            if (state.colonist && killerInBerserk) {
                ViolationUtility.ApplyViolation(killer, 0.6f, "killing a colonist in berserk rage");
            }
        }
    }
}

[Patch]
public abstract class FriendlyFireViolationPatch : DamageWorker {
    [Inject(At.Return, nameof(Apply))]
    private void AfterApply(DamageInfo dinfo, Verse.Thing victim) {
        if (victim is not Pawn targetPawn) return;
        if (!targetPawn.RaceProps.Humanlike) return;

        Pawn? attacker = dinfo.Instigator as Pawn;
        if (attacker == null) return;
        if (attacker == targetPawn) return;

        Surgebinder? surgebinder = ViolationUtility.GetSurgebinder(attacker);
        if (surgebinder == null) return;

        bool targetIsFriendly = !targetPawn.HostileTo(attacker);
        if (!targetIsFriendly) return;

        RadiantOrderDef orderDef = surgebinder.radiantOrderDef;
        if (orderDef == RadiantOrderDefOf.Dustbringer) {
            ViolationUtility.ApplyViolation(attacker, 0.3f, "attacking a friendly");
        }

        if (orderDef == RadiantOrderDefOf.Bondsmith &&
            targetPawn.Faction != null &&
            !targetPawn.Faction.IsPlayer &&
            targetPawn.Faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Ally) {
            ViolationUtility.ApplyViolation(attacker, 0.6f, "attacking an allied faction member");
        }
    }
}
