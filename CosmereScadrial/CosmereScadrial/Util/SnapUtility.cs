using RimWorld;
using Verse;

namespace CosmereScadrial.Util;

public class SnapUtility {
    public static ThoughtDef snappedThought = ThoughtDefOf.Cosmere_Scadrial_Snapped;

    public static void TrySnap(Pawn pawn, string? cause = "", bool withMessage = true) {
        if (IsSnapped(pawn)) return;

        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(snappedThought);
        Thought_Memory? firstMemoryOfDef = pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(snappedThought);
        firstMemoryOfDef!.permanent = true;
        firstMemoryOfDef.moodOffset = 1;
        firstMemoryOfDef.moodPowerFactor = 0f;

        if (!withMessage || !pawn.Faction.IsPlayer || pawn.Map == null) {
            return;
        }

        Find.LetterStack.ReceiveLetter(
            LetterMaker.MakeLetter(
                "CS_PawnSnapped_Title".Translate(),
                "CS_PawnSnapped_Message".Translate(pawn.NameFullColored.Named("PAWN")).Resolve(),
                LetterDefOf.PositiveEvent,
                pawn
            )
        );
    }

    public static bool IsSnapped(Pawn pawn) {
        return pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(snappedThought) != null;
    }
}