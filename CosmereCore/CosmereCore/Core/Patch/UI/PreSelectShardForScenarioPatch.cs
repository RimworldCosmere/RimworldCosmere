using Concord;
using Cosmere.Core.Util;
using RimWorld;
using DefModExtension_Shards = Cosmere.Core.DefModExtension.Shards;

namespace Cosmere.Core.Patch;

/// <summary>
///     Switches on the Shards a scenario declares, before anything reads them.
/// </summary>
/// <remarks>
///     Runs at PreConfigure, which BeginScenarioConfiguration calls on every pass - including
///     when the player navigates back to scenario selection, which builds a fresh Game and wipes
///     the shard component. That is what keeps the set correct across back-navigation.
///     <para>
///         This used to have a twin, LockShardSelectionPatch, which enabled the same list again
///         at StitchedPages and announced it in a toast. The toast referred to a shard-selection
///         page that no longer exists, and the second enable was a no-op, so both went.
///     </para>
/// </remarks>
public abstract class PreSelectShardForScenarioPatch : Scenario {
    [Inject(At.Head, nameof(PreConfigure))]
    private void BeforePreConfigure() {
        DefModExtension_Shards? shards = ScenarioDefUtility.CurrentShards;
        if (shards?.shards is not { Count: > 0 }) return;

        for (int i = 0; i < shards.shards.Count; i++) {
            ShardUtility.Enable(shards.shards[i]);
        }
    }
}
