using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Core.Dev;

/// <summary>
///     Strips the map back to one ore so a vein's shape can actually be seen. Ordinary rock and
///     ordinary ore read as the same grey clutter at map zoom, which makes it impossible to
///     judge whether a generated vein came out the shape it was meant to.
/// </summary>
[StaticConstructorOnStartup]
public static class MineableUtility {
    [DebugAction(
        "Cosmere/Core",
        "Strip map to one ore...",
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void StripToOre() {
        Verse.Map? map = Find.CurrentMap;
        if (map == null) return;

        List<DebugMenuOption> options = new List<DebugMenuOption>();
        foreach (ThingDef keep in OresOn(map)) {
            options.Add(new DebugMenuOption(keep.label, DebugMenuOptionMode.Action, () => Strip(map, keep)));
        }

        if (options.Count == 0) {
            Messages.Message("No ore on this map to keep.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    /// <summary>
    ///     Removes every natural rock and ore except the one named. Destroy with DestroyMode.Vanish
    ///     so a whole mountainside does not turn into thousands of chunks.
    /// </summary>
    private static void Strip(Verse.Map map, ThingDef keep) {
        List<Verse.Thing> doomed = new List<Verse.Thing>();

        List<Verse.Thing> all = map.listerThings.AllThings;
        for (int i = 0; i < all.Count; i++) {
            Verse.Thing thing = all[i];
            if (thing.def == keep || thing is not Mineable) continue;

            doomed.Add(thing);
        }

        for (int i = 0; i < doomed.Count; i++) {
            if (!doomed[i].Destroyed) doomed[i].Destroy(DestroyMode.Vanish);
        }

        // Rock was holding the roof up. Without this the map is left under a mountain roof with
        // nothing supporting it, and the first thing to touch it starts a collapse.
        foreach (IntVec3 cell in map.AllCells) {
            if (map.roofGrid.RoofAt(cell) != null && cell.GetEdifice(map) == null) {
                map.roofGrid.SetRoof(cell, null);
            }
        }

        Logger.Important($"Stripped {doomed.Count} mineables, keeping {keep.defName}.");
        Messages.Message(
            $"Removed {doomed.Count} mineables. Only {keep.label} remains.",
            MessageTypeDefOf.TaskCompletion,
            false
        );
    }

    private static List<ThingDef> OresOn(Verse.Map map) {
        HashSet<ThingDef> found = new HashSet<ThingDef>();

        List<Verse.Thing> all = map.listerThings.AllThings;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Mineable && all[i].def.building?.isResourceRock == true) found.Add(all[i].def);
        }

        return new List<ThingDef>(found);
    }
}
