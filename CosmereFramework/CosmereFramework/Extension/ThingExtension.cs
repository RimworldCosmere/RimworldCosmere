using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CosmereFramework.Extension;

public static class ThingExtension {
    public static bool CanBeEquipped(this Thing thing) {
        return thing.TryGetComp<CompEquippable>() != null || thing.def.IsApparel || thing.def.IsWeapon;
    }

    public static IEnumerable<Thing> ThingsSharingPosition(this Thing thing) {
        return thing.Map.thingGrid.ThingsAt(thing.Position).Where(x => !x.Equals(thing));
    }

    public static IEnumerable<T> ThingsSharingPosition<T>(this Thing thing) {
        return thing.ThingsSharingPosition().OfType<T>();
    }

    public static bool CanBeEquippedBy(this Thing thing, Pawn pawn) {
        if (thing.def.apparel != null) {
            return pawn.apparel.CanWearWithoutDroppingAnything(thing.def) && !HasConflictInApparelSlot(pawn, thing);
        }

        if (thing.TryGetComp<CompEquippable>() != null) {
            return EquipmentUtility.CanEquip(thing, pawn) && !HasConflictInEquipmentSlot(pawn, thing);
        }

        return false;
    }

    public static int GetMaxAmountToPickupForPawn(this Thing thing, Pawn pawn, int desired) {
        try {
            int max = thing.def.orderedTakeGroup?.max ?? thing.stackCount;
            int maxRemaining = max - pawn.inventory?.Count(thing.def) ?? 0;
            int val1 = Math.Min(desired, maxRemaining);

            return thing is { Spawned: true, Map: not null }
                ? Math.Min(val1, thing.Map.reservationManager.CanReserveStack(pawn, thing, 10))
                : val1;
        } catch (Exception) {
            return desired;
        }
    }


    private static bool HasConflictInApparelSlot(Pawn pawn, Thing apparelThing) {
        ApparelProperties? newApparel = apparelThing.def.apparel;
        if (newApparel == null) {
            return false;
        }

        foreach (Apparel existing in pawn.apparel.WornApparel) {
            ApparelProperties? existingApparel = existing.def.apparel;
            if (existingApparel.layers.Any(newApparel.layers.Contains) &&
                existingApparel.bodyPartGroups.Any(newApparel.bodyPartGroups.Contains)) {
                return true;
            }
        }

        return false;
    }

    private static bool HasConflictInEquipmentSlot(Pawn pawn, Thing thing) {
        return thing.TryGetComp<CompEquippable>() != null && pawn.equipment?.Primary != null;
    }
}