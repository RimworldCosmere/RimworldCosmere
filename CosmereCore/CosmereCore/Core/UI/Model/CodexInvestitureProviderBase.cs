using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Radial;
using Verse;

namespace Cosmere.Core.UI.Model;

public abstract class CodexInvestitureProviderBase<TCodex> : IInvestitureProvider
    where TCodex : ICodexContentProvider, new() {
    public ICodexContentProvider Codex { get; } = new TCodex();

    public abstract string SystemId { get; }
    public abstract bool IsInvested(Pawn pawn);
    public abstract InvestitureSnapshot? Snapshot(Pawn pawn);
    public abstract RadialSystem? SnapshotRadial(Pawn pawn);
}
