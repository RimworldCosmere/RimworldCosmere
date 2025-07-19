using CosmereRoshar.Comp.Thing;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Job.Toil;

public class ResphereLamp {
    //public static Toil SwapInNewSpheres(Pawn pawn, Thing lamp, Thing sphere) {
    //    Toil toil = ToilMaker.MakeToil("SwapInNewSpheres");
    //    toil.initAction = delegate {
    //        Job curJob = toil.actor.CurJob;

    //        StormlightLamps lampComp = lamp.TryGetComp<StormlightLamps>();
    //        CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(pawn);
    //        bool dropOnGround = (spherePouch == null);
    //        if (lampComp != null) {

    //            ThingWithComps removedSphere = lampComp.RemoveSphereFromLamp(0, dropOnGround);
    //            lampComp.AddSphereToLamp(sphere as ThingWithComps);

    //            lampComp.CheckStormlightFuel();

    //            if (spherePouch != null) {
    //                spherePouch.removeSpheresFromRemoveList();
    //                spherePouch.AddSphereToPouch(removedSphere);
    //            }
    //            if (pawn.carryTracker.CarriedThing == sphere) {
    //                Thing carriedThing;
    //                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out carriedThing);
    //                if (carriedThing != null && carriedThing.Spawned) {
    //                    carriedThing.DeSpawn();
    //                }
    //            }
    //        }
    //    };
    //    toil.defaultCompleteMode = ToilCompleteMode.Instant;
    //    return toil;
    //}

    public static Verse.AI.Toil SwapInNewSpheres(TargetIndex sphereInd, TargetIndex lampInd) {
        Verse.AI.Toil toil = ToilMaker.MakeToil();


        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Verse.Thing lamp = pawn.CurJob.GetTarget(lampInd).Thing;
            Verse.Thing sphere = pawn.CurJob.GetTarget(sphereInd).Thing;

            StormlightLamps lampComp = lamp.TryGetComp<StormlightLamps>();

            if (lampComp == null) return;
            lampComp.RemoveSphereFromLamp(0);
            lampComp.AddSphereToLamp(sphere as ThingWithComps);
            lampComp.CheckStormlightFuel();
        };

        toil.AddFinishAction(
            delegate {
                Verse.Thing carriedThing = toil.actor.carryTracker.CarriedThing;
                if (carriedThing == null) return;
                toil.actor.carryTracker.TryDropCarriedThing(
                    toil.actor.Position,
                    ThingPlaceMode.Near,
                    out Verse.Thing droppedThing
                );
                if (droppedThing is { Spawned: true }) {
                    droppedThing.DeSpawn();
                }
            }
        );

        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    public static Verse.AI.Toil DropSphereFromPouch(SpherePouch pouch, TargetIndex sphereInd) {
        Verse.AI.Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            if (pawn.CurJob.GetTarget(sphereInd).Thing is not ThingWithComps sphere) return;
            pouch.RemoveSphereFromPouch(sphere, pawn.Map, pawn.Position);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}