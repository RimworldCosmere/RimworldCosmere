using System;
using CosmereCore.Extension;
using CosmereFramework.Extension;
using CosmereScadrial.Def;
using CosmereScadrial.Extension;
using CosmereScadrial.Gene;
using CosmereScadrial.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereScadrial.FloatMenuOptionProvider;

public class AllomanticVialMenuProvider : RimWorld.FloatMenuOptionProvider {
    private static readonly StatDef AllomanticPower = StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower;
    private static readonly StatDef FeruchemicPower = StatDefOf.Cosmere_Scadrial_Stat_FeruchemicPower;
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    private static FloatMenuOption CannotIngestReason(string reason) {
        return new FloatMenuOption("CS_CannotIngest".Translate() + ": " + reason, null);
    }

    private AcceptanceReport CanIngestGodMetal(
        Pawn pawn,
        MetallicArtsMetalDef metal,
        MetallicArtsMetalDef? stuffMetal
    ) {
        bool isAllomancy = metal.IsOneOf(MetallicArtsMetalDefOf.Lerasium, MetallicArtsMetalDefOf.LerasiumAlloy);
        float power = pawn.GetStatValue(isAllomancy ? AllomanticPower : FeruchemicPower);
        float max = isAllomancy ? AllomanticPower.maxValue : FeruchemicPower.maxValue;

        if (metal.Equals(MetallicArtsMetalDefOf.Lerasium)) {
            if (pawn.IsMistborn() && power.Equals(max)) {
                return "CS_AlreadyMistborn".Translate();
            }
        }

        if (metal.Equals(MetallicArtsMetalDefOf.Leratium)) {
            if (pawn.IsFullFeruchemist() && power.Equals(max)) {
                return "CS_AlreadyFullFeruchemist".Translate();
            }
        }

        if (stuffMetal != null) {
            if (metal.Equals(MetallicArtsMetalDefOf.LerasiumAlloy)) {
                if (pawn.IsMisting(stuffMetal) && power.Equals(max)) {
                    return "CS_AlreadyMisting".Translate(metal.allomancy!.userName.CapitalizeFirst());
                }
            }

            if (metal.Equals(MetallicArtsMetalDefOf.LeratiumAlloy)) {
                if (pawn.IsFerring(stuffMetal) && power.Equals(max)) {
                    return "CS_AlreadyFerring".Translate(metal.feruchemy!.userName.CapitalizeFirst());
                }
            }
        }


        return metal.godMetal;
    }

    private AcceptanceReport CanIngest(Pawn pawn, MetallicArtsMetalDef metal, MetallicArtsMetalDef? stuffMetal) {
        if (metal.godMetal) {
            return CanIngestGodMetal(pawn, metal, stuffMetal);
        }

        if (metal.allomancy == null) {
            return "CS_NoAllomanticProperties".Translate();
        }

        if (!pawn.IsAllomancer()) {
            return "CS_NotAllomancer".Translate();
        }

        if (pawn.IsShieldedAgainstInvestiture()) {
            return "CS_CurrentlyShielded".Translate();
        }

        Allomancer? gene = pawn.genes.GetAllomanticGeneForMetal(metal);
        if (gene is null) {
            return "CS_NotMisting".Translate(metal.allomancy!.userName.Named("MISTING"));
        }

        if (gene.Value > .9f) {
            return "CS_TooFull".Translate();
        }

        return true;
    }

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not AllomanticVial vial) return null;
        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn is null) return null;
        MetallicArtsMetalDef metal = vial.metal;
        MetallicArtsMetalDef? stuffMetal = vial.Stuff != null
            ? DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(vial.Stuff.defName)
            : null;

        AcceptanceReport report = CanIngest(pawn, metal, stuffMetal);
        if (!report) {
            return CannotIngestReason(report.Reason);
        }

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