using Cosmere.Core;
using Cosmere.Core.Ability;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Allomancy.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.UI.Radial;

public sealed class AllomancyRadialHandler : IRadialActionHandler {
    public bool CanHandle(RadialActionKind kind) {
        return kind == RadialActionKind.StartAllomancyBurn;
    }

    public void Dispatch(Pawn pawn, RadialLeaf leaf, string subsystemId, bool flareShift) {
        if (pawn.abilities == null) {
            Logger.Verbose($"radial dispatch: allomancy metal {subsystemId} skipped - pawn has no abilities");
            return;
        }

        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is not AllomancyAbility a || a.metal.defName != subsystemId) continue;
            Status next;
            if (flareShift) {
                // Steps down to Burning rather than Off, matching the dock. Dropping straight
                // to Off pulls the aura hediff and kills the metal lines mid-flare.
                next = a.status.power > 1 ? BurningStatus.Burning : BurningStatus.Flaring;
            } else {
                next = a.atLeastBurning ? BurningStatus.Off : BurningStatus.Burning;
            }

            a.UpdateStatus(next);
            return;
        }

        Logger.Verbose($"radial dispatch: allomancy metal {subsystemId} not on pawn");
    }
}
