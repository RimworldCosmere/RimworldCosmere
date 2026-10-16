using Concord;
using Cosmere.System.Roshar.Surgebinding;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class SurgeryViolationPatch : Recipe_Surgery {
    [Inject(At.Return, nameof(CheckSurgeryFail))]
    private void AfterCheckSurgeryFail(Pawn surgeon, Pawn patient, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (surgeon == null || patient == null) return;

        if (ViolationUtility.IsSurgebinderOfOrder(surgeon, RadiantOrderDefOf.Truthwatcher)) {
            ViolationUtility.ApplyViolation(surgeon, 0.3f, "botching a surgery");
        }
    }
}
