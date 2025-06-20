using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comps.Things {
    public class MetalBurningProperties : CompProperties {
        public MetalBurningProperties() {
            compClass = typeof(MetalBurning);
        }
    }

    public class MetalBurning : ThingComp {
        private const int SECONDS_INTERVAL = 1;

        // Tracks all active burn rates per metal and source
        public readonly Dictionary<(MetallicArtsMetalDef metal, AllomanticAbilityDef def), float> burnSourceMap =
            new Dictionary<(MetallicArtsMetalDef metal, AllomanticAbilityDef def), float>();

        public readonly Dictionary<MetallicArtsMetalDef, List<float>> burnSources =
            new Dictionary<MetallicArtsMetalDef, List<float>>();

        public MetalBurning() {
            var allomanticMetals =
                DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => !x.godMetal && x.allomancy != null);
            foreach (var metal in allomanticMetals) {
                burnSources.Add(metal, []);
            }
        }

        private Pawn pawn => parent as Pawn;

        private MetalReserves metalReserves => pawn.GetComp<MetalReserves>();

        public bool CanBurn(MetallicArtsMetalDef metal, float requiredBEUs) {
            var amountToBurn = AllomancyUtility.GetMetalNeededForBeu(requiredBEUs * SECONDS_INTERVAL);
            return metalReserves.CanLowerReserve(metal, amountToBurn) ||
                   AllomancyUtility.PawnHasVialForMetal(pawn, metal);
        }

        public override void CompTick() {
            base.CompTick();

            if (!GenTicks.IsTickInterval(GenTicks.SecondsToTicks(SECONDS_INTERVAL))) {
                return;
            }

            if (pawn == null || pawn.Dead) {
                return;
            }

            foreach (var (metal, sources) in burnSources) {
                var totalRate = sources.Sum();
                if (totalRate <= 0f) {
                    continue;
                }

                if (AllomancyUtility.TryBurnMetalForInvestiture(pawn, metal, totalRate * SECONDS_INTERVAL)) continue;
                if (AllomancyUtility.PawnConsumeVialWithMetal(pawn, metal)) continue;

                // Auto-stop all sources if out of reserve
                Log.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
                RemoveAllBurnSourcesForMetal(metal);
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
            var key = (metal, defName: def);
            burnSourceMap[key] = rate;
            RebuildSourceList(metal);
        }

        public void UnregisterBurnSource(MetallicArtsMetalDef metal, AllomanticAbilityDef def) {
            var key = (metal, defName: def);
            if (burnSourceMap.Remove(key)) {
                RebuildSourceList(metal);
            }
        }

        public void RemoveAllBurnSources() {
            var metalsToRemove = burnSources.Keys.ToArray();
            foreach (var metal in metalsToRemove) {
                RemoveAllBurnSourcesForMetal(metal);
            }
        }

        public void RemoveAllBurnSourcesForMetal(MetallicArtsMetalDef metal) {
            var keysToRemove = burnSourceMap.Keys.Where(k => k.metal == metal).ToList();
            foreach (var key in keysToRemove) {
                var ability = (AbstractAbility)pawn.abilities.GetAbility(key.def);
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
            var metals = metal == null ? burnSources.Keys.ToArray() : [metal];

            return metals.Sum(x => burnSources.TryGetValue(x, out var list) ? list.Sum() : 0f);
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
            List<MetallicArtsMetalDef> metalDefs = null;
            List<AllomanticAbilityDef> defs = null;
            List<float> rates = null;

            if (Scribe.mode == LoadSaveMode.Saving) {
                metalDefs = [];
                defs = [];
                rates = [];

                foreach (var ((metal, def), rate) in burnSourceMap) {
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

            var count = Mathf.Min(metalDefs.Count, defs.Count, rates.Count);
            for (var i = 0; i < count; i++) {
                if (metalDefs[i] == null || defs[i] == null) {
                    continue;
                }

                burnSourceMap[(metalDefs[i], defs[i])] = rates[i];
            }

            // Rebuild burnSources dictionary from scratch
            burnSources.Clear();
            foreach (var metal in burnSourceMap.Keys.Select(k => k.metal).Distinct()) {
                RebuildSourceList(metal);
            }

            // Ensure all metals are represented, even if empty
            var allMetals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading
                .Where(x => !x.godMetal && x.allomancy != null);

            foreach (var metal in allMetals) {
                if (!burnSources.ContainsKey(metal)) {
                    burnSources[metal] = [];
                }
            }
        }
    }
}