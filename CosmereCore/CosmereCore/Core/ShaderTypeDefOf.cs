#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.Core;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class ShaderTypeDefOf {
    public static ShaderTypeDef CutoutAdvanced;

    static ShaderTypeDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(ShaderTypeDefOf));
    }
}