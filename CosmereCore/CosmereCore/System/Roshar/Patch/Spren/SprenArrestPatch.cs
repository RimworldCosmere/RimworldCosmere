using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Spren;

/// <summary>
///     Keeps a bonded spren from being arrested.
/// </summary>
/// <remarks>
///     A bonded spren is humanlike so its Radiant can talk to it, and vanilla reads humanlike as
///     arrestable. Jailing one stuck it there for good: the prisoner tab has nothing to offer a
///     creature with no needs, no faction of its own and no interest in being recruited, so there
///     was no way to let it out again.
///     <para>
///         Patched at <c>GenAI.CanBeArrestedBy</c> rather than at the float menu, because
///         <c>JobDriver_TakeToBed</c> reads the same method and would otherwise still carry one to
///         a cell.
///     </para>
/// </remarks>
[Patch(typeof(GenAI))]
public static class SprenArrestPatch {
    [Inject(At.Return, nameof(GenAI.CanBeArrestedBy))]
    private static void AfterCanBeArrestedBy(Pawn pawn, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (pawn?.TryGetComp<SprenBond>() == null) return;

        ch.ReturnValue = false;
    }
}
