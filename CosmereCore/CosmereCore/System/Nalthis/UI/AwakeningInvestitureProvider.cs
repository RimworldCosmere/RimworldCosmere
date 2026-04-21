using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using RimWorld;
using Verse;

namespace Cosmere.System.Nalthis.UI;

public sealed class AwakeningInvestitureProvider : IInvestitureProvider {
    public string SystemId => "Awakening";

    public bool IsInvested(Pawn pawn) {
        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        return null;
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        return null;
    }
}
