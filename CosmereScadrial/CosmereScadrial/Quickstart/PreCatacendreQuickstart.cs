using System.Collections.Generic;
using System.Linq;
using CosmereCore.Need;
using CosmereFramework.Extension;
using CosmereFramework.Quickstart;
using CosmereResources;
using CosmereResources.Def;
using CosmereScadrial.Def;
using CosmereScadrial.Dev;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Quickstart;

public class PreCatacendreQuickstart : AbstractQuickstart {
    public override int mapSize => 100;
    public override TaggedString description => "Used to test Pre-catacendre pawns";
    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;
    public override DifficultyDef difficulty => DifficultyDefOf.Easy;
    public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    public override void PostStart() {
        DebugSettings.godMode = true;
        DebugViewSettings.showFpsCounter = true;
        DebugViewSettings.showTpsCounter = true;
        DebugViewSettings.showMemoryInfo = true;
    }

    public override void PostLoaded() {
        Current.Game?.researchManager.DebugSetAllProjectsFinished();
    }

    public override void PrepareColonists(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        if (pawns.TryPopFront(out Pawn pawn)) {
            ScadrianUtility.PrepareDevPawn(pawn);
            pawn.Name = new NameSingle("Vin Venture");
            pawn.gender = Gender.Female;

            Pawn grandfather = GeneratePawn(Gender.Male, XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble);
            grandfather.Name = new NameSingle("Vin Pat Grandfather");
            GeneUtility.AddMistborn(grandfather, false, true);
            grandfather.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
            Pawn grandmother = GeneratePawn(Gender.Female, XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble);
            grandmother.Name = new NameSingle("Vin Pat Grandmother");
            GeneUtility.TryAddRandomAllomanticGene(grandmother, false, true);
            grandmother.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasiumAlloy);

            Pawn father = GeneratePawn(Gender.Male, XenotypeDefOf.Cosmere_Scadrial_Xenotype_Noble);
            father.Name = new NameSingle("Vin Father");
            GeneUtility.AddMistborn(father, false, true);
            father.relations.AddDirectRelation(PawnRelationDefOf.Parent, grandfather);
            father.relations.AddDirectRelation(PawnRelationDefOf.Parent, grandmother);

            Pawn mother = GeneratePawn(Gender.Female, XenotypeDefOf.Cosmere_Scadrial_Xenotype_Skaa);
            GeneUtility.AddMistborn(mother, false, true);
            mother.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
            mother.Name = new NameSingle("Vin Mother");

            pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
            pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, mother);
            //GenSpawn.Spawn(grandfather, DropCellFinder.RandomDropSpot(pawn.Map), pawn.Map);
            //GenSpawn.Spawn(grandmother, DropCellFinder.RandomDropSpot(pawn.Map), pawn.Map);
            //GenSpawn.Spawn(father, DropCellFinder.RandomDropSpot(pawn.Map), pawn.Map);
            //GenSpawn.Spawn(mother, DropCellFinder.RandomDropSpot(pawn.Map), pawn.Map);
            StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower.Worker.ClearCacheForThing(pawn);
            StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower.Worker.ClearCacheForThing(pawn);
        }

        if (pawns.TryPopFront(out pawn)) {
            if (pawn.needs.TryGetNeed<Investiture>() is not { } investiture) return;
            investiture.CurLevel = 10;

            ScadrianUtility.PrepareDevPawn(pawn);
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs.Where(x =>
                         x.feruchemy?.userName != null
                     )) {
                pawn.inventory.innerContainer.TryAdd(
                    ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item)
                );
            }

            pawn.Name = new NameSingle("Rashek");
            pawn.gender = Gender.Male;
            pawn.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
            pawn.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium);
        }

        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Atium);
        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Steel);
        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Gold);

        if (pawns.TryPopFront(out pawn)) {
            PrepareColonistAsMisting(pawn, false, true, MetalDefOf.Steel);
            PrepareColonistAsFerring(pawn, false, true, MetalDefOf.Iron);
            pawn.Name = new NameTriple("Waxillium", "Wax", "Ladrian");
            pawn.gender = Gender.Male;
        }
    }

    private Pawn GeneratePawn(Gender gender, XenotypeDef xenotype) {
        return PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                tile: Current.Game.CurrentMap.Tile,
                forcedXenotype: xenotype,
                fixedGender: gender,
                biologicalAgeRange: new FloatRange(40, 60)
            )
        );
    }

    private static void PrepareColonistAsTwinborn(
        Pawn pawn,
        bool fillReserves,
        bool createMetalmind,
        bool snapped,
        params MetalDef[] metals
    ) {
        foreach (MetalDef metal in metals) {
            PrepareColonistAsMisting(pawn, fillReserves, snapped, metal);
            PrepareColonistAsFerring(pawn, createMetalmind, snapped, metal);
        }

        pawn.Name = new NameSingle("TB: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }

    private static void PrepareColonistAsMisting(Pawn pawn, bool fillReserves, bool snapped, params MetalDef[] metals) {
        foreach (MetalDef metal in metals) {
            GeneUtility.AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), false, snapped);
            if (fillReserves) {
                Allomancer gene = pawn.genes.GetAllomanticGeneForMetal(metal)!;
                gene.FillReserve();
            }
        }

        pawn.Name = new NameSingle("Misting: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }

    private static void PrepareColonistAsFerring(
        Pawn pawn,
        bool createMetalmind,
        bool snapped,
        params MetalDef[] metals
    ) {
        foreach (MetalDef metal in metals) {
            GeneUtility.AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(metal), false, snapped);
            if (createMetalmind) {
                pawn.inventory.innerContainer.TryAdd(
                    ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item)
                );
            }
        }

        pawn.Name = new NameSingle("Ferring: " + string.Join(" and ", metals.Select(m => m.LabelCap)));
    }
}