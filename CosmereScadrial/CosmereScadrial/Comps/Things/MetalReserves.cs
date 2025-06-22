using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.Utils;
using CosmereScadrial.Defs;
using Verse;

namespace CosmereScadrial.Comps.Things;

public class MetalReservesProperties : CompProperties {
    public MetalReservesProperties() {
        compClass = typeof(MetalReserves);
    }
}

public class MetalReserves : ThingComp {
    public const float MaxAmount = 1f;
    private const float SleepDecayAmount = .0025f;

    private Dictionary<MetallicArtsMetalDef, float> reserves = new Dictionary<MetallicArtsMetalDef, float>();

    public MetalReserves() {
        IEnumerable<MetallicArtsMetalDef> allomanticMetals =
            DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => !x.godMetal && x.allomancy != null);
        foreach (MetallicArtsMetalDef? metal in allomanticMetals) {
            reserves.Add(metal, 0);
        }
    }

    public List<MetallicArtsMetalDef> GetAllAvailableMetals() {
        return reserves.Where(x => x.Value > 0f).Select(x => x.Key).ToList();
    }

    public override void CompTickRare() {
        base.CompTickRare();
        if (parent is not Pawn pawn) return;
        if (!PawnUtility.IsAsleep(pawn)) return;

        MetallicArtsMetalDef[] keys = reserves.Keys.Where(metal => reserves[metal] > 0).ToArray();
        foreach (MetallicArtsMetalDef metal in keys) {
            LowerReserve(metal, SleepDecayAmount);
        }
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Collections.Look(ref reserves, "metalReserves", LookMode.Def, LookMode.Value);
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

        // Ensure all valid metals are present after loading
        IEnumerable<MetallicArtsMetalDef> allMetals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => !x.godMetal && x.allomancy != null);

        foreach (MetallicArtsMetalDef? metal in allMetals) {
            if (!reserves.ContainsKey(metal)) {
                reserves[metal] = 0f;
            }
        }
    }

    public void SetReserve(MetallicArtsMetalDef metal, float amount) {
        reserves[metal] = amount;
    }

    public void AddReserve(MetallicArtsMetalDef metal, float amount) {
        if (!reserves.ContainsKey(metal)) {
            reserves[metal] = 0;
        }

        SetReserve(metal, Math.Min(reserves[metal] + amount, MaxAmount));
    }

    public bool CanLowerReserve(MetallicArtsMetalDef metal, float amount) {
        return reserves.ContainsKey(metal) && reserves[metal] >= amount;
    }

    public bool HasReserve(MetallicArtsMetalDef metal) {
        return reserves.ContainsKey(metal) && reserves[metal] > 0;
    }

    public void LowerReserve(MetallicArtsMetalDef metal, float amount) {
        if (!reserves.ContainsKey(metal)) {
            reserves[metal] = 0;
        }

        SetReserve(metal, Math.Max(reserves[metal] - amount, 0));
    }

    public void RemoveReserve(MetallicArtsMetalDef metal) {
        SetReserve(metal, 0);
    }

    public float GetReserve(MetallicArtsMetalDef metal) {
        return reserves.TryGetValue(metal, out float value) ? value : 0f;
    }

    public bool TryGetReserve(MetallicArtsMetalDef metal, out float value) {
        return reserves.TryGetValue(metal, out value);
    }

    public void EnsureReserveExists(MetallicArtsMetalDef metal) {
        if (!reserves.ContainsKey(metal)) {
            SetReserve(metal, 0);
        }
    }
}