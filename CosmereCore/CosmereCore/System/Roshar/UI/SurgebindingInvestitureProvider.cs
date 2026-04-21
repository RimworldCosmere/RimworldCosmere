using Cosmere.Core.UI.Model;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.UI;

public sealed class SurgebindingInvestitureProvider : IInvestitureProvider {
    public string SystemId => "Surgebinding";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        return pawn.genes.GetFirstGeneOfType<Surgebinder>() != null;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        Surgebinder? s = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (s == null) return null;

        ResourceBar bar = new ResourceBar(
            Label: "Stormlight",
            Current: s.Value,
            Max: s.Max,
            TargetValue: s.targetValue
        );

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Surgebinding",
            PrimaryBar: bar,
            Cells: [],
            Subsections: [],
            FlatAbilities: []
        );
    }
}
