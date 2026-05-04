using Cosmere.Core;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.UI.Radial;

public sealed class FeruchemyRadialHandler : IRadialActionHandler {
    public bool CanHandle(RadialActionKind kind) {
        return kind == RadialActionKind.ToggleFeruchemyTap
            || kind == RadialActionKind.ToggleFeruchemyStore
            || kind == RadialActionKind.ResetFeruchemyIdle;
    }

    public void Dispatch(Pawn pawn, RadialLeaf leaf, string subsystemId, bool flareShift) {
        if (pawn.genes == null) {
            Logger.Verbose($"radial dispatch: feruchemy metal {subsystemId} skipped - pawn has no genes");
            return;
        }

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.metal.defName != subsystemId) continue;
            switch (leaf.Kind) {
                case RadialActionKind.ToggleFeruchemyTap:
                    f.targetValue = 75f;
                    return;
                case RadialActionKind.ToggleFeruchemyStore:
                    f.targetValue = 25f;
                    return;
                case RadialActionKind.ResetFeruchemyIdle:
                    f.Reset();
                    return;
            }
        }

        Logger.Verbose($"radial dispatch: feruchemy metal {subsystemId} not on pawn");
    }
}
