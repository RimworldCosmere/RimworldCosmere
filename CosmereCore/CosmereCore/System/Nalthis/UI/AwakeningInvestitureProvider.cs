using Cosmere.Core.UI.Model;
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
}
