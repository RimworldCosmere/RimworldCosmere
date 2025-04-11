using CosmereScadrial.Comps;
using Verse;

namespace CosmereScadrial.Utils {
    public static class AllomancyUtility {
        public static void AddMetalReserve(Pawn pawn, string metal, float amount) {
            var tracker = pawn.GetComp<CompScadrialInvestiture>();
            if (tracker == null) {
                Log.Warning($"{pawn.NameShortColored} has no InvestitureTracker. Skipping {metal} absorption.");
                return;
            }

            tracker.AddReserve(metal, amount);
        }
    }
}