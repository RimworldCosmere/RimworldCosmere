using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Utils.GeneUtility;

namespace CosmereScadrial.Things;

public class AllomanticMetal : AllomanticVial {
    protected override void PostIngested(Pawn ingester) {
        MetallicArtsMetalDef? metal = metals.First();
        if (metal.defName == MetallicArtsMetalDefOf.Lerasium.defName) {
            SnapUtility.TrySnap(ingester, "ingested Lerasium", false);
            GeneUtility.AddMistborn(ingester, false, true);
            MetalReserves? comp = ingester.GetComp<MetalReserves>();
            foreach (MetallicArtsMetalDef? m in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                comp.SetReserve(m, MetalReserves.MaxAmount);
            }

            Find.LetterStack.ReceiveLetter(
                "Pawn burned Lerasium!",
                $"{ingester.NameFullColored} consumes the god metal. In an instant, their spirit web reshapes — all paths of Allomancy now lie open.",
                LetterDefOf.PositiveEvent,
                ingester
            );
            return;
        }

        base.PostIngested(ingester);
    }
}