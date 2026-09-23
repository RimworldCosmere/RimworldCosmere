using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class ChargeCorpseSpike : Verse.AI.JobDriver {
    private const int WorkDuration = 1500;

    private Corpse corpse => (Corpse)job.targetA.Thing;

    private Verse.Thing spike => job.targetB.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.Reserve(job.targetB, job, 10, 1, errorOnFailed: errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        AddFailCondition(() => corpse.Age > HemalurgicConstants.CorpseFreshnessTickLimit);

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnForbidden(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        yield return Toils_General.Wait(WorkDuration)
            .WithProgressBarToilDelay(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);

        Toil applyCharge = ToilMaker.MakeToil("ApplyCorpseCharge");
        applyCharge.initAction = ApplyCorpseCharge;
        applyCharge.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return applyCharge;
    }

    private void ApplyCorpseCharge() {
        Verse.Thing? carried = pawn.carryTracker.CarriedThing;
        if (carried == null) return;

        HemalurgicSpike? spikeComp = carried.TryGetComp<HemalurgicSpike>();
        if (spikeComp == null || spikeComp.isCharged) return;

        Pawn donor = corpse.InnerPawn;
        MetallicArtsMetalDef? metal = spikeComp.metal;
        if (metal == null) return;
        HemalurgicStealType stealType = HemalurgicConstants.GetStealType(metal);
        bool isThinNeedle = carried.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
        float strengthMultiplier = HemalurgicConstants.CorpseChargeMultiplier;
        if (isThinNeedle) {
            strengthMultiplier *= HemalurgicConstants.ThinNeedleStrengthMultiplier;
        }

        strengthMultiplier *= HemalurgicConstants.GetDonorAttributeMultiplier(donor, stealType);

        if (!HemalurgicConstants.RequiresSelection(stealType)) {
            HemalurgicChargeUtility.DriveHemalurgicCharge(
                donor, pawn, spikeComp, stealType, null, strengthMultiplier, false, isThinNeedle
            );
            DropSpikeAndEnd(carried);
            return;
        }

        List<GeneDef> candidates = StealTargetSelector.GetStealCandidates(donor, stealType);
        if (candidates.Count == 0) {
            Messages.Message(
                "CS_Hemalurgy_NothingToSteal".Translate(donor.Named("DONOR")),
                corpse,
                MessageTypeDefOf.RejectInput
            );
            DropSpikeAndEnd(carried);
            return;
        }

        if (candidates.Count == 1) {
            HemalurgicChargeUtility.DriveHemalurgicCharge(
                donor, pawn, spikeComp, stealType, candidates[0], strengthMultiplier, false, isThinNeedle
            );
            DropSpikeAndEnd(carried);
            return;
        }

        StealTargetSelector.ShowSelectionDialog(
            donor,
            stealType,
            geneDef => {
                HemalurgicChargeUtility.DriveHemalurgicCharge(
                    donor, pawn, spikeComp, stealType, geneDef, strengthMultiplier, false, isThinNeedle
                );
                DropSpikeAndEnd(carried);
            },
            () => {
                Messages.Message(
                    "CS_Hemalurgy_NothingToSteal".Translate(donor.Named("DONOR")),
                    corpse,
                    MessageTypeDefOf.RejectInput
                );
                DropSpikeAndEnd(carried);
            }
        );
    }

    private void DropSpikeAndEnd(Verse.Thing carried) {
        if (pawn.carryTracker.CarriedThing == carried) {
            pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
        }
    }
}
