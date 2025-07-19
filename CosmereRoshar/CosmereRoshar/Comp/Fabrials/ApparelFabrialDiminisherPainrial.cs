using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Comp.Fabrials;

public class FabrialDiminisher : ThingComp {
    public ThingWithComps? insertedGemstone;
    public bool powerOn;

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref insertedGemstone, "insertedGemstone");
        Scribe_Values.Look(ref powerOn, "PowerOn");
    }

    public override void Notify_Equipped(Pawn pawn) {
        base.Notify_Equipped(pawn);

        if (!pawn.health.hediffSet.HasHediff(CosmereRosharDefs.WhtwlApparelPainrialDiminisherHediff)) {
            Hediff? hediff = HediffMaker.MakeHediff(
                CosmereRosharDefs.WhtwlApparelPainrialDiminisherHediff,
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

// Todo Move to an Hediff namespace
public class HediffCompFabrialPainDiminisher : HediffComp {
    public override string CompLabelInBracketsExtra {
        get {
            FabrialDiminisher? comp = Pawn?.apparel?.WornApparel?
                .FirstOrDefault(app => app.GetComp<FabrialDiminisher>() != null)
                ?
                .GetComp<FabrialDiminisher>();

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

        foreach (RimWorld.Apparel? apparel in pawn.apparel.WornApparel) {
            FabrialDiminisher? comp = apparel.GetComp<FabrialDiminisher>();
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

        foreach (RimWorld.Apparel? apparel in pawn.apparel.WornApparel) {
            FabrialDiminisher? comp = apparel.GetComp<FabrialDiminisher>();
            if (comp == null) {
                continue;
            }

            if (!comp.CheckPower()) {
                Hediff? hediff = pawn.health.hediffSet.GetFirstHediffOfDef(
                    CosmereRosharDefs.WhtwlApparelPainrialDiminisherHediff
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

// Move to the patch namespace
[HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
public static class PatchJobOnThingBlockBadGems {
    private static bool Prefix(
        ref Verse.AI.Job result,
        Pawn pawn,
        Verse.Thing thing,
        bool forced,
        WorkGiver_DoBill instance
    ) {
        IBillGiver billGiver = thing as IBillGiver;
        if (billGiver == null || !billGiver.BillStack.AnyShouldDoNow) {
            return true;
        }

        foreach (Bill bill in billGiver.BillStack) {
            if (bill.recipe.defName != "whtwl_Make_Apparel_Fabrial_Painrial_Diminisher") {
                continue;
            }

            bool hasValidGem = false;

            foreach (Verse.Thing gem in pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("whtwl_CutEmerald"))) {
                if (!pawn.CanReserveAndReach(gem, PathEndMode.ClosestTouch, Danger.Deadly)) {
                    continue;
                }

                CompCutGemstone? comp = gem.TryGetComp<CompCutGemstone>();
                if (comp != null && comp.capturedSpren == Spren.Pain) {
                    hasValidGem = true;
                    break;
                }
            }

            if (!hasValidGem) {
                result = null;
                return false;
            }
        }

        return true;
    }
}