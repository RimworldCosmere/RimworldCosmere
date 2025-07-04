using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Extension;
using CosmereFramework.Quickstart;
using CosmereResources;
using CosmereResources.Def;
using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Def;
using CosmereScadrial.Dev;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Util.GeneUtility;

namespace CosmereScadrial.Quickstart;

public class PreCatacendre : AbstractQuickstart {
    public override int mapSize => 75;
    public override TaggedString description => "Used to test Pre-catacendre pawns";
    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;
    public override DifficultyDef difficulty => DifficultyDefOf.Easy;
    public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    public override void PostStart() {
        Current.Game?.researchManager.DebugSetAllProjectsFinished();
        DebugSettings.godMode = true;
        DebugViewSettings.showFpsCounter = true;
        DebugViewSettings.showTpsCounter = true;
        DebugViewSettings.showMemoryInfo = true;
    }

    public override void PostLoaded() {
        Mod modInstance = LoadedModManager.GetMod<CosmereFramework.CosmereFramework>();
        Find.WindowStack.Add(new Dialog_ModSettings(modInstance));
    }

    public override void PrepareColonists(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        if (pawns.TryPopFront(out Pawn pawn)) {
            ScadrianUtility.PrepareDevPawn(pawn);
            pawn.Name = new NameSingle("Vin Venture");
            pawn.gender = Gender.Female;
        }

        if (pawns.TryPopFront(out pawn)) {
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            foreach (MetallicArtsMetalDef? metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                pawn.inventory.innerContainer.TryAdd(
                    ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, metal.Item)
                );
            }

            pawn.Name = new NameSingle("Sazed");
            pawn.gender = Gender.Male;
        }

        if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Steel);

        if (pawns.TryPopFront(out pawn)) PrepareColonistAsFerring(pawn, true, true, MetalDefOf.Brass);

        if (pawns.TryPopFront(out pawn)) {
            PrepareColonistAsMisting(pawn, true, true, MetalDefOf.Steel);
            PrepareColonistAsFerring(pawn, true, true, MetalDefOf.Iron);
            pawn.Name = new NameTriple("Waxillium", "Wax", "Ladrian");
            pawn.gender = Gender.Male;
        }
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
        MetalReserves reserves = pawn.GetComp<MetalReserves>();
        foreach (MetalDef metal in metals) {
            GeneUtility.AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(metal), false, snapped);
            if (fillReserves) reserves.SetReserve(MetallicArtsMetalDef.FromMetalDef(metal), MetalReserves.MaxAmount);
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