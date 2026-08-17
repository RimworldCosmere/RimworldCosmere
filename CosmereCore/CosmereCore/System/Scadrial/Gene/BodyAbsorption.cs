using System.Collections.Generic;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Lets a kandra wear any body it has eaten.
/// </summary>
/// <remarks>
///     The repertoire lives on <see cref="CompKandraForms" /> rather than here, because it has
///     to survive the pawn losing this gene. A mistwraith that ate somebody still has the bones.
/// </remarks>
[StaticConstructorOnStartup]
public class BodyAbsorption : Shapeshifter {
    // Textures have to be pulled on the main thread at startup, never from a gizmo draw.
    private static readonly Texture2D Icon =
        ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/Kandra_BodyAbsorption", false) ?? BaseContent.BadTex;

    private CompKandraForms? Forms => pawn.TryGetComp<CompKandraForms>();

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        // base.GetGizmos can return null; foreaching it directly throws every frame while selected.
        IEnumerable<Verse.Gizmo>? inherited = base.GetGizmos();
        if (inherited != null) {
            foreach (Verse.Gizmo gizmo in inherited) yield return gizmo;
        }

        CompKandraForms? forms = Forms;
        if (forms == null) yield break;

        // not IsColonistPlayerControlled: that also requires no MentalStateDef, blocking revert mid-break.
        if (!pawn.IsColonist || pawn.Downed) yield break;

        // buttons vanish rather than grey out: a mistwraith isn't a colonist on cooldown, it just cant.
        if (!Util.KandraUtility.CanHoldAShape(pawn)) yield break;

        yield return WearGizmo(forms);

        yield return FreeFormGizmo(forms);

        if (forms.IsWearingSomeoneElse) yield return RevertGizmo();
    }

    private Command_Action WearGizmo(CompKandraForms forms) {
        Command_Action wear = new Command_Action {
            defaultLabel = "CS_Kandra_TakeForm".Translate(),
            defaultDesc = "CS_Kandra_TakeFormDesc".Translate(forms.Known.Count.Named("COUNT")),
            icon = Icon,
            action = () => Find.WindowStack.Add(new Dialog_KandraForms(forms, StartChange)),
        };

        if (forms.Known.Count == 0) wear.Disable("CS_Kandra_NoFormsYet".Translate());

        return wear;
    }

    /// <summary>
    ///     Shaping a body nobody has eaten, for a kandra practised enough to invent one.
    /// </summary>
    /// <remarks>
    ///     CanFreeForm has existed unused since the comp was written. This is what it was for:
    ///     below the skill threshold a kandra can only reproduce what it has taken bones from,
    ///     and above it the shape no longer needs a template.
    /// </remarks>
    private Command_Action FreeFormGizmo(CompKandraForms forms) {
        Command_Action free = new Command_Action {
            defaultLabel = "CS_Kandra_FreeForm".Translate(),
            defaultDesc = "CS_Kandra_FreeFormDesc".Translate(),
            icon = Icon,
            action = () => {
                KandraShapeshift.WearInvented(pawn);
                forms.Mind.Store(pawn);
            },
        };

        if (!forms.CanFreeForm) {
            free.Disable("CS_Kandra_FreeFormLocked".Translate(forms.Props.freeFormSkill.Named("LEVEL")));
        }

        return free;
    }

    private Command_Action RevertGizmo() {
        return new Command_Action {
            defaultLabel = "CS_Kandra_Revert".Translate(),
            defaultDesc = "CS_Kandra_RevertDesc".Translate(),
            icon = Icon,
            action = () => StartChange(JobDriver.KandraChangeShape.RevertIndex),
        };
    }

    private void StartChange(int index) {
        Verse.AI.Job job = JobMaker.MakeJob(JobDefOf.Cosmere_Scadrial_Job_KandraChangeShape);
        job.count = index;
        pawn.jobs?.TryTakeOrderedJob(job);
    }
}
