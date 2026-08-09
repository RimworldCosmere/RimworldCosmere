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
    /// <summary>
    ///     How many times this has hidden a human part, for the spike to report.
    /// </summary>
    /// <remarks>
    ///     The question the spike exists to answer is whether vanilla consults a subworker added
    ///     to its own tree by a patch. Eyeballing a dog cannot separate "the veto works" from "the
    ///     patch never applied", and both look identical. A counter can.
    /// </remarks>
    public static int Vetoes { get; private set; }

    public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms) {
        if (parms.pawn?.TryGetComp<CompKandraForms>()?.Current?.animalKind == null) return true;

        Vetoes++;
        return false;
    }
}
