using System;
using Cosmere.Core.Settings;
using Cosmere.Core.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace Cosmere.Core.Quickstart;

[StaticConstructorOnStartup]
public class Quickstarter {
    private static bool Started;
    private static bool Finished;
    internal static Quickstarter? instance;
    public readonly AbstractQuickstart? Quickstart;
    private readonly StatusBox? statusBox;

    static Quickstarter() {
        instance = new Quickstarter();
    }

    private Quickstarter() {
        if (!Prefs.DevMode) return;
        string? quickstartName = Mod.GetModSettings<CoreModSettings>().quickstartName;
        if (quickstartName == null) return;

        Type? type = Type.GetType(quickstartName);
        if (type == null) {
            Logger.Error("Could not find the quickstart with type: " + quickstartName);
            return;
        }

        Quickstart = (AbstractQuickstart)Activator.CreateInstance(type);
        statusBox = new StatusBox(this);

        LongEventHandler.ExecuteWhenFinished(() => {
                if (Started) return;
                Finished = false;
                Started = true;
                TryStartGame();
                Finished = true;
            }
        );
    }

    private static string seed => GenText.RandomSeedString();

    public void OnGUI() {
        if (Quickstart == null || Finished) return;
        statusBox?.OnGUI();
    }

    private void TryStartGame() {
        LongEventHandler.QueueLongEvent(
            () => {
                MemoryUtility.ClearAllMapsAndWorld();
                ApplyConfiguration();
                PageUtility.InitGameStart();
                DelayedActionScheduler.Schedule(
                    () => {
                        Quickstart!.PrepareColonists(
                            Find.World.PlayerPawnsForStoryteller.Where(p =>
                                    p is { Spawned: true, Map: not null, story: not null, needs: not null }
                                )
                                .ToList()
                        );
                        Quickstart.PostLoaded();
                        if (Quickstart.pauseAfterLoad) Find.TickManager.Pause();
                    },
                    GenTicks.TicksPerRealSecond / 2
                );
            },
            "CC_Quickstart_StartGame",
            true,
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );
        Quickstart!.PostStart();
    }

    private void ApplyConfiguration() {
        Current.ProgramState = ProgramState.Entry;
        Current.Game = new Game {
            InitData = new GameInitData(),
            Scenario = Quickstart!.scenario.scenario,
        };
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(Quickstart.storyteller, Quickstart.difficulty);
        Current.Game.World = WorldGenerator.GenerateWorld(
            Quickstart.planetCoverage,
            seed,
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.Normal,
            LandmarkDensity.Normal
        );
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = Quickstart.mapSize;
        Find.Scenario.PostIdeoChosen();

        Quickstart.PostApplyConfiguration();
    }

    internal static void DrawDebugToolbarButton(WidgetRow widgets) {
        const string quickstartButtonTooltip = "Click to quick-generate a new map.";
        if (widgets.ButtonIcon(ContentFinder<Texture2D>.Get("UI/Debug/quickstartIcon"), quickstartButtonTooltip)) {
            Current.ProgramState = ProgramState.Entry;
            Current.Game = null;
            Started = false;
            instance = new Quickstarter();
        }
    }
}