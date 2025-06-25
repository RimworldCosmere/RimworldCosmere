using System;
using RimWorld;
using Verse;

namespace CosmereFramework.Extension;

public static class ThingExtension {
    public static bool CanBeEquipped(this Thing thing) {
        return thing.TryGetComp<CompEquippable>() != null || thing.def.IsApparel || thing.def.IsWeapon;
    }

    public static bool CanBeEquippedBy(this Thing thing, Pawn pawn) {
        if (thing.def.apparel != null) {
            if (!pawn.apparel.CanWearWithoutDroppingAnything(thing.def)) {
                return false;
            }

            if (HasConflictInApparelSlot(pawn, thing)) {
                return false;
            }

            return true;
        }

        if (thing.TryGetComp<CompEquippable>() != null) {
            if (!EquipmentUtility.CanEquip(thing, pawn)) {
                return false;
            }

            if (HasConflictInEquipmentSlot(pawn, thing)) {
                return false;
            }

            return true;
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
        } catch (Exception e) {
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