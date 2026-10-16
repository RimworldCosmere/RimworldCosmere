using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Shows the kandra's own eyes only while it is in its own shape, so a kandra wearing somebody
///     else's face shows that face's eyes instead.
/// </summary>
/// <remarks>
///     Appended to the vanilla Humanlike tree, so this runs for every pawn in the game - it must
///     stay a single null-checked hediff lookup and must never grow a second condition.
/// </remarks>
public class PawnRenderSubWorker_ShowOnlyWhileFormless : PawnRenderSubWorker {
    public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms) {
        return KandraAppearance.IsFormless(parms.pawn);
    }
}
