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
    private const int MapSize = 50;
    private static bool Started;
    private static readonly StorytellerDef Storyteller = StorytellerDefOf.Cassandra;
    private static readonly DifficultyDef Difficulty = DifficultyDefOf.Easy;
    private static readonly ScenarioDef Scenario = DefDatabase<ScenarioDef>.GetNamed("Cosmere_Scadrial_PreCatacendre");

    static ScadrianQuickstart() {
        if (!Prefs.DevMode) return;
        if (!CosmereFramework.CosmereFramework.cosmereSettings.debugMode) return;

        LongEventHandler.ExecuteWhenFinished(() => {
            if (Started) return;
            Started = true;
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
            Scenario = Scenario.scenario,
        };
        Find.Scenario.PreConfigure();
        Current.Game.storyteller = new Storyteller(Storyteller, Difficulty);
        Current.Game.World = WorldGenerator.GenerateWorld(
            0.05f,
            seed,
            OverallRainfall.Normal,
            OverallTemperature.Normal,
            OverallPopulation.Normal,
            LandmarkDensity.Normal
        );
        Find.GameInitData.ChooseRandomStartingTile();
        Find.GameInitData.mapSize = MapSize;
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
            pawn.Name = new NameSingle("Mistborn");
        }

        if (false && pawns.TryPopFront(out pawn)) {
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            pawn.Name = new NameSingle("Full Feru");
        }

        if (false && pawns.TryPopFront(out pawn)) {
            GeneUtility.AddMistborn(pawn, false, true);
            ScadrianUtility.FillAllReserves(pawn);
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            pawn.Name = new NameSingle("Full Twin");
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, GeneDefOf.Cosmere_Misting_Steel.defName, false, true);
            GeneUtility.AddGene(pawn, GeneDefOf.Cosmere_Misting_Aluminum.defName, false, true);
            pawn.Name = new NameSingle("Steel and Aluminum");
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, GeneDefOf.Cosmere_Misting_Aluminum.defName, false, true);
            pawn.GetComp<MetalReserves>().SetReserve(MetallicArtsMetalDefOf.Aluminum, MetalReserves.MaxAmount);
            pawn.Name = new NameSingle("Aluminum");
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, GeneDefOf.Cosmere_Misting_Steel.defName, false, true);
            pawn.GetComp<MetalReserves>().SetReserve(MetallicArtsMetalDefOf.Steel, MetalReserves.MaxAmount);
            pawn.Name = new NameSingle("Steel");
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddGene(pawn, GeneDefOf.Cosmere_Misting_Atium.defName, false, true);
            pawn.GetComp<MetalReserves>().SetReserve(MetallicArtsMetalDefOf.Atium, MetalReserves.MaxAmount);
            pawn.Name = new NameSingle("Atium");
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