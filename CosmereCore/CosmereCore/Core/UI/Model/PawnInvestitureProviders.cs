using Verse;

namespace Cosmere.Core.UI.Model;

public static class PawnInvestitureProviders {
    private static readonly List<IInvestitureProvider> providers = [];

    public static IReadOnlyList<IInvestitureProvider> All => providers;

    public static void Register(IInvestitureProvider provider) {
        for (int i = 0; i < providers.Count; i++) {
            if (providers[i].SystemId == provider.SystemId) return;
        }
        providers.Add(provider);
    }

    public static bool HasAnyInvestment(Pawn pawn) {
        for (int i = 0; i < providers.Count; i++) {
            if (providers[i].IsInvested(pawn)) return true;
        }
        return false;
    }

    public static List<InvestitureSnapshot> SnapshotsFor(Pawn pawn) {
        List<InvestitureSnapshot> result = [];
        for (int i = 0; i < providers.Count; i++) {
            if (!providers[i].IsInvested(pawn)) continue;
            InvestitureSnapshot? snap = providers[i].Snapshot(pawn);
            if (snap != null) result.Add(snap);
        }
        return result;
    }
}
