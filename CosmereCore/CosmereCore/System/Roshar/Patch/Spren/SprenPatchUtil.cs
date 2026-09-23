using Cosmere.System.Roshar.Comp.Thing;
using Verse;
using DecoyHediff = Cosmere.System.Roshar.Surgebinding.Hediff.Illumination.LightweavingDecoy;

namespace Cosmere.System.Roshar.Patch.Spren;

internal static class SprenPatchUtil {
    internal static bool IsSprenOrDecoy(Pawn pawn) =>
        pawn.TryGetComp<SprenBond>() != null || DecoyHediff.IsDecoy(pawn);

    internal static void RemoveSprenAndDecoys(List<Pawn> pawns) {
        for (int i = pawns.Count - 1; i >= 0; i--) {
            if (IsSprenOrDecoy(pawns[i])) pawns.RemoveAt(i);
        }
    }
}
