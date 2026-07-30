using System.Collections.Generic;
using Verse;

namespace Cosmere.Core.Quest;

/// <summary>
///     Charges the player silver for a quest choice. RimWorld.TradeUtility.LaunchThingsOfType
///     only pulls from cells within a powered orbital trade beacon's radius and silently
///     charges nothing if no beacon is in range, which would let a bribe succeed for free.
///     This counts silver across every player home map first and refuses the charge entirely
///     if the player cannot afford it.
/// </summary>
public static class QuestSilver {
    /// <summary>
    ///     Attempts to destroy amount silver across player home maps. Returns false and
    ///     charges nothing if the player does not have enough.
    /// </summary>
    public static bool TryCharge(int amount) {
        if (amount <= 0) return true;

        List<Map> maps = Find.Maps;
        List<Verse.Thing> stacks = new List<Verse.Thing>();
        int total = 0;

        for (int i = 0; i < maps.Count; i++) {
            Map map = maps[i];
            if (!map.IsPlayerHome) continue;

            List<Verse.Thing> onMap = map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Silver);
            for (int j = 0; j < onMap.Count; j++) {
                stacks.Add(onMap[j]);
                total += onMap[j].stackCount;
            }
        }

        if (total < amount) return false;

        int remaining = amount;
        for (int i = stacks.Count - 1; i >= 0 && remaining > 0; i--) {
            Verse.Thing stack = stacks[i];
            int take = stack.stackCount < remaining ? stack.stackCount : remaining;
            stack.SplitOff(take).Destroy();
            remaining -= take;
        }

        return true;
    }
}
