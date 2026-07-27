using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Util;
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
        BodyPartRecord torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == BodyPartDefOf.Torso);
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

        HemalurgicChargeData charge = spikeComp.chargeData!;
        float strength = spikeComp.currentStrength;
        bool isThinNeedle = spikeComp.parent.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle;

        ImplantedSpikeData spikeData = new ImplantedSpikeData {
            metalDefName = spikeComp.metal?.defName ?? "Unknown",
            stealType = charge.stealType,
            stolenDefName = charge.stolenDefName,
            stolenDefNames = [.. charge.stolenDefNames],
            chargeStrength = strength,
            storedInvestiture = charge.storedInvestiture,
            isThinNeedle = isThinNeedle,
        };

        ApplyHemalurgicEffect(pawn, spikeData);
        AddToUnifiedHediff(pawn, spikeData, part);
        GrantStoredInvestiture(pawn, charge.storedInvestiture);
        RemoveDrabIfPresent(pawn);
        UpdateRuinsInfluence(pawn);

        Messages.Message(
            "CS_Hemalurgy_ImplantSuccess".Translate(
                billDoer.Named("SURGEON"),
                spikeComp.metal?.Named("METAL") ?? "unknown".Named("METAL"),
                pawn.Named("RECIPIENT")
            ),
            pawn,
            MessageTypeDefOf.PositiveEvent
        );
    }

    private void ApplyHemalurgicEffect(Pawn pawn, ImplantedSpikeData spike) {
        if (HemalurgicConstants.IsAllomanticSteal(spike.stealType) ||
            HemalurgicConstants.IsFeruchemicSteal(spike.stealType) ||
            spike.stealType == HemalurgicStealType.AnyPower) {
            GrantGene(pawn, spike.stolenDefName);
        } else if (spike.stealType == HemalurgicStealType.AllAbilities) {
            for (int i = 0; i < spike.stolenDefNames.Count; i++) {
                GrantGene(pawn, spike.stolenDefNames[i]);
            }
        } else if (spike.stealType == HemalurgicStealType.ConnectionIdentity) {
            ApplyConnectionImplant(pawn);
        }
    }

    private void GrantGene(Pawn pawn, string geneDefName) {
        if (geneDefName.NullOrEmpty()) return;
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
        if (geneDef == null) return;
        if (pawn.genes.HasActiveGene(geneDef)) return;
        pawn.genes.AddGene(geneDef, true);
    }

    private void ApplyConnectionImplant(Pawn pawn) {
        if (!pawn.IsSnapped()) {
            SnapUtility.Snap(pawn, "CS_Hemalurgy_SnappedByConnection");
        }
    }

    private void GrantStoredInvestiture(Pawn pawn, float amount) {
        if (amount <= 0f) return;
        InvestitureHolder? holder = pawn.TryGetComp<InvestitureHolder>();
        if (holder == null) return;
        holder.currentInvestitureSelf += amount;
    }

    private void RemoveDrabIfPresent(Pawn pawn) {
        Verse.Hediff? drab = pawn.health.hediffSet.GetFirstHediffOfDef(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab);
        if (drab != null) {
            pawn.health.RemoveHediff(drab);
        }
    }

    private void AddToUnifiedHediff(Pawn pawn, ImplantedSpikeData spikeData, BodyPartRecord part) {
        HemalurgicSpikes? hediff = (HemalurgicSpikes?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (hediff == null) {
            hediff = (HemalurgicSpikes)HediffMaker.MakeHediff(
                HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes,
                pawn,
                part
            );
            pawn.health.AddHediff(hediff, part);
        }

        hediff.AddSpike(spikeData);
    }

    private void UpdateRuinsInfluence(Pawn pawn) {
        HemalurgicSpikes? spikesHediff = (HemalurgicSpikes?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        int spikeCount = spikesHediff?.spikeCount ?? 0;

        RuinsInfluence? ruinsInfluence = (RuinsInfluence?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence
        );
        if (ruinsInfluence == null && spikeCount > 0) {
            Verse.Hediff hediff = HediffMaker.MakeHediff(HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence, pawn);
            pawn.health.AddHediff(hediff);
            ruinsInfluence = (RuinsInfluence)hediff;
        }

        ruinsInfluence?.UpdateSpikeCount(spikeCount);
    }

    private HemalurgicSpike? FindChargedSpike(List<Verse.Thing> ingredients) {
        for (int i = 0; i < ingredients.Count; i++) {
            HemalurgicSpike? comp = ingredients[i].TryGetComp<HemalurgicSpike>();
            if (comp is { isCharged: true }) return comp;
        }

        return null;
    }
}
