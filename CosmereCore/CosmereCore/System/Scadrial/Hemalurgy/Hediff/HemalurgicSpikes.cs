using System.Text;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Hediff;

public class HemalurgicSpikes : HediffWithComps {
    private HediffStage? cachedStage;
    public List<ImplantedSpikeData> spikes = [];
    private bool stageDirty = true;

    public int spikeCount => spikes.Count;

    public override HediffStage CurStage {
        get {
            if (stageDirty || cachedStage == null) {
                RebuildStage();
                stageDirty = false;
            }

            return cachedStage!;
        }
    }

    public override bool ShouldRemove => spikes.Count == 0;

    public void AddSpike(ImplantedSpikeData spike) {
        spikes.Add(spike);
        Severity = spikes.Count;
        stageDirty = true;
    }

    public ImplantedSpikeData? RemoveLastSpikeOfType(HemalurgicStealType type, string stolenDefName = "") {
        for (int i = spikes.Count - 1; i >= 0; i--) {
            ImplantedSpikeData spike = spikes[i];
            if (spike.stealType != type) continue;
            if (!stolenDefName.NullOrEmpty() && spike.stolenDefName != stolenDefName) continue;
            spikes.RemoveAt(i);
            Severity = spikes.Count;
            stageDirty = true;
            return spike;
        }

        return null;
    }

    public ImplantedSpikeData? RemoveSpikeAt(int index) {
        if (index < 0 || index >= spikes.Count) return null;
        ImplantedSpikeData spike = spikes[index];
        spikes.RemoveAt(index);
        Severity = spikes.Count;
        stageDirty = true;
        return spike;
    }

    private void RebuildStage() {
        cachedStage = new HediffStage { label = "hemalurgic spikes" };

        float strengthWeight = 0f;
        float sensesWeight = 0f;
        float emotionalWeight = 0f;
        float mentalWeight = 0f;

        for (int i = 0; i < spikes.Count; i++) {
            ImplantedSpikeData spike = spikes[i];
            float mult = spike.chargeStrength;
            switch (spike.stealType) {
                case HemalurgicStealType.HumanStrength:
                    strengthWeight += mult;
                    break;
                case HemalurgicStealType.HumanSenses:
                    sensesWeight += mult;
                    break;
                case HemalurgicStealType.EmotionalFortitude:
                    emotionalWeight += mult;
                    break;
                case HemalurgicStealType.MentalFortitude:
                    mentalWeight += mult;
                    break;
            }
        }

        List<StatModifier> factors = [];
        List<StatModifier> offsets = [];
        List<PawnCapacityModifier> capMods = [];

        if (strengthWeight > 0f) {
            factors.Add(
                new StatModifier { stat = StatDef.Named("MeleeDamageFactor"), value = 1f + strengthWeight * 0.15f }
            );
            factors.Add(new StatModifier { stat = StatDef.Named("MoveSpeed"), value = 1f + strengthWeight * 0.10f });
            factors.Add(
                new StatModifier { stat = StatDef.Named("ArmorRating_Sharp"), value = 1f + strengthWeight * 0.05f }
            );
            factors.Add(
                new StatModifier { stat = StatDef.Named("ArmorRating_Blunt"), value = 1f + strengthWeight * 0.05f }
            );
        }

        if (sensesWeight > 0f) {
            capMods.Add(
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Sight, postFactor = 1f + sensesWeight * 0.20f }
            );
            capMods.Add(
                new PawnCapacityModifier { capacity = PawnCapacityDefOf.Hearing, postFactor = 1f + sensesWeight * 0.20f }
            );
            offsets.Add(new StatModifier { stat = StatDef.Named("ShootingAccuracyPawn"), value = sensesWeight * 3f });
            factors.Add(
                new StatModifier { stat = StatDef.Named("AimingDelayFactor"), value = 1f - sensesWeight * 0.05f }
            );
        }

        if (emotionalWeight > 0f) {
            offsets.Add(
                new StatModifier { stat = StatDef.Named("MentalBreakThreshold"), value = emotionalWeight * -0.10f }
            );
            factors.Add(
                new StatModifier { stat = StatDef.Named("SocialImpact"), value = 1f + emotionalWeight * 0.10f }
            );
        }

        if (mentalWeight > 0f) {
            factors.Add(
                new StatModifier { stat = StatDef.Named("GlobalLearningFactor"), value = 1f + mentalWeight * 0.15f }
            );
            factors.Add(new StatModifier { stat = StatDef.Named("ResearchSpeed"), value = 1f + mentalWeight * 0.10f });
        }

        if (factors.Count > 0) cachedStage.statFactors = factors;
        if (offsets.Count > 0) cachedStage.statOffsets = offsets;
        if (capMods.Count > 0) cachedStage.capMods = capMods;
    }

    public override string GetTooltip(Pawn pawn, bool showHediffsDebugInfo) {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(def.LabelCap);
        sb.AppendLine(def.Description);
        sb.AppendLine();
        sb.AppendLine("CS_Hemalurgy_SpikeCount".Translate(spikes.Count));
        for (int i = 0; i < spikes.Count; i++) {
            ImplantedSpikeData spike = spikes[i];
            string typeLabel = HemalurgicConstants.GetStealTypeLabel(spike.stealType);
            string strengthLabel = spike.chargeStrength switch {
                >= 0.8f => "CS_Hemalurgy_StrengthStrong".Translate(),
                >= 0.5f => "CS_Hemalurgy_StrengthModerate".Translate(),
                >= 0.2f => "CS_Hemalurgy_StrengthWeak".Translate(),
                _ => "CS_Hemalurgy_StrengthFading".Translate(),
            };
            string needle = spike.isThinNeedle ? " (needle)" : string.Empty;
            sb.AppendLine($"  - {spike.metalDefName}{needle}: {typeLabel} ({strengthLabel})");
        }

        return sb.ToString().TrimEnd();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref spikes, "spikes", LookMode.Deep);
        spikes ??= [];
        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            stageDirty = true;
        }
    }
}
