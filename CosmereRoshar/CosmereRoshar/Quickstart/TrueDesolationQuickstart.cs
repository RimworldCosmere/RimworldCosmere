using Cosmere.Core.Comp.Thing;
using Cosmere.Foundation.Comp.Thing;
using Cosmere.Foundation.Quickstart;
using Cosmere.Resources.Def;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Quickstart;

public class TrueDesolationQuickstart : AbstractQuickstart {
    //public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    public override int mapSize => 100;

    public override TaggedString description => "Used to test True Desolation pawns";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

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

        foreach (GemDef gemDef in DefDatabase<GemDef>.AllDefsListForReading) {
            Verse.Thing? gem = ThingMaker.MakeThing(ThingDefOf.Cosmere_Roshar_Thing_Mark, gemDef.Item);
            gem.stackCount = 25;
            gem.TryGetComp<InvestitureHolder>().FillInvestiture();
            GenPlace.TryPlaceThing(gem, pawns[0].Position, pawns[0].Map, ThingPlaceMode.Near);
        }


        ThingDef shardbladeDef = ThingDefOf.Cosmere_Roshar_MeleeWeapon_Shardblade;
        ThingWithComps shardblade = (ThingWithComps)ThingMaker.MakeThing(
            shardbladeDef,
            DefDatabase<GemDef>.GetRandom().Item
        );
        GenPlace.TryPlaceThing(
            shardblade,
            pawns[0].Position,
            pawns[0].Map,
            ThingPlaceMode.Near
        );

        if (pawns.TryPopFront(out Pawn pawn)) {
            pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, 4);
            Apparel? pouch = (Apparel)ThingMaker.MakeThing(
                ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch,
                GenStuff.RandomStuffFor(ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch)
            );
            Verse.Thing? broam = ThingMaker.MakeThing(
                ThingDefOf.Cosmere_Roshar_Thing_Broam,
                Resources.ThingDefOf.RawEmerald
            );
            if (broam.TryGetComp(out InvestitureHolder broamInvestiture)) {
                broamInvestiture.currentInvestitureSelf = broamInvestiture.maxInvestitureSelf;
            }

            pouch.TryGetComp<InnerStorage>().innerContainer!.TryAdd(broam);
            pawn.apparel.Wear(pouch);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }

        /*if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Renarin", "Son of Thorns", "Kohlin");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher);
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
        }*/

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameSingle("Wit");
            pawn.gender = Gender.Male;
            pawn.BecomeMistborn(cause: "ingested lerasium");

            // RadiantOrder.BondWithSpren(pawn);
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver);
            pawn.GetInvestiture().currentInvestitureSelf = 50000;
        }
    }
}