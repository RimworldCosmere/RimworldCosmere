using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Verse;

namespace Cosmere.System.Roshar.Utility;

public static class RadiantOrder {
    public static bool BondWithSpren(Pawn pawn, bool showLetter = true) {
        if (pawn.IsSurgebinder()) return false;

        HediffDef? drabHediff = DefDatabase<HediffDef>.GetNamedSilentFail("Cosmere_Scadrial_Hediff_Drab");
        if (drabHediff != null && pawn.health.hediffSet.HasHediff(drabHediff)) return false;

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