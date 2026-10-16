using Cosmere.Core;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.JobDriver;

public class ImplantSpike : Verse.AI.JobDriver {
    private enum DeliveryMode {
        CarryAnesthetize,
        CarryDirect,
        AwaitInBed,
    }

    private const int SurgeryDuration = 100;
    private const int WaitForRecipientTimeout = 10000;

    private Pawn Recipient => (Pawn)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed)) return false;
        if (!pawn.Reserve(job.targetB, job, 10, 1, errorOnFailed: errorOnFailed)) return false;
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        AddFailCondition(() => Recipient.Dead);
        AddFinishAction(condition => {
            if (pawn.carryTracker.CarriedThing != null) {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Verse.Thing _);
            }
        }
        );

        bool recipientInBed = Recipient.InBed();
        bool voluntary = Recipient.Faction == Faction.OfPlayer &&
                         !Recipient.IsPrisonerOfColony &&
                         !Recipient.IsSlaveOfColony;

        DeliveryMode mode;
        if (recipientInBed || (voluntary && !Recipient.Downed)) {
            mode = DeliveryMode.AwaitInBed;
        } else {
            mode = Recipient.Downed ? DeliveryMode.CarryDirect : DeliveryMode.CarryAnesthetize;
        }

        if (mode == DeliveryMode.CarryAnesthetize) {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return MakeAnesthetizeToil();
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        } else if (mode == DeliveryMode.CarryDirect) {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        }

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnForbidden(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);

        if (mode == DeliveryMode.AwaitInBed && !recipientInBed) {
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
            yield return MakeWaitForRecipientToil();
        } else {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        }

        if (mode == DeliveryMode.AwaitInBed) {
            yield return MakeAnesthetizeToil();
        }

        yield return Toils_General.Wait(SurgeryDuration, TargetIndex.A)
            .WithProgressBarToilDelay(TargetIndex.A);

        yield return MakeImplantToil();
    }

    private Toil MakeAnesthetizeToil() {
        Toil toil = ToilMaker.MakeToil("Anesthetize");
        toil.initAction = () => {
            Verse.Hediff anesthesia = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Anesthetic, Recipient);
            anesthesia.Severity = 1.0f;
            Recipient.health.AddHediff(anesthesia);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil MakeWaitForRecipientToil() {
        int waitedTicks = 0;
        Toil toil = ToilMaker.MakeToil("WaitForRecipientInBed");
        toil.initAction = () => {
            pawn.pather.StopDead();
            waitedTicks = 0;
        };
        toil.tickAction = () => {
            waitedTicks++;
            if (Recipient.InBed()) {
                ReadyForNextToil();
                return;
            }

            if (waitedTicks > WaitForRecipientTimeout) {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.AddFailCondition(() => Recipient.Dead || Recipient.Downed);
        return toil;
    }

    private Toil MakeImplantToil() {
        Toil toil = ToilMaker.MakeToil("ImplantSpike");
        toil.initAction = () => {
            Verse.Thing? carried = pawn.carryTracker.CarriedThing;
            if (carried == null) {
                Log.Warn("ImplantSpike: carried thing is null at implant time");
                return;
            }

            HemalurgicSpike? spikeComp = carried.TryGetComp<HemalurgicSpike>();
            if (spikeComp == null || !spikeComp.IsCharged) {
                Log.Warn($"ImplantSpike: spike comp null={spikeComp == null}, charged={spikeComp?.IsCharged}");
                return;
            }

            if (spikeComp.chargeData!.stealType == HemalurgicStealType.RemoveAllPowers) {
                Messages.Message(
                    "CS_Hemalurgy_CannotImplantAluminum".Translate(),
                    Recipient,
                    MessageTypeDefOf.RejectInput
                );
                DropSpike(carried);
                return;
            }

            HemalurgicChargeData charge = spikeComp.chargeData;
            float strength = spikeComp.CurrentStrength;
            bool isThinNeedle = carried.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;

            ImplantedSpikeData spikeData = new ImplantedSpikeData {
                metalDefName = spikeComp.Metal?.defName ?? "Unknown",
                stealType = charge.stealType,
                stolenDefName = charge.stolenDefName,
                stolenDefNames = [.. charge.stolenDefNames],
                chargeStrength = strength,
                storedInvestiture = charge.storedInvestiture,
                isThinNeedle = isThinNeedle,
            };

            BodyPartRecord? torso = null;
            foreach (BodyPartRecord part in Recipient.health.hediffSet.GetNotMissingParts()) {
                if (part.def == pawn.RaceProps.body.corePart.def) {
                    torso = part;
                    break;
                }
            }

            HemalurgicImplantUtility.ApplyHemalurgicEffect(Recipient, spikeData);
            HemalurgicImplantUtility.AddToUnifiedHediff(Recipient, spikeData, torso);
            HemalurgicImplantUtility.GrantStoredInvestiture(Recipient, charge.storedInvestiture);
            HemalurgicImplantUtility.RemoveDrabIfPresent(Recipient);
            HemalurgicImplantUtility.UpdateRuinsInfluence(Recipient);

            Messages.Message(
                "CS_Hemalurgy_ImplantSuccess".Translate(
                    pawn.Named("SURGEON"),
                    spikeComp.Metal?.Named("METAL") ?? "unknown".Named("METAL"),
                    Recipient.Named("RECIPIENT")
                ),
                Recipient,
                MessageTypeDefOf.PositiveEvent
            );

            carried.Destroy();
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
