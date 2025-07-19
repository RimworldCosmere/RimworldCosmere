using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Job.Toil;

public class RefuelFabrial {
    public static Verse.AI.Toil SwapInNewGemstone(TargetIndex gemInd, TargetIndex fabrialInd) {
        Verse.AI.Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Verse.Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;
            Verse.Thing gemstone = pawn.CurJob.GetTarget(gemInd).Thing;


            if (IsHeatrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompHeatrial>());
                DoReplaceToil(fabrial.TryGetComp<CompHeatrial>(), gemstone);
            } else if (IsSprenTrapper(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<SprenTrapper>());
                DoReplaceToil(fabrial.TryGetComp<SprenTrapper>(), gemstone);
            } else if (IsBasicAugmenterFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<BasicFabrialAugmenter>());
                DoReplaceToil(fabrial.TryGetComp<BasicFabrialAugmenter>(), gemstone);
            } else if (IsBasicDiminisherFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<BasicFabrialDiminisher>());
                DoReplaceToil(fabrial.TryGetComp<BasicFabrialDiminisher>(), gemstone);
            } else if (IsFabrialPowerGenerator(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<FabrialPowerGenerator>());
                DoReplaceToil(fabrial.TryGetComp<FabrialPowerGenerator>(), gemstone);
            }
        };

        toil.AddFinishAction(
            delegate {
                Verse.Thing carriedThing = toil.actor.carryTracker.CarriedThing;
                if (carriedThing != null) {
                    toil.actor.carryTracker.TryDropCarriedThing(
                        toil.actor.Position,
                        ThingPlaceMode.Near,
                        out Verse.Thing droppedThing
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

    public static Verse.AI.Toil RemoveGemstone(TargetIndex fabrialInd) {
        Verse.AI.Toil toil = ToilMaker.MakeToil();
        toil.initAction = delegate {
            Pawn pawn = toil.actor;
            Verse.Thing fabrial = pawn.CurJob.GetTarget(fabrialInd).Thing;

            if (IsHeatrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<CompHeatrial>());
            } else if (IsSprenTrapper(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<SprenTrapper>());
            } else if (IsBasicAugmenterFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<BasicFabrialAugmenter>());
            } else if (IsBasicDiminisherFabrial(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<BasicFabrialDiminisher>());
            } else if (IsFabrialPowerGenerator(fabrial)) {
                DoRemoveToil(fabrial.TryGetComp<FabrialPowerGenerator>());
            }
        };

        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }


    private static void DoReplaceToil<T>(T fabComp, Verse.Thing gemstone) where T : IGemstoneHandler {
        fabComp.AddGemstone(gemstone as ThingWithComps);
    }

    private static void DoRemoveToil<T>(T fabComp) where T : IGemstoneHandler {
        fabComp.RemoveGemstone();
    }

    private static bool IsGemstone(Verse.Thing thing) {
        return thing.TryGetComp(out CompCutGemstone _);
    }

    private static bool IsFabrialCage(Verse.Thing thing) {
        return thing.TryGetComp(out FabrialCage _);
    }

    private static bool IsHeatrial(Verse.Thing thing) {
        return thing.TryGetComp(out CompHeatrial _);
    }

    private static bool IsSprenTrapper(Verse.Thing thing) {
        return thing.TryGetComp(out SprenTrapper _);
    }

    private static bool IsBasicAugmenterFabrial(Verse.Thing thing) {
        return thing.TryGetComp(out BasicFabrialAugmenter _);
    }

    private static bool IsBasicDiminisherFabrial(Verse.Thing thing) {
        return thing.TryGetComp(out BasicFabrialDiminisher _);
    }

    private static bool IsFabrialPowerGenerator(Verse.Thing thing) {
        return thing.TryGetComp(out FabrialPowerGenerator _);
    }
}