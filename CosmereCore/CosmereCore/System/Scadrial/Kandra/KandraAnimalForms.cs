using System.Collections.Generic;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Maps a real animal to the kandra-shaped version of it.</summary>
/// <remarks>
///     A kandra cannot become a husky, because a husky is an animal and animals take no orders.
///     It becomes a thing that looks like a husky and answers to the player, which is a separate
///     pawnkind. This is the lookup between the two, and the list of shapes we actually support.
/// </remarks>
public static class KandraAnimalForms {
    private static Dictionary<PawnKindDef, PawnKindDef>? shapes;

    /// <summary>What a kandra turns into after eating this kind of animal, if anything.</summary>
    public static PawnKindDef? ShapeFor(PawnKindDef eaten) {
        shapes ??= Build();

        return shapes.GetValueOrDefault(eaten);
    }

    public static IEnumerable<PawnKindDef> AllShapes => (shapes ??= Build()).Values;

    private static Dictionary<PawnKindDef, PawnKindDef> Build() {
        Dictionary<PawnKindDef, PawnKindDef> map = [];

        Add(map, "Husky", "Cosmere_Scadrial_PawnKind_KandraWolfhound");
        Add(map, "LabradorRetriever", "Cosmere_Scadrial_PawnKind_KandraWolfhound");
        Add(map, "Wolf_Timber", "Cosmere_Scadrial_PawnKind_KandraWolfhound");

        return map;
    }

    private static void Add(Dictionary<PawnKindDef, PawnKindDef> map, string eaten, string shape) {
        PawnKindDef? from = DefDatabase<PawnKindDef>.GetNamedSilentFail(eaten);
        PawnKindDef? to = DefDatabase<PawnKindDef>.GetNamedSilentFail(shape);
        if (from == null || to == null) return;

        map[from] = to;
    }
}
