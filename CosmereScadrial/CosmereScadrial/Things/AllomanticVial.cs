using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereResources.ModExtensions;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Extensions;
using CosmereScadrial.Genes;
using CosmereScadrial.Utils;
using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Things;

public class AllomanticVial : ThingWithComps {
    public List<MetallicArtsMetalDef> metals => def.GetModExtension<MetalsLinked>().Metals
        .Select(x => x.ToMetallicArts()).ToList();

    public override bool IngestibleNow {
        get {
            if (!base.IngestibleNow) {
                return false;
            }

            Pawn? pawn = null;
            if (holdingOwner.Owner is Pawn_InventoryTracker tracker) {
                pawn = tracker?.pawn;
            } else if (Find.Selector?.SingleSelectedThing is Pawn selectedThing) {
                pawn = selectedThing;
            } else if (Spawned && Position.GetFirstPawn(Map) is { } standingPawn) {
                pawn = standingPawn;
            }

            if (pawn == null) {
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

    public bool IsForMetal(MetallicArtsMetalDef metal, bool singleMetal = true) {
        if (singleMetal && metals.Count > 1) return false;

        return metals.Contains(metal);
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn) {
        if (!pawn.story.traits.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Allomancer)) {
            yield return new FloatMenuOption("Cannot ingest: Not Allomancer", null);
            yield break;
        }

        if (InvestitureDetector.IsShielded(pawn)) {
            yield return new FloatMenuOption("Cannot ingest: currently shielded", null);
            yield break;
        }

        if (metals.Count > 1) {
            if (pawn.IsMistborn() &&
                metals.Any(x => pawn.genes.GetAllomanticGeneForMetal(x)?.ShouldConsumeVialNow() ?? false)) {
                foreach (FloatMenuOption? option in base.GetFloatMenuOptions(pawn)) {
                    yield return option;
                }

                yield break;
            }
        }

        MetallicArtsMetalDef? metal = metals.First();
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        if (gene == null) {
            yield return new FloatMenuOption("Cannot ingest: not a " + metal.allomancy!.userName, null);
            yield break;
        }

        if (Mathf.Approximately(gene.Value, 1) || !gene.ShouldConsumeVialNow()) {
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