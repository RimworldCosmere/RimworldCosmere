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
            systems.Add(CollapseSingleLeafSubsections(sys));
        }

        if (systems.Count == 0) return null;

        return new RadialSnapshot(pawn, systems, Find.TickManager.TicksGame);
    }

    private static RadialSystem CollapseSingleLeafSubsections(RadialSystem system) {
        bool anyCollapsible = false;
        for (int i = 0; i < system.Subsections.Count; i++) {
            if (system.Subsections[i].Leaves.Count == 1) {
                anyCollapsible = true;
                break;
            }
        }

        if (!anyCollapsible) return system;

        List<RadialSubsection> collapsed = new List<RadialSubsection>(system.Subsections.Count);
        for (int i = 0; i < system.Subsections.Count; i++) {
            RadialSubsection subsection = system.Subsections[i];
            if (subsection.Leaves.Count != 1) {
                collapsed.Add(subsection);
                continue;
            }

            RadialLeaf leaf = subsection.Leaves[0];
            collapsed.Add(new RadialSubsection(
                subsection.SubsectionId,
                leaf.Label,
                leaf.Icon ?? subsection.Icon,
                subsection.AccentColor,
                subsection.Leaves
            ));
        }

        return new RadialSystem(system.SystemId, system.Label, system.Icon, collapsed);
    }
}