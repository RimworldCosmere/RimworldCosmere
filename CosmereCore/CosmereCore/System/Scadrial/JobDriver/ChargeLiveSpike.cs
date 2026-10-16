using Cosmere.Core;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class ChargeLiveSpike : Verse.AI.JobDriver {
    private const int SurgeryDuration = 100;
    private const int WaitForDonorTimeout = 10000;

    private Pawn Donor => (Pawn)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.Reserve(job.targetB, job, 10, 1, errorOnFailed: errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        AddFailCondition(() => Donor.Dead);
        AddFinishAction(condition => {
            if (pawn.carryTracker.CarriedThing != null) {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Verse.Thing _);
            }
        }
        );

        bool donorInBed = Donor.InBed();
        bool voluntary = Donor.Faction == Faction.OfPlayer && !Donor.IsPrisonerOfColony && !Donor.IsSlaveOfColony;

        if (!donorInBed && !voluntary && !Donor.Downed) {
            // Involuntary and conscious: anesthetize, carry to bed
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return MakeAnesthetizeToil();
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        } else if (!donorInBed && Donor.Downed) {
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

        if (!donorInBed && voluntary && !Donor.Downed) {
            // Voluntary: donor is walking to bed via WaitInBed job, go wait at bed
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
            yield return MakeWaitForDonorToil();
        } else {
            // Donor is in bed already
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        }

        // Anesthetize (not needed for involuntary - already done above)
        if (donorInBed || voluntary && !Donor.Downed) {
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
            Verse.Hediff anesthesia = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Anesthetic, Donor);
            anesthesia.Severity = 1.0f;
            Donor.health.AddHediff(anesthesia);
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
            if (Donor.InBed()) {
                ReadyForNextToil();
                return;
            }

            if (waitedTicks > WaitForDonorTimeout) {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.AddFailCondition(() => Donor.Dead || Donor.Downed);
        return toil;
    }

    private Toil MakeApplyChargeToil() {
        Toil toil = ToilMaker.MakeToil("ApplyLiveCharge");
        toil.initAction = () => {
            Verse.Thing? carried = pawn.carryTracker.CarriedThing;
            if (carried == null) {
                Log.Warn("ChargeLiveSpike: carried thing is null at charge time");
                return;
            }

            HemalurgicSpike? spikeComp = carried.TryGetComp<HemalurgicSpike>();
            if (spikeComp == null || spikeComp.IsCharged) {
                Log.Warn($"ChargeLiveSpike: spike comp null={spikeComp == null}, charged={spikeComp?.IsCharged}");
                return;
            }

            MetallicArtsMetalDef? metal = spikeComp.Metal;
            if (metal == null) return;
            HemalurgicStealType stealType = HemalurgicConstants.GetStealType(metal);
            bool isThinNeedle = carried.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
            float strengthMultiplier = 1f;
            if (isThinNeedle) {
                strengthMultiplier *= HemalurgicConstants.ThinNeedleStrengthMultiplier;
            }

            strengthMultiplier *= HemalurgicConstants.GetDonorAttributeMultiplier(Donor, stealType);

            GeneDef? selectedGene = spikeComp.pendingStealTarget;
            spikeComp.pendingStealTarget = null;

            HemalurgicChargeUtility.DriveHemalurgicCharge(
                Donor,
                pawn,
                spikeComp,
                stealType,
                selectedGene,
                strengthMultiplier,
                true,
                isThinNeedle
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
