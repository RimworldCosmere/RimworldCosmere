using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.RecipeWorker;

public class ImplantSpike : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn) return false;
        if (!HemalurgicDefOf.Cosmere_Scadrial_Hemalurgy.IsFinished) return false;
        if (!IsHemalurgyEnabled()) return false;
        return true;
    }

    private static bool IsHemalurgyEnabled() {
        return ShardUtility.AreAnyEnabled(ShardDefOf.Ruin, ShardDefOf.Harmony);
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        BodyPartRecord? torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == pawn.RaceProps.body.corePart.def);
        if (torso != null) {
            yield return torso;
        }
    }

    public override void ApplyOnPawn(
        Pawn pawn,
        BodyPartRecord part,
        Pawn billDoer,
        List<Verse.Thing> ingredients,
        Bill bill
    ) {
        if (!IsHemalurgyEnabled()) return;

        HemalurgicSpike? spikeComp = FindChargedSpike(ingredients);
        if (spikeComp == null) {
            Messages.Message("CS_Hemalurgy_NoChargedSpike".Translate(), pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        if (spikeComp.chargeData!.stealType == HemalurgicStealType.RemoveAllPowers) {
            Messages.Message("CS_Hemalurgy_CannotImplantAluminum".Translate(), pawn, MessageTypeDefOf.RejectInput);
            return;
        }

        if (billDoer != null) {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) {
                return;
            }

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        HemalurgicChargeData charge = spikeComp.chargeData;
        float strength = spikeComp.CurrentStrength;
        bool isThinNeedle = spikeComp.parent.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;

        ImplantedSpikeData spikeData = new ImplantedSpikeData {
            metalDefName = spikeComp.Metal?.defName ?? "Unknown",
            stealType = charge.stealType,
            stolenDefName = charge.stolenDefName,
            stolenDefNames = [.. charge.stolenDefNames],
            chargeStrength = strength,
            storedInvestiture = charge.storedInvestiture,
            isThinNeedle = isThinNeedle,
        };

        HemalurgicImplantUtility.ApplyHemalurgicEffect(pawn, spikeData);
        HemalurgicImplantUtility.AddToUnifiedHediff(pawn, spikeData, part);
        HemalurgicImplantUtility.GrantStoredInvestiture(pawn, charge.storedInvestiture);
        HemalurgicImplantUtility.RemoveDrabIfPresent(pawn);
        HemalurgicImplantUtility.UpdateRuinsInfluence(pawn);

        Messages.Message(
            "CS_Hemalurgy_ImplantSuccess".Translate(
                billDoer.Named("SURGEON"),
                spikeComp.Metal?.Named("METAL") ?? "unknown".Named("METAL"),
                pawn.Named("RECIPIENT")
            ),
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    private static HemalurgicSpike? FindChargedSpike(List<Verse.Thing> ingredients) {
        for (int i = 0; i < ingredients.Count; i++) {
            HemalurgicSpike? comp = ingredients[i].TryGetComp<HemalurgicSpike>();
            if (comp is { IsCharged: true }) return comp;
        }

        return null;
    }
}
