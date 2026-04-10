using System.Collections.Generic;
using System.Linq;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.RecipeWorker;

public class ImplantMetalmind : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn) return false;
        if (!ResearchProjectDef.Named("Cosmere_Scadrial_Feruchemy").IsFinished) return false;
        return true;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        BodyPartRecord torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == BodyPartDefOf.Torso);
        if (torso != null) {
            yield return torso;
        }
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Verse.Thing> ingredients, Bill bill) {
        Metalmind? metalmindComp = FindMetalmind(ingredients);
        if (metalmindComp == null) {
            Messages.Message("CS_Feruchemy_NoMetalmind".Translate(), pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        if (billDoer != null) {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) {
                return;
            }
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        ImplantedMetalmindData data = new ImplantedMetalmindData {
            metalDefName = metalmindComp.metal.defName,
            metalmindType = metalmindComp.parent.def.defName,
            storedAmount = metalmindComp.storedAmount,
            maxAmount = metalmindComp.maxAmount,
            ownerName = metalmindComp.owner?.Name?.ToStringFull ?? "",
        };

        AddToUnifiedHediff(pawn, data, part);

        Messages.Message(
            "CS_Feruchemy_ImplantSuccess".Translate(
                billDoer.Named("SURGEON"),
                metalmindComp.metal.Named("METAL"),
                pawn.Named("RECIPIENT")
            ),
            pawn, MessageTypeDefOf.PositiveEvent
        );
    }

    private void AddToUnifiedHediff(Pawn pawn, ImplantedMetalmindData data, BodyPartRecord part) {
        ImplantedMetalminds? hediff = (ImplantedMetalminds?)pawn.health.hediffSet.GetFirstHediffOfDef(
            DefDatabase<HediffDef>.GetNamed("Cosmere_Scadrial_Hediff_ImplantedMetalminds")
        );
        if (hediff == null) {
            hediff = (ImplantedMetalminds)HediffMaker.MakeHediff(
                DefDatabase<HediffDef>.GetNamed("Cosmere_Scadrial_Hediff_ImplantedMetalminds"), pawn, part
            );
            pawn.health.AddHediff(hediff, part);
        }
        hediff.AddMetalmind(data);
    }

    private Metalmind? FindMetalmind(List<Verse.Thing> ingredients) {
        for (int i = 0; i < ingredients.Count; i++) {
            Metalmind? comp = ingredients[i].TryGetComp<Metalmind>();
            if (comp != null) return comp;
        }
        return null;
    }
}
