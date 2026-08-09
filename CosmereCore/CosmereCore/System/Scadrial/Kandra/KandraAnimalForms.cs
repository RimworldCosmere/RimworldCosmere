using System.Collections.Generic;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Maps a real animal to the kandra-shaped version of it.</summary>
/// <remarks>
///     A kandra cannot become a husky, because a husky is an animal and animals take no orders.
///     It becomes a thing that looks like a husky and answers to the player, which is a separate
///     race. Those are generated at load for every animal in the game, so this is only the lookup.
/// </remarks>
public static class KandraAnimalForms {
    /// <summary>What a kandra turns into after eating this kind of animal, if anything.</summary>
    public static PawnKindDef? ShapeFor(PawnKindDef eaten) {
        return KandraShapeGenerator.Shapes.GetValueOrDefault(eaten);
    }

    public static IEnumerable<PawnKindDef> AllShapes => KandraShapeGenerator.Shapes.Values;
}
