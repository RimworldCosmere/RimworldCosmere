using System;
using System.Linq;
using CosmereFramework.Settings;
using CosmereFramework.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace CosmereFramework.Quickstart;

[StaticConstructorOnStartup]
public class Quickstarter {
    private static bool Started;
    private static readonly AbstractQuickstart quickstart;

    static Quickstarter() {
        if (!Prefs.DevMode) return;
        if (!CosmereFramework.debugMode) return;
        string? quickstartName = CosmereFramework.GetModSettings<FrameworkModSettings>().quickstartName;
        if (quickstartName == null) return;

        Type? type = Type.GetType(quickstartName);
        if (type == null) {
            Logger.Error("Could not find the quickstart with type: " + quickstartName);
            return;
        }

        quickstart = (AbstractQuickstart)Activator.CreateInstance(type);

        LongEventHandler.ExecuteWhenFinished(() => {
                if (Started) return;
                Started = true;
                TryStartGame();
            }
        );
    }

    private static string seed => GenText.RandomSeedString();

    private static void TryStartGame() {
        if (Current.ProgramState != ProgramState.Entry) return;
        if (Find.GameInitData != null || Current.Game != null) return;

        LongEventHandler.QueueLongEvent(
            () => {
                MemoryUtility.ClearAllMapsAndWorld();
                ApplyConfiguration();
                PageUtility.InitGameStart();
                DelayedActionScheduler.Schedule(
                    () => {
                        quickstart.PrepareColonists(Find.World.PlayerPawnsForStoryteller.ToList());
                        quickstart.PostLoaded();
                        if (quickstart.pauseAfterLoad) Find.TickManager.Pause();
                    },
                    10
                );
            },
            "GeneratingMap",
            true,
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );
        quickstart.PostStart();
    }

    private static void ApplyConfiguration() {
        Current.ProgramState = ProgramState.Entry;
        Current.Game = new Game {
            InitData = new GameInitData(),
            Scenario = quickstart.scenario?.scenario,
        };
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(quickstart.storyteller, quickstart.difficulty);
        Current.Game.World = WorldGenerator.GenerateWorld(
            0.05f,
            seed,
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.Normal,
            LandmarkDensity.Normal
        );
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = quickstart.mapSize;
        Find.Scenario.PostIdeoChosen();
        Find.GameInitData.PrepForMapGen();
        Find.Scenario.PreMapGenerate();

        quickstart.PostApplyConfiguration();
    }
}