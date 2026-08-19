using Cosmere.Core.Comp.Game;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Changes which Shards hold the world. For the one beat in the campaign where that actually
///     happens: two Shards taken up together stop being two Shards.
/// </summary>
public class SetShardsAction : ProgressionAction {
    public List<string> disable = [];
    public List<string> enable = [];

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Shards? shards = Current.Game?.GetComponent<Shards>();
        if (shards == null) {
            Logger.Warning("ScenarioProgression: no Shards component, cannot change Shards.");
            return;
        }

        for (int i = 0; i < disable.Count; i++) {
            shards.DisableShard(disable[i]);
        }

        // conflicts allowed on purpose: Harmony replaces the two disabled above, so overlap here is expected
        for (int i = 0; i < enable.Count; i++) {
            shards.EnableShard(enable[i], true);
        }

        Logger.Important($"ScenarioProgression: Shards changed - off [{string.Join(", ", disable)}], on [{string.Join(", ", enable)}].");
    }

    public override string? Describe() {
        return enable.Count == 0
            ? null
            : "CS_Progression_Effect_Shards".Translate(string.Join(", ", enable).Named("SHARDS")).Resolve();
    }
}
