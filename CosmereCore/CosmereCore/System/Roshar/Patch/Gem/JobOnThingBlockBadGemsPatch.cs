using Concord;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Gem;

[Patch]
public abstract class JobOnThingBlockBadGemsPatch : WorkGiver_DoBill {
    [Inject(At.Head, nameof(JobOnThing))]
    private Control BeforeJobOnThing(Pawn pawn, Verse.Thing thing, bool forced, ControlHandle<Verse.AI.Job> ch) {
        IBillGiver? billGiver = thing as IBillGiver;
        if (billGiver == null || !billGiver.BillStack.AnyShouldDoNow) {
            return Control.Continue;
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
                ch.ReturnValue = null!;
                return Control.Cancel;
            }
        }

        return Control.Continue;
    }
}
