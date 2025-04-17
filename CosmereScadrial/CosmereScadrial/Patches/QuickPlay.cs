using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Patches {
    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.SetupForQuickTestPlay))]
    public class QuickPlay {
        public static void Postfix() {
            var scenarioDefName = LoadedModManager.GetMod<CosmereFramework.CosmereFramework>().settings.quickstartScenario;
            var scenarioDef = DefDatabase<ScenarioDef>.GetNamedSilentFail(scenarioDefName);
            if (scenarioDef == null) {
                Log.Error($"Failed to find scenario def: {scenarioDefName}. Falling back to Crashlanded.");
                scenarioDef = ScenarioDefOf.Crashlanded;
            }

            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            Current.Game.Scenario = scenarioDef.scenario;
            Find.Scenario.PreConfigure();
            Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Easy);
            Current.Game.World = WorldGenerator.GenerateWorld(0.05f, GenText.RandomSeedString(), OverallRainfall.Normal, OverallTemperature.Normal, OverallPopulation.Normal);
            Find.GameInitData.ChooseRandomStartingTile();
            Find.GameInitData.mapSize = 500;
            Find.Scenario.PostIdeoChosen();
        }
    }
}