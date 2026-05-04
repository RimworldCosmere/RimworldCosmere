using System;
using Verse;

namespace Cosmere.Core.Savant;

public static class SavantCandidateRegistry {
    private static readonly List<ISavantCandidateProvider> providers = [];

    public static void Register(ISavantCandidateProvider provider) {
        providers.Add(provider);
    }

    public static bool ApplyRandomSavant(Pawn pawn) {
        List<Action> candidates = [];
        for (int i = 0; i < providers.Count; i++) {
            providers[i].CollectCandidates(pawn, candidates);
        }

        if (candidates.Count == 0) return false;
        candidates[Rand.Range(0, candidates.Count)]();
        return true;
    }
}
