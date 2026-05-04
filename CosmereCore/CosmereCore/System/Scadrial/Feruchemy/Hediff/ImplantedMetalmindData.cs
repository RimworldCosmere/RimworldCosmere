using Cosmere.Core.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class ImplantedMetalmindData : IExposable, IMetalmindSource {
    private MetalDef? cachedMetal;
    private float maxAmountInt;
    public string metalDefName = "";
    public string metalmindType = "";
    public string ownerName = "";
    private float storedAmountInt;

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", "");
        Scribe_Values.Look(ref metalmindType, "metalmindType", "");
        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref maxAmountInt, "maxAmount");
        Scribe_Values.Look(ref ownerName, "ownerName", "");
    }

    public float StoredAmount {
        get => storedAmountInt;
        set => storedAmountInt = value;
    }

    public float MaxAmount {
        get => maxAmountInt;
        set => maxAmountInt = value;
    }

    public bool CanStore => Equipped && StoredAmount < MaxAmount;
    public bool CanTap => Equipped && StoredAmount > 0;
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
        storedAmountInt = Mathf.Clamp(storedAmountInt + amount, 0, maxAmountInt);
    }

    public void ConsumeStored(float amount) {
        if (!CanTap) return;
        storedAmountInt = Mathf.Clamp(storedAmountInt - amount, 0, maxAmountInt);
    }
}