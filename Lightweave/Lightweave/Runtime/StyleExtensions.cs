using System;
using Cosmere.Lightweave.Theme;

namespace Cosmere.Lightweave.Runtime;

public static class StyleExtensions {
    public static bool IsInFlow(this LightweaveNode node) {
        if (!node.Style.HasValue && (node.Classes == null || node.Classes.Length == 0)) {
            return true;
        }

        Theme.Theme? theme = RenderContext.CurrentOrNull?.Theme;
        if (theme == null) {
            return true;
        }

        Style resolved = theme.ResolveStyle(node);
        if (resolved.Display == Display.None) {
            return false;
        }

        Position pos = resolved.Position ?? Position.Static;
        return pos == Position.Static || pos == Position.Relative;
    }

    public static LightweaveNode WithStyle(this LightweaveNode node, Style style) {
        node.Style = node.Style.HasValue
            ? Style.Merge(node.Style.Value, style)
            : style;
        return node;
    }

    public static LightweaveNode WithStyle(this LightweaveNode node, Func<Style, Style> mutate) {
        Style baseStyle = node.Style ?? default;
        node.Style = mutate(baseStyle);
        return node;
    }

    public static LightweaveNode WithId(this LightweaveNode node, string id) {
        node.Id = id;
        return node;
    }

    public static LightweaveNode WithClass(this LightweaveNode node, params string[] classes) {
        if (classes == null || classes.Length == 0) {
            return node;
        }

        if (node.Classes == null || node.Classes.Length == 0) {
            node.Classes = classes;
            return node;
        }

        string[] merged = new string[node.Classes.Length + classes.Length];
        Array.Copy(node.Classes, merged, node.Classes.Length);
        Array.Copy(classes, 0, merged, node.Classes.Length, classes.Length);
        node.Classes = merged;
        return node;
    }
}
