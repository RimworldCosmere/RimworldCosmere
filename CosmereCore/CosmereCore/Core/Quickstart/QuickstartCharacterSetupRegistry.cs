using System;
using Verse;

namespace Cosmere.Core.Quickstart;

public static class QuickstartCharacterSetupRegistry {
    private static readonly Dictionary<int, List<Action<Pawn>>> setups = [];

    public static void Register(int pawnIndex, Action<Pawn> setup) {
        if (!setups.TryGetValue(pawnIndex, out List<Action<Pawn>>? list)) {
            list = [];
            setups[pawnIndex] = list;
        }

        list.Add(setup);
    }

    public static void Apply(int pawnIndex, Pawn pawn) {
        if (!setups.TryGetValue(pawnIndex, out List<Action<Pawn>>? list)) return;
        for (int i = 0; i < list.Count; i++) list[i](pawn);
    }
}
