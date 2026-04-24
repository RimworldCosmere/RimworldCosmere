using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.FloatMenuOptionProvider;

public class CorpseSpikeMenuProvider : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => false;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not Corpse corpse) return null;
        if (!corpse.InnerPawn.RaceProps.Humanlike) return null;

        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn == null) return null;

        if (!ResearchProjectDef.Named("Cosmere_Scadrial_Hemalurgy").IsFinished) return null;
        if (!ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony)) return null;

        if (corpse.Age > HemalurgicConstants.CorpseFreshnessTickLimit) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeCorpseSpike".Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_CorpseTooOld".Translate(),
                null
            );
        }

        Verse.Thing? spike = FindNearestUnchargedSpike(pawn);
        if (spike == null) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeCorpseSpike".Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "CS_Hemalurgy_NoUnchargedSpike".Translate(),
                null
            );
        }

        if (!pawn.CanReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeCorpseSpike".Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        if (!pawn.CanReach(spike, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(
                "CS_Hemalurgy_ChargeCorpseSpike".Translate(corpse.InnerPawn.LabelShortCap) +
                ": " +
                "NoPath".Translate().CapitalizeFirst(),
                null
            );
        }

        return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                "CS_Hemalurgy_ChargeCorpseSpike".Translate(corpse.InnerPawn.LabelShortCap),
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

    private Verse.Thing? FindNearestUnchargedSpike(Pawn pawn) {
        Map map = pawn.Map;
        Verse.Thing? best = null;
        float bestDist = float.MaxValue;

        List<Verse.Thing> spikes = map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike);
        for (int i = 0; i < spikes.Count; i++) {
            HemalurgicSpike? comp = spikes[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.isCharged) continue;
            if (spikes[i].IsForbidden(pawn)) continue;
            float dist = spikes[i].Position.DistanceToSquared(pawn.Position);
            if (dist < bestDist) {
                bestDist = dist;
                best = spikes[i];
            }
        }

        List<Verse.Thing> needles =
            map.listerThings.ThingsOfDef(HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle);
        for (int i = 0; i < needles.Count; i++) {
            HemalurgicSpike? comp = needles[i].TryGetComp<HemalurgicSpike>();
            if (comp == null || comp.isCharged) continue;
            if (needles[i].IsForbidden(pawn)) continue;
            float dist = needles[i].Position.DistanceToSquared(pawn.Position);
            if (dist < bestDist) {
                bestDist = dist;
                best = needles[i];
            }
        }

        return best;
    }
}