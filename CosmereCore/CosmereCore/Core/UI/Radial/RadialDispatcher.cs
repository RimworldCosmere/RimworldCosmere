using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialDispatcher {
    public static void Dispatch(Pawn pawn, RadialLeaf leaf, string subsystemId, bool flareShift) {
        if (leaf.IsLocked) return;

        if (leaf.Kind == RadialActionKind.CastAbility) {
            CastOrToggle(pawn, leaf.AbilityDef);
            return;
        }

        IReadOnlyList<IRadialActionHandler> handlers = RadialActionRegistry.All;
        for (int i = 0; i < handlers.Count; i++) {
            if (handlers[i].CanHandle(leaf.Kind)) {
                handlers[i].Dispatch(pawn, leaf, subsystemId, flareShift);
                return;
            }
        }

        Logger.Verbose($"radial dispatch: no handler registered for action kind {leaf.Kind}");
    }

    /// <summary>
    ///     Public because the wheel is not the only way to reach an ability: the dock's Surge panel
    ///     lists them too, and both must obey the same toggle, affordability, and targeting rules.
    /// </summary>
    public static void CastOrToggle(Pawn pawn, AbilityDef? def) {
        if (def == null) return;
        if (pawn.abilities == null) return;

        RimWorld.Ability? ability = pawn.abilities.GetAbility(def);
        if (ability == null) return;

        // turning off skips CanCast: an unaffordable toggle is exactly the one that most needs to turn off.
        if (ability is IToggleableAbility { IsToggleable: true, IsActive: true } toggle) {
            toggle.TurnOff();
            return;
        }

        if (!ability.CanCast) return;

        if (def.targetRequired) {
            Find.Targeter.BeginTargeting(ability.verb);
            return;
        }

        ability.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
    }
}
