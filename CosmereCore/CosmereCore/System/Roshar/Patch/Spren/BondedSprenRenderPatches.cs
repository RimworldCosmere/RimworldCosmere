using Concord;
using Cosmere.Core.Patch;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class BondedSprenBaseHeadOffsetPatch : PawnRenderer {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected BondedSprenBaseHeadOffsetPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Head, nameof(BaseHeadOffsetAt))]
    private Control BeforeBaseHeadOffsetAt(ControlHandle<Vector3> ch) {
        Pawn? pawn = trackedPawn;
        if (pawn?.story == null || pawn.ageTracker?.CurLifeStage == null) {
            ch.ReturnValue = Vector3.zero;
            return Control.Cancel;
        }

        if (pawn.story.bodyType == null && pawn.RaceProps?.Humanlike == true) {
            pawn.story.bodyType = PawnBodyTypeFallbackPatch.FallbackBodyTypeFor(pawn);
        }

        if (pawn.story.bodyType == null) {
            ch.ReturnValue = Vector3.zero;
            return Control.Cancel;
        }

        return Control.Continue;
    }
}

[Patch]
public abstract class BondedSprenApparelGetDynamicNodesPatch : DynamicPawnRenderNodeSetup_Apparel {
    [Inject(At.Head, nameof(GetDynamicNodes))]
    private Control BeforeGetDynamicNodes(
        Pawn pawn,
        ControlHandle<IEnumerable<(PawnRenderNode node, PawnRenderNode parent)>> ch
    ) {
        if (pawn?.story?.bodyType != null) return Control.Continue;
        ch.ReturnValue = [];
        return Control.Cancel;
    }
}
