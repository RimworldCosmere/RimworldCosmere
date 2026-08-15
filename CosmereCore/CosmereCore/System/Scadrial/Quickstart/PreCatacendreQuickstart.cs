using Cosmere.Core.Def;
using Cosmere.Core.Need;
using Cosmere.Core.Quickstart;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Dev;
using Cosmere.System.Scadrial.Hemalurgy;
using RimWorld;
using UnityEngine;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Quickstart;

public class PreCatacendreQuickstart : AbstractQuickstart {
    public override int mapSize => 100;

    public override TaggedString description => "Used to test Pre-catacendre pawns";

    public override StorytellerDef storyteller => StorytellerDefOf.Cassandra;

    public override DifficultyDef difficulty => DifficultyDefOf.Easy;

    public override ScenarioDef scenario => ScenarioDefOf.Cosmere_Scadrial_Scenario_PreCatacendre;

    // Nothing beyond what the scenario itself sets, which is Ruin and Preservation. Adding Honor,
    // Cultivation and Odium here turned a Mistborn colony into one with highstorms scheduled and
    // Rosharan genes rolling on every pawn. The All-Stars quickstart is where Roshar content gets
    // exercised.
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

    public override void PrepareColonists(List<Pawn> pawns) {
        if (pawns.Count == 0) return;

        for (int i = 0; i < pawns.Count; i++) {
            StockUp(pawns[i]);
        }

        SpawnRashek(pawns[0]);
        SpawnHuman(pawns[0]);
        LayOutTheKolossBench(pawns[0]);
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

    /// <summary>How many to stand up. One tells you nothing about a band of them.</summary>
    private const int Band = 10;

    /// <summary>
    ///     Human, the koloss who took a name, and nine who did not.
    /// </summary>
    /// <remarks>
    ///     Deliberately here rather than in the scenario. He belongs to the Hero of Ages, not to a
    ///     Pre-Catacendre colony, and a scenario roster is meant to read as the crew you started
    ///     with. What he is for is a koloss that already exists to look at, without spending four
    ///     spikes and a colonist to get one.
    /// </remarks>
    private static void SpawnHuman(Pawn nearby) {
        Map? map = nearby.Map;
        if (map == null) return;

        PawnKindDef? kind = DefDatabase<PawnKindDef>.GetNamedSilentFail("Cosmere_Scadrial_PawnKind_Koloss");
        XenotypeDef? koloss = DefDatabase<XenotypeDef>.GetNamedSilentFail(
            Util.KolossUtility.KolossXenotype
        );
        if (kind == null || koloss == null) return;

        // Ten of them, spread across the whole growth range, because one koloss says nothing about
        // what a band of them looks like - and size is the thing that varies most.
        for (int i = 0; i < Band; i++) {
            // No faction. A koloss in the colony is already somebody's, and the point of having
            // them stand there is to seize them - which needs them to belong to nobody first.
            Pawn one = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                kind,
                null,
                PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                canGeneratePawnRelations: false,
                fixedGender: Gender.Male,
                forcedXenotype: koloss,
                fixedBiologicalAge: 24f
            ));

            // The one koloss anybody ever called anything, and nine nobody did.
            if (i == 0) one.Name = new NameSingle("Human", false);

            // fixedChronologicalAge on the request does not survive generation - the tracker has
            // to be written afterwards. Years a koloss, in a body that was grown when it got it.
            if (one.ageTracker != null) {
                one.ageTracker.AgeChronologicalTicks = (i + 1) * GenDate.TicksPerYear;
            }

            Grow(one, Band == 1 ? 0.5f : i / (float)(Band - 1));

            GenSpawn.Spawn(one, CellFinder.RandomClosewalkCellNear(nearby.Position, map, 10), map);
        }
    }

    /// <summary>
    ///     Ages one forward, so the band shows the whole range from newly spiked to about to split.
    /// </summary>
    private static void Grow(Pawn koloss, float along) {
        Verse.Hediff? growth = koloss.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );
        if (growth == null) return;

        // Never a literal zero. Hediff.ShouldRemove is Severity <= 0f, so the hediff would delete
        // itself on the next tick and the koloss would have no growth at all.
        growth.Severity = Mathf.Max(0.001f, along);
    }

    /// <summary>
    ///     Everything the make-koloss bill needs, on the ground next to the colony.
    /// </summary>
    /// <remarks>
    ///     The bill wants four charged iron spikes and somewhere to lie down, and building both by
    ///     hand is several minutes of setup before the thing under test can be reached at all.
    ///     Charged with stolen human strength, which is what iron takes and what a koloss is made
    ///     out of - an uncharged spike is refused by the bill on purpose.
    /// </remarks>
    private static void LayOutTheKolossBench(Pawn nearby) {
        Map? map = nearby.Map;
        if (map == null) return;

        ThingDef? spikeDef = DefDatabase<ThingDef>.GetNamedSilentFail("Cosmere_Scadrial_Thing_HemalurgicSpike");
        ThingDef? iron = DefDatabase<ThingDef>.GetNamedSilentFail("Iron");

        if (spikeDef != null && iron != null) {
            for (int i = 0; i < 4; i++) {
                Verse.Thing spike = ThingMaker.MakeThing(spikeDef, iron);
                spike.TryGetComp<Hemalurgy.Comp.Thing.HemalurgicSpike>()?.Charge(new HemalurgicChargeData {
                    stealType = HemalurgicStealType.HumanStrength,
                    strength = 1f,
                    chargedTick = Find.TickManager?.TicksGame ?? 0,
                });

                GenPlace.TryPlaceThing(
                    spike,
                    CellFinder.RandomClosewalkCellNear(nearby.Position, map, 4),
                    map,
                    ThingPlaceMode.Near
                );
            }
        }

        ThingDef? medicine = DefDatabase<ThingDef>.GetNamedSilentFail("MedicineUltratech");
        if (medicine != null) {
            Verse.Thing stack = ThingMaker.MakeThing(medicine);
            stack.stackCount = 20;
            GenPlace.TryPlaceThing(
                stack,
                CellFinder.RandomClosewalkCellNear(nearby.Position, map, 4),
                map,
                ThingPlaceMode.Near
            );
        }

        ThingDef? bedDef = DefDatabase<ThingDef>.GetNamedSilentFail("HospitalBed");
        if (bedDef == null) return;

        ThingDef stuff = GenStuff.DefaultStuffFor(bedDef);
        Verse.Thing bed = ThingMaker.MakeThing(bedDef, stuff);
        GenSpawn.Spawn(bed, CellFinder.RandomClosewalkCellNear(nearby.Position, map, 6), map, Rot4.South);
        bed.SetFaction(Faction.OfPlayer);

        // A hospital bed still has to be flagged medical, or no surgery bill will ever be taken to it.
        if (bed is Building_Bed built) built.Medical = true;
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
