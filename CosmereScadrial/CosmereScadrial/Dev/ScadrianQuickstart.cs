using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereFramework.Util;
using CosmereResources;
using CosmereResources.Def;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Dev;

[StaticConstructorOnStartup]
public static class ScadrianQuickstart {
    private const int MapSize = 75;
    private static bool Started;
    private static readonly StorytellerDef Storyteller = StorytellerDefOf.Cassandra;
    private static readonly DifficultyDef Difficulty = DifficultyDefOf.Easy;
    private static readonly ScenarioDef Scenario = ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

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
            Current.Game?.researchManager.DebugSetAllProjectsFinished();
            DebugSettings.godMode = true;
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

        if (pawns.TryPopFront(out Pawn pawn)) {
            ScadrianUtility.PrepareDevPawn(pawn);
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
            PrepareColonistAsTwinborn(pawn, true, true, MetalDefOf.Steel);
        }

        if (pawns.TryPopFront(out pawn)) {
            PrepareColonistAsTwinborn(pawn, true, true, MetalDefOf.Brass);
        }

        if (pawns.TryPopFront(out pawn)) {
            PrepareColonistAsTwinborn(pawn, true, true, MetalDefOf.Bronze);
        }

        if (pawns.TryPopFront(out pawn)) {
            PrepareColonistAsTwinborn(pawn, true, true, MetalDefOf.Zinc);
        }

        if (false && pawns.TryPopFront(out pawn)) {
            PrepareColonistAsMisting(pawn, true, MetalDefOf.Steel);
        }

        if (false && pawns.TryPopFront(out pawn)) {
            PrepareColonistAsMisting(pawn, false, MetalDefOf.Iron);
            pawn.health.forceDowned = true;
        }

        Find.TickManager.Pause();
    }

    private static void PrepareColonistAsTwinborn(
        Pawn pawn,
        bool fillReserves,
        bool createMetalmind,
        params MetalDef[] metals
    ) {
        foreach (MetalDef metal in metals) {
            PrepareColonistAsMisting(pawn, fillReserves, metal);
            PrepareColonistAsFerring(pawn, createMetalmind, metal);
        }

        pawn.Name = new NameSingle("TB: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }

    private static void PrepareColonistAsMisting(Pawn pawn, bool fillReserves, params MetalDef[] metals) {
        MetalReserves reserves = pawn.GetComp<MetalReserves>();
        foreach (MetalDef metal in metals) {
            GeneUtility.AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), false, true);
            if (fillReserves) reserves.SetReserve(MetallicArtsMetalDef.FromMetalDef(metal), MetalReserves.MaxAmount);
        }

        pawn.Name = new NameSingle("Misting: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }

    private static void PrepareColonistAsFerring(Pawn pawn, bool createMetalmind, params MetalDef[] metals) {
        foreach (MetalDef metal in metals) {
            GeneUtility.AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(metal), false, true);
            if (createMetalmind) {
                pawn.inventory.innerContainer.TryAdd(
                    ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item));
            }
        }

        pawn.Name = new NameSingle("Ferring: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }
}