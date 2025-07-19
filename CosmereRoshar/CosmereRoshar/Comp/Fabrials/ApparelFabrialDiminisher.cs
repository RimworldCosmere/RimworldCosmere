using CosmereRoshar.Comp.Thing;
using Verse;

namespace CosmereRoshar.Comp.Fabrials;

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

        if (!pawn.health.hediffSet.HasHediff(CosmereRosharDefs.Cosmere_Roshar_ApparelPainrialDiminisherHediff)) {
            Verse.Hediff? hediff = HediffMaker.MakeHediff(
                CosmereRosharDefs.Cosmere_Roshar_ApparelPainrialDiminisherHediff,
                pawn
            );
            pawn.health.AddHediff(hediff);
        }
    }

    public override void CompTick() {
        CheckPower();
    }

    public void InfuseStormlight(float amount) {
        insertedGemstone?.TryGetComp<Stormlight>()?.InfuseStormlight(amount);
    }

    public bool CheckPower() {
        if (insertedGemstone == null || !insertedGemstone.TryGetComp(out Stormlight stormlight)) {
            return powerOn = false;
        }

        powerOn = stormlight.hasStormlight;
        if (!powerOn) return powerOn;

        stormlight.drainFactor = 0.25f;
        stormlight.CompTick();

        return powerOn;
    }

    public override string CompInspectStringExtra() {
        if (insertedGemstone == null) return "No gem in fabrial.";

        ThingWithComps gemstone = insertedGemstone;
        return "Spren: " +
               gemstone.GetComp<CompCutGemstone>().capturedSpren +
               "\nStormlight: " +
               gemstone.GetComp<Stormlight>().currentStormlight.ToString("F0");
    }
}