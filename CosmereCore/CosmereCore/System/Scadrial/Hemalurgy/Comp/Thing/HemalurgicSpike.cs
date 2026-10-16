using System.Text;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Def;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy.Comp.Thing;

public class HemalurgicSpikeProperties : CompProperties {
    public HemalurgicSpikeProperties() {
        compClass = typeof(HemalurgicSpike);
    }
}

public class HemalurgicSpike : ThingComp {
    private MetallicArtsMetalDef? cachedMetal;
    public HemalurgicChargeData? chargeData;
    public GeneDef? pendingStealTarget;

    public bool IsCharged => chargeData is { IsValid: true };

    public MetallicArtsMetalDef? Metal {
        get {
            if (cachedMetal != null) return cachedMetal;
            if (parent.Stuff != null) {
                cachedMetal = MetallicArtsMetalDef.FromMetalDef(
                    DefDatabase<MetalDef>.GetNamed(parent.Stuff.defName)
                );
            }

            if (cachedMetal != null) return cachedMetal;

            Log.Error("HemalurgicSpike doesn't have a metal");
            return null;
        }
    }

    public HemalurgicStealType StealType => Metal != null ? HemalurgicConstants.GetStealType(Metal) : default;

    public bool IsInAluminumCase {
        get {
            if (parent.Map == null || !parent.Spawned) return false;
            List<Verse.Thing> things = parent.Map.thingGrid.ThingsListAtFast(parent.Position);
            for (int i = 0; i < things.Count; i++) {
                if (things[i] is Building b && b.def == HemalurgicDefOf.Cosmere_Scadrial_Thing_AluminumSpikeCase) {
                    return true;
                }
            }

            return false;
        }
    }

    public float CurrentStrength {
        get {
            if (chargeData == null) return 0f;
            if (parent.holdingOwner?.Owner is Pawn) return chargeData.strength;
            return chargeData.GetCurrentStrength(Find.TickManager.TicksGame, IsInAluminumCase);
        }
    }

    public void Charge(HemalurgicChargeData data) {
        chargeData = data;
    }

    public HemalurgicChargeData? Discharge() {
        HemalurgicChargeData? data = chargeData;
        chargeData = null;
        return data;
    }

    public override void CompTick() {
        if (chargeData == null) return;
        if (parent.holdingOwner?.Owner is Pawn) return;
        if (!parent.IsHashIntervalTick(HemalurgicConstants.DecayIntervalTicks)) return;

        float strength = chargeData.GetCurrentStrength(Find.TickManager.TicksGame, IsInAluminumCase);
        if (strength <= 0f) {
            chargeData = null;
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref chargeData, "chargeData");
        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            cachedMetal = null;
        }
    }

    public override string CompInspectStringExtra() {
        StringBuilder sb = new StringBuilder();
        if (IsCharged) {
            sb.AppendLine("CS_Hemalurgy_SpikeCharged".Translate());
            string typeLabel = HemalurgicConstants.GetStealTypeLabel(chargeData!.stealType);
            sb.AppendLine("CS_Hemalurgy_SpikeCategory".Translate(typeLabel));
            float str = CurrentStrength;
            string strengthLabel = str switch {
                >= 0.8f => "CS_Hemalurgy_StrengthStrong".Translate(),
                >= 0.5f => "CS_Hemalurgy_StrengthModerate".Translate(),
                >= 0.2f => "CS_Hemalurgy_StrengthWeak".Translate(),
                _ => "CS_Hemalurgy_StrengthFading".Translate(),
            };
            sb.Append("CS_Hemalurgy_SpikeStrength".Translate(strengthLabel));
            if (IsInAluminumCase) {
                sb.AppendLine();
                sb.Append("CS_Hemalurgy_DecayPrevented".Translate());
            }
        } else {
            sb.Append("CS_Hemalurgy_SpikeUncharged".Translate());
        }

        return sb.ToString();
    }
}
