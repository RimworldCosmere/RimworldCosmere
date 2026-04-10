using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Scadrial.Hemalurgy;
using Verse;

namespace Cosmere.System.Roshar.Utility;

public static class RadiantOrder {
    public static bool BondWithSpren(Pawn pawn, bool showLetter = true) {
        if (pawn.IsSurgebinder()) return false;
        if (pawn.health.hediffSet.HasHediff(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab)) return false;

        RadiantTracker? tracker = Current.Game?.GetComponent<RadiantTracker>();
        if (tracker != null && !tracker.HasAnyAvailableOrder(pawn)) return false;

        if (showLetter) {
            Find.LetterStack.ReceiveLetter(
                "CRO_Bond_Spren_Title".Translate(pawn.LabelShortCap.Named("PAWN")),
                "CRO_Bond_Spren_Content".Translate(pawn.NameFullColored.Named("PAWN")).Resolve(),
                LetterDefOf.Cosmere_Roshar_ChooseRadiantOrder,
                new LookTargets(pawn)
            );
        }

        pawn.AllComps.Add(new ChooseRadiantOrder { parent = pawn });
        return true;
    }
}