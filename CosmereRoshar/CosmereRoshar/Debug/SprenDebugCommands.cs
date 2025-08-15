using Cosmere.Foundation;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Debug;

public static class SprenDebugCommands {
    [DebugAction("Cosmere/Roshar", "Toggle Spren Debug Overlay", allowedGameStates = AllowedGameStates.Playing)]
    public static void ToggleSprenDebugOverlay() {
        SprenDebugOverlay.ShowOverlay = !SprenDebugOverlay.ShowOverlay;

        string status = SprenDebugOverlay.ShowOverlay ? "enabled" : "disabled";
        Messages.Message($"Spren debug overlay {status}", MessageTypeDefOf.NeutralEvent);

        Logger.Info($"[Spren Debug] Overlay {status}");
    }

    [DebugAction("Cosmere/Roshar", "Toggle Active Spawn Cell Display", allowedGameStates = AllowedGameStates.Playing)]
    public static void ToggleActiveSpawnCellDisplay() {
        SprenDebugOverlay.ShowActualParticles = !SprenDebugOverlay.ShowActualParticles;

        string status = SprenDebugOverlay.ShowActualParticles ? "enabled" : "disabled";
        Messages.Message($"Active spawn cell display {status}", MessageTypeDefOf.NeutralEvent);

        Logger.Info($"[Spren Debug] Active spawn cell display {status}");
    }
}