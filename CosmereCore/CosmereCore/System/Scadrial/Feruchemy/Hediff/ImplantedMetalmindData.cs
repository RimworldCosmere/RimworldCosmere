using Cosmere.Core.Def;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.Hediff;

public class ImplantedMetalmindData : IExposable, IMetalmindSource {
    private float storedAmountInt;
    private float maxAmountInt;
    public string metalDefName = "";
    public string metalmindType = "";
    public string ownerName = "";

    private MetalDef? cachedMetal;

    public float storedAmount {
        get => storedAmountInt;
        set => storedAmountInt = value;
    }

    public float maxAmount {
        get => maxAmountInt;
        set => maxAmountInt = value;
    }

    public bool canStore => equipped && storedAmount < maxAmount;
    public bool canTap => equipped && storedAmount > 0;
    public bool equipped => true;

    public MetalDef metal {
        get {
            if (cachedMetal != null) return cachedMetal;
            cachedMetal = DefDatabase<MetalDef>.GetNamedSilentFail(metalDefName);
            return cachedMetal!;
        }
    }

    public void AddStored(float amount) {
        if (!canStore) return;
        storedAmountInt = Mathf.Clamp(storedAmountInt + amount, 0, maxAmountInt);
    }

    public void ConsumeStored(float amount) {
        if (!canTap) return;
        storedAmountInt = Mathf.Clamp(storedAmountInt - amount, 0, maxAmountInt);
    }

    public void ExposeData() {
        Scribe_Values.Look(ref metalDefName, "metalDefName", "");
        Scribe_Values.Look(ref metalmindType, "metalmindType", "");
        Scribe_Values.Look(ref storedAmountInt, "storedAmount");
        Scribe_Values.Look(ref maxAmountInt, "maxAmount");
        Scribe_Values.Look(ref ownerName, "ownerName", "");
    }
}
