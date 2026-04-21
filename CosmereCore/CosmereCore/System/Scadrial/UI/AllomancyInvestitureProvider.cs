using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyInvestitureProvider : IInvestitureProvider {
    public string SystemId => "Allomancy";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) return true;
        }
        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Allomancer a || a.Overridden) continue;

            ResourceBar bar = new ResourceBar(
                Label: a.metal.LabelCap,
                Current: a.Value,
                Max: a.Max,
                TargetValue: a.targetValue
            );

            cells.Add(new InvestitureCell(
                SubsystemId: a.metal.defName,
                Label: a.metal.LabelCap,
                Icon: a.metal.allomancy?.invertedIcon,
                Bar: bar,
                IsActive: a.Burning,
                IsFlaring: false
            ));
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Allomancy",
            PrimaryBar: null,
            Cells: cells,
            Subsections: [],
            FlatAbilities: []
        );
    }
}
