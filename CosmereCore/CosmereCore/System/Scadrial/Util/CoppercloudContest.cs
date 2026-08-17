namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Whether a coppercloud beats someone reaching through it.
/// </summary>
/// <remarks>
///     Kept free of Verse types so it can be tested. <see cref="Coppercloud" /> does the map work
///     and hands the numbers here; several clouds over one pawn are added up by
///     <see cref="Cosmere.Core.Util.DiminishingStack" />, the same way the hediff severity adds
///     them, so the number the player can see is the number that decides the contest.
/// </remarks>
public static class CoppercloudContest {
    /// <summary>Whether a cloud this strong hides from a burn this strong. Ties go to the cloud.</summary>
    /// <remarks>
    ///     Both sides are read off the same GetStrength, which already folds in skill, savant stage
    ///     and flaring - and already spikes tenfold on a duralumin burn. So duralumin piercing a
    ///     cloud, and a duralumin-fed cloud swallowing a Seeker, both fall out of this comparison
    ///     without a rule of their own.
    /// </remarks>
    public static bool Blocks(float cloud, float sense) {
        return cloud > 0f && cloud >= sense;
    }
}
