using System.Collections.Generic;
using CosmereRoshar.Comp.Apparel;
using CosmereRoshar.Comp.Furniture;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Jobs;

public class ToilsResphereLamp {
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

    public static Toil SwapInNewSpheres(TargetIndex sphereInd, TargetIndex lampInd) {
        Toil toil = ToilMaker.MakeToil();


        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Thing lamp = pawn.CurJob.GetTarget(lampInd).Thing;
            Thing sphere = pawn.CurJob.GetTarget(sphereInd).Thing;

            StormlightLamps lampComp = lamp.TryGetComp<StormlightLamps>();

            if (lampComp != null) {
                ThingWithComps removedSphere = lampComp.RemoveSphereFromLamp(0);
                lampComp.AddSphereToLamp(sphere as ThingWithComps);
                lampComp.CheckStormlightFuel();
            }
        };

        toil.AddFinishAction(
            delegate {
                Thing carriedThing = toil.actor.carryTracker.CarriedThing;
                if (carriedThing != null) {
                    toil.actor.carryTracker.TryDropCarriedThing(
                        toil.actor.Position,
                        ThingPlaceMode.Near,
                        out Thing droppedThing
                    );
                    if (droppedThing != null && droppedThing.Spawned) {
                        droppedThing.DeSpawn();
                    }
                }
            }
        );

        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    public static Toil DropSphereFromPouch(CompSpherePouch pouch, TargetIndex sphereInd) {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Job curJob = toil.actor.CurJob;
            Pawn pawn = toil.actor;
            Thing sphere = pawn.CurJob.GetTarget(sphereInd).Thing;
            pouch.RemoveSphereFromPouch(sphere, pawn.Map, pawn.Position);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}

public class JobDriverAddSphereToLamp : JobDriver_Refuel {
    private const TargetIndex LampIndex = TargetIndex.A;
    private const TargetIndex SphereIndex = TargetIndex.B;

    public const int RespheringDuration = 240;

    protected StormlightLamps lampComp => lamp.TryGetComp<StormlightLamps>();

    protected Thing lamp => job.GetTarget(LampIndex).Thing;
    protected Thing sphere => job.GetTarget(SphereIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(lamp, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(sphere, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(LampIndex);

        AddFailCondition(() => sphere == null);
        AddFailCondition(() => lamp == null);

        yield return Toils_General.DoAtomic(delegate { job.count = lampComp.GetNumberOfSpheresToReplace(); });

        if (IsSphereInPouch()) {
            yield return ToilsResphereLamp.DropSphereFromPouch(GetSpherePouch(), SphereIndex);
        }

        yield return Toils_Goto.GotoThing(SphereIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(SphereIndex)
            .FailOnSomeonePhysicallyInteracting(SphereIndex);
        yield return Toils_Haul.StartCarryThing(SphereIndex, false, true).FailOnDestroyedNullOrForbidden(SphereIndex);

        yield return Toils_Goto.GotoThing(LampIndex, PathEndMode.Touch);
        yield return Toils_General.Wait(RespheringDuration)
            .FailOnDestroyedNullOrForbidden(SphereIndex)
            .FailOnDestroyedNullOrForbidden(LampIndex)
            .FailOnCannotTouch(LampIndex, PathEndMode.Touch)
            .WithProgressBarToilDelay(LampIndex);
        yield return ToilsResphereLamp.SwapInNewSpheres(SphereIndex, LampIndex); //custom toil
    }

    private bool IsSphereInPouch() {
        CompSpherePouch spherePouch = CompSpherePouch.GetWornSpherePouch(pawn);
        if (spherePouch != null && spherePouch.PouchContainsSpecificSphere(sphere)) {
            return true;
        }

        return false;
    }

    private CompSpherePouch GetSpherePouch() {
        return CompSpherePouch.GetWornSpherePouch(pawn);
    }
}