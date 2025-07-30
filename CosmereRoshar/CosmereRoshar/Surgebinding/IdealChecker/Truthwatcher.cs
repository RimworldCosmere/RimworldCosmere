using System;
using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.IdealChecker;

public class Truthwatcher(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        if (nextLevel == 1) {
            return pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PatientsSaved) > 0;
        }

        return true;
    }

    public override bool Satisfy(Pawn pawn, Surgebinder surgebinder, int nextLevel) {
        throw new NotImplementedException();
    }
}