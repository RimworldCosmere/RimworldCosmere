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
        Cosmere.Core.Logger.Important($"TWO-PAWN path: {kandra.LabelShort} spawning a {form.animalKind?.defName}.");
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

        CompKandraShapePair? pair = animal.TryGetComp<CompKandraShapePair>();
        if (pair == null) {
            Cosmere.Core.Logger.Warning(
                $"KandraAnimalShape: {form.animalKind.defName} has no shape pair comp, so the kandra would be lost."
            );
            animal.Destroy();
            return null;
        }

        // The colony is watching the same person walk around on four legs, so everything that
        // belongs to the kandra rather than to the body goes with it.
        KandraShapeTransfer.Into(kandra, animal);

        // The shape hediff is NOT added here any more. It now carries a render node, and a
        // generated shape race is humanlike, so it passes every gate in
        // DynamicPawnRenderNodeSetup_Hediffs and the animal gets drawn a second time - full size
        // from the race's own tree, and shrunk by the portrait clamp on top of it in the colonist
        // bar. This path gets its speed and tools from the generated race's statBases and tools
        // instead, and dies with the rest of it.

        // While the kandra is still on the map. Taking apparel off needs a floor to put it on,
        // and a despawned pawn has none.
        StowGear(kandra, animal, pair);

        // Read before despawning: taking a pawn off the map clears the selection, so asking
        // afterwards always says no and the player loses track of their own colonist.
        bool wasSelected = Find.Selector.IsSelected(kandra);

        // Out of the world before the animal takes its place, and held rather than discarded.
        kandra.DeSpawn();
        pair.Hold(kandra);

        GenSpawn.Spawn(animal, where, map);

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

        // What the shape learned or felt comes back with it. Its injuries do not: they were done
        // to a body the kandra was wearing rather than to the kandra.
        KandraShapeTransfer.OutOf(animal, kandra);
        UnstowGear(animal, kandra, pair);

        // Only now. Release clears the record of what was equipment and what was apparel, and
        // without it everything comes back as cargo sitting in a pocket.
        pair.Release();

        animal.Destroy();

        GenSpawn.Spawn(kandra, where, map);
        kandra.TryGetComp<CompKandraForms>()?.SetCurrent(null);

        if (wasSelected) Find.Selector.Select(kandra, false, false);

        return kandra;
    }

    /// <summary>
    ///     Moves the kandra's gear into the animal's pack rather than leaving it behind.
    /// </summary>
    /// <remarks>
    ///     A wolfhound cannot hold a rifle, but it can carry one, and a kandra that walked out
    ///     with its things should not have to come back for them. What was equipment and what was
    ///     apparel is remembered so it all goes back the way it came.
    /// </remarks>
    private static void StowGear(Pawn kandra, Pawn animal, CompKandraShapePair pair) {
        if (animal.inventory == null) return;

        List<Verse.Thing> equipped = [];
        List<Verse.Thing> worn = [];

        if (kandra.equipment != null) {
            List<ThingWithComps> weapons = [.. kandra.equipment.AllEquipmentListForReading];
            for (int i = 0; i < weapons.Count; i++) {
                if (!kandra.equipment.TryTransferEquipmentToContainer(weapons[i], animal.inventory.innerContainer)) {
                    continue;
                }

                equipped.Add(weapons[i]);
            }
        }

        if (kandra.apparel != null) {
            List<Apparel> clothes = [.. kandra.apparel.WornApparel];
            for (int i = 0; i < clothes.Count; i++) {
                if (!kandra.apparel.TryDrop(clothes[i], out Apparel? dropped, kandra.Position, false)) continue;
                if (dropped == null) continue;

                dropped.DeSpawn();
                if (animal.inventory.innerContainer.TryAdd(dropped)) worn.Add(dropped);
            }
        }

        // Anything already in the kandra's pack rides along as cargo.
        if (kandra.inventory != null) {
            kandra.inventory.innerContainer.TryTransferAllToContainer(animal.inventory.innerContainer);
        }

        pair.RememberGear(equipped, worn);
    }

    /// <summary>
    ///     Puts back whatever survived the trip, the way it was carried.
    /// </summary>
    /// <remarks>
    ///     Only what is still in the pack. Anything the player made the animal drop stays
    ///     dropped, because the point of putting it in the inventory was that it could be.
    /// </remarks>
    private static void UnstowGear(Pawn animal, Pawn kandra, CompKandraShapePair pair) {
        if (animal.inventory == null) return;

        List<Verse.Thing> equipped = [.. pair.WasEquipped];
        List<Verse.Thing> worn = [.. pair.WasWorn];

        for (int i = 0; i < equipped.Count; i++) {
            if (!animal.inventory.innerContainer.Contains(equipped[i])) continue;
            if (equipped[i] is not ThingWithComps weapon) continue;

            animal.inventory.innerContainer.Remove(weapon);
            kandra.equipment?.AddEquipment(weapon);
        }

        for (int i = 0; i < worn.Count; i++) {
            if (!animal.inventory.innerContainer.Contains(worn[i])) continue;
            if (worn[i] is not Apparel clothing) continue;

            animal.inventory.innerContainer.Remove(clothing);
            kandra.apparel?.Wear(clothing, false);
        }

        // Everything else was cargo and stays cargo.
        if (kandra.inventory != null) {
            animal.inventory.innerContainer.TryTransferAllToContainer(kandra.inventory.innerContainer);
        }
    }
}
