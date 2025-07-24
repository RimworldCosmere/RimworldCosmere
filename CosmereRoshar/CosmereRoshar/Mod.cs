#nullable disable
using Cosmere.Framework;
using Cosmere.Roshar.Settings;
using Verse;

namespace Cosmere.Roshar;

public class Mod(ModContentPack content) : CosmereMod<RosharModSettings>(content, "Roshar") {
    public static bool enableHighstormPushing => Settings.enableHighstormPushing;
    public static bool enableHighstormDamage => Settings.enableHighstormDamage;
    public static bool enablePawnGlow => Settings.enablePawnGlow;
    public static bool devOptionAutofillSpheres => Settings.devOptionAutofillSpheres;
    public static float bondChanceMultiplier => Settings.bondChanceMultiplier;
}