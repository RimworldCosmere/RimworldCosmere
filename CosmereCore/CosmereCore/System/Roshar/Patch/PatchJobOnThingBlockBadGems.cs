using Cosmere;
using Cosmere.System.Roshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
public static class PatchJobOnThingBlockBadGems {
    private static bool Prefix(
        ref Verse.AI.Job __result,
        Pawn pawn,
        Verse.Thing thing
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
                if (comp != null) {
                    // TODO: Fix spren type comparison when new spren system is ready
                    hasValidGem = true;
                    break;
                }
            }

            if (!hasValidGem) {
                __result = null;
                return false;
            }
        }

        return true;
    }
}