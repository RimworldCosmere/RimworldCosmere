using System.Collections.Generic;
using CosmereResources.Def;

namespace CosmereResources.DefModExtension;

public class MetalsLinked : Verse.DefModExtension {
    // ReSharper disable once InconsistentNaming
    public List<MetalDef> Metals => metals ?? (metal != null ? [metal] : []);
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private MetalDef? metal;
    private List<MetalDef>? metals;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}