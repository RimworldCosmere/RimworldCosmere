using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.Job;

public class JobDriver_ChargeLiveSpike : Verse.AI.JobDriver {
    private const int SurgeryDuration = 100;
    private const int WaitForDonorTimeout = 10000;

    private Pawn donor => (Pawn)job.targetA.Thing;
    private Verse.Thing spike => job.targetB.Thing;
    private Building_Bed? bed => job.targetC.Thing as Building_Bed;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.Reserve(job.targetB, job, 10, 1, errorOnFailed: errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        AddFailCondition(() => donor.Dead);
        AddFinishAction((JobCondition condition) => {
            if (pawn.carryTracker.CarriedThing != null) {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Verse.Thing _);
            }
        });

        bool donorInBed = donor.InBed();
        bool voluntary = donor.Faction == Faction.OfPlayer
                         && !donor.IsPrisonerOfColony
                         && !donor.IsSlaveOfColony;

        if (!donorInBed && !voluntary && !donor.Downed) {
            // Involuntary and conscious: anesthetize, carry to bed
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return MakeAnesthetizeToil();
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        } else if (!donorInBed && donor.Downed) {
            // Already downed: just carry to bed
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        }

        // Get spike
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnForbidden(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);

        if (!donorInBed && voluntary && !donor.Downed) {
            // Voluntary: donor is walking to bed via WaitInBed job, go wait at bed
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
            yield return MakeWaitForDonorToil();
        } else {
            // Donor is in bed already
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        }

        // Anesthetize (not needed for involuntary - already done above)
        if (donorInBed || (voluntary && !donor.Downed)) {
            yield return MakeAnesthetizeToil();
        }

        // Surgery
        yield return Toils_General.Wait(SurgeryDuration, TargetIndex.A)
            .WithProgressBarToilDelay(TargetIndex.A);

        // Apply charge
        yield return MakeApplyChargeToil();
    }

    private Toil MakeAnesthetizeToil() {
        Toil toil = ToilMaker.MakeToil("Anesthetize");
        toil.initAction = () => {
            Verse.Hediff anesthesia = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Anesthetic, donor);
            anesthesia.Severity = 1.0f;
            donor.health.AddHediff(anesthesia);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil MakeWaitForDonorToil() {
        int waitedTicks = 0;
        Toil toil = ToilMaker.MakeToil("WaitForDonorInBed");
        toil.initAction = () => {
            pawn.pather.StopDead();
            waitedTicks = 0;
        };
        toil.tickAction = () => {
            waitedTicks++;
            if (donor.InBed()) {
                ReadyForNextToil();
                return;
            }
            if (waitedTicks > WaitForDonorTimeout) {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.AddFailCondition(() => donor.Dead || donor.Downed);
        return toil;
    }

    private Toil MakeApplyChargeToil() {
        Toil toil = ToilMaker.MakeToil("ApplyLiveCharge");
        toil.initAction = () => {
            Verse.Thing? carried = pawn.carryTracker.CarriedThing;
            if (carried == null) {
                Core.Logger.Warning("ChargeLiveSpike: carried thing is null at charge time");
                return;
            }

            HemalurgicSpike? spikeComp = carried.TryGetComp<HemalurgicSpike>();
            if (spikeComp == null || spikeComp.isCharged) {
                Core.Logger.Warning($"ChargeLiveSpike: spike comp null={spikeComp == null}, charged={spikeComp?.isCharged}");
                return;
            }

            MetallicArtsMetalDef? metal = spikeComp.metal;
            if (metal == null) return;
            HemalurgicStealType stealType = HemalurgicConstants.GetStealType(metal);
            bool isThinNeedle = carried.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
            float strengthMultiplier = 1f;
            if (isThinNeedle) {
                strengthMultiplier *= HemalurgicConstants.ThinNeedleStrengthMultiplier;
            }
            strengthMultiplier *= HemalurgicConstants.GetDonorAttributeMultiplier(donor, stealType);

            GeneDef? selectedGene = spikeComp.pendingStealTarget;
            spikeComp.pendingStealTarget = null;

            HemalurgicChargeUtility.PerformCharge(
                donor, pawn, spikeComp, stealType, selectedGene,
                strengthMultiplier, true, isThinNeedle
            );
            DropSpike(carried);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private void DropSpike(Verse.Thing carried) {
        if (pawn.carryTracker.CarriedThing == carried) {
            pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
        }
    }
}
