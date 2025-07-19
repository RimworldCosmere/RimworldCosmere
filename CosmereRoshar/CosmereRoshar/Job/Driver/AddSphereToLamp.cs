using System.Collections.Generic;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Job.Toil;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Job.Driver;

public class AddSphereToLamp : JobDriver_Refuel {
    private const TargetIndex LampIndex = TargetIndex.A;
    private const TargetIndex SphereIndex = TargetIndex.B;

    public const int RespheringDuration = 240;

    protected StormlightLamps lampComp => lamp.TryGetComp<StormlightLamps>();

    protected Verse.Thing? lamp => job.GetTarget(LampIndex).Thing;
    protected Verse.Thing? sphere => job.GetTarget(SphereIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(lamp, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(sphere, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(LampIndex);

        AddFailCondition(() => sphere == null);
        AddFailCondition(() => lamp == null);

        yield return Toils_General.DoAtomic(delegate { job.count = lampComp.GetNumberOfSpheresToReplace(); });

        if (IsSphereInPouch()) {
            yield return ResphereLamp.DropSphereFromPouch(GetSpherePouch(), SphereIndex);
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
        yield return ResphereLamp.SwapInNewSpheres(SphereIndex, LampIndex); //custom toil
    }

    private bool IsSphereInPouch() {
        SpherePouch spherePouch = SpherePouch.GetWornSpherePouch(pawn);
        if (spherePouch != null && spherePouch.PouchContainsSpecificSphere(sphere)) {
            return true;
        }

        return false;
    }

    private SpherePouch GetSpherePouch() {
        return SpherePouch.GetWornSpherePouch(pawn);
    }
}