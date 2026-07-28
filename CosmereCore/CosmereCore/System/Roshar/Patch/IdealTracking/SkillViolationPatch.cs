using Concord;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class SkillViolationPatch : SkillRecord {
    // The pre-call level is only knowable before Learn runs, so the whole call is wrapped.
    [Inject(At.Around, nameof(Learn))]
    private void AroundLearn(
        float xp,
        bool direct,
        bool ignoreLearnRate,
        VoidOperation<float, bool, bool> original
    ) {
        SkillRecord self = this;
        int oldLevel = self.Level;

        original.Invoke(xp, direct, ignoreLearnRate);

        if (xp >= 0) return;
        if (self.Level >= oldLevel) return;

        Pawn pawn = self.Pawn;
        if (pawn == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(pawn, RadiantOrderDefOf.Elsecaller)) {
            ViolationUtility.ApplyViolation(pawn, 0.3f, "losing a skill level");
        }
    }
}
