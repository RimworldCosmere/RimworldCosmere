using System;
using Cosmere.Core.Comp.Thing;
using Verse;

namespace Cosmere.System.Roshar.Comp.Fabrials;

public class ApparelFabrialDiminisher : ThingComp {
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
    }

    public override void Notify_Equipped(Pawn pawn) {
        base.Notify_Equipped(pawn);

        if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Cosmere_Roshar_Apparel_Painrial_Diminisher_Hediff)) {
            Verse.Hediff? hediff = HediffMaker.MakeHediff(
                HediffDefOf.Cosmere_Roshar_Apparel_Painrial_Diminisher_Hediff,
                pawn
            );
            pawn.health.AddHediff(hediff);
        }
    }

    public bool IsActive => powerOn;

    public override void CompTick() {
        UpdatePowerState();
    }

    public void InfuseStormlight(float amount) {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture != null) {
            investiture.currentInvestitureSelf = Math.Min(
                investiture.currentInvestitureSelf + amount,
                investiture.maxInvestitureSelf
            );
        }
    }

    private void UpdatePowerState() {
        InvestitureHolder? investiture = insertedGemstone?.TryGetComp<InvestitureHolder>();
        if (investiture == null) {
            powerOn = false;
            return;
        }

        powerOn = investiture.currentInvestiture > 0;
        if (powerOn) investiture.drainRate = 0.25f;
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        InvestitureHolder? investiture = insertedGemstone.TryGetComp<InvestitureHolder>();
        return "Stormlight: " + (investiture?.currentInvestiture.ToString("F0") ?? "0");
    }
}
