using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Puts a kandra into an animal shape, and takes it back out.
/// </summary>
/// <remarks>
///     A human shape is worn by editing the kandra in place. An animal one cannot be, because a
///     pawn's race is its ThingDef and changing that on a live pawn breaks its body, its hediffs
///     and its verbs. So the kandra steps out and a pawn of the animal kind steps in.
///     <para>
///         The two halves know about each other through <see cref="CompKandraShapePair" />: the
///         animal holds the stashed kandra, and reverting kills the animal off and puts the
///         kandra back where it stood. Both directions carry the name, because the colony should
///         see the same character either way.
///     </para>
/// </remarks>
public static class KandraAnimalShape {
    /// <summary>Steps the kandra out and the animal in. Returns the animal, or null on failure.</summary>
    public static Pawn? Wear(Pawn kandra, KandraForm form) {
        if (form.animalKind == null) return null;
        if (!kandra.Spawned) return null;

        Map map = kandra.Map;
        IntVec3 where = kandra.Position;

        Pawn animal;
        try {
            animal = PawnGenerator.GeneratePawn(
                new PawnGenerationRequest(form.animalKind, kandra.Faction, forceGenerateNewPawn: true)
            );
        } catch (global::System.Exception ex) {
            Cosmere.Core.Logger.Warning($"KandraAnimalShape: could not build {form.animalKind.defName}: {ex}");
            return null;
        }

        // The colony is watching the same person walk around on four legs, so everything that
        // belongs to the kandra rather than to the body goes with it.
        KandraShapeTransfer.Into(kandra, animal);

        // Four legs and a mouth. Whatever the kandra knows, it cannot hold a scalpel today.
        Verse.HediffDef? shapeLimits = DefDatabase<Verse.HediffDef>.GetNamedSilentFail(
            "Cosmere_Scadrial_Hediff_AnimalShape"
        );
        if (shapeLimits != null) animal.health?.AddHediff(shapeLimits);

        CompKandraShapePair? pair = animal.TryGetComp<CompKandraShapePair>();
        if (pair == null) {
            Cosmere.Core.Logger.Warning(
                $"KandraAnimalShape: {form.animalKind.defName} has no shape pair comp, so the kandra would be lost."
            );
            animal.Destroy();
            return null;
        }

        // Read before despawning: taking a pawn off the map clears the selection, so asking
        // afterwards always says no and the player loses track of their own colonist.
        bool wasSelected = Find.Selector.IsSelected(kandra);

        // Out of the world before the animal takes its place, and held rather than discarded.
        kandra.DeSpawn();
        pair.Hold(kandra);

        GenSpawn.Spawn(animal, where, map);

        // Losing the selection mid-shapeshift means hunting for your own colonist afterwards.
        if (wasSelected) Find.Selector.Select(animal, false, false);

        Messages.Message(
            "CS_Kandra_TookAnimalForm".Translate(
                animal.Name?.ToStringShort.Named("PAWN") ?? string.Empty.Named("PAWN"),
                form.animalKind.label.Named("FORM")
            ),
            animal,
            MessageTypeDefOf.SilentInput,
            false
        );

        return animal;
    }

    /// <summary>Steps the kandra back out of the animal. Returns the kandra, or null.</summary>
    public static Pawn? Revert(Pawn animal) {
        CompKandraShapePair? pair = animal.TryGetComp<CompKandraShapePair>();
        Pawn? kandra = pair?.Held;
        if (pair == null || kandra == null) return null;
        if (!animal.Spawned) return null;

        Map map = animal.Map;
        IntVec3 where = animal.Position;

        bool wasSelected = Find.Selector.IsSelected(animal);

        pair.Release();

        // What the shape learned or felt comes back with it. Its injuries do not: they were done
        // to a body the kandra was wearing rather than to the kandra.
        KandraShapeTransfer.OutOf(animal, kandra);

        animal.Destroy();

        GenSpawn.Spawn(kandra, where, map);
        kandra.TryGetComp<CompKandraForms>()?.SetCurrent(null);

        if (wasSelected) Find.Selector.Select(kandra, false, false);

        return kandra;
    }
}
