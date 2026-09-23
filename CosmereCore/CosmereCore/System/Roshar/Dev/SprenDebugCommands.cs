using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Dev;

public static class SprenDebugCommands {
    [DebugAction("Cosmere/Roshar", "Toggle Spren Debug Overlay", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void ToggleSprenDebugOverlay() {
        SprenDebugOverlay.showOverlay = !SprenDebugOverlay.showOverlay;

        string status = SprenDebugOverlay.showOverlay ? "enabled" : "disabled";
        Messages.Message($"Spren debug overlay {status}", MessageTypeDefOf.NeutralEvent);
    }
}
