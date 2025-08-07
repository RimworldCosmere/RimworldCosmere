using System;
using System.Reflection;
using Cosmere.Core.Comp.Thing;
using Cosmere.Foundation.Comp.Thing;
using Cosmere.Foundation.Quickstart;
using Cosmere.Resources.Def;
using Cosmere.Roshar.Utility;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Quickstart;

public class TrueDesolationQuickstart : AbstractQuickstart {
    //public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    private readonly Assembly? scadrial = LoadedModManager.RunningMods
        .FirstOrDefault(m => m.PackageId.Equals("cosmere.scadrial", StringComparison.CurrentCultureIgnoreCase))
        ?.assemblies.loadedAssemblies.FirstOrDefault();

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

        if (pawns.TryPopFront(out Pawn pawn)) {
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
            pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner);
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
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.GetInvestiture().currentInvestitureSelf = 1000;
            pawn.Name = new NameTriple("Renarin", "Son of Thorns", "Kohlin");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher);
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.GetInvestiture().currentInvestitureSelf = 50000;
            pawn.Name = new NameSingle("Wit");
            pawn.gender = Gender.Male;
            if (ModsConfig.IsActive("Cosmere.Scadrial") && scadrial != null) {
                Type? geneUtility = scadrial.GetType("Cosmere.Scadrial.Utility.GeneUtility");
                MethodInfo? addMistborn = geneUtility?.GetMethod(
                    "AddMistborn",
                    BindingFlags.Public | BindingFlags.Static
                );

                addMistborn?.Invoke(null, [pawn, false, false, null]);
            }


            RadiantOrder.BondWithSpren(pawn);
            //pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver);
        }
    }
}