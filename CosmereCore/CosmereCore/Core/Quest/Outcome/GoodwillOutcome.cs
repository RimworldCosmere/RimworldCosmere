using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Outcome;

/// <summary>Adjusts goodwill with the quest's target faction on failure.</summary>
public class GoodwillOutcome : QuestOutcome {
    public int goodwillChange = -20;

    public override void Resolve(QuestBuildContext ctx) {
        if (ctx.def?.targetFaction == null) return;

        Faction? faction = Find.FactionManager.FirstFactionOfDef(ctx.def.targetFaction);
        faction?.TryAffectGoodwillWith(Faction.OfPlayer, goodwillChange);
    }
}
