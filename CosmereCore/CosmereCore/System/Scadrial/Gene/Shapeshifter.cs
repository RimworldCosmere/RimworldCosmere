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
///     almost every time. Being noticed is loud on purpose: the colony finding out is the story
///     beat, not a quiet stat change.
/// </remarks>
public class Shapeshifter : Verse.Gene {
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (!pawn.Spawned) return;
        if (!pawn.IsHashIntervalTick(KandraDisguise.LookInterval, delta)) return;
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
    ///     The shape drops. A kandra that has been seen is not fooling that room any more, and
    ///     holding the face afterwards would be pretending at the player rather than at anybody
    ///     in the world.
    /// </remarks>
    private static void Exposed(Pawn pawn, string key) {
        Find.LetterStack.ReceiveLetter(
            (key + "_Title").Translate(pawn.NameShortColored.Named("PAWN")),
            key.Translate(pawn.NameShortColored.Named("PAWN")),
            LetterDefOf.NegativeEvent,
            pawn
        );

        // Out of the shape, whichever kind it is.
        if (pawn.TryGetComp<CompKandraShapePair>()?.Held != null) {
            KandraAnimalShape.Revert(pawn);
            return;
        }

        KandraShapeshift.Revert(pawn);
    }
}
