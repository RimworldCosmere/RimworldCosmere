using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Comps.Things;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;
using GeneUtility = CosmereScadrial.Utils.GeneUtility;

namespace CosmereScadrial.Dev;

[StaticConstructorOnStartup]
public static class ScadrianQuickstart {
    private const int MAP_SIZE = 25;
    private static bool started;
    private static readonly StorytellerDef storyteller = StorytellerDefOf.Cassandra;
    private static readonly DifficultyDef difficulty = DifficultyDefOf.Easy;
    private static readonly ScenarioDef scenario = DefDatabase<ScenarioDef>.GetNamed("Cosmere_Scadrial_PreCatacendre");

    static ScadrianQuickstart() {
        if (!Prefs.DevMode) return;
        if (!CosmereFramework.CosmereFramework.CosmereSettings.debugMode) return;

        LongEventHandler.ExecuteWhenFinished(() => {
            if (started) return;
            started = true;
            TryStartGame();
        });
    }

    private static string seed => GenText.RandomSeedString();

    private static void TryStartGame() {
        if (Current.ProgramState != ProgramState.Entry) return;
        if (Find.GameInitData != null || Current.Game != null) return;

        LongEventHandler.QueueLongEvent(() => {
            MemoryUtility.ClearAllMapsAndWorld();
            ApplyConfiguration();
            PageUtility.InitGameStart();
            DelayedActionScheduler.Schedule(PrepareColonists, 20);
        }, "GeneratingMap", true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
    }

    private static void ApplyConfiguration() {
        Current.ProgramState = ProgramState.Entry;
        Current.Game = new Game {
            InitData = new GameInitData(),
            Scenario = scenario.scenario,
        };
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(storyteller, difficulty);
        Current.Game.World = WorldGenerator.GenerateWorld(
            0.05f,
            seed,
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.Normal,
            LandmarkDensity.Normal
        );
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = MAP_SIZE;
        Find.Scenario.PostIdeoChosen();
        Find.GameInitData.PrepForMapGen();
        Find.Scenario.PreMapGenerate();
    }

    private static void PrepareColonists() {
        List<Pawn> pawns = Find.World.PlayerPawnsForStoryteller.ToList();
        if (pawns.Count == 0) {
            Find.TickManager.Pause();
            return;
        }

        pawns.Shuffle();
        if (pawns.TryPopFront(out Pawn pawn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            ScadrianUtility.FillAllReserves(pawn);
        }

        if (false && pawns.TryPopFront(out pawn)) {
            GeneUtility.AddFullFeruchemist(pawn, false, true);
        }

        if (false && pawns.TryPopFront(out pawn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            ScadrianUtility.FillAllReserves(pawn);
            GeneUtility.AddFullFeruchemist(pawn, false, true);
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, "Cosmere_Misting_Steel", false, true);
            GeneUtility.AddGene(pawn, "Cosmere_Misting_Aluminum", false, true);
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, "Cosmere_Misting_Aluminum", false, true);
            ScadrianUtility.FillAllReserves(pawn);
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, "Cosmere_Misting_Steel", false, true);
            pawn.GetComp<MetalReserves>().SetReserve(MetallicArtsMetalDefOf.Steel, MetalReserves.MAX_AMOUNT);
        }

        Find.TickManager.Pause();
    }

    public static bool TryPopFront<T>(this List<T> list, out T element) {
        if (list.Count == 0) {
            element = default;
            return false;
        }

        element = list.PopFront();
        return true;
    }
}