using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Hediff;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy;

// Compounding spends the metalmind along with its charge, so anything drained to
// nothing has to be cleared out rather than left as an empty husk.
public static class MetalmindBurnout {
    public static void Sweep(Pawn pawn) {
        SweepImplants(pawn);
        SweepCarried(pawn);
    }

    private static void SweepImplants(Pawn pawn) {
        if (pawn.health?.hediffSet == null) return;

        ImplantedMetalminds? hediff =
            pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ImplantedMetalminds) as
                ImplantedMetalminds;
        if (hediff == null) return;

        for (int i = hediff.metalminds.Count - 1; i >= 0; i--) {
            ImplantedMetalmindData data = hediff.metalminds[i];
            if (!data.IsBurnedOut) continue;

            hediff.RemoveMetalmindAt(i);
            Announce(pawn, data.Metal?.label ?? data.metalDefName);
        }
    }

    private static void SweepCarried(Pawn pawn) {
        if (pawn.inventory == null) return;

        List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
        for (int i = items.Count - 1; i >= 0; i--) {
            Metalmind? comp = items[i].TryGetComp<Metalmind>();
            if (comp == null || !comp.IsBurnedOut) continue;

            string label = comp.Metal?.label ?? items[i].LabelNoCount;
            items[i].Destroy();
            Announce(pawn, label);
        }
    }

    private static void Announce(Pawn pawn, string metalLabel) {
        Messages.Message(
            "CS_Feruchemy_MetalmindConsumed".Translate(pawn.Named("PAWN"), metalLabel.Named("METAL")),
            pawn,
            MessageTypeDefOf.NegativeEvent
        );
    }
}
