using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>Stand in something and live through it.</summary>
public class SurviveConditionObjective : QuestObjective {
    public GameConditionDef? conditionDef;
    public bool requireUnroofed = true;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        if (ctx.subject == null) {
            throw new QuestBuildFailure("SurviveConditionObjective requires a pawn-scoped quest.");
        }

        QuestPart_SurviveCondition survive = new QuestPart_SurviveCondition {
            quest = quest,
            subject = ctx.subject,
            conditionDef = conditionDef,
            requireUnroofed = requireUnroofed,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(survive);
    }

    public override string? ConfigError() {
        return conditionDef == null ? "SurviveConditionObjective has no conditionDef." : null;
    }
}
