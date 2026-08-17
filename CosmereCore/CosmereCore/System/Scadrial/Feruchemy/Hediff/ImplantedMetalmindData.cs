using Cosmere.Core.Def;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class ImplantedMetalmindData : IExposable, IMetalmindSource {
    private MetalDef? cachedMetal;
    private float maxAmountInt;
    public string metalDefName = string.Empty;
    public string metalmindType = string.Empty;
    public string ownerName = string.Empty;
    public int loadId = -1;
    private float compoundedAmountInt;
    private float storedAmountInt;

    // Keyed by ConnectionKey; empty for every metal but duralumin.
    private Dictionary<string, float> chargeByLedger = [];

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", string.Empty);
        Scribe_Values.Look(ref metalmindType, "metalmindType", string.Empty);
        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref compoundedAmountInt, "compoundedAmount", 0f);
        Scribe_Values.Look(ref maxAmountInt, "maxAmount");
        Scribe_Values.Look(ref ownerName, "ownerName", string.Empty);
        Scribe_Values.Look(ref loadId, "loadId", -1);
        Scribe_Collections.Look(ref chargeByLedger, "chargeByLedger", LookMode.Value, LookMode.Value);
        chargeByLedger ??= [];
    }

    public float StoredAmount {
        get => storedAmountInt;
        set => storedAmountInt = value;
    }

    public float MaxAmount {
        get => maxAmountInt;
        set => maxAmountInt = value;
    }

    public float CompoundedAmount {
        get => compoundedAmountInt;
        set => compoundedAmountInt = value;
    }

    public float TotalStored => storedAmountInt + compoundedAmountInt;

    public float FreeSpace => Mathf.Max(0f, maxAmountInt - TotalStored);

    // Both pools draw on the same space, so filling either is bounded by the total.
    public bool CanStore => Equipped && FreeSpace > 0f;

    public bool CanTap => Equipped && StoredAmount > 0f;

    /// Burning draws on whatever the metalmind holds; stored-by-hand charge is the
    /// same charge, just set alight rather than drawn out normally.
    public bool CanTapCompounded => Equipped && TotalStored > 0f;

    public bool CanStoreCompounded => Equipped && FreeSpace > 0f;

    public bool IsImplanted => true;

    public bool Equipped => true;

    public MetalDef Metal {
        get {
            if (cachedMetal != null) return cachedMetal;
            cachedMetal = DefDatabase<MetalDef>.GetNamedSilentFail(metalDefName);
            return cachedMetal!;
        }
    }

    public float AddStored(float amount, string? ledgerKey = null) {
        if (!CanStore) return 0f;

        float before = storedAmountInt;
        storedAmountInt = Mathf.Clamp(storedAmountInt + amount, 0, maxAmountInt - compoundedAmountInt);
        float moved = storedAmountInt - before;
        RecordStored(ledgerKey, moved);

        return moved;
    }

    public float ConsumeStored(float amount, string? ledgerKey = null) {
        if (!CanTap) return 0f;

        float before = storedAmountInt;
        storedAmountInt = Mathf.Clamp(storedAmountInt - amount, 0, maxAmountInt);
        float moved = before - storedAmountInt;

        if (moved != 0f) {
            if (ledgerKey != null) RecordConsumed(ledgerKey, moved);
            else ChargeAttribution.Drain(chargeByLedger, moved);
        }

        return moved;
    }

    public float StoredFor(string ledgerKey) {
        return chargeByLedger.TryGetValue(ledgerKey, out float amount) ? amount : 0f;
    }

    private void RecordStored(string? ledgerKey, float moved) {
        if (ledgerKey == null || moved == 0f) return;

        chargeByLedger.TryGetValue(ledgerKey, out float existing);
        chargeByLedger[ledgerKey] = existing + moved;
    }

    private void RecordConsumed(string ledgerKey, float moved) {
        ChargeAttribution.DrainNamed(chargeByLedger, ledgerKey, moved);
    }

    // Copy of the current attribution map, for handing charge across on explant.
    public Dictionary<string, float> AttributionSnapshot() {
        return new Dictionary<string, float>(chargeByLedger);
    }

    /// Deep-scribed data never sees PostLoadInit, so the owning hediff calls this
    /// after load to keep the two pools inside a capacity that may have changed.
    public void ReconcileCapacity() {
        if (storedAmountInt + compoundedAmountInt > maxAmountInt) {
            compoundedAmountInt = Mathf.Max(0f, maxAmountInt - storedAmountInt);
        }

        ReconcileAttribution();
    }

    // Duralumin only, after the clamp above: drops unclaimed charge, then rescales.
    private void ReconcileAttribution() {
        if (Metal?.defName != "Duralumin") return;

        float attributed = ChargeAttribution.Total(chargeByLedger);
        if (storedAmountInt > attributed) {
            float discarded = storedAmountInt - attributed;
            storedAmountInt = attributed;
            Logger.Info($"Duralumin: discarded {discarded:F1} unattributed charge from {SourceLabel} on load");
        }

        ChargeAttribution.Rescale(chargeByLedger, storedAmountInt);
    }

    public float AddCompounded(float amount) {
        if (!CanStore) return 0f;

        float before = compoundedAmountInt;
        compoundedAmountInt = Mathf.Clamp(compoundedAmountInt + amount, 0, maxAmountInt - storedAmountInt);

        return compoundedAmountInt - before;
    }

    /// Drawing compounded charge spends the metalmind that carries it: capacity falls by
    /// what was drawn, so the metal runs out exactly when the charge does.
    public float ConsumeCompounded(float amount, string? ledgerKey = null) {
        if (!CanTapCompounded) return 0f;

        float spent = Mathf.Min(amount, TotalStored);

        float fromCompounded = Mathf.Min(spent, compoundedAmountInt);
        compoundedAmountInt -= fromCompounded;
        storedAmountInt -= spent - fromCompounded;

        maxAmountInt = Mathf.Max(0f, maxAmountInt - spent);

        // only the ordinary pool is attributed, so a burn drains the map by the part it took from there.
        float fromStored = spent - fromCompounded;
        if (fromStored > 0f) {
            if (ledgerKey != null) RecordConsumed(ledgerKey, fromStored);
            else ChargeAttribution.Drain(chargeByLedger, fromStored);
        }

        return spent;
    }

    public bool IsBurnedOut => maxAmountInt <= 0f;

    public string SourceId => "implant:" + loadId;

    public string SourceLabel =>
        "CC_Dock_Feruchemy_TargetImplant".Translate((Metal?.label ?? metalDefName).Named("METAL")).Resolve();
}
