#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static partial class GeneDefOf {
    static GeneDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(GeneDefOf));
    }
}
