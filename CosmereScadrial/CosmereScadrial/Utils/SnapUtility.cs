using RimWorld;
using Verse;

namespace CosmereScadrial.Utils {
    public class SnapUtility {
        public static ThoughtDef SnappedThought = ThoughtDefOf.Cosmere_Scadrial_Snapped;

        public static void TrySnap(Pawn pawn, string cause = "") {
            if (!IsSnapped(pawn)) return;

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(SnappedThought);
            var firstMemoryOfDef = pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(SnappedThought);
            firstMemoryOfDef!.permanent = true;
            firstMemoryOfDef.moodOffset = 1;
            firstMemoryOfDef.moodPowerFactor = 0f;

            Find.LetterStack.ReceiveLetter(
                "A pawn has snapped!",
                $"{pawn.NameFullColored} has snapped and gained access to some new Investiture related powers!",
                LetterDefOf.PositiveEvent,
                pawn
            );
        }

        public static bool IsSnapped(Pawn pawn) {
            return pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(SnappedThought) != null;
        }
    }
}