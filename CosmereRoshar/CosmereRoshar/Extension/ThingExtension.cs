using Verse;

namespace Cosmere.Roshar.Extension;

public static class ThingExtension {
    public static bool ShouldBeMovedByStorm(this Verse.Thing thing) {
        if (!thing.Spawned || thing.Map == null) return false;

        Room room = thing.GetRoom();

        if (StormShelterManager.IsInsideShelter(thing.Position)) return false;
        if (room == null) return true;
        if (room.PsychologicallyOutdoors) return true;

        return !thing.Position.Roofed(thing.Map);
    }
}