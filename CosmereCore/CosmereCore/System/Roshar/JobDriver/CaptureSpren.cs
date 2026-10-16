using Cosmere.System.Roshar.LesserSpren.CaptureSystem;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.JobDriver;

public class CaptureSpren : Verse.AI.JobDriver {
    private const TargetIndex CellIndex = TargetIndex.A;
    private const TargetIndex GemIndex = TargetIndex.B;
    private const int CaptureDuration = 180; // 3 seconds

    protected IntVec3 TargetCell => job.GetTarget(CellIndex).Cell;

    protected Verse.Thing? Gemstone => job.GetTarget(GemIndex).Thing;

    protected SprenType? TargetSprenType => job.targetC.Cell.x >= 0 ? (SprenType?)job.targetC.Cell.x : null;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (Gemstone != null && !Gemstone.Spawned) {
            return true; // Gem is in inventory
        }

        return Gemstone != null && pawn.Reserve(Gemstone, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFailCondition(() => Gemstone == null);
        AddFailCondition(() => !LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(TargetCell, Map).Any());

        // If gem is not in inventory, go get it
        if (Gemstone != null && Gemstone.Spawned) {
            yield return Toils_Goto.GotoThing(GemIndex, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(GemIndex)
                .FailOnSomeonePhysicallyInteracting(GemIndex);
            yield return Toils_Haul.StartCarryThing(GemIndex, false, true)
                .FailOnDestroyedNullOrForbidden(GemIndex);
        }

        // Move to capture location
        yield return Toils_Goto.GotoCell(CellIndex, PathEndMode.OnCell);

        // Capture animation/delay
        Verse.AI.Toil captureToil = Toils_General.Wait(CaptureDuration, CellIndex)
            .WithProgressBarToilDelay(CellIndex);

        captureToil.AddPreTickAction(() => {
            if (pawn.IsHashIntervalTick(60)) {
                FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
            }
        }
        );

        captureToil.tickAction = () => {
            // Check if spren is still available
            if (Find.TickManager.TicksGame % 60 == 0 &&
                !LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(TargetCell, Map).Any()) {
                EndJobWith(JobCondition.Incompletable);
            }
        };

        yield return captureToil;

        // Attempt capture
        yield return new Verse.AI.Toil {
            initAction = () => {
                if (Gemstone is not ThingWithComps gem) {
                    EndJobWith(JobCondition.Errored);
                    return;
                }

                bool success;
                if (TargetSprenType.HasValue) {
                    // Try to capture specific spren type
                    success = LesserSprenCaptureSystem.TryCaptureSprenWithinRadius(
                        TargetCell,
                        Map,
                        gem,
                        TargetSprenType.Value
                    );
                } else {
                    // Try to capture any available spren
                    success = LesserSprenCaptureSystem.TryCaptureAnySprenWithinRadius(
                        TargetCell,
                        Map,
                        gem
                    );
                }

                if (!success) {
                    Messages.Message(
                        "SprenCapture_Failed".Translate(pawn.Name.ToStringShort),
                        pawn,
                        MessageTypeDefOf.NegativeEvent
                    );
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant,
        };
    }
}
