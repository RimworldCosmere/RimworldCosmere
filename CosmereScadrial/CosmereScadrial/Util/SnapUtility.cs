using RimWorld;
using Verse;

namespace CosmereScadrial.Util;

public class SnapUtility {
    public static void TrySnap(Pawn pawn, string? cause = "", bool withMessage = true) {
        if (IsSnapped(pawn)) return;


        Thought_Memory memory = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Scadrial_Snapped, 1);
        memory.permanent = true;
        memory.moodOffset = 1;
        memory.moodPowerFactor = 0f;
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(memory);

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
        return pawn.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(ThoughtDefOf.Cosmere_Scadrial_Snapped) != null;
    }
}