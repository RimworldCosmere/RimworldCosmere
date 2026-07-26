using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Util;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Extension;

public static class ThingExtension {
    public static InvestitureHolder? GetInvestiture(this Verse.Thing thing) {
        return thing.TryGetComp<InvestitureHolder>();
    }

    public static bool ShouldBeMovedByStorm(this Verse.Thing thing) {
        if (!thing.Spawned || thing.Map == null) return false;
        if (thing.Position.Fogged(thing.Map)) return false;
        if (thing is Mineable) return false;
        if (thing is Pawn pawn && StormlightUtility.IsHighstormImmune(pawn)) return false;

        Room room = thing.GetRoom();

        if (StormShelterManager.IsInsideShelter(thing.Position)) return false;
        if (room == null) return true;
        if (room.PsychologicallyOutdoors) return true;

        return !thing.Position.Roofed(thing.Map);
    }
}