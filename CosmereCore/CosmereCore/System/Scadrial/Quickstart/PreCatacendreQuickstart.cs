using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.Need;
using Cosmere.Core.Quickstart;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Dev;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Quickstart;

public class PreCatacendreQuickstart : AbstractQuickstart {
    public override int mapSize => 100;

    public override TaggedString description => "Used to test Pre-catacendre pawns";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override ScenarioDef scenario => ScenarioDefOf.Cosmere_Scadrial_Scenario_PreCatacendre;

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
        // The PreCatacendre scenario enables Ruin and Preservation, so Allomancy works out of the
        // box. Honor, Cultivation and Odium are added here rather than in the scenario so this
        // quickstart can also exercise Roshar content - every Radiant grant silently no-ops without
        // Honor. Enabled by name: the Roshar ShardDefOf lives in a namespace Scadrial code must not
        // import.
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards != null) {
            shards.EnableShard("Honor", true);
            shards.EnableShard("Cultivation", true);
            shards.EnableShard("Odium", true);
        }

        if (pawns.Count == 0) return;

        for (int i = 0; i < pawns.Count; i++) {
            StockUp(pawns[i]);
        }

        SpawnRashek(pawns[0]);
    }

    /// <summary>
    ///     Tops up reserves and hands over vials and metalminds for the metals a pawn can actually
    ///     use. Deliberately leaves names, genders and genes alone so the scenario's own roster is
    ///     what you see.
    /// </summary>
    private static void StockUp(Pawn pawn, List<(ThingDef kind, int count)>? metalminds = null) {
        if (pawn.genes == null) return;

        pawn.FillAllAllomanticReserves();

        metalminds ??= [
            (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindEarring, 3),
            (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBracelet, 3),
            (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, 3),
            (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindImplant, 3),
        ];

        foreach (MetallicArtsMetalDef metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading) {
            if (pawn.genes.GetAllomanticGeneForMetal(metal) != null) {
                Verse.Thing vial = ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial, metal.Item);
                vial.stackCount = 20;
                pawn.inventory.innerContainer.TryAdd(vial);
            }

            if (pawn.genes.GetFeruchemicGeneForMetal(metal) == null) continue;

            foreach ((ThingDef kind, int count) in metalminds) {
                if (kind == ThingDefOf.Cosmere_Scadrial_Thing_MetalmindImplant) {
                    ScadrianUtility.AddImplantedMetalminds(pawn, metal, count);
                    continue;
                }

                // Metalminds have a stack limit of one, so each is its own thing.
                for (int i = 0; i < count; i++) {
                    pawn.inventory.innerContainer.TryAdd(ThingMaker.MakeThing(kind, metal.Item));
                }
            }
        }

        StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower.Worker.ClearCacheForThing(pawn);
        StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower.Worker.ClearCacheForThing(pawn);
    }

    private void SpawnRashek(Pawn nearby) {
        Map? map = nearby.MapHeld;
        if (map == null) return;

        Pawn rashek = GeneratePawn(
            Gender.Male,
            XenotypeDefOf.Cosmere_Scadrial_Xenotype_Terris,
            21f
        );

        rashek.Name = new NameTriple("Lord", "Rashek", "Ruler");
        rashek.ageTracker.AgeChronologicalTicks = 1050 * 3600000L;

        GeneUtility.AddMistborn(rashek, false, true);
        GeneUtility.AddFullFeruchemist(rashek, false, true);

        rashek.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
        rashek.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium);
        ScadrianUtility.SetMetallicArtsSkills(rashek, 20);

        // A full Feruchemist would otherwise walk out under a couple of hundred metalminds.
        StockUp(
            rashek,
            [
                (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, 1),
                (ThingDefOf.Cosmere_Scadrial_Thing_MetalmindImplant, 3),
            ]
        );

        if (rashek.needs.TryGetNeed(out Investiture investiture)) {
            investiture.CurLevel = 10;
        }

        GenSpawn.Spawn(rashek, CellFinder.RandomClosewalkCellNear(nearby.Position, map, 5), map);
    }

    private Pawn GeneratePawn(Gender gender, XenotypeDef xenotype, float? fixedAge = null) {
        return PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                RimWorld.PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                tile: Current.Game.CurrentMap.Tile,
                forcedXenotype: xenotype,
                fixedGender: gender,
                fixedBiologicalAge: fixedAge,
                biologicalAgeRange: fixedAge.HasValue ? null : new FloatRange(40, 60)
            )
        );
    }
}
