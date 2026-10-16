using Cosmere.Core;
using Cosmere.System.Scadrial.Settings;
using Verse;

namespace Cosmere.System.Scadrial;

public class Mod(ModContentPack content) : CosmereMod<ScadrialModSettings>(content) {
    public static bool enableMists => Settings.enableMists;

    public static bool enableAshfall => Settings.enableAshfall;

    public static MistsFrequency mistsFrequency => Settings.mistsFrequency;

    public static bool alwaysShowAllomanticAuras => Settings.alwaysShowAllomanticAuras;

    /// <summary>Gets how many years pass between the fourth spike and the skin giving out.</summary>
    public static float kolossGrowthYears => Settings.kolossGrowthYears;

    /// <summary>Gets how many cells a vent's fertile ground creeps out to.</summary>
    public static float ventSoilReach => Settings.ventSoilReach;

    /// <summary>Gets how many seconds a koloss stands loose before it turns on whatever is nearest.</summary>
    public static float kolossGraceSeconds => Settings.kolossGraceSeconds;

    /// <summary>Gets what holding one costs per interval, against what seizing it cost.</summary>
    public static float kolossHoldFraction => Settings.kolossHoldFraction;

    /// <summary>Gets the multiplier on how hard a koloss or kandra is to take hold of.</summary>
    public static float kolossResistance => Settings.kolossResistance;

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
