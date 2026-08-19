using Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.RecipeWorker;

public class RemoveSpike : Recipe_Surgery {
    public override bool AvailableOnNow(Verse.Thing thing, BodyPartRecord? part = null) {
        if (!base.AvailableOnNow(thing, part)) return false;
        if (thing is not Pawn pawn) return false;
        return GetSpikesHediff(pawn) != null;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe) {
        HemalurgicSpikes? hediff = GetSpikesHediff(pawn);
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

        HemalurgicSpikes? hediff = GetSpikesHediff(pawn);
        if (hediff == null || hediff.spikeCount == 0) return;

        ImplantedSpikeData? removed = hediff.RemoveSpikeAt(hediff.spikeCount - 1);
        if (removed == null) return;

        RemoveGrantedGene(pawn, removed);
        SpawnExtractedSpike(pawn, removed);

        DamageInfo damage = new DamageInfo(DamageDefOf.SurgicalCut, 10f, 0f, -1f, billDoer, part);
        pawn.TakeDamage(damage);

        UpdateRuinsInfluence(pawn);

        // a kandra is two spikes - reconcile now, not on the next slow tick, while the surgeon is watching
        KandraUtility.ReconcileSpikes(pawn);
    }

    private void RemoveGrantedGene(Pawn pawn, ImplantedSpikeData spike) {
        if (HemalurgicConstants.IsAllomanticSteal(spike.stealType) ||
            HemalurgicConstants.IsFeruchemicSteal(spike.stealType) ||
            spike.stealType == HemalurgicStealType.AnyPower) {
            TryRemoveGene(pawn, spike.stolenDefName);
        } else if (spike.stealType == HemalurgicStealType.AllAbilities) {
            for (int i = 0; i < spike.stolenDefNames.Count; i++) {
                TryRemoveGene(pawn, spike.stolenDefNames[i]);
            }
        }
    }

    private void TryRemoveGene(Pawn pawn, string geneDefName) {
        if (geneDefName.NullOrEmpty()) return;
        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
        if (geneDef == null) return;
        Verse.Gene? gene = pawn.genes.GetGene(geneDef);
        if (gene != null) {
            pawn.genes.RemoveGene(gene);
        }
    }

    private void SpawnExtractedSpike(Pawn pawn, ImplantedSpikeData spike) {
        ThingDef? metalStuff = DefDatabase<ThingDef>.GetNamedSilentFail(spike.metalDefName);
        if (metalStuff == null) return;

        ThingDef spikeDef = spike.isThinNeedle
            ? HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicNeedle
            : HemalurgicDefOf.Cosmere_Scadrial_Thing_HemalurgicSpike;

        Verse.Thing spikeItem = ThingMaker.MakeThing(spikeDef, metalStuff);
        HemalurgicSpike? spikeComp = spikeItem.TryGetComp<HemalurgicSpike>();
        if (spikeComp != null) {
            spikeComp.Charge(
                new HemalurgicChargeData {
                    stealType = spike.stealType,
                    stolenDefName = spike.stolenDefName,
                    stolenDefNames = [.. spike.stolenDefNames],
                    strength = spike.chargeStrength * HemalurgicConstants.ExtractionChargeMultiplier,
                    storedInvestiture = spike.storedInvestiture * HemalurgicConstants.ExtractionChargeMultiplier,
                    chargedTick = Find.TickManager.TicksGame,
                }
            );
        }

        GenPlace.TryPlaceThing(spikeItem, pawn.Position, pawn.Map, ThingPlaceMode.Near);
    }

    private void UpdateRuinsInfluence(Pawn pawn) {
        HemalurgicSpikes? spikesHediff = GetSpikesHediff(pawn);
        int spikeCount = spikesHediff?.spikeCount ?? 0;

        RuinsInfluence? ruinsInfluence = (RuinsInfluence?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_RuinsInfluence
        );
        if (ruinsInfluence != null) {
            if (spikeCount == 0) {
                pawn.health.RemoveHediff(ruinsInfluence);
            } else {
                ruinsInfluence.UpdateSpikeCount(spikeCount);
            }
        }
    }

    private HemalurgicSpikes? GetSpikesHediff(Pawn pawn) {
        return (HemalurgicSpikes?)pawn.health.hediffSet.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
    }
}
