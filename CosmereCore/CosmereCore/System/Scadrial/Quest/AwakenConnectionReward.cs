using System.Collections.Generic;
using Cosmere.Core.Comp.Game;
using Cosmere.Core.Def;
using Cosmere.Core.Quest;
using Cosmere.Core.Quest.Reward;
using RimWorld;
using Verse;
using GeneUtility = Cosmere.System.Scadrial.Util.GeneUtility;

namespace Cosmere.System.Scadrial.Quest;

/// <summary>
///     Wakes Allomancy in colonists who were carrying it dormant, and leaves the colony tied
///     more closely to the Shard so the ones who come after are likelier to carry it too.
/// </summary>
public class AwakenConnectionReward : QuestReward {
    /// <summary>How much the colony's tie to the Shard deepens. 1.0 doubles the odds.</summary>
    public float connectionGain = 1f;

    public int countMax = 2;
    public int countMin = 1;
    public ShardDef? shard;

    public override void Give(QuestBuildContext ctx) {
        Current.Game?.GetComponent<ShardConnections>()?.Add(shard, connectionGain);

        Verse.Map? map = ctx.map;
        if (map == null) return;

        List<Pawn> candidates = new List<Pawn>();
        List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (pawn.story?.traits?.HasTrait(TraitDefOf.Cosmere_Scadrial_Trait_Mistborn) == true) continue;

            candidates.Add(pawn);
        }

        if (candidates.Count == 0) return;

        int wanted = Rand.RangeInclusive(countMin, countMax);
        int awakened = 0;
        candidates.Shuffle();
        for (int i = 0; i < candidates.Count && awakened < wanted; i++) {
            GeneUtility.AddRandomAllomanticGene(candidates[i], false, true);
            awakened++;
        }

        if (awakened == 0) return;

        Messages.Message(
            "CS_Quest_Reward_Awakened".Translate(awakened.Named("COUNT")),
            candidates[0],
            MessageTypeDefOf.PositiveEvent
        );
    }

    public override string Describe() {
        return "CS_Quest_Reward_Connection".Translate().Resolve();
    }

    public override string? ConfigError() {
        if (shard == null) return "AwakenConnectionReward has no shard.";
        if (countMin < 0) return "AwakenConnectionReward countMin cannot be negative.";
        if (countMax < countMin) return "AwakenConnectionReward countMax is below countMin.";
        if (connectionGain < 0f) return "AwakenConnectionReward connectionGain cannot be negative.";
        return null;
    }
}
