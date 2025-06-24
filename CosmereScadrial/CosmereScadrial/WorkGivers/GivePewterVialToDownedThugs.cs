using System.Collections.Generic;
using System.Linq;
using CosmereResources;
using CosmereResources.Defs;
using CosmereScadrial.Extensions;
using CosmereScadrial.Genes;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.WorkGivers;

public class GivePewterVialToDownedThugs : WorkGiver_FeedPatient {
    private MetalDef pewter => MetalDefOf.Pewter;
    private ThingDef vialDef => pewter.GetVial();

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
        return pawn.Map.mapPawns.FreeColonistsSpawned.Where(x => x.genes.HasAllomanticGeneForMetal(MetalDefOf.Pewter));
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
        if (t is not Pawn target) return false;
        if (target == pawn) return false;
        if (target.DevelopmentalStage.Baby()) return false;
        if (target.genes == null) return false;
        if (!target.genes.HasAllomanticGeneForMetal(pewter)) return false;


        Allomancer pewterGene = target.genes.GetAllomanticGeneForMetal(pewter)!;
        if (!pewterGene.ShouldConsumeVialNow()) return false;
        if (!pawn.CanReserve(t, ignoreOtherReservations: forced)) return false;

        if (TryFindBestVialSourceFor(pawn, out _)) {
            return true;
        }

        JobFailReason.Is((string)"NoFood".Translate());
        return false;
    }

    public override Job? JobOnThing(Pawn pawn, Thing t, bool forced = false) {
        Pawn target = (Pawn)t;
        Thing vialSource;
        if (!TryFindBestVialSourceFor(pawn, out vialSource)) {
            return null;
        }

        Job job = JobMaker.MakeJob(RimWorld.JobDefOf.FeedPatient);
        job.targetA = (LocalTargetInfo)vialSource;
        job.targetB = (LocalTargetInfo)(Thing)target;
        job.count = 1;
        return job;
    }

    private bool TryFindBestVialSourceFor(
        Pawn pawn,
        out Thing vialSource) {
        vialSource = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(vialDef),
            PathEndMode.Touch, TraverseParms.For(pawn), 9999f, x => VialValidator(pawn, x));

        return vialSource != null;
    }


    private bool VialValidator(Pawn pawn, Thing vial) {
        if (!vial.Spawned) return true;
        if (vial.IsForbidden(pawn)) return false;

        return pawn.CanReserveAndReach(vial, PathEndMode.ClosestTouch, Danger.None, 10, 1);
    }
}