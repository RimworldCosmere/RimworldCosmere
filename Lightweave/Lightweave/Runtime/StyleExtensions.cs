using System;
using Cosmere.Lightweave.Theme;

namespace Cosmere.Lightweave.Runtime;

public static class StyleExtensions {
    public static Style GetResolvedStyle(this LightweaveNode node) {
        if (!node.Style.HasValue && (node.Classes == null || node.Classes.Length == 0)) {
            return default;
        }

        Theme.Theme? theme = RenderContext.CurrentOrNull?.Theme;
        if (theme == null) {
            return node.Style ?? default;
        }

        return theme.ResolveStyle(node);
    }

    public static Style GetResolvedStyle(this LightweaveNode node, bool hovered, bool pressed, bool focused, bool disabled) {
        Style baseStyle = node.GetResolvedStyle();
        if (disabled && baseStyle.Disabled != null) {
            baseStyle = Style.Merge(baseStyle, baseStyle.Disabled.Value);
        }
        if (focused && baseStyle.Focus != null) {
            baseStyle = Style.Merge(baseStyle, baseStyle.Focus.Value);
        }
        if (hovered && baseStyle.Hover != null) {
            baseStyle = Style.Merge(baseStyle, baseStyle.Hover.Value);
        }
        if (pressed && baseStyle.Active != null) {
            baseStyle = Style.Merge(baseStyle, baseStyle.Active.Value);
        }
        return baseStyle;
    }

    public static bool IsInFlow(this LightweaveNode node) {
        if (!node.Style.HasValue && (node.Classes == null || node.Classes.Length == 0)) {
            return true;
        }

        Style resolved = node.GetResolvedStyle();
        if (resolved.Visible == false) {
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

    public static LightweaveNode ApplyStyling(
        this LightweaveNode node,
        string baseClass,
        Style? style,
        string[]? classes,
        string? id
    ) {
        string[] merged;
        if (classes == null || classes.Length == 0) {
            merged = new[] { baseClass };
        }
        else {
            merged = new string[classes.Length + 1];
            merged[0] = baseClass;
            Array.Copy(classes, 0, merged, 1, classes.Length);
        }

        node.Classes = merged;
        if (style.HasValue) {
            node.Style = style.Value;
        }
        if (id != null) {
            node.Id = id;
        }
        return node;
    }

    public static string[] PrependClass(string head, string[]? tail) {
        if (tail == null || tail.Length == 0) {
            return new[] { head };
        }
        string[] result = new string[tail.Length + 1];
        result[0] = head;
        Array.Copy(tail, 0, result, 1, tail.Length);
        return result;
    }
}
