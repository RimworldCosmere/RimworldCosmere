using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Reward;

/// <summary>
///     Hands every colonist the same memory. For a quest the whole colony lived through rather
///     than one that paid a courier.
/// </summary>
public class ColonyThoughtReward : QuestReward {
    public ThoughtDef? thought;

    public override void Give(QuestBuildContext ctx) {
        if (thought == null) return;

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            Verse.Map map = maps[i];
            if (!map.IsPlayerHome) continue;

            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int j = 0; j < colonists.Count; j++) {
                colonists[j].needs?.mood?.thoughts?.memories?.TryGainMemory(thought);
            }
        }
    }

    public override string Describe() {
        return thought == null
            ? string.Empty
            : "CC_Quest_Reward_ColonyThought".Translate(thought.stages[0].label.Named("THOUGHT")).Resolve();
    }

    public override string? ConfigError() {
        if (thought == null) return "ColonyThoughtReward has no thought.";
        if (thought.stages.NullOrEmpty()) return "ColonyThoughtReward thought has no stages.";
        return null;
    }
}
