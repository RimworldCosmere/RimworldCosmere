using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Def;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public readonly struct MetalRow {
    public MetalRow(InvestitureCell cell, string axisGlyph) {
        Cell = cell;
        AxisGlyph = axisGlyph;
    }

    public InvestitureCell Cell { get; }
    public string AxisGlyph { get; }
}

public sealed class MetalGroup {
    public MetalGroup(string labelKey) {
        LabelKey = labelKey;
    }

    public string LabelKey { get; }
    public List<MetalRow> Rows { get; } = [];
}

public static class MetalGroupTable {
    public static IReadOnlyList<MetalGroup> AllomancyGroups(IReadOnlyList<InvestitureCell> cells) {
        List<(InvestitureCell cell, AllomancyGroup group, AllomancyPolarity polarity, AllomancyAxis axis)> resolved = new(cells.Count);
        for (int i = 0; i < cells.Count; i++) {
            MetallicArtsMetalDef? def = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cells[i].SubsystemId);
            MetalAllomancyDef? a = def?.allomancy;
            resolved.Add((
                cells[i],
                a?.group ?? AllomancyGroup.None,
                a?.polarity ?? AllomancyPolarity.None,
                a?.axis ?? AllomancyAxis.None
            ));
        }

        resolved.Sort((x, y) => {
            int g = GroupOrder(x.group).CompareTo(GroupOrder(y.group));
            if (g != 0) return g;
            int p = PolarityOrder(x.polarity).CompareTo(PolarityOrder(y.polarity));
            if (p != 0) return p;
            int ax = AxisOrder(x.axis).CompareTo(AxisOrder(y.axis));
            if (ax != 0) return ax;
            return string.CompareOrdinal(x.cell.SubsystemId, y.cell.SubsystemId);
        });

        List<MetalGroup> groups = [];
        MetalGroup? current = null;
        AllomancyGroup? currentKind = null;
        for (int i = 0; i < resolved.Count; i++) {
            (InvestitureCell cell, AllomancyGroup group, AllomancyPolarity polarity, AllomancyAxis axis) = resolved[i];
            if (current == null || currentKind != group) {
                current = new MetalGroup(AllomancyLabelKey(group));
                currentKind = group;
                groups.Add(current);
            }

            current.Rows.Add(new MetalRow(cell, Glyph(polarity, axis)));
        }

        return groups;
    }

    public static IReadOnlyList<MetalGroup> FeruchemyGroups(IReadOnlyList<InvestitureCell> cells) {
        List<(InvestitureCell cell, FeruchemyGroup group)> resolved = new(cells.Count);
        for (int i = 0; i < cells.Count; i++) {
            MetallicArtsMetalDef? def = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cells[i].SubsystemId);
            resolved.Add((cells[i], def?.feruchemy?.group ?? FeruchemyGroup.None));
        }

        resolved.Sort((x, y) => {
            int g = FeruchemyGroupOrder(x.group).CompareTo(FeruchemyGroupOrder(y.group));
            if (g != 0) return g;
            return string.CompareOrdinal(x.cell.SubsystemId, y.cell.SubsystemId);
        });

        List<MetalGroup> groups = [];
        MetalGroup? current = null;
        FeruchemyGroup? currentKind = null;
        for (int i = 0; i < resolved.Count; i++) {
            (InvestitureCell cell, FeruchemyGroup group) = resolved[i];
            if (current == null || currentKind != group) {
                current = new MetalGroup(FeruchemyLabelKey(group));
                currentKind = group;
                groups.Add(current);
            }

            current.Rows.Add(new MetalRow(cell, ""));
        }

        return groups;
    }

    private static int GroupOrder(AllomancyGroup group) {
        return group == AllomancyGroup.None ? int.MaxValue : (int)group;
    }

    private static int FeruchemyGroupOrder(FeruchemyGroup group) {
        return group == FeruchemyGroup.None ? int.MaxValue : (int)group;
    }

    private static int PolarityOrder(AllomancyPolarity polarity) {
        return polarity switch {
            AllomancyPolarity.Pushing => 0,
            AllomancyPolarity.Pulling => 1,
            _ => 2,
        };
    }

    private static int AxisOrder(AllomancyAxis axis) {
        return axis switch {
            AllomancyAxis.External => 0,
            AllomancyAxis.Internal => 1,
            _ => 2,
        };
    }

    private static string Glyph(AllomancyPolarity polarity, AllomancyAxis axis) {
        if (polarity == AllomancyPolarity.None && axis == AllomancyAxis.None) return "";
        string p = polarity == AllomancyPolarity.Pushing ? "^" : polarity == AllomancyPolarity.Pulling ? "v" : "";
        string a = axis == AllomancyAxis.External ? "E" : axis == AllomancyAxis.Internal ? "I" : "";
        return p + a;
    }

    private static string AllomancyLabelKey(AllomancyGroup group) {
        return group switch {
            AllomancyGroup.Physical => "CC_Dock_Group_Physical",
            AllomancyGroup.Mental => "CC_Dock_Group_Mental",
            AllomancyGroup.Enhancement => "CC_Dock_Group_Enhancement",
            AllomancyGroup.Temporal => "CC_Dock_Group_Temporal",
            _ => "CC_Dock_Group_Godmetal",
        };
    }

    private static string FeruchemyLabelKey(FeruchemyGroup group) {
        return group switch {
            FeruchemyGroup.Physical => "CC_Dock_Group_Physical",
            FeruchemyGroup.Cognitive => "CC_Dock_Group_Cognitive",
            FeruchemyGroup.Spiritual => "CC_Dock_Group_Spiritual",
            FeruchemyGroup.Hybrid => "CC_Dock_Group_Hybrid",
            _ => "CC_Dock_Group_Godmetal",
        };
    }
}
