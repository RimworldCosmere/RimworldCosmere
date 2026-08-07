using Cosmere.Core;
using Cosmere.System.Scadrial.Settings;
using Verse;

namespace Cosmere.System.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static bool enableMists => Settings.enableMists;

    public static bool enableAshfall => Settings.enableAshfall;

    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;

    public static bool alwaysShowAllomanticAuras => Settings.alwaysShowAllomanticAuras;

    /// <summary>Years from the fourth spike to the skin giving out.</summary>
    public static float kolossGrowthYears => Settings.kolossGrowthYears;

    /// <summary>
    ///     Some settings are baked into defs rather than read live, so they have to be pushed
    ///     back out when the player changes one. Without this, koloss growth would keep the rate
    ///     it had at startup until the game was restarted.
    /// </summary>
    public override void WriteSettings() {
        base.WriteSettings();
        Util.KolossGrowthTuning.Apply();
    }
}
