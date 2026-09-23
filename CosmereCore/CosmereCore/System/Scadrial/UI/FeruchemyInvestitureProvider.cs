using Cosmere.Core.UI.Codex;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class FeruchemyInvestitureProvider : CodexInvestitureProviderBase<FeruchemyCodexContent> {
    public const string Id = "Feruchemy";

    public override string SystemId => Id;

    public override bool IsInvested(Pawn pawn) {
        if (pawn.genes == null) return false;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Feruchemist f && !f.Overridden) return true;
        }

        return false;
    }

    public override InvestitureSnapshot? Snapshot(Pawn pawn) {
        if (pawn.genes == null) return null;

        List<InvestitureCell> cells = [];
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            List<IMetalmindSource> mms = f.metalminds;
            float totalMax = 0f;
            float totalValue = 0f;
            for (int j = 0; j < mms.Count; j++) {
                totalMax += mms[j].MaxAmount;
                totalValue += mms[j].StoredAmount;
            }

            ResourceBar bar = new ResourceBar(
                f.metal.LabelCap,
                totalValue,
                totalMax,
                f.targetValue
            );

            // inverted copy, like Allomancy's - GUI.color multiplies, so raw black art renders black on black.
            cells.Add(
                new InvestitureCell(
                    f.metal.defName,
                    f.metal.LabelCap,
                    f.metal.feruchemy?.invertedIcon,
                    bar,
                    f.isTapping || f.isStoring,
                    f.isCompounding,
                    []
                )
            );
        }

        if (cells.Count == 0) return null;

        return new InvestitureSnapshot(
            SystemId,
            "Feruchemy",
            null,
            cells
        );
    }

    public override RadialSystem? SnapshotRadial(Pawn pawn) {
        // dock and codex only - tapping/storing are sustained states, not the quick casts radial exists for.
        return null;
    }
}
