using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>Get a specific thing back to a player home map.</summary>
public class CarryHomeObjective : QuestObjective {
    public int count = 1;
    public ThingDef? thingDef;

    public override void AddParts(RimWorld.Quest quest, string inSignal, string outSignal, QuestBuildContext ctx) {
        QuestPart_CarryHome carry = new QuestPart_CarryHome {
            quest = quest,
            requiredThing = thingDef,
            requiredCount = count,
            inSignalEnable = inSignal,
            outSignalsCompleted = new List<string> { outSignal },
        };
        quest.AddPart(carry);
    }

    public override string? ConfigError() {
        if (thingDef == null) return "CarryHomeObjective has no thingDef.";
        if (count < 1) return "CarryHomeObjective count must be at least 1.";
        return null;
    }
}
