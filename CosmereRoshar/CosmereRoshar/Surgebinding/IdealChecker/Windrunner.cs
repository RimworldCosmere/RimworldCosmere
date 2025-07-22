using Cosmere.Roshar.Def;
using Cosmere.Roshar.Gene;
using Verse;

namespace Cosmere.Roshar.Surgebinding.IdealChecker;

public class Windrunner(RadiantOrderDef def) : AbstractIdealChecker(def) {
    public override bool IsSatisfied(Pawn pawn, Surgebinder surgebinder, int currentLevel, int nextLevel) {
        if (nextLevel == 1) {
            return pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_PatientsSaved) > 0;
        }

        if (nextLevel == 2) {
            return pawn.records.GetValue(RecordDefOf.Cosmere_Roshar_Record_EnemyPatientsSaved) > 0;
        }

        return true;
    }
}