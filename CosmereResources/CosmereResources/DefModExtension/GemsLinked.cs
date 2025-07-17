using System.Collections.Generic;
using CosmereResources.Def;

namespace CosmereResources.DefModExtension;

public class GemsLinked : Verse.DefModExtension {
    // ReSharper disable once InconsistentNaming
    public List<GemDef> Gems => gems ?? (gem != null ? [gem] : []);
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private GemDef? gem;
    private List<GemDef>? gems;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}