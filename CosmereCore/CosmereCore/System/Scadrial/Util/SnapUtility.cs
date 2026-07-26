using RimWorld;
using Verse;
using Cosmere.System.Scadrial.Hemalurgy;
using DormantConnectionComp = Cosmere.Core.Comp.Thing.DormantConnection;

namespace Cosmere.System.Scadrial.Util;

public static class SnapUtility {
    public static void Snap(Pawn pawn, string? cause = "", bool withMessage = true) {
        if (pawn.IsSnapped()) return;
        if (pawn.health.hediffSet.HasHediff(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab)) return;

        DormantConnectionComp? comp = pawn.TryGetComp<DormantConnectionComp>();
        if (comp == null || !comp.hasDormantConnections) return;

        Thought_Memory memory = ThoughtMaker.MakeThought(ThoughtDefOf.Cosmere_Scadrial_Snapped, 0);
        memory.permanent = true;
        memory.moodOffset = 1;
        memory.moodPowerFactor = 0f;
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(memory);

        if (!withMessage || pawn.Faction == null || !pawn.Faction.IsPlayer || pawn.Map == null) {
            return;
        }

        TaggedString title = "CS_PawnSnapped_Title".Translate();
        TaggedString message = string.IsNullOrEmpty(cause)
            ? "CS_PawnSnapped_Message".Translate(pawn.NameFullColored.Named("PAWN")).Resolve()
            : cause.Translate(pawn.NameFullColored.Named("PAWN")).Resolve();

        Find.LetterStack.ReceiveLetter(
            LetterMaker.MakeLetter(title, message, LetterDefOf.PositiveEvent, pawn)
        );
    }
}