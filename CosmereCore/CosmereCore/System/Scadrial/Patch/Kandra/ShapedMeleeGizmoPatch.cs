using Concord;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Puts the animal on the melee button instead of a human fist.
/// </summary>
/// <remarks>
///     <c>PawnAttackGizmoUtility.GetMeleeAttackGizmo</c> hardcodes
///     <c>command_Target.icon = TexCommand.AttackMelee</c>, which is a fist. That is fine for a
///     colonist and wrong for something with a muzzle, and it is the last human tell left on a
///     drafted shape after the fists themselves were taken away.
/// </remarks>
[Patch(typeof(PawnAttackGizmoUtility))]
public static class ShapedMeleeGizmoPatch {
    [Inject(At.Return, nameof(PawnAttackGizmoUtility.GetMeleeAttackGizmo))]
    private static void AfterGetMeleeAttackGizmo(Pawn pawn, ControlHandle<Verse.Gizmo> ch) {
        if (ch.ReturnValue is not Command_Target command) return;

        PawnKindDef? worn = KandraShapeGraphicUtility.WornKind(pawn);
        if (worn?.race?.uiIcon == null) return;

        command.icon = worn.race.uiIcon;
        command.iconAngle = worn.race.uiIconAngle;
        command.iconOffset = worn.race.uiIconOffset;
    }
}
