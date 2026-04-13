using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding;

public static class ViolationUtility {
    public static void ApplyViolation(Pawn pawn, float severity, string? reason = null) {
        if (pawn == null || pawn.Dead) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        ILoadReferenceable? bondTarget = surgebinder.GetBondTarget();
        if (bondTarget == null) return;

        float multiplier = 1f;
        if (bondTarget is Verse.Pawn sprenPawn) {
            CompSprenBond? sprenBond = sprenPawn.TryGetComp<CompSprenBond>();
            if (sprenBond != null) {
                multiplier = sprenBond.GetViolationSeverityMultiplier();
            }
        }

        SpiritWeb.Instance?.AdjustConnection(pawn, bondTarget, -severity * multiplier);

        string violationReason = reason ?? "unknown action";
        string severityLabel = severity switch {
            >= 0.5f => "catastrophic",
            >= 0.2f => "major",
            _ => "minor",
        };
        string orderLabel = surgebinder.radiantOrderDef.LabelCap;

        Find.PlayLog.Add(new BondViolationLogEntry(pawn, orderLabel, violationReason, severityLabel));
    }

    public static bool IsSurgebinderOfOrder(Pawn pawn, string orderDefName) {
        if (pawn == null || pawn.Dead) return false;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;

        return surgebinder.radiantOrderDef.defName == orderDefName;
    }

    public static Surgebinder? GetSurgebinder(Pawn pawn) {
        if (pawn == null || pawn.Dead) return null;
        return pawn.genes?.GetFirstGeneOfType<Surgebinder>();
    }
}
