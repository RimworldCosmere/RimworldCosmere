using System.Collections.Generic;
using CosmereResources.Defs;
using Verse;

namespace CosmereResources.ModExtensions {
    public class MetalsLinked : DefModExtension {
        private MetalDef metal;
        private List<MetalDef> metals;

        public List<MetalDef> Metals => metals ?? (metal != null ? [metal] : []);
    }
}