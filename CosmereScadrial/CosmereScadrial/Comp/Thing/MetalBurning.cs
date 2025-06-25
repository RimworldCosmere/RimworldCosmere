using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Ability.Allomancy;
using CosmereScadrial.Def;
using CosmereScadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comp.Thing;

public class MetalBurningProperties : CompProperties {
    public MetalBurningProperties() {
        compClass = typeof(MetalBurning);
    }
}

/**
 * @TODO Notify the Hediff to recalculate severity on a rate change
 */
public class MetalBurning : ThingComp {
    private const int SecondsInterval = 1;

    // Tracks all active burn rates per metal and source
    public readonly Dictionary<(MetallicArtsMetalDef metal, AllomanticAbilityDef def), float> burnSourceMap =
        new Dictionary<(MetallicArtsMetalDef metal, AllomanticAbilityDef def), float>();

    public readonly Dictionary<MetallicArtsMetalDef, List<float>> burnSources =
        new Dictionary<MetallicArtsMetalDef, List<float>>();

    public MetalBurning() {
        IEnumerable<MetallicArtsMetalDef> allomanticMetals =
            DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => !x.godMetal && x.allomancy != null);
        foreach (MetallicArtsMetalDef? metal in allomanticMetals) {
            burnSources.Add(metal, []);
        }
    }

    private Pawn pawn => (parent as Pawn)!;
    private float timeDilationFactor => pawn.GetStatValue(CosmereCore.StatDefOf.Cosmere_Time_Dilation_Factor);
    private int tickRate => Mathf.RoundToInt(GenTicks.TickRareInterval / timeDilationFactor);

    private MetalReserves metalReserves => pawn.GetComp<MetalReserves>();

    public AcceptanceReport CanBurn(MetallicArtsMetalDef metal, float requiredBeUs) {
        float amountToBurn = AllomancyUtility.GetMetalNeededForBeu(requiredBeUs);

        if (!metalReserves.CanLowerReserve(metal, amountToBurn) &&
            !AllomancyUtility.PawnHasVialForMetal(pawn, metal)) {
            return "CS_CannotBurn".Translate(pawn.Named("PAWN"), metal.Named("METAL"));
        }

        return AcceptanceReport.WasAccepted;
    }

    public override void CompTickInterval(int delta) {
        base.CompTickInterval(delta);

        if (!pawn.IsHashIntervalTick(tickRate, delta)) return;

        TryBurnMetals();
    }

    public void TryBurnMetals() {
        if (pawn.Dead) {
            return;
        }

        foreach ((MetallicArtsMetalDef? metal, List<float>? sources) in burnSources) {
            float totalRate = sources.Sum();
            if (totalRate <= 0f) {
                continue;
            }

            if (AllomancyUtility.TryBurnMetalForInvestiture(pawn, metal, totalRate)) continue;

            // Auto-stop all sources if out of reserve
            RemoveAllBurnSourcesForMetal(metal);
            Log.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
            return;
        }
    }

    public void UpdateBurnSource(MetallicArtsMetalDef metal, float rate, AllomanticAbilityDef def) {
        if (rate <= 0f) {
            UnregisterBurnSource(metal, def);
        } else {
            RegisterBurnSource(metal, rate, def);
        }
    }

    public void RegisterBurnSource(MetallicArtsMetalDef metal, float rate, AllomanticAbilityDef def) {
        (MetallicArtsMetalDef metal, AllomanticAbilityDef defName) key = (metal, defName: def);
        burnSourceMap[key] = rate;
        RebuildSourceList(metal);
    }

    public void UnregisterBurnSource(MetallicArtsMetalDef metal, AllomanticAbilityDef def) {
        (MetallicArtsMetalDef metal, AllomanticAbilityDef defName) key = (metal, defName: def);
        if (burnSourceMap.Remove(key)) {
            RebuildSourceList(metal);
        }
    }

    public void RemoveAllBurnSources() {
        MetallicArtsMetalDef[] metalsToRemove = burnSources.Keys.ToArray();
        foreach (MetallicArtsMetalDef metal in metalsToRemove) {
            RemoveAllBurnSourcesForMetal(metal);
        }
    }

    public void RemoveAllBurnSourcesForMetal(MetallicArtsMetalDef metal) {
        List<(MetallicArtsMetalDef metal, AllomanticAbilityDef def)> keysToRemove =
            burnSourceMap.Keys.Where(k => k.metal == metal).ToList();
        foreach ((MetallicArtsMetalDef metal, AllomanticAbilityDef def) key in keysToRemove) {
            AbstractAbility? ability = (AbstractAbility)pawn.abilities.GetAbility(key.def);
            ability?.UpdateStatus(BurningStatus.Off);

            burnSourceMap.Remove(key);
        }

        burnSources[metal].Clear();
    }

    public float GetBurnRateForMetalDef(MetallicArtsMetalDef metal, AllomanticAbilityDef def) {
        return (from kv in burnSourceMap where kv.Key.metal == metal && kv.Key.def == def select kv.Value)
            .FirstOrDefault();
    }

    public float GetTotalBurnRate(MetallicArtsMetalDef metal = null) {
        MetallicArtsMetalDef[] metals = metal == null ? burnSources.Keys.ToArray() : [metal];

        return metals.Sum(x => burnSources.TryGetValue(x, out List<float>? list) ? list.Sum() : 0f);
    }

    public bool IsBurning(MetallicArtsMetalDef metal = null) {
        return GetTotalBurnRate(metal) > 0f;
    }

    public List<MetallicArtsMetalDef> GetBurningMetals() {
        return burnSources.Where(x => x.Value.Sum() > 0).Select(x => x.Key).ToList();
    }

    private void RebuildSourceList(MetallicArtsMetalDef metal) {
        if (!burnSources.ContainsKey(metal)) {
            burnSources[metal] = new List<float>();
        }

        burnSources[metal] = burnSourceMap
            .Where(kvp => kvp.Key.metal == metal)
            .Select(kvp => kvp.Value)
            .ToList();
    }

    public override void PostExposeData() {
        base.PostExposeData();

        // Serialize burnSourceMap into three parallel lists
        List<MetallicArtsMetalDef>? metalDefs = null;
        List<AllomanticAbilityDef>? defs = null;
        List<float>? rates = null;

        if (Scribe.mode == LoadSaveMode.Saving) {
            metalDefs = [];
            defs = [];
            rates = [];

            foreach (((MetallicArtsMetalDef metal, AllomanticAbilityDef def), float rate) in burnSourceMap) {
                metalDefs.Add(metal);
                defs.Add(def);
                rates.Add(rate);
            }
        }

        Scribe_Collections.Look(ref metalDefs, "burnSourceMetals", LookMode.Def);
        Scribe_Collections.Look(ref defs, "burnSourceDefs", LookMode.Def);
        Scribe_Collections.Look(ref rates, "burnSourceRates", LookMode.Value);

        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

        burnSourceMap.Clear();

        if (metalDefs == null || defs == null || rates == null) {
            return;
        }

        int count = Mathf.Min(metalDefs.Count, defs.Count, rates.Count);
        for (int i = 0; i < count; i++) {
            if (metalDefs[i] == null || defs[i] == null) {
                continue;
            }

            burnSourceMap[(metalDefs[i], defs[i])] = rates[i];
        }

        // Rebuild burnSources dictionary from scratch
        burnSources.Clear();
        foreach (MetallicArtsMetalDef? metal in burnSourceMap.Keys.Select(k => k.metal).Distinct()) {
            RebuildSourceList(metal);
        }

        // Ensure all metals are represented, even if empty
        IEnumerable<MetallicArtsMetalDef> allMetals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
            .Where(x => !x.godMetal && x.allomancy != null);

        foreach (MetallicArtsMetalDef? metal in allMetals) {
            if (!burnSources.ContainsKey(metal)) {
                burnSources[metal] = [];
            }
        }
    }
}