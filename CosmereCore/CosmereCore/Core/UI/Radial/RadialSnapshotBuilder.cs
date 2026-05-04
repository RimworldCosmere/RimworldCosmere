using Cosmere.Core.UI.Model;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialSnapshotBuilder {
    public static RadialSnapshot? Build(Pawn pawn) {
        List<RadialSystem> systems = [];
        IReadOnlyList<IInvestitureProvider> providers = InvestitureProviderRegistry.All;
        for (int i = 0; i < providers.Count; i++) {
            if (!providers[i].IsInvested(pawn)) continue;
            RadialSystem? sys = providers[i].SnapshotRadial(pawn);
            if (sys == null) continue;
            if (sys.Subsections.Count == 0) continue;
            systems.Add(sys);
        }

        if (systems.Count == 0) return null;

        return new RadialSnapshot(pawn, systems, Find.TickManager.TicksGame);
    }
}