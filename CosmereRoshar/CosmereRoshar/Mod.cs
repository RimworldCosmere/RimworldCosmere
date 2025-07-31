#nullable disable
using System;
using Cosmere.Framework;
using Cosmere.Roshar.Settings;
using Verse;

namespace Cosmere.Roshar;

public class Mod(ModContentPack content) : CosmereMod<RosharModSettings>(content) {
    public static bool enableHighstormPushing => Settings.enableHighstormPushing;

    public static bool enableHighstormDamage => Settings.enableHighstormDamage;

    public static bool enablePawnGlow => Settings.enablePawnGlow;

    [Obsolete]
    public static bool devOptionAutofillSpheres => false;

    public static float bondChanceMultiplier => Settings.bondChanceMultiplier;
}