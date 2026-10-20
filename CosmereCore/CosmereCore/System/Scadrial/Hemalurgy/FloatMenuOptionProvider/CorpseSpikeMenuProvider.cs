using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class CorpseSpikeMenuProvider : RimWorld.FloatMenuOptionProvider {
    private const string ChargeCorpseSpikeKey = "CS_Hemalurgy_ChargeCorpseSpike";

    protected override bool Drafted => false;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not Corpse corpse) return null;
        if (!corpse.InnerPawn.RaceProps.Humanlike) return null;

        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn == null) return null;

        if (!HemalurgicDefOf.Cosmere_Scadrial_Hemalurgy.IsFinished) return null;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony)) return null;

        if (corpse.Age > HemalurgicConstants.CorpseFreshnessTickLimit) {
            return new FloatMenuOption(
                ChargeCorpseSpikeKey.Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_CorpseTooOld".Translate(),
                null
            );
        }

        Verse.Thing? spike = FindNearestUnchargedSpike(pawn);
        if (spike == null) {
            return new FloatMenuOption(
                ChargeCorpseSpikeKey.Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_NoUnchargedSpike".Translate(),
                null
            );
        }

        if (!pawn.CanReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                ChargeCorpseSpikeKey.Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        if (!pawn.CanReach(spike, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                ChargeCorpseSpikeKey.Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                ChargeCorpseSpikeKey.Translate(corpse.InnerPawn.LabelShortCap),
                () => {
                    Verse.Thing? spikeNow = FindNearestUnchargedSpike(pawn);
                    if (spikeNow == null) return;
                    Verse.AI.Job job = JobMaker.MakeJob(
                        JobDefOf.Cosmere_Scadrial_Job_ChargeCorpseSpike,
                        corpse,
                        spikeNow
                    );
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            ),
            pawn,
            (LocalTargetInfo)corpse
        );
    }

    private static Verse.Thing? FindNearestUnchargedSpike(Pawn pawn) {
        Map map = pawn.Map;
        Verse.Thing? best = null;
        float bestDist = float.MaxValue;

        KeepNearestUncharged(
            map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike),
            pawn,
            ref best,
            ref bestDist
        );
        KeepNearestUncharged(
            map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle),
            pawn,
            ref best,
            ref bestDist
        );

        return best;
    }

    /// <summary>Walks one def's spikes and keeps the closest reachable empty one seen so far.</summary>
    private static void KeepNearestUncharged(
        List<Verse.Thing> candidates, Pawn pawn, ref Verse.Thing? best, ref float bestDist
    ) {
        for (int i = 0; i < candidates.Count; i++) {
            HemalurgicSpike? comp = candidates[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.IsCharged) continue;
            if (candidates[i].IsForbidden(pawn)) continue;

            float dist = candidates[i].Position.DistanceToSquared(pawn.Position);
            if (dist >= bestDist) continue;

            bestDist = dist;
            best = candidates[i];
        }
    }
}
