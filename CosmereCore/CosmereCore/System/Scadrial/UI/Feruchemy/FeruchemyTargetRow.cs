using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI.Feruchemy;

public static class FeruchemyTargetRow {
    // Which metalmind the dials act on. A pawn wearing a band and carrying three
    // implants needs to say which one they mean before compounding makes sense,
    // since burning one destroys it.
    public static float Draw(Rect inner, float y, Feruchemist gene) {
        return DockDropdownRow.Draw(
            new Rect(inner.x, y, inner.width, DockDropdownRow.Height),
            "CC_Dock_Feruchemy_TargetLabel".Translate(TargetLabel(gene).Named("TARGET")),
            "CC_Dock_Feruchemy_TargetTip".Translate(TargetUnits(gene).Named("UNITS")),
            () => TargetMenu(gene)
        );
    }

    // The raw figures behind the percentage, for the hover.
    private static string TargetUnits(Feruchemist gene) {
        float held = 0f;
        float max = 0f;

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            if (!MatchesDisplayTarget(gene, sources[i])) continue;

            held += sources[i].TotalStored;
            max += sources[i].MaxAmount;
        }

        return $"{held:0} / {max:0}";
    }

    private static bool MatchesDisplayTarget(Feruchemist gene, IMetalmindSource source) {
        return gene.targetMetalmindId switch {
            Feruchemist.TargetAll => true,
            Feruchemist.TargetInternal => source.IsImplanted,
            Feruchemist.TargetExternal => !source.IsImplanted,
            _ => source.SourceId == gene.targetMetalmindId,
        };
    }

    private static string Percent(float held, float max) {
        return max > 0f ? $"{held / max * 100f:0}%" : "0%";
    }

    // What the row reads while pointed at a group, or at one metalmind.
    private static string TargetLabel(Feruchemist gene) {
        if (!Feruchemist.IsGroupTarget(gene.targetMetalmindId)) {
            IMetalmindSource? selected = gene.SelectedSource;

            return selected == null
                ? "CC_Dock_Feruchemy_TargetAll".Translate().Resolve()
                : $"{selected.SourceLabel} ({Percent(selected.TotalStored, selected.MaxAmount)})";
        }

        string key = gene.targetMetalmindId switch {
            Feruchemist.TargetInternal => "CC_Dock_Feruchemy_TargetInternal",
            Feruchemist.TargetExternal => "CC_Dock_Feruchemy_TargetExternal",
            _ => "CC_Dock_Feruchemy_TargetAll",
        };

        return GroupSummary(gene, key, gene.targetMetalmindId);
    }

    // Groups carry their own running total, so choosing one does not hide how much
    // is actually in there.
    private static string GroupSummary(Feruchemist gene, string key, string target) {
        float stored = 0f;
        float max = 0f;
        int count = 0;

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            bool internalOnly = target == Feruchemist.TargetInternal;
            bool externalOnly = target == Feruchemist.TargetExternal;
            if (internalOnly && !sources[i].IsImplanted) continue;
            if (externalOnly && sources[i].IsImplanted) continue;

            stored += sources[i].TotalStored;
            max += sources[i].MaxAmount;
            count++;
        }

        return $"{key.Translate(count.Named("COUNT")).Resolve()} ({Percent(stored, max)})";
    }

    private static List<FloatMenuOption> TargetMenu(Feruchemist gene) {
        List<FloatMenuOption> options = [
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetAll", Feruchemist.TargetAll),
                () => gene.targetMetalmindId = Feruchemist.TargetAll
            ),
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetInternal", Feruchemist.TargetInternal),
                () => gene.targetMetalmindId = Feruchemist.TargetInternal
            ),
            new FloatMenuOption(
                GroupSummary(gene, "CC_Dock_Feruchemy_TargetExternal", Feruchemist.TargetExternal),
                () => gene.targetMetalmindId = Feruchemist.TargetExternal
            ),
        ];

        List<IMetalmindSource> sources = gene.metalminds;
        for (int i = 0; i < sources.Count; i++) {
            IMetalmindSource source = sources[i];
            string id = source.SourceId;
            options.Add(
                new FloatMenuOption(
                    $"{source.SourceLabel} ({Percent(source.TotalStored, source.MaxAmount)})",
                    () => gene.targetMetalmindId = id
                )
            );
        }

        return options;
    }
}
