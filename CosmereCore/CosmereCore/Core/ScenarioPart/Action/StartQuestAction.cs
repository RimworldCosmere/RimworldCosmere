using Cosmere.Core.Quest;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class StartQuestAction : ProgressionAction {
    public CosmereQuestDef? questDef;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        if (questDef == null) {
            Logger.Warning("ScenarioProgression: StartQuestAction has no questDef set");
            return;
        }

        if (questDef.kind != QuestKind.Capstone) {
            Logger.Error(
                $"ScenarioProgression: StartQuestAction targets '{questDef.defName}', which is not a Capstone quest (kind={questDef.kind})"
            );
            return;
        }

        Map? map = Find.AnyPlayerHomeMap;
        if (map == null) {
            Logger.Warning($"ScenarioProgression: no player home map available to start capstone '{questDef.defName}'");
            return;
        }

        CosmereQuestManager? manager = Current.Game?.GetComponent<CosmereQuestManager>();
        if (manager == null) {
            Logger.Warning($"ScenarioProgression: CosmereQuestManager unavailable, cannot start capstone '{questDef.defName}'");
            return;
        }

        manager.TryStartCapstone(questDef, map, null);
    }

    public override string? Describe() {
        return questDef == null
            ? null
            : "CC_Progression_Effect_Quest".Translate(questDef.label.Named("QUEST")).Resolve();
    }
}
