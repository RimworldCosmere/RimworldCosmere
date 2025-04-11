using System.Collections.Generic;
using CosmereScadrial.Comps;

namespace CosmereScadrial.CompProperties {
    public class CompProperties_AllomanticVial : Verse.CompProperties {
        public List<string> metalNames;

        public CompProperties_AllomanticVial() {
            this.compClass = typeof(CompAllomanticVial);
        }
    }
}