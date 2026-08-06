using System;
using Cosmere.Core.Comp.Game;
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
    private const string CommandLineArg = "cosmerequickstart";

    private static bool Started;
    private static bool Finished;
    internal static Quickstarter? instance;
    public readonly AbstractQuickstart? Quickstart;
    private readonly StatusBox? statusBox;

    static Quickstarter() {
        instance = new Quickstarter(ConfiguredQuickstart());

        // A configured quickstart owns the launch, so the picker would be torn down a moment later.
        if (instance.Quickstart == null) VanillaQuicktest.ClaimCommandLineArg();
    }

    private Quickstarter(AbstractQuickstart? quickstart) {
        Quickstart = quickstart;
        if (quickstart == null) return;

        statusBox = new StatusBox(this);

        LongEventHandler.ExecuteWhenFinished(() => {
            if (Started) return;
            Finished = false;
            Started = true;
            StartGame();
            Finished = true;
        }
        );
    }

    private static AbstractQuickstart? ConfiguredQuickstart() {
        if (!Prefs.DevMode) {
            if (GenCommandLine.TryGetCommandLineArg(CommandLineArg, out string _)) {
                Logger.Warning($"-{CommandLineArg} was passed, but dev mode is off, so no quickstart will run.");
            }

            return null;
        }

        // A name the arg cannot resolve stops the launch rather than falling back to the setting,
        // which would quietly boot a different colony than the one that was asked for.
        Type? type = GenCommandLine.TryGetCommandLineArg(CommandLineArg, out string value)
            ? CommandLineQuickstart(value)
            : SettingsQuickstart();

        return type == null ? null : (AbstractQuickstart)Activator.CreateInstance(type);
    }

    private static Type? CommandLineQuickstart(string value) {
        Type? type = QuickstartLookup.Resolve(
            value,
            typeof(AbstractQuickstart).AllSubclassesNonAbstract(),
            out string? error
        );
        if (type == null) {
            Logger.Error($"-{CommandLineArg}: {error}");
            return null;
        }

        Logger.Important($"Command line picked the {type.Name} quickstart.");

        return type;
    }

    private static Type? SettingsQuickstart() {
        string? quickstartName = Mod.GetModSettings<CoreModSettings>().quickstartName;
        if (string.IsNullOrEmpty(quickstartName)) return null;

        Type? type = Type.GetType(quickstartName);
        if (type == null) Logger.Error("Could not find the quickstart with type: " + quickstartName);

        return type;
    }

    private static string seed => GenText.RandomSeedString();

    public void OnGUI() {
        if (Quickstart == null || Finished) return;
        statusBox?.OnGUI();
    }

    private void StartGame() {
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
                        Logger.Important("Game loaded and ready");
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

        // Before GenerateWorld, not next to EnableShards below: a WorldGenStep that reads the
        // world during generation sees null otherwise.
        WorldUtility.SeedFromScenario();

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
        Quickstart.PostApplyConfiguration();

        Find.Scenario.PostIdeoChosen();

        // After PostIdeoChosen, not before: the scenario's own shard extension enables its set
        // during PreConfigure without allowing conflicts, so a quickstart asking for both Ruin
        // and Preservation would lose one of them if it ran first.
        EnableShards();
    }

    private void EnableShards() {
        IReadOnlyList<string> wanted = Quickstart!.shards;
        if (wanted.Count == 0) return;

        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards == null) {
            Logger.Warning("Quickstart shards skipped: the game has no Shards component yet.");
            return;
        }

        for (int i = 0; i < wanted.Count; i++) {
            shards.EnableShard(wanted[i], true);
        }
    }

    internal static void DrawDebugToolbarButton(WidgetRow widgets) {
        const string quickstartButtonTooltip = "Click to quick-generate a new map.";
        if (widgets.ButtonIcon(ContentFinder<Texture2D>.Get("UI/Debug/quickstartIcon"), quickstartButtonTooltip)) {
            ReloadQuickstart();
        }
    }

    public static void ReloadQuickstart() {
        Restart(ConfiguredQuickstart());
    }

    /// <summary>
    ///     Starts a quickstart the player picked by hand, ignoring whatever the settings say.
    /// </summary>
    public static void Launch(AbstractQuickstart quickstart) {
        Restart(quickstart);
    }

    private static void Restart(AbstractQuickstart? quickstart) {
        LongEventHandler.QueueLongEvent(
            () => {
                Current.ProgramState = ProgramState.Entry;
                Current.Game = null;
                Started = false;
                instance = new Quickstarter(quickstart);
            },
            "CC_Quickstart_Reload",
            true,
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );
    }
}
