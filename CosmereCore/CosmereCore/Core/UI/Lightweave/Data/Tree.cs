using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Data;

public sealed record TreeNode(
    string Label,
    IReadOnlyList<TreeNode>? Children = null,
    object? Payload = null);

public static class Tree
{
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem IndentPerLevel = new Rem(1.5f);
    private static readonly Rem ChevronWidth = new Rem(1.25f);
    private static readonly Rem LabelSize = new Rem(0.875f);

    private const string ChevronCollapsedLtr = "▸";
    private const string ChevronCollapsedRtl = "◂";
    private const string ChevronExpanded = "▾";

    private sealed class ReferenceComparer : IEqualityComparer<TreeNode>
    {
        public static readonly ReferenceComparer Instance = new ReferenceComparer();
        public bool Equals(TreeNode? x, TreeNode? y) => ReferenceEquals(x, y);
        public int GetHashCode(TreeNode obj) => global::System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    public static LightweaveNode Create(
        IReadOnlyList<TreeNode> roots,
        Action<TreeNode>? onSelect = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Hooks.Hooks.StateHandle<HashSet<TreeNode>> expandedState =
            Hooks.Hooks.UseState<HashSet<TreeNode>>(
                new HashSet<TreeNode>(ReferenceComparer.Instance),
                line,
                file);
        Hooks.Hooks.RefHandle<ScrollViewStatus> statusRef =
            Hooks.Hooks.UseRef(new ScrollViewStatus(), line, file + "#scroll");

        LightweaveNode node = NodeBuilder.New("Tree", line, file);

        node.Paint = (rect, _) =>
        {
            if (roots == null || roots.Count == 0)
            {
                return;
            }

            HashSet<TreeNode> expanded = expandedState.Value;
            List<(TreeNode Node, int Depth)> visible = new List<(TreeNode, int)>();
            for (int i = 0; i < roots.Count; i++)
            {
                Flatten(roots[i], 0, expanded, visible);
            }

            float rh = RowHeight.ToPixels();
            statusRef.Current.height = visible.Count * rh;

            using (new ScrollView(rect, statusRef.Current))
            {
                float scrollbarGutter = statusRef.Current.scrollVisibile ? 20f : 0f;
                float innerWidth = rect.width - scrollbarGutter;

                for (int i = 0; i < visible.Count; i++)
                {
                    Rect rowRect = new Rect(0f, i * rh, innerWidth, rh);
                    PaintRow(rowRect, visible[i].Node, visible[i].Depth, expanded, expandedState, onSelect);
                }
            }
        };

        return node;
    }

    private static void Flatten(
        TreeNode current,
        int depth,
        HashSet<TreeNode> expanded,
        List<(TreeNode, int)> output)
    {
        output.Add((current, depth));
        if (current.Children == null || current.Children.Count == 0)
        {
            return;
        }
        if (!expanded.Contains(current))
        {
            return;
        }
        for (int i = 0; i < current.Children.Count; i++)
        {
            Flatten(current.Children[i], depth + 1, expanded, output);
        }
    }

    private static void PaintRow(
        Rect rowRect,
        TreeNode treeNode,
        int depth,
        HashSet<TreeNode> expanded,
        Hooks.Hooks.StateHandle<HashSet<TreeNode>> expandedState,
        Action<TreeNode>? onSelect)
    {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        bool rtl = dir == Direction.Rtl;
        Event e = Event.current;

        if (rowRect.Contains(e.mousePosition))
        {
            Widgets.DrawHighlight(rowRect);
        }

        float indentPx = depth * IndentPerLevel.ToPixels();
        float chevronPx = ChevronWidth.ToPixels();
        float padPx = SpacingScale.Xs.ToPixels();
        bool hasChildren = treeNode.Children != null && treeNode.Children.Count > 0;
        bool isExpanded = hasChildren && expanded.Contains(treeNode);

        Rect chevronRect;
        Rect labelRect;
        if (rtl)
        {
            float chevronX = rowRect.xMax - indentPx - chevronPx;
            chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            float labelEndX = chevronX - padPx;
            labelRect = new Rect(rowRect.x, rowRect.y, Mathf.Max(0f, labelEndX - rowRect.x), rowRect.height);
        }
        else
        {
            float chevronX = rowRect.x + indentPx;
            chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            float labelStartX = chevronX + chevronPx + padPx;
            labelRect = new Rect(labelStartX, rowRect.y, Mathf.Max(0f, rowRect.xMax - labelStartX), rowRect.height);
        }

        if (hasChildren)
        {
            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronPixelSize = Mathf.RoundToInt(new Rem(1f).ToPixels());
            GUIStyle chevronStyle = GuiStyleCache.Get(chevronFont, chevronPixelSize, FontStyle.Normal);
            chevronStyle.alignment = TextAnchor.MiddleCenter;

            string glyph = isExpanded
                ? ChevronExpanded
                : (rtl ? ChevronCollapsedRtl : ChevronCollapsedLtr);

            Color savedChevron = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(chevronRect), glyph, chevronStyle);
            GUI.color = savedChevron;
        }

        Font labelFont = theme.GetFont(FontRole.Body);
        int labelPixelSize = Mathf.RoundToInt(LabelSize.ToPixels());
        GUIStyle labelStyle = GuiStyleCache.Get(labelFont, labelPixelSize, FontStyle.Normal);
        labelStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

        Color savedLabel = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(RectSnap.Snap(labelRect), treeNode.Label ?? string.Empty, labelStyle);
        GUI.color = savedLabel;

        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (hasChildren && chevronRect.Contains(e.mousePosition))
            {
                HashSet<TreeNode> next = new HashSet<TreeNode>(expanded, ReferenceComparer.Instance);
                if (!next.Add(treeNode))
                {
                    next.Remove(treeNode);
                }
                expandedState.Set(next);
                e.Use();
            }
            else if (labelRect.Contains(e.mousePosition))
            {
                onSelect?.Invoke(treeNode);
                e.Use();
            }
        }
    }
}
