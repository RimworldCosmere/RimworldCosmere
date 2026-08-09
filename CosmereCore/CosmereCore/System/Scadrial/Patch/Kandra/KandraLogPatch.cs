using Concord;
using Cosmere.Core.Framework;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Shows the person's history on the body they are wearing.
/// </summary>
/// <remarks>
///     Log entries live in one global list and hold pawn references, so they cannot be moved onto
///     a shape the way skills can. The tab filters that list per pawn instead, which makes this
///     the one place worth teaching.
///     <para>
///         Both halves are shown rather than one. A kandra in a wolf remembers the fight it had
///         as itself, and the fight it just had as the wolf; picking either alone would drop
///         something that really happened.
///     </para>
/// </remarks>
[Patch(typeof(ITab_Pawn_Log_Utility))]
public static class KandraLogPatch {
    /// <summary>Stops the appended call from re-entering this injection forever.</summary>
    [global::System.ThreadStatic]
    private static bool inside;

    [Inject(At.Return, nameof(ITab_Pawn_Log_Utility.GenerateLogLinesFor))]
    private static void AfterGenerateLogLinesFor(
        Pawn pawn,
        bool showAll,
        bool showCombat,
        bool showSocial,
        int maxLines,
        ControlHandle<List<ITab_Pawn_Log_Utility.LogLineDisplayable>> ch
    ) {
        if (inside) return;
        if (pawn == null || ch.ReturnValue == null) return;

        Pawn real = PawnIdentityRegistry.Real(pawn);
        if (real == pawn) return;

        inside = true;
        try {
            List<ITab_Pawn_Log_Utility.LogLineDisplayable> theirs =
                ITab_Pawn_Log_Utility.GenerateLogLinesFor(real, showAll, showCombat, showSocial, maxLines);

            ch.ReturnValue.AddRange(theirs);
        } finally {
            inside = false;
        }
    }
}
