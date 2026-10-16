using Concord;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch(typeof(AttackTargetFinder))]
public static class InvisibilityTargetingPatch {
    [Inject(At.Return, nameof(AttackTargetFinder.IsAutoTargetable))]
    private static void AfterIsAutoTargetable(IAttackTarget target, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;

        if (target.Thing is Pawn targetPawn && Invisibility.InvisiblePawns.Contains(targetPawn)) {
            ch.ReturnValue = false;
        }
    }
}

[Patch]
public abstract class InvisibilityBreakOnAttackPatch : Verb {
    [Inject(At.Head, nameof(TryCastNextBurstShot))]
    private void BeforeTryCastNextBurstShot() {
        Verb self = this;
        if (self.CasterPawn == null) return;

        Pawn casterPawn = self.CasterPawn;
        if (!Invisibility.InvisiblePawns.Contains(casterPawn)) return;

        Ability? invisAbility = casterPawn.abilities?.GetAbility(
            AbilityDefOf.Cosmere_Roshar_Ability_Invisibility
        );
        if (invisAbility is Invisibility { status.IsActive: true } invis) {
            invis.UpdateStatus(Active.Off);
        }
    }
}
