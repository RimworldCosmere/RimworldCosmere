using Verse;

namespace Cosmere.System.Scadrial.Feruchemy;

public interface IConnectionLedger {
    DuraluminLedger Ledger { get; }

    float CurrentPoints(Pawn pawn);

    float HeadroomPoints(Pawn pawn);

    // Signed: negative takes Connection off the pawn, positive gives it back.
    // Returns what actually moved, not what was asked.
    float Move(Pawn pawn, float points);
}
