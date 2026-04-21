using Cosmere.Core.UI.Radial;
using Verse;

namespace Cosmere.Core.UI.Model;

public interface IInvestitureProvider {
    string SystemId { get; }
    bool IsInvested(Pawn pawn);
    InvestitureSnapshot? Snapshot(Pawn pawn);
    RadialSystem? SnapshotRadial(Pawn pawn);
}
