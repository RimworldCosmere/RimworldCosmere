#nullable disable
using RimWorld;
using Verse;
using System.Diagnostics.CodeAnalysis;

namespace Cosmere.Core.Shader;

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