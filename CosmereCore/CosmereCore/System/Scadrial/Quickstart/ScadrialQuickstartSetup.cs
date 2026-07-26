using RimWorld;
using Verse;
using Cosmere.Core;
using Cosmere.Core.Quickstart;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Dev;
using Cosmere.System.Scadrial.Gene;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Quickstart;

[StaticConstructorOnStartup]
internal static class ScadrialQuickstartSetup {
    static ScadrialQuickstartSetup() {
        // 0: Wit — Mistborn
        QuickstartCharacterSetupRegistry.Register(0, pawn => {
            GeneUtility.AddMistborn(pawn, false, true);
        });

        // 4: Vin — full Scadrial dev setup
        QuickstartCharacterSetupRegistry.Register(4, ScadrianUtility.PrepareDevPawn);

        // 12: Sazed — full Feruchemist + metalminds
        QuickstartCharacterSetupRegistry.Register(12, pawn => {
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            AddAllMetalminds(pawn);
        });

        // 13: Rashek — Mistborn + full Feruchemist + metalminds + records
        QuickstartCharacterSetupRegistry.Register(13, pawn => {
            GeneUtility.AddMistborn(pawn, false, true);
            GeneUtility.AddFullFeruchemist(pawn, false, true);
            pawn.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLerasium);
            pawn.records.Increment(RecordDefOf.Cosmere_Scadrial_Record_IngestedLeratium);
            AddAllMetalminds(pawn);
            ScadrianUtility.SetMetallicArtsSkills(pawn, 20);
        });

        // 14: Waxillium — Steel misting + Iron ferring + iron metalmind
        QuickstartCharacterSetupRegistry.Register(14, pawn => {
            GeneUtility.AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(MetalDefOf.Steel), false, true);
            GeneUtility.AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(MetalDefOf.Iron), false, true);
            pawn.inventory.innerContainer.TryAdd(
                ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, MetalDefOf.Iron.Item)
            );
        });

        // 15: Wayne — Bendalloy misting + Gold ferring + gold metalmind
        QuickstartCharacterSetupRegistry.Register(15, pawn => {
            GeneUtility.AddGene(pawn, GeneDefOf.GetMistingGeneForMetal(MetalDefOf.Bendalloy), false, true);
            GeneUtility.AddGene(pawn, GeneDefOf.GetFerringGeneForMetal(MetalDefOf.Gold), false, true);
            pawn.inventory.innerContainer.TryAdd(
                ThingMaker.MakeThing(ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand, MetalDefOf.Gold.Item)
            );
        });
    }


    private static void AddAllMetalminds(Pawn pawn) {
        foreach (MetallicArtsMetalDef metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading) {
            if (metal.feruchemy?.userName == null) continue;
            Verse.Thing metalmind = ThingMaker.MakeThing(
                ThingDefOf.Cosmere_Scadrial_Thing_MetalmindBand,
                metal.Item
            );
            pawn.inventory.innerContainer.TryAdd(metalmind);
        }
    }
}
