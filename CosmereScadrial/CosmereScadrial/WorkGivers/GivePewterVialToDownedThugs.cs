using System.Collections.Generic;
using System.Linq;
using CosmereResources;
using CosmereResources.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.WorkGivers;

public class GivePewterVialToDownedThugs : WorkGiver_Scanner {
    private MetalDef pewter => MetalDefOf.Pewter;
    private ThingDef vialDef => ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial;

    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

    public override Danger MaxPathDanger(Pawn pawn) {
        return Danger.None;
    }

    public override IEnumerable<Verse.Thing> PotentialWorkThingsGlobal(Pawn pawn) {
        return pawn.Map.mapPawns.FreeColonistsSpawned.Where(x => x.genes.HasAllomanticGeneForMetal(MetalDefOf.Pewter));
    }

    public override bool HasJobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        if (t is not Pawn target) return false;
        if (target == pawn) return false;
        if (target.DevelopmentalStage.Baby()) return false;
        if (target.genes == null) return false;
        if (!target.genes.HasAllomanticGeneForMetal(pewter)) return false;
        if (!pawn.CanReserve(t, ignoreOtherReservations: forced)) return false;

        Allomancer pewterGene = target.genes.GetAllomanticGeneForMetal(pewter)!;
        if (!pewterGene.ShouldConsumeVialNow()) return false;

        if (TryFindBestVialSourceFor(pawn, out _)) {
            return true;
        }

        JobFailReason.Is((string)"CS_NoVials".Translate(pewter.Named("METAL")));
        return false;
    }

    public override Job? JobOnThing(Pawn pawn, Verse.Thing t, bool forced = false) {
        if (!TryFindBestVialSourceFor(pawn, out Verse.Thing vialSource)) {
            return null;
        }

        Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_GivePatientVial, vialSource, t);
        job.count = 1;

        return job;
    }

    private bool TryFindBestVialSourceFor(
        Pawn pawn,
        out Verse.Thing vialSource
    ) {
        vialSource = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(vialDef),
            PathEndMode.Touch, TraverseParms.For(pawn), 9999f, x => VialValidator(pawn, x));

        return vialSource != null;
    }


    private bool VialValidator(Pawn pawn, Verse.Thing vial) {
        if (!vial.Spawned) return true;
        if (vial.IsForbidden(pawn)) return false;
        if (!vial.Stuff.Equals(pewter.Item)) return false;

        return pawn.CanReserveAndReach(vial, PathEndMode.ClosestTouch, Danger.None, 10, 1);
    }
}