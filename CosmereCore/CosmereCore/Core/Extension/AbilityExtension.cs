using Verse;

namespace Cosmere.Core.Extension;

public static class AbilityExtension {
    public static AcceptanceReport GizmoEnabled(this RimWorld.Ability ability) {
        bool isDisabled = ability.GizmoDisabled(out string reason);
        if (!isDisabled) return true;

        return new AcceptanceReport(reason);
    }
}
