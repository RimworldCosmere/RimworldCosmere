using Cosmere.Core;
using Cosmere.System.Scadrial.Comp.Game;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Taking hold of a koloss, letting one go, and what happens in between.
/// </summary>
/// <remarks>
///     Two acts, not one. Seizing is a contest against the creature's resistance and can fail;
///     holding is a bill the roster collects. Everything the player sees hangs off which of those
///     two is happening.
/// </remarks>
public static class KolossControl {
    /// <summary>
    ///     Ticks a koloss stands loose before it turns. Long enough to fix a mistake.
    /// </summary>
    /// <remarks>
    ///     Read from settings rather than fixed, because how forgiving a lost hold should be is
    ///     the kind of thing one colony wants tense and another wants survivable.
    /// </remarks>
    public static int GraceTicks =>
        Mathf.Max(0, Mathf.RoundToInt(Mod.kolossGraceSeconds
                                      * GenTicks.TicksPerRealSecond));

    public static bool IsHeld(Pawn? koloss) {
        return KolossRoster.Current?.HolderOf(koloss) != null;
    }

    /// <summary>
    ///     Attempts a seizure, and says why not when it fails.
    /// </summary>
    /// <remarks>
    ///     Both numbers go in the failure text. A bind that fails without saying how short it fell
    ///     reads as a broken button, and the player has no way to learn that duralumin is the
    ///     answer.
    /// </remarks>
    public static AcceptanceReport TryBind(
        Pawn holder,
        Pawn koloss,
        float reach,
        Cosmere.Core.Def.MetalDef? metal = null,
        Cosmere.Core.Def.AbilityDef? through = null
    ) {
        KolossRoster? roster = KolossRoster.Current;
        if (roster == null) return "CS_KolossBind_NoGame".Translate();

        float needed = EmotionalResistance.Of(koloss);
        if (needed <= 0f) return "CS_KolossBind_NotBindable".Translate(koloss.LabelShortCap.Named("TARGET"));

        Cosmere.Core.Logger.Verbose(
            $"Koloss seize: {holder.LabelShort} reach {reach:F2} against {koloss.LabelShort} needing {needed:F2}."
        );

        if (reach < needed) {
            return "CS_KolossBind_TooWeak".Translate(
                koloss.LabelShortCap.Named("TARGET"),
                reach.ToString("F1").Named("REACH"),
                needed.ToString("F1").Named("NEEDED")
            );
        }

        roster.Bind(holder, koloss, metal, through);

        return AcceptanceReport.WasAccepted;
    }

    /// <summary>
    ///     Lets one koloss go without touching anything else the holder is holding.
    /// </summary>
    public static void Release(Pawn koloss) {
        KolossRoster.Current?.Release(koloss);

        // Immediately. Letting one go is a decision the player made, so it takes effect when they
        // make it - the grace window is for the metal running out, which they could not have
        // prevented and might still fix.
        Lapse(koloss);
    }

    /// <summary>
    ///     Hands a koloss nobody is holding back to the koloss.
    /// </summary>
    /// <remarks>
    ///     It changes sides rather than going berserk. A berserk state made it attack the nearest
    ///     thing, which meant a band of loose koloss tore each other apart in the field - and
    ///     koloss do not fight koloss, they march with them. Joining a permanently hostile faction
    ///     gets everything the mental state was for, keeps them dangerous to the colony, and lets
    ///     them behave like an army instead of a riot.
    ///     <para>
    ///         Undraft and drop first, because neither happens on a faction change the way it does
    ///         inside TryStartMentalState. Without it the koloss walks off carrying a colonist.
    ///     </para>
    /// </remarks>
    public static void Lapse(Pawn koloss) {
        Faction? theirs = Faction.OfPlayerSilentFail == null
            ? null
            : Find.FactionManager?.FirstFactionOfDef(
                DefDatabase<FactionDef>.GetNamedSilentFail(KolossFaction)
            );
        if (theirs == null || koloss.Faction == theirs) return;

        koloss.drafter?.Drafted = false;
        koloss.carryTracker?.TryDropCarriedThing(koloss.PositionHeld, ThingPlaceMode.Near, out _);
        koloss.jobs?.EndCurrentJob(JobCondition.InterruptForced);

        koloss.SetFaction(theirs);

        Messages.Message(
            "CS_KolossLapsed".Translate(koloss.LabelShortCap.Named("KOLOSS")),
            koloss,
            MessageTypeDefOf.NegativeEvent
        );
    }

    /// <summary>The faction a koloss belongs to when nobody is holding it.</summary>
    public const string KolossFaction = "Cosmere_Scadrial_Faction_Koloss";
}
