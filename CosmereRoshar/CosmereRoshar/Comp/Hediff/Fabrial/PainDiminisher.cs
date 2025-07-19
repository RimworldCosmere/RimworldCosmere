using CosmereRoshar.Comp.Fabrials;
using CosmereRoshar.Comp.Thing;
using RimWorld;
using Verse;

namespace CosmereRoshar.Comp.Hediff.Fabrial;

public class PainDiminisher : HediffComp {
    public override string CompLabelInBracketsExtra {
        get {
            ApparelFabrialDiminisher? comp = Pawn?.apparel?.WornApparel?
                .FirstOrDefault(app => app.GetComp<ApparelFabrialDiminisher>() != null)
                ?
                .GetComp<ApparelFabrialDiminisher>();

            if (comp?.insertedGemstone?.TryGetComp<Stormlight>() is Stormlight stormlight) {
                return $"Stormlight: {stormlight.currentStormlight:F0}";
            }

            return null;
        }
    }

    public bool GetActive() {
        Pawn pawn = Pawn;
        if (pawn?.apparel?.WornApparel == null) {
            return false;
        }

        foreach (Apparel? apparel in pawn.apparel.WornApparel) {
            ApparelFabrialDiminisher? comp = apparel.GetComp<ApparelFabrialDiminisher>();
            if (comp == null) {
                continue;
            }

            return comp.CheckPower();
        }

        return false;
    }

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);

        Pawn pawn = Pawn;
        if (pawn?.apparel?.WornApparel == null) {
            return;
        }

        foreach (Apparel? apparel in pawn.apparel.WornApparel) {
            ApparelFabrialDiminisher? comp = apparel.GetComp<ApparelFabrialDiminisher>();
            if (comp == null) {
                continue;
            }

            if (!comp.CheckPower()) {
                Verse.Hediff? hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    CosmereRosharDefs.Cosmere_Roshar_ApparelPainrialDiminisherHediff
                );
                if (hediff != null) {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            return;
        }

        // If none of the worn Fabrials are powered, remove this Hediff
        if (!pawn.Dead) {
            pawn.health.RemoveHediff(parent);
        }
    }
}