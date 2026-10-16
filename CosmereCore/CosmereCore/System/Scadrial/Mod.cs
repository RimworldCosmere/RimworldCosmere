using Cosmere.Core;
using Cosmere.System.Scadrial.Settings;
using Verse;

namespace Cosmere.System.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static bool EnableMists => Settings.enableMists;

    public static bool EnableAshfall => Settings.enableAshfall;

    public static MistsFrequency MistsFrequency => Settings.mistsFrequency;

    public static bool AlwaysShowAllomanticAuras => Settings.alwaysShowAllomanticAuras;

    /// <summary>Gets how many years pass between the fourth spike and the skin giving out.</summary>
    public static float KolossGrowthYears => Settings.kolossGrowthYears;

    /// <summary>Gets how many cells a vent's fertile ground creeps out to.</summary>
    public static float VentSoilReach => Settings.ventSoilReach;

    /// <summary>Gets how many seconds a koloss stands loose before it turns on whatever is nearest.</summary>
    public static float KolossGraceSeconds => Settings.kolossGraceSeconds;

    /// <summary>Gets what holding one costs per interval, against what seizing it cost.</summary>
    public static float KolossHoldFraction => Settings.kolossHoldFraction;

    /// <summary>Gets the multiplier on how hard a koloss or kandra is to take hold of.</summary>
    public static float KolossResistance => Settings.kolossResistance;

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
