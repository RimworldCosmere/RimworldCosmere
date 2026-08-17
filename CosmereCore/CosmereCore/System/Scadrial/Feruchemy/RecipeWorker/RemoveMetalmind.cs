using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.RecipeWorker;

public class RemoveMetalmind : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn pawn) return false;
        return GetMetalmindsHediff(pawn) != null;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        ImplantedMetalminds? hediff = GetMetalmindsHediff(pawn);
        if (hediff?.Part != null) {
            yield return hediff.Part;
        }
    }

    public override void ApplyOnPawn(
        Pawn pawn,
        BodyPartRecord part,
        Pawn billDoer,
        List<Verse.Thing> ingredients,
        Bill bill
    ) {
        if (billDoer != null) {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) {
                return;
            }

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        ImplantedMetalminds? hediff = GetMetalmindsHediff(pawn);
        if (hediff == null || hediff.metalmindCount == 0) return;

        ImplantedMetalmindData? removed = hediff.RemoveMetalmindAt(hediff.metalmindCount - 1);
        if (removed == null) return;

        SpawnExtractedMetalmind(pawn, removed);

        DamageInfo damage = new DamageInfo(DamageDefOf.SurgicalCut, 5f, 0f, -1f, billDoer, part);
        pawn.TakeDamage(damage);
    }

    private void SpawnExtractedMetalmind(Pawn pawn, ImplantedMetalmindData removed) {
        ThingDef? metalmindDef = DefDatabase<ThingDef>.GetNamedSilentFail(removed.metalmindType);
        if (metalmindDef == null) return;

        ThingDef? stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(removed.metalDefName);

        Verse.Thing metalmindItem = ThingMaker.MakeThing(metalmindDef, stuffDef);
        Metalmind? metalmindComp = metalmindItem.TryGetComp<Metalmind>();
        if (metalmindComp != null) {
            if (removed.StoredAmount > 0f) {
                float accepted = metalmindComp.AddStored(removed.StoredAmount);

                // AddStored clamps to the item's free space, which may differ from what
                // the implant held, so attribution is scaled to what actually landed.
                Dictionary<string, float> transferred = removed.AttributionSnapshot();
                ChargeAttribution.Rescale(transferred, accepted);
                metalmindComp.ReceiveAttribution(transferred);
            }

            if (removed.CompoundedAmount > 0f) metalmindComp.AddCompounded(removed.CompoundedAmount);
        }

        GenPlace.TryPlaceThing(metalmindItem, pawn.Position, pawn.Map, ThingPlaceMode.Near);
    }

    private ImplantedMetalminds? GetMetalmindsHediff(Pawn pawn) {
        return pawn.health.hediffSet.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds
        ) as ImplantedMetalminds;
    }
}
