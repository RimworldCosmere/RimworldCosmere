using Cosmere.Core.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class ImplantedMetalmindData : IExposable, IMetalmindSource {
    private MetalDef? cachedMetal;
    private float maxAmountInt;
    public string metalDefName = string.Empty;
    public string metalmindType = string.Empty;
    public string ownerName = string.Empty;
    private float compoundedAmountInt;
    private float storedAmountInt;

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", string.Empty);
        Scribe_Values.Look(ref metalmindType, "metalmindType", string.Empty);
        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref compoundedAmountInt, "compoundedAmount", 0f);
        Scribe_Values.Look(ref maxAmountInt, "maxAmount");
        Scribe_Values.Look(ref ownerName, "ownerName", string.Empty);
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

    public bool CanTapCompounded => Equipped && CompoundedAmount > 0f;

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

    public void AddStored(float amount) {
        if (!CanStore) return;
        storedAmountInt = Mathf.Clamp(storedAmountInt + amount, 0, maxAmountInt - compoundedAmountInt);
    }

    public void ConsumeStored(float amount) {
        if (!CanTap) return;
        storedAmountInt = Mathf.Clamp(storedAmountInt - amount, 0, maxAmountInt);
    }

    // Deep-scribed data never sees PostLoadInit, so the owning hediff calls this
    // after load to keep the two pools inside a capacity that may have changed.
    public void ReconcileCapacity() {
        if (storedAmountInt + compoundedAmountInt <= maxAmountInt) return;
        compoundedAmountInt = Mathf.Max(0f, maxAmountInt - storedAmountInt);
    }

    public void AddCompounded(float amount) {
        if (!CanStore) return;
        compoundedAmountInt = Mathf.Clamp(compoundedAmountInt + amount, 0, maxAmountInt - storedAmountInt);
    }

    public void ConsumeCompounded(float amount) {
        if (!CanTapCompounded) return;
        compoundedAmountInt = Mathf.Clamp(compoundedAmountInt - amount, 0, maxAmountInt);
    }
}
