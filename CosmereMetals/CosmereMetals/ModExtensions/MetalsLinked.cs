using System.Collections.Generic;
using CosmereMetals.Defs;
using Verse;

namespace CosmereMetals.ModExtensions {
    public class MetalsLinked : DefModExtension {
        private MetalDef metal;
        private List<MetalDef> metals;

        public List<MetalDef> Metals => metals ?? (metal != null ? [metal] : []);
    }
}