using System.Collections.Generic;

namespace Cosmere.Core.Quest.Prereq;

/// <summary>
///     Requires a bonded Radiant in the colony. Leave order empty to accept any of them, which
///     is what the Oathgate wants: someone who can turn the lock, not a particular someone.
/// </summary>
public class RadiantOrderPrereq : QuestPrereq {
    public int minIdeal = 1;
    public string? order;

    public override bool IsMet(QuestWorldState state) {
        if (!string.IsNullOrEmpty(order)) {
            return state.bondedOrders.TryGetValue(order!, out int ideal) && ideal >= minIdeal;
        }

        foreach (KeyValuePair<string, int> pair in state.bondedOrders) {
            if (pair.Value >= minIdeal) return true;
        }

        return false;
    }

    public override string? ConfigError() {
        return minIdeal < 1 ? "RadiantOrderPrereq minIdeal must be at least 1." : null;
    }
}
