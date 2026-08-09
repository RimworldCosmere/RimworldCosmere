using Cosmere.System.Scadrial.Kandra;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Hides a vanilla humanlike node while the pawn is wearing an animal.
/// </summary>
/// <remarks>
///     <c>PawnRenderNode.AppendRequests</c> consults every subworker and returns before it
///     recurses into children, so vetoing the Body node takes the tattoo, wounds, swaddle,
///     firefoam and the whole apparel subtree with it. Three nodes need this - Body, Head and
///     Head stump - and Head stump is a sibling of Head rather than a child, so it does not come
///     along for free.
///     <para>
///         This is appended to the vanilla Humanlike tree, so it is consulted for every colonist,
///         raider and visitor in the game. It must stay a single null-checked comp lookup.
///     </para>
/// </remarks>
public class PawnRenderSubWorker_HideWhileShaped : PawnRenderSubWorker {
    public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms) {
        return parms.pawn?.TryGetComp<CompKandraForms>()?.Current?.animalKind == null;
    }
}
