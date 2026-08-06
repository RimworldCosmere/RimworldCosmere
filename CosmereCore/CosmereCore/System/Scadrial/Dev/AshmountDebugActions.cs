using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Comp.Game;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Scadrial.Dev;

/// <summary>
///     Exposure is a world-generation number the player can never undo, so it needs to be
///     readable before anything is tuned against it.
/// </summary>
public static class AshmountDebugActions {
    [DebugAction("Cosmere", "Ashmounts: exposure here", allowedGameStates = AllowedGameStates.Playing)]
    private static void ExposureHere() {
        PlanetTile tile = Find.WorldSelector.SelectedTile;
        if (!tile.Valid) {
            Messages.Message("Select a world tile first.", MessageTypeDefOf.RejectInput, false);
            return;
        }

        float exposure = AshmountExposureCache.For(tile.tileId);
        Messages.Message(
            $"Tile {tile.tileId}: ash exposure {exposure:0.00}x (generation gate: {FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Ashfall)})",
            MessageTypeDefOf.NeutralEvent,
            false
        );
    }
}
