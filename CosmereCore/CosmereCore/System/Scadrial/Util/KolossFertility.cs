using Cosmere.Core.Quest;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Whether a koloss can father a child yet.
/// </summary>
/// <remarks>
///     Before the Catacendre they cannot. A koloss is made out of a person with four spikes, and
///     what comes out the other side does not breed - the books are plain about it. Harmony
///     changed that afterwards, which is where koloss-blooded lines come from at all.
///     <para>
///         Keyed to the era rather than the Shard, following <see cref="AshEra" />. Asking whether
///         Harmony is held would pass in a sandbox Pre-Catacendre start with Harmony toggled on,
///         and that is a start where koloss are still what Rashek made.
///     </para>
/// </remarks>
public static class KolossFertility {
    /// <summary>Pure so it can be tested without Verse, the same as IsAshEra.</summary>
    public static bool CanBreed(string? eraDefName) {
        return eraDefName != null && eraDefName != AshEra.AshEraDefName;
    }

    public static bool CanBreedNow() {
        return CanBreed(CosmereQuestManager.FindActiveEra());
    }
}
