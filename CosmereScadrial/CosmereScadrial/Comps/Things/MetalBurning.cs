using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Abilities;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Comps.Things {
    public class MetalBurning : ThingComp {
        private const int SECONDS_INTERVAL = 1;

        // Tracks all active burn rates per metal and source
        private readonly Dictionary<(MetallicArtsMetalDef metal, Def def), float> burnSourceMap = new Dictionary<(MetallicArtsMetalDef metal, Def def), float>();
        public readonly Dictionary<MetallicArtsMetalDef, List<float>> burnSources = new Dictionary<MetallicArtsMetalDef, List<float>>();

        public MetalBurning() {
            var allomanticMetals = DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => !x.godMetal && x.allomancy != null);
            foreach (var metal in allomanticMetals) {
                burnSources.Add(metal, []);
            }
        }

        private Pawn pawn => parent as Pawn;

        private MetalReserves metalReserves => pawn.GetComp<MetalReserves>();

        public bool CanBurn(MetallicArtsMetalDef metal, float requiredBEUs) {
            var amountToBurn = AllomancyUtility.GetMetalNeededForBeu(requiredBEUs * SECONDS_INTERVAL);
            return metalReserves.CanLowerReserve(metal, amountToBurn) || AllomancyUtility.PawnHasVialForMetal(pawn, metal);
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

                if (!AllomancyUtility.TryBurnMetalForInvestiture(pawn, metal, totalRate * SECONDS_INTERVAL)) {
                    if (!AllomancyUtility.PawnConsumeVialWithMetal(pawn, metal)) {
                        // Auto-stop all sources if out of reserve
                        Log.Info($"{pawn.NameFullColored} can't burn {metal} any more. Removing all burn sources for {metal}.");
                        RemoveAllBurnSourcesForMetal(metal);
                        return;
                    }
                }
            }
        }

        public void UpdateBurnSource(MetallicArtsMetalDef metal, float rate, Def def) {
            if (rate <= 0f) {
                UnregisterBurnSource(metal, def);
            } else {
                RegisterBurnSource(metal, rate, def);
            }
        }

        public void RegisterBurnSource(MetallicArtsMetalDef metal, float rate, Def def) {
            var key = (metal, defName: def);
            burnSourceMap[key] = rate;
            RebuildSourceList(metal);
        }

        public void UnregisterBurnSource(MetallicArtsMetalDef metal, Def def) {
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
                if (key.def is AllomanticAbilityDef def) {
                    var ability = (AbstractAllomanticAbility)pawn.abilities.GetAbility(def);
                    ability?.UpdateStatus(BurningStatus.Off);
                }

                burnSourceMap.Remove(key);
            }

            burnSources[metal].Clear();
        }

        public float GetBurnRateForMetalDef(MetallicArtsMetalDef metal, Def def) {
            return (from kv in burnSourceMap where kv.Key.metal == metal && kv.Key.def == def select kv.Value).FirstOrDefault();
        }

        public float GetTotalBurnRate(MetallicArtsMetalDef metal = null) {
            var metals = metal == null ? burnSources.Keys.ToArray() : [metal];

            return metals.Sum(x => burnSources.TryGetValue(x, out var list) ? list.Sum() : 0f);
        }

        public bool IsBurning(MetallicArtsMetalDef metal = null) {
            return GetTotalBurnRate(metal) > 0f;
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

            var metalDefs = new List<MetallicArtsMetalDef>();
            var defs = new List<Def>();
            var burnRates = new List<float>();

            if (Scribe.mode == LoadSaveMode.Saving) {
                foreach (var ((metal, def), rate) in burnSourceMap) {
                    metalDefs.Add(metal);
                    defs.Add(def);
                    burnRates.Add(rate);
                }
            }

            Scribe_Collections.Look(ref metalDefs, "burnSourceMetals", LookMode.Def);
            Scribe_Collections.Look(ref defs, "burndefs", LookMode.Def);
            Scribe_Collections.Look(ref burnRates, "burnSourceRates", LookMode.Value);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            {
                burnSourceMap.Clear();
                if (metalDefs == null || defs == null || burnRates == null) return;
                var count = Mathf.Min(metalDefs.Count, defs.Count, burnRates.Count);
                for (var i = 0; i < count; i++) {
                    burnSourceMap[(metalDefs[i], defs[i])] = burnRates[i];
                }

                burnSources.Clear();
                foreach (var metal in burnSourceMap.Keys.Select(k => k.metal).Distinct()) {
                    RebuildSourceList(metal);
                }
            }
        }

        /*public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            if (!Prefs.DevMode || parent is not Pawn pawn || !CosmereCore.CosmereCore.CosmereSettings.debugMode) {
                yield break;
            }

            foreach (var metal in _burnSources.Keys.ToList()) {
                var total = GetTotalBurnRate(metal);
                var isOn = total > 0f;

                yield return new Command_Action {
                    defaultLabel = $"[Debug] {metal}: {(isOn ? "ON" : "OFF")}",
                    defaultDesc = $"Toggle burning for {metal}. Total rate: {total:0.#####} per tick.",
                    action = () => {
                        if (isOn) {
                            // Turn it off
                            RemoveAllBurnSourcesForMetal(metal);
                        }
                        else {
                            // Turn it on at a test rate (e.g. default 0.001666f)
                            RegisterBurnSource(metal, 0.001666f, "DebugManualToggle");
                        }
                    },
                    icon = isOn ? ContentFinder<Texture2D>.Get("UI/Overlays/OutOfFuel") : ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"),
                };
            }
        }*/
    }
}