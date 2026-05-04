using Cosmere.System.Roshar.LesserSpren.CaptureSystem;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Comp.Thing;

public class SprenContainer : ThingComp {
    private SprenType? capturedSprenType;

    public bool hasCapturedSpren => capturedSprenType.HasValue;
    public SprenType? CapturedSprenType => capturedSprenType;

    public void CaptureSpren(SprenType sprenType) {
        capturedSprenType = sprenType;
    }

    public void ReleaseSpren() {
        capturedSprenType = null;
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Values.Look(ref capturedSprenType, "capturedSprenType");
    }

    public override string? CompInspectStringExtra() {
        if (hasCapturedSpren) {
            return $"Contains: {capturedSprenType} spren";
        }

        return null;
    }

    public override bool AllowStackWith(Verse.Thing other) {
        SprenContainer? otherComp = other.TryGetComp<SprenContainer>();
        if (otherComp == null) return true;

        // Only allow stacking if both have the same spren type (or both empty)
        return capturedSprenType == otherComp.capturedSprenType;
    }

    public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra() {
        if (Core.Mod.debugMode) {
            yield return new Command_Action {
                defaultLabel = "Debug: Test Spren Capture",
                defaultDesc = "Test the spren capture system at this position",
                icon = TexCommand.DesirePower,
                action = () => {
                    IntVec3 position = parent.Position;
                    Verse.Map? map = parent.Map;
                    if (map == null) return;

                    Pawn? selectedPawn = Find.Selector.SingleSelectedThing as Pawn;
                    if (selectedPawn == null) {
                        Messages.Message("Select a pawn first", MessageTypeDefOf.RejectInput);
                        return;
                    }

                    List<BaseSprenController> capturable = LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(
                        position,
                        map
                    );

                    Messages.Message(
                        $"Found {capturable.Count} capturable spren types at {position}: {string.Join(", ", capturable)}",
                        MessageTypeDefOf.NeutralEvent
                    );
                },
            };
        }
    }
}