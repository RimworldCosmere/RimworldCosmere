using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class ResearchWindowHeaderLabelPatch : MainTabWindow_Research {
    [Inject(At.Head, "HeaderLabel")]
    private Control BeforeHeaderLabel(
        ResearchPrerequisitesUtility.UnlockedHeader headerProject,
        ControlHandle<string> ch
    ) {
        ch.ReturnValue = string.Join(
            ", ",
            headerProject.unlockedBy.Select<ResearchProjectDef, object>(rp =>
                rp.IsFinished
                    ? rp.LabelCap
                    : rp.LabelCap.Colorize(ColorLibrary.RedReadable)
            )
        );

        return Control.Cancel;
    }
}
