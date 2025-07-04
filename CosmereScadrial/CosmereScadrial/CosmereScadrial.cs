#nullable disable
using CosmereFramework;
using CosmereScadrial.Settings;
using Verse;

namespace CosmereScadrial;

public class CosmereScadrial(ModContentPack content) : Mod(content) {
    public static MistsFrequency mistsFrequency =>
        CosmereFramework.CosmereFramework.GetModSettings<ScadrialModSettings>().mistsFrequency;
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Scadrial");
    }
}