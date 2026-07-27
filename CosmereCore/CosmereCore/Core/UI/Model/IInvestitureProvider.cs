using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Radial;
using Verse;

namespace Cosmere.Core.UI.Model;

public interface IInvestitureProvider {
    string SystemId { get; }

    ICodexContentProvider Codex { get; }

    bool IsInvested(Pawn pawn);

    InvestitureSnapshot? Snapshot(Pawn pawn);

    RadialSystem? SnapshotRadial(Pawn pawn);
}
