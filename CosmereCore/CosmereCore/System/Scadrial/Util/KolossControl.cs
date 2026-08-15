using Cosmere.System.Scadrial.Comp.Game;
using RimWorld;
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
    /// <summary>Ticks a koloss stands loose before it turns. Long enough to fix a mistake.</summary>
    public const int GraceTicks = 600;

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
    public static AcceptanceReport TryBind(Pawn holder, Pawn koloss, float reach) {
        KolossRoster? roster = KolossRoster.Current;
        if (roster == null) return "CS_KolossBind_NoGame".Translate();

        float needed = EmotionalResistance.Of(koloss);
        if (needed <= 0f) return "CS_KolossBind_NotBindable".Translate(koloss.LabelShortCap.Named("TARGET"));

        int capacity = KolossRoster.CapacityOf(holder);
        if (roster.HolderOf(koloss) != holder && roster.UsedBy(holder) >= capacity) {
            return "CS_KolossBind_NoCapacity".Translate(
                holder.LabelShortCap.Named("HOLDER"),
                capacity.Named("CAPACITY")
            );
        }

        if (reach < needed) {
            return "CS_KolossBind_TooWeak".Translate(
                koloss.LabelShortCap.Named("TARGET"),
                reach.ToString("F1").Named("REACH"),
                needed.ToString("F1").Named("NEEDED")
            );
        }

        roster.Bind(holder, koloss);
        Calm(koloss);

        return AcceptanceReport.WasAccepted;
    }

    /// <summary>
    ///     Lets one koloss go without touching anything else the holder is holding.
    /// </summary>
    public static void Release(Pawn koloss) {
        KolossRoster.Current?.Release(koloss);
    }

    /// <summary>
    ///     Turns a loose koloss on whatever is nearest.
    /// </summary>
    /// <remarks>
    ///     transitionSilently kills the letter, the tale and the TimesInMentalState increment more
    ///     cleanly than blanking def fields, but it also skips the recover-from-previous
    ///     transition, which is why this checks InMentalState first.
    ///     <para>
    ///         No undrafting or dropping here. TryStartMentalState does both itself - it clears
    ///         Drafted, calls StopAll, and drops whatever the pawn was carrying - so the koloss
    ///         will not walk a colonist's corpse into the fight it is about to start.
    ///     </para>
    /// </remarks>
    public static void Lapse(Pawn koloss) {
        if (koloss.InMentalState) return;

        bool started = koloss.mindState?.mentalStateHandler?.TryStartMentalState(
            MentalStateDefOf.Cosmere_Scadrial_MentalState_KolossBloodlust,
            "CS_KolossBloodlust_Reason".Translate(koloss.LabelShortCap.Named("KOLOSS")).Resolve(),
            true,
            true,
            transitionSilently: true
        ) ?? false;

        if (!started) return;

        Messages.Message(
            "CS_KolossLapsed".Translate(koloss.LabelShortCap.Named("KOLOSS")),
            koloss,
            MessageTypeDefOf.NegativeEvent
        );
    }

    /// <summary>
    ///     Ends a bloodlust and stops the swing that was already coming.
    /// </summary>
    /// <remarks>
    ///     RecoverFromState only when the current state is ours, or taking hold of a koloss would
    ///     also cure a berserk colonist standing next to it. JobGiver_Berserk sets an expiry of
    ///     420 to 900 ticks, so without ending the job and clearing the target the koloss lands
    ///     another one to three hits after the Allomancer has it back.
    /// </remarks>
    public static void Calm(Pawn koloss) {
        MentalState? state = koloss.MentalState;
        if (state == null || state.def != MentalStateDefOf.Cosmere_Scadrial_MentalState_KolossBloodlust) return;

        state.RecoverFromState();
        koloss.jobs?.EndCurrentJob(JobCondition.InterruptForced);
        if (koloss.mindState != null) koloss.mindState.enemyTarget = null;
    }
}
