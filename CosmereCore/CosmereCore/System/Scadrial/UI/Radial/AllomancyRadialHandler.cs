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
            if (abilities[i] is not AllomancyAbility a || a.def != leaf.AbilityDef) continue;

            a.UpdateStatus(BurnToggle.Next(a.status, flareShift));
            return;
        }

        Logger.Verbose($"radial dispatch: allomancy ability {leaf.LeafId} not on pawn");
    }
}
