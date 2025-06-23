#nullable disable
using System.Diagnostics.CodeAnalysis;
using RimWorld;
using Verse;

namespace CosmereScadrial;

[DefOf]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class WeatherDefOf {
    public static WeatherDef Cosmere_Scadrial_MistsWeather;

    static WeatherDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(WeatherDefOf));
    }
}