using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.FloatMenuOptionProvider;

/// <summary>
///     Right-click a corpse as a kandra: take its bones.
/// </summary>
public class KandraBonesMenuProvider : RimWorld.FloatMenuOptionProvider {
    // A drafted kandra is exactly the one standing over a fresh body.
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => false;

    protected override FloatMenuOption? GetSingleOptionFor(Verse.Thing clickedThing, FloatMenuContext context) {
        if (clickedThing is not Corpse corpse) return null;

        // Animals count, but only the ones a kandra can actually wear. Eating a mechanoid or a
        // thrumbo teaches nothing it could put on afterwards.
        if (!corpse.InnerPawn.RaceProps.Humanlike
            && !Kandra.KandraShapeEligibility.Wearable(corpse.InnerPawn.kindDef)) {
            return null;
        }

        Pawn? pawn = context.FirstSelectedPawn;
        if (pawn?.TryGetComp<CompKandraForms>() == null) return null;

        string label = "CS_Kandra_ConsumeBones".Translate(corpse.InnerPawn.LabelShortCap.Named("FORM"));

        if (!pawn.CanReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly)) {
            return new FloatMenuOption(label + ": " + "NoPath".Translate().CapitalizeFirst(), null);
        }

        return FloatMenuUtility.DecoratePrioritizedTask(
            new FloatMenuOption(
                label,
                () => {
                    Verse.AI.Job job = JobMaker.MakeJob(
                        JobDefOf.Cosmere_Scadrial_Job_KandraConsumeBones,
                        corpse
                    );
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            ),
            pawn,
            (LocalTargetInfo)corpse
        );
    }
}
