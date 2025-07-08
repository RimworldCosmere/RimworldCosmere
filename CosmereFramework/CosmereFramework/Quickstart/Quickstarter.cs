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
    private static readonly AbstractQuickstart? Quickstart;

    static Quickstarter() {
        if (!Prefs.DevMode) return;
        string? quickstartName = CosmereFramework.GetModSettings<FrameworkModSettings>().quickstartName;
        if (quickstartName == null) return;

        Type? type = Type.GetType(quickstartName);
        if (type == null) {
            Logger.Error("Could not find the quickstart with type: " + quickstartName);
            return;
        }

        Quickstart = (AbstractQuickstart)Activator.CreateInstance(type);

        LongEventHandler.ExecuteWhenFinished(() => {
                if (Started) return;
                Started = true;
                TryStartGame();
            }
        );
    }

    private static string seed => GenText.RandomSeedString();

    private static void TryStartGame() {
        if (Quickstart == null) return;
        if (Current.ProgramState != ProgramState.Entry) return;
        if (Find.GameInitData != null || Current.Game != null) return;

        LongEventHandler.QueueLongEvent(
            () => {
                MemoryUtility.ClearAllMapsAndWorld();
                ApplyConfiguration();
                PageUtility.InitGameStart();
                DelayedActionScheduler.Schedule(
                    () => {
                        Quickstart.PrepareColonists(Find.World.PlayerPawnsForStoryteller.ToList());
                        Quickstart.PostLoaded();
                        if (Quickstart.pauseAfterLoad) Find.TickManager.Pause();
                    },
                    10
                );
            },
            "GeneratingMap",
            true,
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );
        Quickstart.PostStart();
    }

    private static void ApplyConfiguration() {
        if (Quickstart == null) return;
        Current.ProgramState = ProgramState.Entry;
        Current.Game = new Game {
            InitData = new GameInitData(),
            Scenario = Quickstart.scenario?.scenario,
        };
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(Quickstart.storyteller, Quickstart.difficulty);
        Current.Game.World = WorldGenerator.GenerateWorld(
            0.05f,
            seed,
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.Normal,
            LandmarkDensity.Normal
        );
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = Quickstart.mapSize;
        Find.Scenario.PostIdeoChosen();
        Find.GameInitData.PrepForMapGen();
        Find.Scenario.PreMapGenerate();

        Quickstart.PostApplyConfiguration();
    }
}