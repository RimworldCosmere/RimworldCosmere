using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;
using GeneUtility = CosmereScadrial.Utils.GeneUtility;

namespace CosmereScadrial.Things;

public class AllomanticMetal : AllomanticVial {
    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn) {
        if (metals.Count > 1) {
            if (AllomancyUtility.IsMistborn(pawn) &&
                metals.Any(metal => AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f)) {
                foreach (FloatMenuOption? option in base.GetFloatMenuOptions(pawn)) {
                    yield return option;
                }

                yield break;
            }
        }

        MetallicArtsMetalDef? metal = metals.First();
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer) && !metal.godMetal) {
            yield return new FloatMenuOption("Cannot ingest: Not Allomancer", null);
            yield break;
        }

        if (metal.Equals(MetallicArtsMetalDefOf.Lerasium) && AllomancyUtility.IsMistborn(pawn)) {
            yield return new FloatMenuOption("Cannot ingest: already consumed", null);
            yield break;
        }

        if (!AllomancyUtility.CanUseMetal(pawn, metal)) {
            yield return new FloatMenuOption("Cannot ingest: wrong metal", null);
            yield break;
        }

        if (AllomancyUtility.GetReservePercent(pawn, metal) >= 0.8f) {
            yield return new FloatMenuOption("Cannot ingest: reserve too full", null);
            yield break;
        }

        // Default ingest option
        foreach (FloatMenuOption? option in base.GetFloatMenuOptions(pawn)) {
            yield return option;
        }
    }

    protected override void PostIngested(Pawn ingester) {
        foreach (ThingComp? t in AllComps) {
            t.PostIngested(ingester);
        }

        if (metals.Count > 1) {
            metals.ForEach(metal => {
                AllomancyUtility.AddMetalReserve(ingester, metal, 100f); // or adjust amount
            });
            Messages.Message(
                $"{ingester.LabelShortCap} downed metal containing: {string.Join(", ", metals.Select(x => x.LabelCap))}.",
                ingester, MessageTypeDefOf.PositiveEvent);

            return;
        }

        MetallicArtsMetalDef? metal = metals.First();
        if (!metal.godMetal) {
            AllomancyUtility.AddMetalReserve(ingester, metal, 100f); // or adjust amount
            Messages.Message($"{ingester.LabelShortCap} downed some {metal.LabelCap}.", ingester,
                MessageTypeDefOf.PositiveEvent);
            return;
        }

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
        }
    }
}