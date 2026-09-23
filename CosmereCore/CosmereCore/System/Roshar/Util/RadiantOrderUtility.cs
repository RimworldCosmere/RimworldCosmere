using Cosmere.Core.Framework;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Verse;

namespace Cosmere.System.Roshar.Util;

public enum SprenBondResult {
    /// <summary>The pawn was offered an order and now carries a ChooseRadiantOrder comp.</summary>
    Offered,
    AlreadySurgebinder,
    InvestitureBlocked,
    BondOnCooldown,
}

public static class RadiantOrderUtility {
    public static bool BondWithSpren(Pawn pawn, bool showLetter = true) {
        return TryBondWithSpren(pawn, showLetter) == SprenBondResult.Offered;
    }

    /// <summary>
    ///     Offers the pawn a Nahel bond. This does NOT make them a Surgebinder on its own - it attaches
    ///     the order-choice comp and sends the letter, and the order is only granted once that choice is
    ///     made. Returns why it declined so callers can say so rather than failing silently.
    /// </summary>
    public static SprenBondResult TryBondWithSpren(Pawn pawn, bool showLetter = true) {
        if (pawn.IsSurgebinder()) return SprenBondResult.AlreadySurgebinder;

        if (InvestitureBlockingHediffRegistry.IsBlocked(pawn)) return SprenBondResult.InvestitureBlocked;

        RadiantTracker? tracker = Current.Game?.GetComponent<RadiantTracker>();
        if (tracker != null && !tracker.HasAnyAvailableOrder(pawn)) return SprenBondResult.BondOnCooldown;

        if (showLetter) {
            Find.LetterStack.ReceiveLetter(
                "CRO_Bond_Spren_Title".Translate(pawn.LabelShortCap.Named("PAWN")),
                "CRO_Bond_Spren_Content".Translate(pawn.NameFullColored.Named("PAWN")).Resolve(),
                LetterDefOf.Cosmere_Roshar_ChooseRadiantOrder,
                new LookTargets(pawn)
            );
        }

        pawn.AllComps.Add(new ChooseRadiantOrder { parent = pawn });
        return SprenBondResult.Offered;
    }
}
