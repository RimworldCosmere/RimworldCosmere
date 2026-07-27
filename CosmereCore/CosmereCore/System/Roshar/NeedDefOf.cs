#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;

namespace Cosmere.System.Roshar;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class NeedDefOf {
    [MayRequire("Cosmere.Roshar")]
    public static NeedDef Cosmere_Roshar_Need_Fury;

    static NeedDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(NeedDefOf));
    }
}
