using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Makes the formless body cost something, and only while the kandra is actually in it.
/// </summary>
/// <remarks>
///     A flat social penalty on the gene would be backwards. The whole point of a disguise is
///     that it works - a kandra wearing a colonist's face should read as that colonist. The
///     penalty belongs to the wet grey shape underneath, so it comes and goes with the disguise.
/// </remarks>
public class TrueBody : Verse.Gene {
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        // Backstop. CompKandraForms.SetCurrent does the flip the moment a shape changes.
        if (pawn.IsHashIntervalTick(GenTicks.TickRareInterval, delta)) KandraAppearance.SyncFormless(pawn);
    }

    public override void PostRemove() {
        base.PostRemove();

        Hediff? showing = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_Formless
        );
        if (showing == null) return;

        pawn.health?.RemoveHediff(showing);
        pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }
}
