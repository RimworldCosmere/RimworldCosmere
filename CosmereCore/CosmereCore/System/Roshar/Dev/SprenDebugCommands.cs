using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Dev;

public static class SprenDebugCommands {
    [DebugAction("Cosmere/Roshar", "Toggle Spren Debug Overlay", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void ToggleSprenDebugOverlay() {
        SprenDebugOverlay.ShowOverlay = !SprenDebugOverlay.ShowOverlay;

        string status = SprenDebugOverlay.ShowOverlay ? "enabled" : "disabled";
        Messages.Message($"Spren debug overlay {status}", MessageTypeDefOf.NeutralEvent);
    }
}
