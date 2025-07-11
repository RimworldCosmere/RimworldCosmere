using RimWorld;
using Verse;

namespace CosmereFramework.Extension;

public static class AbilityExtension {
    public static AcceptanceReport GizmoEnabled(this Ability ability) {
        bool isDisabled = ability.GizmoDisabled(out string reason);
        if (!isDisabled) return true;

        return new AcceptanceReport(reason);
    }
}