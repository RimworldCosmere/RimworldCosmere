using Concord;
using Cosmere.Core.Framework;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Shows the person's relationships on the body they are wearing.
/// </summary>
/// <remarks>
///     Unlike the log, this one substitutes rather than merges. A freshly generated shape has no
///     relationships of its own worth showing - nobody in the colony has an opinion about a wolf
///     that appeared this morning - so blending the two would only dilute the kandra's.
/// </remarks>
[Patch(typeof(SocialCardUtility))]
public static class KandraSocialCardPatch {
    [Inject(At.Head, nameof(SocialCardUtility.DrawSocialCard))]
    private static Control BeforeDrawSocialCard(Rect rect, Pawn pawn) {
        if (pawn == null) return Control.Continue;

        Pawn real = PawnIdentityRegistry.Real(pawn);
        if (real == pawn) return Control.Continue;

        SocialCardUtility.DrawSocialCard(rect, real);
        return Control.Cancel;
    }
}
