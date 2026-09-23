using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Hides apparel and tattoos while the kandra is in its own shape.
/// </summary>
/// <remarks>
///     Vetoing a node returns before its children are visited, so the two apparel roots take every
///     worn item with them. Appended to the vanilla Humanlike tree, so this runs for every pawn in
///     the game - it must stay a single null-checked hediff lookup.
/// </remarks>
public class PawnRenderSubWorker_HideWhileFormless : PawnRenderSubWorker {
    public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms) {
        return !KandraAppearance.IsFormless(parms.pawn);
    }
}
