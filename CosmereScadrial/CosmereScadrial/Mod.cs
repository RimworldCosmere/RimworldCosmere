using Cosmere.Framework;
using Cosmere.Scadrial.Settings;
using Verse;

namespace Cosmere.Scadrial;

public class Mod(ModContentPack content) : Verse.Mod(content) {
    public static MistsFrequency mistsFrequency =>
        Framework.Mod.GetModSettings<ScadrialModSettings>().mistsFrequency;
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Scadrial");
    }
}