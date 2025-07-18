using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps.Gems;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Comps.Fabrials;

public class CompApparelFabrialDiminisher : ThingComp {
    public Thing insertedGemstone;
    public bool powerOn;
    public CompPropertiesApparelFabrialDiminisher props => (CompPropertiesApparelFabrialDiminisher)base.props;
    public bool hasGemstone => insertedGemstone != null;

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

    public override void PostSpawnSetup(bool respawningAfterLoad) {
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public override void CompTick() {
        CheckPower();
    }

    public void InfuseStormlight(float amount) {
        if (hasGemstone) {
            Stormlight? stormlightComp = (insertedGemstone as ThingWithComps).GetComp<Stormlight>();
            stormlightComp.InfuseStormlight(amount);
        }
    }

    public bool CheckPower() {
        if (insertedGemstone != null) {
            Stormlight? stormlightComp = (insertedGemstone as ThingWithComps).GetComp<Stormlight>();
            if (stormlightComp != null) {
                powerOn = stormlightComp.hasStormlight;
                if (powerOn) {
                    stormlightComp.drainFactor = 0.25f;
                    stormlightComp.CompTick();
                }
            } else {
                powerOn = false;
            }
        } else {
            powerOn = false;
        }

        return powerOn;
    }

    public override string CompInspectStringExtra() {
        string gemName = "No gem in fabrial.";

        if (insertedGemstone != null) {
            ThingWithComps gemstone = insertedGemstone as ThingWithComps;
            gemName = "Spren: " +
                      gemstone.GetComp<CompCutGemstone>().capturedSpren +
                      "\nStormlight: " +
                      gemstone.GetComp<Stormlight>().currentStormlight.ToString("F0");
        }

        return gemName;
    }
}

public class CompPropertiesApparelFabrialDiminisher : CompProperties {
    public CompPropertiesApparelFabrialDiminisher() {
        compClass = typeof(CompApparelFabrialDiminisher);
    }
}


public class HediffCompFabrialPainDiminisher : HediffComp {
    public override string CompLabelInBracketsExtra {
        get {
            CompApparelFabrialDiminisher? comp = Pawn?.apparel?.WornApparel?
                .FirstOrDefault(app => app.GetComp<CompApparelFabrialDiminisher>() != null)
                ?
                .GetComp<CompApparelFabrialDiminisher>();

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
            CompApparelFabrialDiminisher? comp = apparel.GetComp<CompApparelFabrialDiminisher>();
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
            CompApparelFabrialDiminisher? comp = apparel.GetComp<CompApparelFabrialDiminisher>();
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

public class HediffCompPropertiesFabrialPainDiminisher : HediffCompProperties {
    public HediffCompPropertiesFabrialPainDiminisher() {
        compClass = typeof(HediffCompFabrialPainDiminisher);
    }
}


[HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
public static class PatchJobOnThingBlockBadGems {
    private static bool Prefix(
        ref Job result,
        Pawn pawn,
        Thing thing,
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

            foreach (Thing gem in pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("whtwl_CutEmerald"))) {
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