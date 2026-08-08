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

        // The colony is watching the same person walk around on four legs.
        if (kandra.Name != null) animal.Name = kandra.Name;

        CompKandraShapePair? pair = animal.TryGetComp<CompKandraShapePair>();
        if (pair == null) {
            Cosmere.Core.Logger.Warning(
                $"KandraAnimalShape: {form.animalKind.defName} has no shape pair comp, so the kandra would be lost."
            );
            animal.Destroy();
            return null;
        }

        // Out of the world before the animal takes its place, and held rather than discarded.
        kandra.DeSpawn();
        pair.Hold(kandra);

        GenSpawn.Spawn(animal, where, map);

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

        pair.Release();

        // Injuries do not carry across. The kandra was never in this body; it was wearing it.
        animal.Destroy();

        GenSpawn.Spawn(kandra, where, map);
        kandra.TryGetComp<CompKandraForms>()?.SetCurrent(null);

        return kandra;
    }
}
