using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Watches a kandra's disguise hold, or not.
/// </summary>
/// <remarks>
///     A shape is only worth wearing if it can fail. Bronze finds a kandra outright and does not
///     care how good it is; everyone else gets a periodic look, and a practised kandra passes it
///     almost every time.
///     <para>
///         Being noticed costs the kandra its cover, never its shape. Which face a colonist wears
///         is the player's decision, and a kandra nobody is playing is always wearing one.
///     </para>
/// </remarks>
public class Shapeshifter : Verse.Gene {
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.Spawned) return;
        if (!pawn.IsHashIntervalTick(KandraDisguise.LookInterval, delta)) return;

        // NPC kandra never show their own face; put one on if caught bare.
        if (!pawn.IsColonist) {
            if (!KandraDisguise.IsDisguised(pawn)) KandraShapeshift.WearAnyFace(pawn);

            return;
        }

        if (!KandraDisguise.IsDisguised(pawn)) return;

        if (KandraDisguise.SeenByBronze(pawn)) {
            Exposed(pawn, "CS_Kandra_SeenByBronze");
            return;
        }

        if (KandraDisguise.Slipped(pawn)) Exposed(pawn, "CS_Kandra_Slipped");
    }

    /// <summary>
    ///     Somebody worked out what they are looking at.
    /// </summary>
    /// <remarks>
    ///     The cover goes and the shape stays. Dropping the face here read as the honest
    ///     consequence until it was played: a colonist shed a disguise the player had chosen,
    ///     mid-job, at random, with no way to refuse. The room knowing is the beat; what to do
    ///     about it belongs to whoever is playing.
    /// </remarks>
    private static void Exposed(Pawn pawn, string key) {
        CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();

        // Once per shape. Being spotted a second time by the same bronze is not a second event.
        if (forms == null || forms.CoverBlown) return;

        forms.BlowCover();

        Find.LetterStack.ReceiveLetter(
            (key + "_Title").Translate(pawn.NameShortColored.Named("PAWN")),
            key.Translate(pawn.NameShortColored.Named("PAWN")),
            LetterDefOf.NegativeEvent,
            pawn
        );
    }
}
