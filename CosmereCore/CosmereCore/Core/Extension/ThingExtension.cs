using System;
using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld;
using Verse;

namespace Cosmere.Core.Extension;

public static class ThingExtension {
    public static bool IsCutGemOfType(this Verse.Thing thing, GemDef gemDef) {
        return thing.def.Equals(ThingDefOf.CutGem) && thing.Stuff.Equals(gemDef.Item);
    }

    public static bool IsCapableOfHavingMetal(this Verse.Thing thing) {
        return MetalDetector.IsCapableOfHavingMetal(thing.def);
    }

    public static float GetMetalMass(this Verse.Thing thing) {
        return MetalDetector.GetMetalMass(thing);
    }

    public static bool CanBeEquipped(this Verse.Thing thing) {
        return thing.TryGetComp<CompEquippable>() != null || thing.def.IsApparel || thing.def.IsWeapon;
    }

    public static IEnumerable<Verse.Thing> ThingsSharingPosition(this Verse.Thing thing) {
        return thing.Map.thingGrid.ThingsAt(thing.Position).Where(x => !x.Equals(thing));
    }

    public static IEnumerable<T> ThingsSharingPosition<T>(this Verse.Thing thing) {
        return thing.ThingsSharingPosition().OfType<T>();
    }

    public static bool CanBeEquippedBy(this Verse.Thing thing, Pawn pawn) {
        if (thing.def.apparel != null) {
            return pawn.apparel.CanWearWithoutDroppingAnything(thing.def) && !HasConflictInApparelSlot(pawn, thing);
        }

        if (thing.TryGetComp<CompEquippable>() != null) {
            return EquipmentUtility.CanEquip(thing, pawn) && !HasConflictInEquipmentSlot(pawn, thing);
        }

        return false;
    }

    public static int GetMaxAmountToPickupForPawn(this Verse.Thing thing, Pawn pawn, int desired) {
        int max = thing.def.orderedTakeGroup?.max ?? thing.stackCount;
        int maxRemaining = max - (pawn.inventory?.Count(thing.def) ?? 0);
        int val1 = Math.Min(desired, maxRemaining);

        return thing is { Spawned: true, Map: not null }
            ? Math.Min(val1, thing.Map.reservationManager.CanReserveStack(pawn, thing, 10))
            : val1;
    }

    public static bool IsBehindSolidThing(
        this Verse.Thing thing,
        IntVec3 direction,
        int spaces,
        Func<Verse.Thing, bool>? predicate = null
    ) {
        for (int i = 1; i < spaces + 1; i++) {
            IntVec3 nextPos = thing.Position + direction * i;
            if (!nextPos.InBounds(thing.Map)) continue;
            List<Verse.Thing>? things = nextPos.GetThingList(thing.Map);
            if (things.Any(x => x.IsSolid() && (predicate == null || predicate.Invoke(x)))) {
                return true;
            }
        }

        return false;
    }

    public static bool CanBeMoved(this Verse.Thing thing) {
        ThingDef? def = thing.def;
        if (def.IsBlueprint) return false;
        if (thing.Map.terrainGrid.TerrainAt(thing.Position).passability == Traversability.Impassable) return false;
        if (thing is Pawn) return true;
        if (thing.IsSolid()) return false;
        if (def.altitudeLayer >= AltitudeLayer.Item && def.altitudeLayer < AltitudeLayer.Weather) return true;

        return def.EverHaulable;
    }

    public static bool IsSolid(this Verse.Thing thing) {
        if (!thing.Spawned || thing.Destroyed) return false;
        if (thing.def.EverHaulable) return false;
        if (thing.HitPoints == -1) return false;
        if (thing.def.blockWeather) return true;
        if (thing.def.blockWind) return true;
        if (thing is Building or Mineable) return true;
        if (thing.def.passability == Traversability.Impassable) return true;

        return thing.def.category == ThingCategory.Building;
    }

    private static bool HasConflictInApparelSlot(Pawn pawn, Verse.Thing apparelThing) {
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

    private static bool HasConflictInEquipmentSlot(Pawn pawn, Verse.Thing thing) {
        return thing.TryGetComp<CompEquippable>() != null && pawn.equipment?.Primary != null;
    }
}