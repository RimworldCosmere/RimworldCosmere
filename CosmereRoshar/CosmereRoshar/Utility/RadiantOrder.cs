using Cosmere.Roshar.Comp.Thing;
using Verse;

namespace Cosmere.Roshar.Utility;

public static class RadiantOrder {
    public static bool BondWithSpren(Pawn pawn) {
        if (pawn.IsSurgebinder()) return false;

        Find.LetterStack.ReceiveLetter(
            "CRO_Bond_Spren_Title".Translate(pawn.LabelShortCap.Named("PAWN")),
            "CRO_Bond_Spren_Content".Translate(pawn.NameFullColored.Named("PAWN")).Resolve(),
            LetterDefOf.Cosmere_Roshar_ChooseRadiantOrder,
            new LookTargets(pawn)
        );

        pawn.AllComps.Add(new ChooseRadiantOrder { parent = pawn });
        return true;
    }
}