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
public class BodyAbsorption : Verse.Gene {
    private static Texture2D? icon;

    private static Texture2D Icon =>
        icon ??= ContentFinder<Texture2D>.Get("UI/Icons/Genes/Gene_BodyAbsorption", false)
                 ?? BaseContent.BadTex;

    private CompKandraForms? Forms => pawn.TryGetComp<CompKandraForms>();

    public override IEnumerable<Verse.Gizmo> GetGizmos() {
        // Gene.GetGizmos returns null rather than an empty sequence, so this cannot be foreached
        // directly. Doing so throws every frame the pawn is selected.
        IEnumerable<Verse.Gizmo>? inherited = base.GetGizmos();
        if (inherited != null) {
            foreach (Verse.Gizmo gizmo in inherited) yield return gizmo;
        }

        CompKandraForms? forms = Forms;
        if (forms == null) yield break;
        if (!pawn.IsColonistPlayerControlled) yield break;

        // Half-blessed and mistwraiths cannot hold a shape, so there is nothing to offer. The
        // buttons go rather than grey out: a mistwraith is not a colonist waiting on a cooldown.
        if (!Util.KandraUtility.CanHoldAShape(pawn)) yield break;

        yield return WearGizmo(forms);

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
