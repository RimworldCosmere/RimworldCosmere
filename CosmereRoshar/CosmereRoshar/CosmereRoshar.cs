#nullable disable
using CosmereFramework;
using CosmereRoshar.Settings;
using Verse;

namespace CosmereRoshar;

public class CosmereRoshar(ModContentPack content) : Mod(content) {
    public static RosharModSettings settings =>
        CosmereFramework.CosmereFramework.GetModSettings<RosharModSettings>();

    public static bool enableHighstormPushing => settings.enableHighstormPushing;
    public static bool enableHighstormDamage => settings.enableHighstormDamage;
    public static bool enablePawnGlow => settings.enablePawnGlow;
    public static bool devOptionAutofillSpheres => settings.devOptionAutofillSpheres;
    public static float bondChanceMultiplier => settings.bondChanceMultiplier;
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Roshar");
    }
}