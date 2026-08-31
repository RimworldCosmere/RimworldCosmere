using RimWorld;
using Verse;

namespace Cosmere.Core.Quickstart;

public class SettingsWindowQuickstart : CosmereQuickstartBase {
    public override int mapSize => 25;

    public override TaggedString description => "Used to test the Settings Window";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override void PostStart() {
        DebugSettings.godMode = true;
        DebugViewSettings.showFpsCounter = true;
        DebugViewSettings.showTpsCounter = true;
        DebugViewSettings.showMemoryInfo = true;
    }

    public override void PostLoaded() {
        Current.Game?.researchManager.DebugSetAllProjectsFinished();
        Find.WindowStack.Add(new Dialog_ModSettings(LoadedModManager.GetMod<Mod>()));
    }
}
