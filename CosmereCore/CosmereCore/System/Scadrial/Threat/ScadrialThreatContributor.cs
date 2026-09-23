using Cosmere.Core.Threat;
using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Hediff;
using Verse;

namespace Cosmere.System.Scadrial.Threat;

/// <summary>
///     Reads a pawn's metals, spikes and kit, and hands the numbers to ScadrialThreat.
/// </summary>
/// <remarks>
///     The rule this replaced bailed on a null trait tracker, which dropped a gene-only pawn's
///     spikes along with everything else. Each source is read on its own here.
/// </remarks>
public class ScadrialThreatContributor : IThreatContributor {
    public string SystemId => "Scadrial";

    public float GainForPawn(Pawn pawn) {
        bool mistborn = pawn.IsMistborn();
        bool fullFeruchemist = pawn.IsFullFeruchemist();

        int allomantic = pawn.genes?.GetAllomanticGenes().Count ?? 0;
        int feruchemic = pawn.genes?.GetFeruchemicGenes().Count ?? 0;

        float worth = ScadrialThreat.ForMetalborn(mistborn, fullFeruchemist, allomantic, feruchemic);
        if (worth <= 1f) return 0f;

        worth += ScadrialThreat.ForSpikes(SpikeCount(pawn));
        worth += ScadrialThreat.ForGear(ChargedMetalminds(pawn), Vials(pawn));

        return worth - 1f;
    }

    private static int SpikeCount(Pawn pawn) {
        HemalurgicSpikes? spikes = (HemalurgicSpikes?)pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );

        return spikes?.spikeCount ?? 0;
    }

    /// <summary>
    ///     Only metalminds this pawn can actually tap. A keyed metalmind in someone else's pack is
    ///     dead weight, not a weapon.
    /// </summary>
    private static int ChargedMetalminds(Pawn pawn) {
        int count = 0;
        count += CountCharged(pawn, pawn.inventory?.innerContainer);
        count += CountCharged(pawn, pawn.apparel?.WornApparel);
        count += CountCharged(pawn, pawn.equipment?.AllEquipmentListForReading);

        return count;
    }

    private static int CountCharged<T>(Pawn pawn, IEnumerable<T>? things)
        where T : Verse.Thing {
        if (things == null) return 0;

        int count = 0;
        foreach (T thing in things) {
            if (thing is not ThingWithComps withComps) continue;

            Metalmind? metalmind = withComps.GetComp<Metalmind>();
            if (metalmind == null || metalmind.owner != pawn) continue;
            if (metalmind.TotalStored > 0f) count++;
        }

        return count;
    }

    private static int Vials(Pawn pawn) {
        ThingOwner? inventory = pawn.inventory?.innerContainer;
        if (inventory == null) return 0;

        int count = 0;
        for (int i = 0; i < inventory.Count; i++) {
            if (inventory[i].def == ThingDefOf.Cosmere_Scadrial_Thing_AllomanticVial) count += inventory[i].stackCount;
        }

        return count;
    }
}
