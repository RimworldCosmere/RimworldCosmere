#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class WeatherDefOf {
    static WeatherDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(WeatherDefOf));
    }

    [MayRequire("Cosmere.Scadrial")]
    public static WeatherDef Cosmere_Scadrial_Weather_MistsWeather;
}