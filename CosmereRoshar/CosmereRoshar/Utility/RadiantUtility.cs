using Cosmere.Roshar.Need;
using Verse;

namespace Cosmere.Roshar.Utility;

public static class RadiantUtility {
    public static void GiveRadiantXp(Pawn pawn, float amount) {
        if (pawn == null) {
            return;
        }

        RadiantProgress progress = pawn.needs?.TryGetNeed<RadiantProgress>();
        if (progress != null) {
            progress.GainXp(amount);
            progress.UpdateRadiantTrait(pawn);
        }
    }
}