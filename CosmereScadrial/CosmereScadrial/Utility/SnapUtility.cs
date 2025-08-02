using RimWorld;
using Verse;

namespace Cosmere.Scadrial.Utility;

public class SnapUtility {
    public static void TrySnap(Pawn pawn, string? cause = "", bool withMessage = true) {
        if (pawn.IsSnapped()) return;

        Thought_Memory memory = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Scadrial_Snapped, 0);
        memory.permanent = true;
        memory.moodOffset = 1;
        memory.moodPowerFactor = 0f;
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(memory);

        if (!withMessage || pawn.Faction == null || !pawn.Faction.IsPlayer || pawn.Map == null) {
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
}