using System.Collections.Generic;
using CosmereRoshar.Comps.Fabrials;
using CosmereRoshar.Comps.Gems;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Jobs;

public class ToilsRefuelFabrial {
    public static Toil SwapInNewGemstone(TargetIndex gemInd, TargetIndex fabrialInd) {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;
            Thing gemstone = pawn.CurJob.GetTarget(gemInd).Thing;


            if (IsHeatrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompHeatrial>());
                DoReplaceToil(fabrial.TryGetComp<CompHeatrial>(), gemstone);
            } else if (IsSprenTrapper(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompSprenTrapper>());
                DoReplaceToil(fabrial.TryGetComp<CompSprenTrapper>(), gemstone);
            } else if (IsBasicAugmenterFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompBasicFabrialAugumenter>());
                DoReplaceToil(fabrial.TryGetComp<CompBasicFabrialAugumenter>(), gemstone);
            } else if (IsBasicDiminisherFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompBasicFabrialDiminisher>());
                DoReplaceToil(fabrial.TryGetComp<CompBasicFabrialDiminisher>(), gemstone);
            } else if (IsFabrialPowerGenerator(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompFabrialPowerGenerator>());
                DoReplaceToil(fabrial.TryGetComp<CompFabrialPowerGenerator>(), gemstone);
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

    public static Toil RemoveGemstone(TargetIndex fabrialInd) {
        Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;

            if (IsHeatrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompHeatrial>());
            } else if (IsSprenTrapper(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompSprenTrapper>());
            } else if (IsBasicAugmenterFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompBasicFabrialAugumenter>());
            } else if (IsBasicDiminisherFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompBasicFabrialDiminisher>());
            } else if (IsFabrialPowerGenerator(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompFabrialPowerGenerator>());
            }
        };

        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }


    private static void DoReplaceToil<T>(T fabComp, Thing gemstone) where T : IGemstoneHandler {
        fabComp.AddGemstone(gemstone as ThingWithComps);
    }

    private static void DoRemoveToil<T>(T fabComp) where T : IGemstoneHandler {
        fabComp.RemoveGemstone();
    }

    private static bool IsGemstone(Thing gemstone) {
        CompCutGemstone fabComp = gemstone.TryGetComp<CompCutGemstone>();
        return fabComp != null;
    }

    private static bool IsFabrialCage(Thing gemstone) {
        CompFabrialCage fabComp = gemstone.TryGetComp<CompFabrialCage>();
        return fabComp != null;
    }

    private static bool IsHeatrial(Thing fabrial) {
        CompHeatrial fabComp = fabrial.TryGetComp<CompHeatrial>();
        return fabComp != null;
    }

    private static bool IsSprenTrapper(Thing fabrial) {
        CompSprenTrapper fabComp = fabrial.TryGetComp<CompSprenTrapper>();
        return fabComp != null;
    }

    private static bool IsBasicAugmenterFabrial(Thing fabrial) {
        CompBasicFabrialAugumenter fabComp = fabrial.TryGetComp<CompBasicFabrialAugumenter>();
        return fabComp != null;
    }

    private static bool IsBasicDiminisherFabrial(Thing fabrial) {
        CompBasicFabrialDiminisher fabComp = fabrial.TryGetComp<CompBasicFabrialDiminisher>();
        return fabComp != null;
    }

    private static bool IsFabrialPowerGenerator(Thing fabrial) {
        CompFabrialPowerGenerator fabComp = fabrial.TryGetComp<CompFabrialPowerGenerator>();
        return fabComp != null;
    }
}

public class JobDriverAddGemToFabrial : JobDriver {
    private const TargetIndex FabrialIndex = TargetIndex.A;
    private const TargetIndex GemIndex = TargetIndex.B;
    public const int ReGemmingDuration = 240;


    protected Thing fabrial => job.GetTarget(FabrialIndex).Thing;
    protected Thing gemstone => job.GetTarget(GemIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(fabrial, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(gemstone, job, 1, -1, null, errorOnFailed);
    }


    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(FabrialIndex);

        AddFailCondition(() => fabrial == null);
        AddFailCondition(() => gemstone == null);

        yield return Toils_General.DoAtomic(delegate { job.count = 1; });

        yield return Toils_Goto.GotoThing(GemIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(GemIndex)
            .FailOnSomeonePhysicallyInteracting(GemIndex);
        yield return Toils_Haul.StartCarryThing(GemIndex, false, true).FailOnDestroyedNullOrForbidden(GemIndex);

        yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
        yield return Toils_General.Wait(ReGemmingDuration)
            .FailOnDestroyedNullOrForbidden(GemIndex)
            .FailOnDestroyedNullOrForbidden(FabrialIndex)
            .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
            .WithProgressBarToilDelay(FabrialIndex);
        yield return ToilsRefuelFabrial.SwapInNewGemstone(GemIndex, FabrialIndex); //custom toil
    }
}

public class JobDriverRemoveGemFromFabrial : JobDriver {
    private const TargetIndex FabrialIndex = TargetIndex.A;
    public const int JobDuration = 75;


    protected Thing fabrial => job.GetTarget(FabrialIndex).Thing;


    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        return pawn.Reserve(fabrial, job, 1, -1, null, errorOnFailed);
    }


    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDespawnedNullOrForbidden(FabrialIndex);

        AddFailCondition(() => fabrial == null);

        yield return Toils_General.DoAtomic(delegate { job.count = 1; });

        yield return Toils_Goto.GotoThing(FabrialIndex, PathEndMode.Touch);
        yield return Toils_General.Wait(JobDuration)
            .FailOnDestroyedNullOrForbidden(FabrialIndex)
            .FailOnCannotTouch(FabrialIndex, PathEndMode.Touch)
            .WithProgressBarToilDelay(FabrialIndex);
        yield return ToilsRefuelFabrial.RemoveGemstone(FabrialIndex); //custom toil
    }
}