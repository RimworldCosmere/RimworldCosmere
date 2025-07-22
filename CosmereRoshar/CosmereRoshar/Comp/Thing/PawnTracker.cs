using Verse;

namespace Cosmere.Roshar.Comp.Thing;

public class PawnTracker : ThingComp {
    public float bondChance;
    public int doCheckWhenThisIsZero;
    public List<Pawn> patientList = [];

    public override void PostExposeData() {
        Scribe_Collections.Look(ref patientList, "PatientList", LookMode.Reference);
        Scribe_Values.Look(ref bondChance, "bondChance");
    }

    public void OnPatientSaved(bool enemy = false) {
        if (parent is not Pawn pawn) return;

        pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PatientsSaved, 1);
        if (enemy) {
            pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_EnemyPatientsSaved, 1);
        }
    }

    public void OnPatientLost() {
        if (parent is not Pawn pawn) return;

        pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_PatientsDied, 1);
    }

    public void OnNahelBondFormed() {
        if (parent is not Pawn pawn) return;

        pawn.records.AddTo(RecordDefOf.Cosmere_Roshar_Record_BondsFormed, 1);
    }
}