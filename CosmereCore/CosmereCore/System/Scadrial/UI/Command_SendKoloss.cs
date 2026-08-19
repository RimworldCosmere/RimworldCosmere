using System.Collections.Generic;
using Cosmere.System.Scadrial.Comp.Game;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.UI;

/// <summary>
///     Points everything an Allomancer is holding at one thing.
/// </summary>
/// <remarks>
///     A held koloss is a slave rather than a colonist, so the ordinary draft-and-click loop does
///     not reach it. Modelled on the interaction vanilla uses for setting animals on a target: the
///     count goes in the label so the player knows what they are committing, and a click nothing
///     can reach is refused with a reason rather than silently ignored.
///     <para>
///         Melee only, deliberately. A koloss has Shooting disabled and carries a blade, so
///         pointing one at something means walking it there.
///     </para>
/// </remarks>
public static class KolossOrders {
    public static IEnumerable<Verse.Gizmo> For(Pawn holder) {
        List<Pawn> held = KolossRoster.Current?.HeldBy(holder) ?? [];
        if (held.Count == 0) yield break;

        yield return new Command_Target {
            defaultLabel = "CS_KolossSend_Label".Translate(held.Count.Named("COUNT")),
            defaultDesc = "CS_KolossSend_Desc".Translate(),
            icon = TexCommand.Attack,
            targetingParams = new TargetingParameters {
                canTargetPawns = true,
                canTargetBuildings = true,
                canTargetAnimals = true,
                canTargetMechs = true,
                canTargetItems = false,
                canTargetSelf = false,
            },
            action = target => Send(holder, held, target),
        };
    }

    private static void Send(Pawn holder, List<Pawn> held, LocalTargetInfo target) {
        int sent = 0;

        for (int i = 0; i < held.Count; i++) {
            Pawn one = held[i];

            // checked here, not filtered from the list, so the label's held count stays honest.
            if (!one.Spawned || one.Dead || one.Downed || one.InMentalState) continue;
            if (one.jobs == null) continue;
            if (!one.CanReach(target, PathEndMode.Touch, Danger.Deadly)) continue;

            Verse.AI.Job job = JobMaker.MakeJob(RimWorld.JobDefOf.AttackMelee, target);
            job.playerForced = true;
            job.killIncappedTarget = target.Pawn?.Downed == true;

            one.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            sent++;
        }

        Messages.Message(
            sent == 0
                ? "CS_KolossSend_None".Translate()
                : "CS_KolossSend_Sent".Translate(sent.Named("COUNT")),
            holder,
            sent == 0 ? MessageTypeDefOf.RejectInput : MessageTypeDefOf.TaskCompletion,
            false
        );
    }
}
