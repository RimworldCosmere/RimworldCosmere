using Cosmere.Core;
using Cosmere.System.Scadrial.Settings;
using Verse;

namespace Cosmere.System.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static bool enableMists => Settings.enableMists;

    public static bool enableAshfall => Settings.enableAshfall;

    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;

    public static bool alwaysShowAllomanticAuras => Settings.alwaysShowAllomanticAuras;
}
