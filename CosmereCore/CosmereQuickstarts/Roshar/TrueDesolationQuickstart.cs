using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.Quickstart;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Quickstart;

public class TrueDesolationQuickstart : CosmereQuickstartBase {
    public override int mapSize => 100;

    public override TaggedString description => "Used to test True Desolation pawns";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override ScenarioDef scenario => ScenarioDefOf.Cosmere_Roshar_Scenario_TrueDesolation;

    /// <summary>
    ///     Nothing beyond what the scenario itself sets: Honor, Cultivation and Odium. Naming them
    ///     here too is how this quickstart used to run without the scenario at all.
    /// </summary>
    public override IReadOnlyList<string> shards => [];

    public override void PostStart() {
        DebugSettings.godMode = true;
        DebugViewSettings.showFpsCounter = true;
        DebugViewSettings.showTpsCounter = true;
        DebugViewSettings.showMemoryInfo = true;
    }

    public override void PostLoaded() {
        Current.Game?.researchManager.DebugSetAllProjectsFinished();
    }

    /// <summary>
    ///     Tops the roster up and lays out gems to test with.
    /// </summary>
    /// <remarks>
    ///     Deliberately leaves names, genders, orders and Ideals alone. The scenario's own
    ///     ScenPart_NamedPawns already sets a radiantOrder and idealLevel per pawn, and this used
    ///     to overwrite all of it - rewriting backstories on everyone, then popping pawns off the
    ///     front to hand out orders the roster had already assigned.
    /// </remarks>
    public override void PrepareColonists(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        for (int i = 0; i < pawns.Count; i++) {
            InvestitureHolder? investiture = pawns[i].GetInvestiture();
            if (investiture != null) investiture.currentInvestitureSelf = 1000;
        }

        LayOutGems(pawns[0]);
        GiveSpherePouch(pawns[0]);
        Find.Selector.Select(pawns[0], false);
    }

    /// <summary>One filled stack of every gem, on the ground where the colony lands.</summary>
    private static void LayOutGems(Pawn pawn) {
        foreach (GemDef gemDef in DefDatabase<GemDef>.AllDefsListForReading) {
            Verse.Thing gem = ThingMaker.MakeThing(ThingDefOf.Cosmere_Roshar_Thing_Mark, gemDef.Item);
            gem.stackCount = 25;
            gem.TryGetComp<InvestitureHolder>()?.FillInvestiture();
            GenPlace.TryPlaceThing(gem, pawn.Position, pawn.Map, ThingPlaceMode.Near);
        }
    }

    /// <summary>A worn pouch holding one charged broam, so Stormlight is on tap immediately.</summary>
    private static void GiveSpherePouch(Pawn pawn) {
        Apparel pouch = (Apparel)ThingMaker.MakeThing(
            ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch,
            GenStuff.RandomStuffFor(ThingDefOf.Cosmere_Roshar_Apparel_SpherePouch)
        );

        Verse.Thing broam = ThingMaker.MakeThing(
            ThingDefOf.Cosmere_Roshar_Thing_Broam,
            Core.ThingDefOf.RawEmerald
        );
        if (broam.TryGetComp(out InvestitureHolder broamInvestiture)) {
            broamInvestiture.currentInvestitureSelf = broamInvestiture.maxInvestitureSelf;
        }

        pouch.TryGetComp<InnerStorage>().innerContainer!.TryAdd(broam);
        pawn.apparel.Wear(pouch);
    }
}
