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

    protected IntVec3 targetCell => job.GetTarget(CellIndex).Cell;

    protected Verse.Thing? gemstone => job.GetTarget(GemIndex).Thing;

    protected SprenType? targetSprenType => job.targetC.Cell.x >= 0 ? (SprenType?)job.targetC.Cell.x : null;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (gemstone != null && !gemstone.Spawned) {
            return true; // Gem is in inventory
        }

        return gemstone != null && pawn.Reserve(gemstone, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Verse.AI.Toil> MakeNewToils() {
        AddFailCondition(() => gemstone == null);
        AddFailCondition(() => !LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(targetCell, Map).Any());

        // If gem is not in inventory, go get it
        if (gemstone != null && gemstone.Spawned) {
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
            if (Find.TickManager.TicksGame % 60 == 0) {
                // Check if spren is still available
                if (!LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(targetCell, Map).Any()) {
                    EndJobWith(JobCondition.Incompletable);
                }
            }
        };

        yield return captureToil;

        // Attempt capture
        yield return new Verse.AI.Toil {
            initAction = () => {
                if (gemstone is not ThingWithComps gem) {
                    EndJobWith(JobCondition.Errored);
                    return;
                }

                bool success;
                if (targetSprenType.HasValue) {
                    // Try to capture specific spren type
                    success = LesserSprenCaptureSystem.TryCaptureSprenWithinRadius(
                        targetCell,
                        Map,
                        gem,
                        targetSprenType.Value
                    );
                } else {
                    // Try to capture any available spren
                    success = LesserSprenCaptureSystem.TryCaptureAnySprenWithinRadius(
                        targetCell,
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
