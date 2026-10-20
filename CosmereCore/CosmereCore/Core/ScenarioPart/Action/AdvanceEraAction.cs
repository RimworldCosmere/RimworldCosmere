using Cosmere.Core.Quest;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Moves the campaign into the era that follows the current one, as declared by
///     EraDef.next. The beat that ends an age uses this - after the Catacendre a colony is not
///     living in the Final Empire any more, and its quest pool should say so.
/// </summary>
public class AdvanceEraAction : ProgressionAction {
    public override void Execute(GameComponent_ScenarioProgression comp) {
        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        if (manager == null) {
            Log.Warn("ScenarioProgression: CosmereQuestManager unavailable, cannot advance the era");
            return;
        }

        manager.AdvanceEra();
    }
}
