using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyInvestitureProvider : IInvestitureProvider {
    public string SystemId => "Feruchemy";

    public bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && !f.Overridden) return true;
        }
        return false;
    }

    public InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].maxAmount;
                totalValue += mms[j].storedAmount;
            }

            ResourceBar bar = new ResourceBar(
                Label: f.metal.LabelCap,
                Current: totalValue,
                Max: totalMax,
                TargetValue: f.targetValue
            );

            cells.Add(new InvestitureCell(
                SubsystemId: f.metal.defName,
                Label: f.metal.LabelCap,
                Icon: f.metal.feruchemy?.icon,
                Bar: bar,
                IsActive: f.isTapping || f.isStoring,
                IsFlaring: f.isCompounding
            ));
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId: SystemId,
            SystemLabel: "Feruchemy",
            PrimaryBar: null,
            Cells: cells,
            Subsections: [],
            FlatAbilities: []
        );
    }

    public RadialSystem? SnapshotRadial(Pawn pawn) {
        return null;
    }
}
