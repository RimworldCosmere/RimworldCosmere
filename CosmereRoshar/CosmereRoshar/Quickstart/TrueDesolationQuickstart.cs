using System.Reflection;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Need;
using Cosmere.Framework.Quickstart;
using Cosmere.Framework.Thing;
using RimWorld;
using Verse;

namespace Cosmere.Roshar.Quickstart;

public class TrueDesolationQuickstart : AbstractQuickstart {
    //public override ScenarioDef? scenario => ScenarioDefOf.Cosmere_Scadrial_PreCatacendre;

    private readonly Assembly? scadrial = LoadedModManager.RunningMods
        .FirstOrDefault(m => m.PackageId == "CryptikLemur.Cosmere.Scadrial")
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

        if (pawns.TryPopFront(out Pawn pawn)) {
            pawn.Name = new NameTriple("Kaladin", "Kal", "Stormblessed");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantWindrunner, 4);
            ApparelWithStorage? pouch = (ApparelWithStorage)ThingMaker.MakeThing(
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

            pouch.innerContainer.TryAdd(broam);
            pawn.apparel.Wear(pouch);
        }

        if (pawns.TryPopFront(out pawn)) {
            pawn.Name = new NameTriple("Renarin", "Son of Thorns", "Kohlin");
            pawn.gender = Gender.Male;
            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantTruthwatcher, 1);
        }

        if (pawns.TryPopFront(out pawn)) {
            if (pawn.needs.TryGetNeed(out Investiture investiture)) {
                investiture.CurLevel = 50000;
            }

            pawn.Name = new NameSingle("Wit");
            pawn.gender = Gender.Male;
            if (ModsConfig.IsActive("CryptikLemur.Cosmere.Scadrial")) {
                scadrial?
                    .GetType("CosmereScadrial.Utility.GeneUtility")
                    ?.GetMethod("AddMistborn", BindingFlags.Public | BindingFlags.Static)
                    ?.Invoke(null, [pawn, false, true]);
            }

            pawn.genes.TryAddRadiantOrder(GeneDefOf.Cosmere_Roshar_Gene_RadiantLightweaver, 2);
        }
    }
}