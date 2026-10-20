using Concord;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.World;

[Patch(typeof(RimWorld.StatsReportUtility))]
public static class StatsReportUtilityPatch {
    [Inject(At.Head, "DescriptionEntry", parameterTypes: [typeof(Verse.Def)])]
    private static Control BeforeDescriptionEntry(Verse.Def def, ControlHandle<StatDrawEntry> ch) {
        if (def is not GeneDef geneDef) return Control.Continue;

        MetalsLinked? extension = geneDef.GetModExtension<MetalsLinked>();
        if (extension == null) return Control.Continue;
        MetalDef? metal = extension.Metals?.FirstOrDefault();
        if (metal == null) return Control.Continue;
        TaggedString description =
            geneDef.description.Formatted("the current pawn".Named("PAWN"), metal.Named("METAL"));
        ch.ReturnValue = new StatDrawEntry(
            StatCategoryDefOf.BasicsImportant,
            (string)"Description".Translate(),
            string.Empty,
            description,
            99999,
            hyperlinks: Dialog_InfoCard.DefsToHyperlinks(def.descriptionHyperlinks)
        );

        return Control.Cancel;
    }
}
