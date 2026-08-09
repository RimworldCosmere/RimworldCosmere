using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Hemalurgy;

/// <summary>
///     One thing Ruin can push a spiked pawn into doing.
/// </summary>
public class RuinCompulsion {
    public string key = string.Empty;

    /// <summary>Looked up by name; not all of these are on vanilla's MentalStateDefOf.</summary>
    public string stateDefName = string.Empty;

    public MentalStateDef? State => DefDatabase<MentalStateDef>.GetNamedSilentFail(stateDefName);

    /// <summary>Fewest spikes at which Ruin will reach for this one.</summary>
    public int minimumSpikes = 4;
}

/// <summary>
///     What Ruin does with somebody it has enough of.
/// </summary>
/// <remarks>
///     Four spikes is enough for Ruin to move a person, so the risk has to be real. Random
///     psychotic wandering was real and not interesting: it arrived without warning, could not be
///     prevented, and read as dice rather than as a god leaning on somebody.
///     <para>
///         So it announces itself. Ruin decides, the colony gets told, and there is a window to
///         do something about it. Putting the pawn down, arresting them or knocking them out all
///         stop it, because none of those leave anybody upright to be moved.
///     </para>
/// </remarks>
public static class RuinCompulsions {
    /// <summary>How long the colony has between the warning and the act. Half a day.</summary>
    public const int WarningTicks = 30000;

    private static List<RuinCompulsion>? all;

    public static List<RuinCompulsion> All => all ??= [
        new RuinCompulsion { key = "Wander", stateDefName = "Wander_Psychotic", minimumSpikes = 4 },
        new RuinCompulsion { key = "Violence", stateDefName = "Berserk", minimumSpikes = 5 },
        new RuinCompulsion { key = "Fire", stateDefName = "FireStartingSpree", minimumSpikes = 6 },
        new RuinCompulsion { key = "Slaughter", stateDefName = "Slaughterer", minimumSpikes = 7 },
    ];

    /// <summary>Picks what Ruin wants, from what this many spikes let it ask for.</summary>
    public static RuinCompulsion? Choose(int spikes) {
        List<RuinCompulsion> available = [];
        for (int i = 0; i < All.Count; i++) {
            if (All[i].minimumSpikes <= spikes && All[i].State != null) available.Add(All[i]);
        }

        return available.Count == 0 ? null : available.RandomElement();
    }

    /// <summary>
    ///     Whether the pawn is still in a state to be used. Ruin needs someone on their feet.
    /// </summary>
    public static bool CanBeMoved(Pawn pawn) {
        if (pawn.Dead || pawn.Downed) return false;
        if (pawn.InMentalState) return false;
        if (pawn.IsPrisoner) return false;

        return pawn.Awake();
    }

    public static void Warn(Pawn pawn, RuinCompulsion compulsion) {
        Find.LetterStack.ReceiveLetter(
            ("CS_Ruin_Compulsion_" + compulsion.key + "_Title").Translate(
                pawn.NameShortColored.Named("PAWN"),
                HemalurgicShard.Name.Named("SHARD")
            ),
            ("CS_Ruin_Compulsion_" + compulsion.key + "_Text").Translate(
                pawn.NameShortColored.Named("PAWN"),
                HemalurgicShard.Name.Named("SHARD")
            ),
            LetterDefOf.ThreatSmall,
            pawn
        );
    }

    public static void Fire(Pawn pawn, RuinCompulsion compulsion) {
        MentalStateDef? state = compulsion.State;
        if (state == null) return;

        pawn.mindState.mentalStateHandler.TryStartMentalState(
            state,
            "CS_Hemalurgy_RuinsControl".Translate(HemalurgicShard.Name.Named("SHARD")),
            forceWake: true,
            causedByMood: false
        );
    }
}
