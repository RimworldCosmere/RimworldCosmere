using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Gem;

[HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
public static class JobOnThingBlockBadGemsPatch {
    private static bool Prefix(
        ref Verse.AI.Job __result,
        Pawn pawn,
        Verse.Thing thing
    ) {
        IBillGiver? billGiver = thing as IBillGiver;
        if (billGiver == null || !billGiver.BillStack.AnyShouldDoNow) {
            return true;
        }

        foreach (Bill bill in billGiver.BillStack) {
            if (bill.recipe.defName != "Cosmere_Roshar_Make_Apparel_Fabrial_Painrial_Diminisher") {
                continue;
            }

            bool hasValidGem = false;

            foreach (Verse.Thing gem in
                     pawn.Map.listerThings.ThingsOfDef(Core.ThingDefOf.RawEmerald)) {
                if (!pawn.CanReserveAndReach(gem, PathEndMode.ClosestTouch, Danger.Deadly)) {
                    continue;
                }

                hasValidGem = true;
                break;
            }

            if (!hasValidGem) {
                __result = null!;
                return false;
            }
        }

        return true;
    }
}
