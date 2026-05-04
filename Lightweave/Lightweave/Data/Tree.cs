using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Data;

public sealed record TreeNode(
    string Label,
    IReadOnlyList<TreeNode>? Children = null,
    object? Payload = null
);

[Doc(
    Id = "tree",
    Summary = "Hierarchical, expandable list of nodes with chevron toggles.",
    WhenToUse = "Browse nested data such as locations, categories, or org structure.",
    SourcePath = "Lightweave/Lightweave/Data/Tree.cs",
    PreferredVariantHeight = 200f
)]
public static class Tree {
    private const string ChevronCollapsedLtr = "▸";
    private const string ChevronCollapsedRtl = "◂";
    private const string ChevronExpanded = "▾";
    private static readonly Rem RowHeight = new Rem(1.75f);
    private static readonly Rem IndentPerLevel = new Rem(1.5f);
    private static readonly Rem ChevronWidth = new Rem(1.25f);
    private static readonly Rem LabelSize = new Rem(0.875f);

    public static LightweaveNode Create(
        [DocParam("Top-level nodes of the tree.")]
        IReadOnlyList<TreeNode> roots,
        [DocParam("Invoked when a leaf or label is clicked.")]
        Action<TreeNode>? onSelect = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.StateHandle<HashSet<TreeNode>> expandedState =
            Hooks.Hooks.UseState<HashSet<TreeNode>>(
                new HashSet<TreeNode>(ReferenceComparer.Instance),
                line,
                file
            );
        Hooks.Hooks.RefHandle<LightweaveScrollStatus> statusRef =
            Hooks.Hooks.UseRef(new LightweaveScrollStatus(), line, file + "#scroll");

        LightweaveNode node = NodeBuilder.New("Tree", line, file);

        node.Measure = _ => {
            if (roots == null || roots.Count == 0) {
                return 0f;
            }

            HashSet<TreeNode> expanded = expandedState.Value;
            int visibleCount = 0;
            for (int i = 0; i < roots.Count; i++) {
                visibleCount += CountVisible(roots[i], expanded);
            }

            return visibleCount * RowHeight.ToPixels();
        };

        node.Paint = (rect, _) => {
            if (roots == null || roots.Count == 0) {
                return;
            }

            HashSet<TreeNode> expanded = expandedState.Value;
            List<(TreeNode Node, int Depth)> visible = new List<(TreeNode, int)>();
            for (int i = 0; i < roots.Count; i++) {
                Flatten(roots[i], 0, expanded, visible);
            }

            float rh = RowHeight.ToPixels();
            statusRef.Current.Height = visible.Count * rh;

            using (new LightweaveScrollView(rect, statusRef.Current)) {
                float scrollbarGutter = LightweaveScrollView.GutterPixels(statusRef.Current.VerticalVisible);
                float innerWidth = rect.width - scrollbarGutter;

                for (int i = 0; i < visible.Count; i++) {
                    Rect rowRect = new Rect(0f, i * rh, innerWidth, rh);
                    PaintRow(rowRect, visible[i].Node, visible[i].Depth, expanded, expandedState, onSelect);
                }
            }
        };

        return node;
    }

    private static int CountVisible(TreeNode current, HashSet<TreeNode> expanded) {
        int count = 1;
        if (current.Children == null || current.Children.Count == 0) {
            return count;
        }

        if (!expanded.Contains(current)) {
            return count;
        }

        for (int i = 0; i < current.Children.Count; i++) {
            count += CountVisible(current.Children[i], expanded);
        }

        return count;
    }

    private static void Flatten(
        TreeNode current,
        int depth,
        HashSet<TreeNode> expanded,
        List<(TreeNode, int)> output
    ) {
        output.Add((current, depth));
        if (current.Children == null || current.Children.Count == 0) {
            return;
        }

        if (!expanded.Contains(current)) {
            return;
        }

        for (int i = 0; i < current.Children.Count; i++) {
            Flatten(current.Children[i], depth + 1, expanded, output);
        }
    }

    private static void PaintRow(
        Rect rowRect,
        TreeNode treeNode,
        int depth,
        HashSet<TreeNode> expanded,
        Hooks.Hooks.StateHandle<HashSet<TreeNode>> expandedState,
        Action<TreeNode>? onSelect
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Direction dir = RenderContext.Current.Direction;
        bool rtl = dir == Direction.Rtl;
        Event e = Event.current;

        if (rowRect.Contains(e.mousePosition)) {
            PaintBox.DrawHighlight(rowRect, RadiusSpec.All(new Rem(0.25f)), true);
        }

        float indentPx = depth * IndentPerLevel.ToPixels();
        float chevronPx = ChevronWidth.ToPixels();
        float padPx = SpacingScale.Xs.ToPixels();
        bool hasChildren = treeNode.Children != null && treeNode.Children.Count > 0;
        bool isExpanded = hasChildren && expanded.Contains(treeNode);

        Rect chevronRect;
        Rect labelRect;
        if (rtl) {
            float chevronX = rowRect.xMax - indentPx - chevronPx;
            chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            float labelEndX = chevronX - padPx;
            labelRect = new Rect(rowRect.x, rowRect.y, Mathf.Max(0f, labelEndX - rowRect.x), rowRect.height);
        }
        else {
            float chevronX = rowRect.x + indentPx;
            chevronRect = new Rect(chevronX, rowRect.y, chevronPx, rowRect.height);
            float labelStartX = chevronX + chevronPx + padPx;
            labelRect = new Rect(labelStartX, rowRect.y, Mathf.Max(0f, rowRect.xMax - labelStartX), rowRect.height);
        }

        if (hasChildren) {
            Font chevronFont = theme.GetFont(FontRole.Body);
            int chevronPixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
            GUIStyle chevronStyle = GuiStyleCache.GetOrCreate(chevronFont, chevronPixelSize);
            chevronStyle.alignment = TextAnchor.MiddleCenter;

            string glyph = isExpanded
                ? ChevronExpanded
                : rtl
                    ? ChevronCollapsedRtl
                    : ChevronCollapsedLtr;

            Color savedChevron = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(chevronRect), glyph, chevronStyle);
            GUI.color = savedChevron;
        }

        Font labelFont = theme.GetFont(FontRole.Body);
        int labelPixelSize = Mathf.RoundToInt(LabelSize.ToFontPx());
        GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPixelSize);
        labelStyle.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

        Color savedLabel = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(RectSnap.Snap(labelRect), treeNode.Label ?? string.Empty, labelStyle);
        GUI.color = savedLabel;

        if (e.type == EventType.MouseUp && e.button == 0) {
            if (hasChildren && chevronRect.Contains(e.mousePosition)) {
                HashSet<TreeNode> next = new HashSet<TreeNode>(expanded, ReferenceComparer.Instance);
                if (!next.Add(treeNode)) {
                    next.Remove(treeNode);
                }

                expandedState.Set(next);
                e.Use();
            }
            else if (labelRect.Contains(e.mousePosition)) {
                onSelect?.Invoke(treeNode);
                e.Use();
            }
        }
    }

    private sealed class ReferenceComparer : IEqualityComparer<TreeNode> {
        public static readonly ReferenceComparer Instance = new ReferenceComparer();

        public bool Equals(TreeNode? x, TreeNode? y) {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(TreeNode obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    private static LightweaveNode BuildSampleTree() {
        TreeNode[] roots = Hooks.Hooks.UseMemo(
            () => {
                TreeNode shatteredPlains = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate()
                );
                TreeNode luthadel = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Luthadel".Translate(),
                    new[] {
                        new TreeNode((string)"CC_Playground_Breadcrumbs_Crumb_CentralDistrict".Translate()),
                        new TreeNode((string)"CC_Playground_Breadcrumbs_Crumb_VentureKeep".Translate()),
                    }
                );
                TreeNode roshar = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate(),
                    new[] { shatteredPlains }
                );
                TreeNode scadrial = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Scadrial".Translate(),
                    new[] { luthadel }
                );
                return new[] { roshar, scadrial };
            },
            Array.Empty<object>()
        );

        return Tree.Create(roots);
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(BuildSampleTree());
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSampleTree());
    }
}