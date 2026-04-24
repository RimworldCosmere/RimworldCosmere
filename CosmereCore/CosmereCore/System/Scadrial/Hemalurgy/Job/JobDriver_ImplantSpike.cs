using Cosmere.Core;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Utility;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Hemalurgy.Job;

public class JobDriver_ImplantSpike : Verse.AI.JobDriver {
    private const int SurgeryDuration = 100;
    private const int WaitForRecipientTimeout = 10000;

    private Pawn recipient => (Pawn)job.targetA.Thing;
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
        AddFailCondition(() => recipient.Dead);
        AddFinishAction(condition => {
                if (pawn.carryTracker.CarriedThing != null) {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Verse.Thing _);
                }
            }
        );

        bool recipientInBed = recipient.InBed();
        bool voluntary = recipient.Faction == Faction.OfPlayer &&
                         !recipient.IsPrisonerOfColony &&
                         !recipient.IsSlaveOfColony;

        if (!recipientInBed && !voluntary && !recipient.Downed) {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return MakeAnesthetizeToil();
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        } else if (!recipientInBed && recipient.Downed) {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.Touch);
            yield return Toils_Bed.TuckIntoBed(TargetIndex.C, TargetIndex.A);
        }

        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnForbidden(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);

        if (!recipientInBed && voluntary && !recipient.Downed) {
            yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
            yield return MakeWaitForRecipientToil();
        } else {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        }

        if (recipientInBed || voluntary && !recipient.Downed) {
            yield return MakeAnesthetizeToil();
        }

        yield return Toils_General.Wait(SurgeryDuration, TargetIndex.A)
            .WithProgressBarToilDelay(TargetIndex.A);

        yield return MakeImplantToil();
    }

    private Toil MakeAnesthetizeToil() {
        Toil toil = ToilMaker.MakeToil("Anesthetize");
        toil.initAction = () => {
            Verse.Hediff anesthesia = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Anesthetic, recipient);
            anesthesia.Severity = 1.0f;
            recipient.health.AddHediff(anesthesia);
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
            if (recipient.InBed()) {
                ReadyForNextToil();
                return;
            }

            if (waitedTicks > WaitForRecipientTimeout) {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.AddFailCondition(() => recipient.Dead || recipient.Downed);
        return toil;
    }

    private Toil MakeImplantToil() {
        Toil toil = ToilMaker.MakeToil("ImplantSpike");
        toil.initAction = () => {
            Verse.Thing? carried = pawn.carryTracker.CarriedThing;
            if (carried == null) {
                Logger.Warning("ImplantSpike: carried thing is null at implant time");
                return;
            }

            HemalurgicSpike? spikeComp = carried.TryGetComp<HemalurgicSpike>();
            if (spikeComp == null || !spikeComp.isCharged) {
                Logger.Warning($"ImplantSpike: spike comp null={spikeComp == null}, charged={spikeComp?.isCharged}");
                return;
            }

            if (spikeComp.chargeData!.stealType == HemalurgicStealType.RemoveAllPowers) {
                Messages.Message(
                    "CS_Hemalurgy_CannotImplantAluminum".Translate(),
                    recipient,
                    MessageTypeDefOf.RejectInput
                );
                DropSpike(carried);
                return;
            }

            HemalurgicChargeData charge = spikeComp.chargeData!;
            float strength = spikeComp.currentStrength;
            bool isThinNeedle = carried.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;

            ImplantedSpikeData spikeData = new ImplantedSpikeData {
                metalDefName = spikeComp.metal?.defName ?? "Unknown",
                stealType = charge.stealType,
                stolenDefName = charge.stolenDefName,
                stolenDefNames = [..charge.stolenDefNames],
                chargeStrength = strength,
                storedInvestiture = charge.storedInvestiture,
                isThinNeedle = isThinNeedle,
            };

            BodyPartRecord? torso = null;
            foreach (BodyPartRecord part in recipient.health.hediffSet.GetNotMissingParts()) {
                if (part.def == BodyPartDefOf.Torso) {
                    torso = part;
                    break;
                }
            }

            ApplyHemalurgicEffect(recipient, spikeData);
            AddToUnifiedHediff(recipient, spikeData, torso);
            GrantStoredInvestiture(recipient, charge.storedInvestiture);
            RemoveDrabIfPresent(recipient);
            UpdateRuinsInfluence(recipient);

            Messages.Message(
                "CS_Hemalurgy_ImplantSuccess".Translate(
                    pawn.Named("SURGEON"),
                    spikeComp.metal?.Named("METAL") ?? "unknown".Named("METAL"),
                    recipient.Named("RECIPIENT")
                ),
                recipient,
                MessageTypeDefOf.PositiveEvent
            );

            carried.Destroy();
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private void ApplyHemalurgicEffect(Pawn target, ImplantedSpikeData spike) {
        if (HemalurgicConstants.IsAllomanticSteal(spike.stealType) ||
            HemalurgicConstants.IsFeruchemicSteal(spike.stealType) ||
            spike.stealType == HemalurgicStealType.AnyPower) {
            GrantGene(target, spike.stolenDefName);
        } else if (spike.stealType == HemalurgicStealType.AllAbilities) {
            for (int i = 0; i < spike.stolenDefNames.Count; i++) {
                GrantGene(target, spike.stolenDefNames[i]);
            }
        } else if (spike.stealType == HemalurgicStealType.ConnectionIdentity) {
            if (!target.IsSnapped()) {
                SnapUtility.TrySnap(target, "CS_Hemalurgy_SnappedByConnection");
            }
        }
    }

    private void GrantGene(Pawn target, string geneDefName) {
        if (geneDefName.NullOrEmpty()) return;
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
        if (geneDef == null) return;
        if (target.genes.HasActiveGene(geneDef)) return;
        target.genes.AddGene(geneDef, true);
    }

    private void GrantStoredInvestiture(Pawn target, float amount) {
        if (amount <= 0f) return;
        InvestitureHolder? holder = target.TryGetComp<InvestitureHolder>();
        if (holder == null) return;
        holder.currentInvestitureSelf += amount;
    }

    private void RemoveDrabIfPresent(Pawn target) {
        Verse.Hediff? drab = target.health.hediffSet.GetFirstHediffOfDef(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab);
        if (drab != null) {
            target.health.RemoveHediff(drab);
        }
    }

    private void AddToUnifiedHediff(Pawn target, ImplantedSpikeData spikeData, BodyPartRecord? part) {
        HemalurgicSpikes? hediff = (HemalurgicSpikes?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (hediff == null) {
            hediff = (HemalurgicSpikes)HediffMaker.MakeHediff(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes,
                target,
                part
            );
            target.health.AddHediff(hediff, part);
        }

        hediff.AddSpike(spikeData);
    }

    private void UpdateRuinsInfluence(Pawn target) {
        HemalurgicSpikes? spikesHediff = (HemalurgicSpikes?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        int spikeCount = spikesHediff?.spikeCount ?? 0;

        RuinsInfluence? ruinsInfluence = (RuinsInfluence?)target.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence
        );
        if (ruinsInfluence == null && spikeCount > 0) {
            Verse.Hediff hediff = HediffMaker.MakeHediff(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence,
                target
            );
            target.health.AddHediff(hediff);
            ruinsInfluence = (RuinsInfluence)hediff;
        }

        ruinsInfluence?.UpdateSpikeCount(spikeCount);
    }

    private void DropSpike(Verse.Thing carried) {
        if (pawn.carryTracker.CarriedThing == carried) {
            pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
        }
    }
}