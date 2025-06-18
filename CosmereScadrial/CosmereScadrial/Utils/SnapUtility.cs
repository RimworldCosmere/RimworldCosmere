using RimWorld;
using Verse;

namespace CosmereScadrial.Utils;

public class SnapUtility {
    public static ThoughtDef SnappedThought = ThoughtDefOf.Cosmere_Scadrial_Snapped;

    public static void TrySnap(Pawn pawn, string cause = "", bool withMessage = true) {
        if (IsSnapped(pawn)) return;

        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(SnappedThought);
        Thought_Memory? firstMemoryOfDef = pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(SnappedThought);
        firstMemoryOfDef!.permanent = true;
        firstMemoryOfDef.moodOffset = 1;
        firstMemoryOfDef.moodPowerFactor = 0f;

        if (!withMessage || !pawn.Faction.IsPlayer) return;
        Find.LetterStack.ReceiveLetter(
            LetterMaker.MakeLetter(
                "Pawn Snapped!",
                $"{pawn.NameFullColored} has snapped and gained access to some new Investiture related powers!",
                LetterDefOf.PositiveEvent,
                pawn
            )
        );
    }

    public static bool IsSnapped(Pawn pawn) {
        return pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(SnappedThought) != null;
    }
}