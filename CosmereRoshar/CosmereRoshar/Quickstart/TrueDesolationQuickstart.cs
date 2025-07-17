using System.Collections.Generic;
using CosmereCore.Need;
using CosmereFramework.Extension;
using CosmereFramework.Quickstart;
using RimWorld;
using Verse;

namespace CosmereRoshar.Quickstart;

public class TrueDesolationQuickstart : AbstractQuickstart {
    public override int mapSize => 100;
    public override TaggedString description => "Used to test True Desolation pawns";
    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;
    public override DifficultyDef difficulty => DifficultyDefOf.Easy;
    //public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

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
            pawn.Name = new NameTriple("Dalinar", "Dalinar", "Kholin");
            pawn.gender = Gender.Male;
        }

        if (pawns.TryPopFront(out pawn)) {
            if (pawn.needs.TryGetNeed<Investiture>() is not { } investiture) return;
            investiture.CurLevel = 50000;

            pawn.Name = new NameSingle("Wit");
            pawn.gender = Gender.Male;
        }

        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Atium);
        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Steel);
        //if (pawns.TryPopFront(out pawn)) PrepareColonistAsTwinborn(pawn, true, true, true, MetalDefOf.Gold);

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Jasnah", "Jas", "Kohlin");
            pawn.gender = Gender.Female;
        }
    }
}