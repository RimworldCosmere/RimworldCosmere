using System.Collections.Generic;
using System.Linq;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Dialog;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.RecipeWorker;

public class ChargeSpike : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn) return false;
        if (!ResearchProjectDef.Named("Cosmere_Scadrial_Hemalurgy").IsFinished) return false;
        return true;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        BodyPartRecord torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == BodyPartDefOf.Torso);
        if (torso != null) {
            yield return torso;
        }
    }

    public override void ApplyOnPawn(Pawn donor, BodyPartRecord part, Pawn billDoer, List<Verse.Thing> ingredients, Bill bill) {
        HemalurgicSpike? spikeComp = FindUnchargedSpike(ingredients);
        if (spikeComp == null) {
            Messages.Message("CS_Hemalurgy_NoUnchargedSpike".Translate(), donor, MessageTypeDefOf.RejectInput);
            return;
        }

        MetallicArtsMetalDef metal = spikeComp.metal;
        HemalurgicStealType stealType = HemalurgicConstants.GetStealType(metal);
        bool isThinNeedle = spikeComp.parent.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;
        float strengthMultiplier = isThinNeedle ? HemalurgicConstants.ThinNeedleStrengthMultiplier : 1f;

        // Remove spike from ingredients to prevent the recipe system from destroying it
        ingredients.Remove(spikeComp.parent);

        if (HemalurgicConstants.RequiresSelection(stealType)) {
            List<GeneDef> candidates = StealTargetSelector.GetStealCandidates(donor, stealType);
            if (candidates.Count == 0) {
                Messages.Message(
                    "CS_Hemalurgy_NothingToSteal".Translate(donor.Named("DONOR")),
                    donor, MessageTypeDefOf.RejectInput
                );
                DropSpike(spikeComp.parent, billDoer);
                return;
            }

            if (candidates.Count == 1) {
                HemalurgicChargeUtility.PerformCharge(
                    donor, billDoer, spikeComp, stealType, candidates[0],
                    strengthMultiplier, true, isThinNeedle
                );
                DropSpike(spikeComp.parent, billDoer);
                return;
            }

            StealTargetSelector.ShowSelectionDialog(
                donor, stealType,
                onSelected: (geneDef) => {
                    HemalurgicChargeUtility.PerformCharge(
                        donor, billDoer, spikeComp, stealType, geneDef,
                        strengthMultiplier, true, isThinNeedle
                    );
                    DropSpike(spikeComp.parent, billDoer);
                },
                onCancel: () => {
                    Messages.Message(
                        "CS_Hemalurgy_NothingToSteal".Translate(donor.Named("DONOR")),
                        donor, MessageTypeDefOf.RejectInput
                    );
                    DropSpike(spikeComp.parent, billDoer);
                }
            );
        } else {
            HemalurgicChargeUtility.PerformCharge(
                donor, billDoer, spikeComp, stealType, null,
                strengthMultiplier, true, isThinNeedle
            );
            DropSpike(spikeComp.parent, billDoer);
        }
    }

    private static void DropSpike(Verse.Thing spike, Pawn dropper) {
        if (dropper.MapHeld == null) return;

        if (spike.Destroyed) {
            Verse.Thing newSpike = ThingMaker.MakeThing(spike.def, spike.Stuff);
            Comp.Thing.HemalurgicSpike? oldComp = spike.TryGetComp<Comp.Thing.HemalurgicSpike>();
            Comp.Thing.HemalurgicSpike? newComp = newSpike.TryGetComp<Comp.Thing.HemalurgicSpike>();
            if (oldComp?.chargeData != null && newComp != null) {
                newComp.Charge(oldComp.chargeData);
            }
            GenPlace.TryPlaceThing(newSpike, dropper.Position, dropper.MapHeld, ThingPlaceMode.Near);
        } else if (!spike.Spawned) {
            GenPlace.TryPlaceThing(spike, dropper.Position, dropper.MapHeld, ThingPlaceMode.Near);
        }
    }

    private HemalurgicSpike? FindUnchargedSpike(List<Verse.Thing> ingredients) {
        for (int i = 0; i < ingredients.Count; i++) {
            HemalurgicSpike? comp = ingredients[i].TryGetComp<HemalurgicSpike>();
            if (comp != null && !comp.isCharged) return comp;
        }
        return null;
    }
}
