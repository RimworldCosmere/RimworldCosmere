using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereResources.ModExtensions;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Things;

public class AllomanticVial : ThingWithComps {
    protected List<MetallicArtsMetalDef> metals => def.GetModExtension<MetalsLinked>().Metals
        .Select(MetallicArtsMetalDef.GetFromMetalDef).ToList();

    public override bool IngestibleNow {
        get {
            if (!base.IngestibleNow) {
                return false;
            }

            Pawn pawn;
            if (holdingOwner.Owner is Pawn_InventoryTracker tracker) {
                pawn = tracker?.pawn;
            } else if (Find.Selector?.SingleSelectedThing is Pawn selectedThing) {
                pawn = selectedThing;
            } else if (Spawned && Position.GetFirstPawn(Map) is { } standingPawn) {
                pawn = standingPawn;
            } else {
                return false;
            }

            if (InvestitureDetector.IsShielded(pawn)) return false;

            if (PawnUtility.IsAsleep(pawn)) return false;

            if (metals.Count > 1) {
                return AllomancyUtility.IsMistborn(pawn) &&
                       metals.Any(metal => AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f);
            }

            MetallicArtsMetalDef? metal = metals.First();
            if (!AllomancyUtility.CanUseMetal(pawn, metal)) {
                return false;
            }

            return AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f;
        }
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn) {
        if (!pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Cosmere_Metalborn"))) {
            yield return new FloatMenuOption("Cannot ingest: Not Allomancer", null);
            yield break;
        }

        if (metals.Count > 1) {
            if (AllomancyUtility.IsMistborn(pawn) &&
                metals.Any(metal => AllomancyUtility.GetReservePercent(pawn, metal) < 0.8f)) {
                foreach (FloatMenuOption? option in base.GetFloatMenuOptions(pawn)) {
                    yield return option;
                }

                yield break;
            }
        }

        if (InvestitureDetector.IsShielded(pawn)) {
            yield return new FloatMenuOption("Cannot ingest: currently shielded", null);
            yield break;
        }

        MetallicArtsMetalDef? metal = metals.First();
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
        base.PostIngested(ingester);

        metals.ForEach(metal => {
            AllomancyUtility.AddMetalReserve(ingester, metal, MetalReserves.MaxAmount); // or adjust amount
        });
        Messages.Message(
            $"{ingester.LabelShortCap} downed a vial containing: {string.Join(", ", metals.Select(x => x.LabelCap))}.",
            ingester, MessageTypeDefOf.PositiveEvent);
    }
}