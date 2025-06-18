using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace CosmereResources;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class ThingDefOf {
    static ThingDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}