using System;
using CosmereCore.Extension;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using CosmereScadrial.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.FloatMenuOptionProvider;

public class AllomanticVialMenuProvider : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    private static FloatMenuOption CannotIngestReason(string reason) {
        return new FloatMenuOption("CS_CannotIngest".Translate() + ": " + reason, null);
    }

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not AllomanticVial vial) return null;
        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn is null) return null;
        MetallicArtsMetalDef metal = vial.metal;
        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);

        if (metal.godMetal) {
            if (metal.Equals(MetallicArtsMetalDefOf.Lerasium) && pawn.IsMistborn()) {
                return CannotIngestReason("CS_AlreadyMistborn".Translate());
            }

            if (metal.Equals(MetallicArtsMetalDefOf.Leratium) && pawn.IsFullFeruchemist()) {
                return CannotIngestReason("CS_AlreadyFullFeruchemist".Translate());
            }

            if (clickedThing.Stuff != null) {
                MetallicArtsMetalDef? stuffMetal =
                    DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(clickedThing.Stuff.defName);
                if (metal.Equals(MetallicArtsMetalDefOf.LerasiumAlloy) &&
                    stuffMetal != null &&
                    pawn.IsMisting(stuffMetal)) {
                    "CS_AlreadyMisting".Translate(metal.allomancy!.userName.CapitalizeFirst());
                }

                if (metal.Equals(MetallicArtsMetalDefOf.LeratiumAlloy) &&
                    stuffMetal != null &&
                    pawn.IsFerring(stuffMetal)) {
                    return CannotIngestReason(
                        "CS_AlreadyFerring".Translate(metal.feruchemy!.userName.CapitalizeFirst())
                    );
                }
            }
        } else {
            if (metal.allomancy == null) return CannotIngestReason("CS_NoAllomanticProperties".Translate());
            if (!pawn.IsAllomancer()) return CannotIngestReason("CS_NotAllomancer".Translate());
            if (pawn.IsShieldedAgainstInvestiture()) return CannotIngestReason("CS_CurrentlyShielded".Translate());
        }

        if (gene is null && !metal.godMetal) {
            return CannotIngestReason("CS_NotMisting".Translate(metal.allomancy!.userName.Named("MISTING")));
        }

        if (gene is not null && gene.Value > .9f) return CannotIngestReason("CS_TooFull".Translate());

        string str = !clickedThing.def.ingestible.ingestCommandString.NullOrEmpty()
            ? (string)clickedThing.def.ingestible.ingestCommandString.Formatted((NamedArgument)clickedThing.LabelShort)
            : (string)"ConsumeThing".Translate((NamedArgument)clickedThing.LabelShort, (NamedArgument)clickedThing);
        if (!context.FirstSelectedPawn.CanReach((LocalTargetInfo)clickedThing, PathEndMode.OnCell, Danger.Deadly)) {
            return new FloatMenuOption((string)(str + ": " + "NoPath".Translate().CapitalizeFirst()), null);
        }

        int maxAmountToPickup1 = FoodUtility.GetMaxAmountToPickup(
            clickedThing,
            context.FirstSelectedPawn,
            FoodUtility.WillIngestStackCountOf(
                context.FirstSelectedPawn,
                clickedThing.def,
                FoodUtility.NutritionForEater(context.FirstSelectedPawn, clickedThing)
            )
        );
        FloatMenuOption singleOptionFor = FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                str,
                (Action)(() => {
                    int maxAmountToPickup2 = FoodUtility.GetMaxAmountToPickup(
                        clickedThing,
                        context.FirstSelectedPawn,
                        FoodUtility.WillIngestStackCountOf(
                            context.FirstSelectedPawn,
                            clickedThing.def,
                            FoodUtility.NutritionForEater(context.FirstSelectedPawn, clickedThing)
                        )
                    );
                    if (maxAmountToPickup2 == 0) {
                        return;
                    }

                    clickedThing.SetForbidden(false);
                    Job job = JobMaker.MakeJob(RimWorld.JobDefOf.Ingest, (LocalTargetInfo)clickedThing);
                    job.count = maxAmountToPickup2;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
                }),
                clickedThing is Corpse ? MenuOptionPriority.Low : MenuOptionPriority.Default
            ),
            context.FirstSelectedPawn,
            (LocalTargetInfo)clickedThing
        );
        if (maxAmountToPickup1 == 0) {
            singleOptionFor.action = null;
        }

        return singleOptionFor;
    }
}