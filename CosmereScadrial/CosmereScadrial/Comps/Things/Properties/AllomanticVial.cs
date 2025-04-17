using System.Collections.Generic;
using CosmereScadrial.Defs;
using Verse;

namespace CosmereScadrial.Comps.Things.Properties {
    public class AllomanticVial : CompProperties {
        public List<MetallicArtsMetalDef> metals;

        public AllomanticVial() {
            compClass = typeof(Things.AllomanticVial);
        }
    }
}