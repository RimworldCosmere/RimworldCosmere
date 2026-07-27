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
    public int loadId = -1;
    private float compoundedAmountInt;
    private float storedAmountInt;

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", string.Empty);
        Scribe_Values.Look(ref metalmindType, "metalmindType", string.Empty);
        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref compoundedAmountInt, "compoundedAmount", 0f);
        Scribe_Values.Look(ref maxAmountInt, "maxAmount");
        Scribe_Values.Look(ref ownerName, "ownerName", string.Empty);
        Scribe_Values.Look(ref loadId, "loadId", -1);
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

    // Burning draws on whatever the metalmind holds. Charge stored by hand is the
    // same charge - what makes it compounding is setting the metal alight rather
    // than drawing it out, so there is no separate pool to fill first.
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

    // Drawing compounded charge eats the metalmind that carried it. Capacity
    // drops by what was spent, so the two run out together and a metalmind filled
    // entirely by compounding is used up exactly when it empties.
    // Spends the metalmind itself along with its charge. Capacity falls by what
    // was drawn, so the metal runs out exactly when the charge does.
    public void ConsumeCompounded(float amount) {
        if (!CanTapCompounded) return;

        float spent = Mathf.Min(amount, TotalStored);

        float fromCompounded = Mathf.Min(spent, compoundedAmountInt);
        compoundedAmountInt -= fromCompounded;
        storedAmountInt -= spent - fromCompounded;

        maxAmountInt = Mathf.Max(0f, maxAmountInt - spent);
    }

    public bool IsBurnedOut => maxAmountInt <= 0f;

    public string SourceId => "implant:" + loadId;

    public string SourceLabel =>
        "CC_Dock_Feruchemy_TargetImplant".Translate((Metal?.label ?? metalDefName).Named("METAL")).Resolve();
}
