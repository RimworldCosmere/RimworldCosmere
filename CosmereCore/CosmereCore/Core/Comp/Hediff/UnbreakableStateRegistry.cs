using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Comp.Hediff;

/// <summary>
///     Mental states that nothing is allowed to calm out of a pawn by accident.
/// </summary>
/// <remarks>
///     Some states are not moods, they are a fact about the world. A koloss in bloodlust is loose
///     because nobody is holding it, and the only thing that should end that is somebody taking
///     hold of it again. The soothing handlers in Core roll against every mental state alike, so
///     without this a Soother standing nearby cures the bloodlust at random and throws a green
///     "Calmed" mote while doing it.
///     <para>
///         A registry rather than a def check, because Core must not import a shard. Scadrial
///         registers the koloss bloodlust at startup and Core never learns what a koloss is.
///     </para>
/// </remarks>
public static class UnbreakableStateRegistry {
    private static readonly HashSet<MentalStateDef> Protected = [];

    public static void Register(MentalStateDef? state) {
        if (state != null) Protected.Add(state);
    }

    /// <summary>True when this pawn's current state must be left exactly where it is.</summary>
    public static bool Guards(Pawn? pawn) {
        MentalStateDef? state = pawn?.MentalStateDef;

        return state != null && Protected.Contains(state);
    }
}
