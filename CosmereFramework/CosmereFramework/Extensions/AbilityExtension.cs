using RimWorld;
using Verse;

namespace CosmereFramework.Extensions;

public static class AbilityExtension {
    public static AcceptanceReport GizmoDisabled(this Ability ability) {
        bool isDisabled = ability.GizmoDisabled(out string reason);
        if (!isDisabled) return false;

        return new AcceptanceReport(reason);
    }
}