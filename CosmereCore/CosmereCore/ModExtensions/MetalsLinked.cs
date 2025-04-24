using System.Collections.Generic;
using CosmereCore.Defs;
using Verse;

namespace CosmereCore.ModExtensions {
    public class MetalsLinked : DefModExtension {
        public MetalDef metal;
        public List<MetalDef> metals;

        public List<MetalDef> Metals => metals ?? (metal != null ? [metal] : []);
    }
}