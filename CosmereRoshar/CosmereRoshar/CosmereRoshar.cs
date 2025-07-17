#nullable disable
using CosmereFramework;
using Verse;
using CosmereRoshar.Settings;
namespace CosmereRoshar;

public class CosmereRoshar(ModContentPack content) : Mod(content) {
    public static RosharModSettings Settings =>
        CosmereFramework.CosmereFramework.GetModSettings<RosharModSettings>();

    public static bool EnableHighstormPushing => Settings.enableHighstormPushing;
    public static bool EnableHighstormDamage => Settings.enableHighstormDamage;
    public static bool EnablePawnGlow => Settings.enablePawnGlow;
    public static bool DevOptionAutofillSpheres => Settings.devOptionAutofillSpheres;
    public static float BondChanceMultiplier => Settings.bondChanceMultiplier;
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Roshar");
    }
}