using Verse;

namespace Cosmere.System.Roshar.Hediff;

public class NightwatcherPassiveHediff : HediffWithComps {
    public override string Description {
        get {
            string intro = def.isBad
                ? "The Nightwatcher has laid a curse upon {PAWN_nameDef}. It cannot be removed by any ordinary means."
                : "The Nightwatcher has granted {PAWN_nameDef} a boon. The gift is permanent, woven into {PAWN_possessive} very Spiritweb.";
            return (intro + "\n\n" + def.description).Formatted(pawn.Named("PAWN"));
        }
    }
}