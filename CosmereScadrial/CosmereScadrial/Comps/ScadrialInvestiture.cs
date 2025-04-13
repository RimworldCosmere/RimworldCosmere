using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Registry;
using Verse;

namespace CosmereScadrial.Comps {
    /**
 * @todo Should these reserves decay?
 */
    public class ScadrialInvestiture : ThingComp {
        public Dictionary<string, float> metalReserves = new Dictionary<string, float>();

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Collections.Look(ref metalReserves, "metalReserves", LookMode.Value, LookMode.Value);
        }

        public void SetReserve(string metal, float amount) {
            metalReserves[metal] = amount;
        }

        public void AddReserve(string metal, float amount) {
            if (!metalReserves.ContainsKey(metal)) {
                metalReserves[metal] = 0;
            }

            var max = MetalRegistry.Metals[metal].MaxAmount;
            SetReserve(metal, Math.Max(metalReserves[metal] + amount, max));
        }

        public void RemoveReserve(string metal, bool delete) {
            if (delete) {
                metalReserves.Remove(metal);
            }
            else {
                SetReserve(metal, 0);
            }
        }

        public float GetReserve(string metal) {
            return metalReserves.TryGetValue(metal, out var value) ? value : 0f;
        }

        public bool AnyInvestiture() {
            return metalReserves.Values.Any(v => v > 0);
        }
    }
}