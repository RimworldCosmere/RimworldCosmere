#nullable disable
using System;
using Cosmere.Core;
using Cosmere.System.Roshar.Settings;
using Verse;

namespace Cosmere.System.Roshar;

public class Mod(ModContentPack content) : CosmereMod<RosharModSettings>(content) {
    public static bool enableHighstormPushing => Settings.enableHighstormPushing;

    public static bool enableHighstormDamage => Settings.enableHighstormDamage;

    public static bool enablePawnGlow => Settings.enablePawnGlow;

    [Obsolete]
    public static bool devOptionAutofillSpheres => false;
}