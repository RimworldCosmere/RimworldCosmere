using System;
using Verse;

namespace Cosmere.Core.UI.Model;

public static class InvestitureProviderRegistry {
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

    public static bool IsInvestedIn(Pawn pawn, string systemId) {
        for (int i = 0; i < providers.Count; i++) {
            if (providers[i].SystemId == systemId) return providers[i].IsInvested(pawn);
        }

        return false;
    }

    public static IReadOnlyList<InvestitureSnapshot> SnapshotsFor(Pawn pawn) {
        List<InvestitureSnapshot> result = [];
        for (int i = 0; i < providers.Count; i++) {
            try {
                if (!providers[i].IsInvested(pawn)) continue;
                InvestitureSnapshot? snap = providers[i].Snapshot(pawn);
                if (snap != null) result.Add(snap);
            } catch (Exception ex) {
                Log.Warn($"InvestitureProviderRegistry: provider {providers[i].GetType().Name} threw: {ex}");
            }
        }

        return result;
    }
}
