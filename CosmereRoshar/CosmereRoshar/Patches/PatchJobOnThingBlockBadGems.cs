using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Patches;

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
            if (bill.recipe.defName != "Cosmere_Roshar_Make_Apparel_Fabrial_Painrial_Diminisher") {
                continue;
            }

            bool hasValidGem = false;

            foreach (Verse.Thing gem in
                     pawn.Map.listerThings.ThingsOfDef(ThingDef.Named("Cosmere_Roshar_CutEmerald"))) {
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