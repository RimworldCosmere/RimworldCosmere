using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningInvestitureProvider : CodexInvestitureProviderBase<AwakeningCodexContent> {
    public override string SystemId => "Awakening";

    public override bool IsInvested(Pawn pawn) {
        return false;
    }

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        return null;
    }

    public override RadialSystem? SnapshotRadial(Pawn pawn) {
        return null;
    }
}
