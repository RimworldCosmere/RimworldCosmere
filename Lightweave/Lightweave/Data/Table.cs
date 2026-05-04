using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Code = Cosmere.Lightweave.Typography.Typography.Code;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;
using Icon = Cosmere.Lightweave.Typography.Typography.Icon;
using Label = Cosmere.Lightweave.Typography.Typography.Label;
using RichText = Cosmere.Lightweave.Typography.Typography.RichText;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Data;

public sealed record TableColumn<T>(
    string Header,
    Func<T, LightweaveNode> CellRenderer,
    Rem? Width = null
);

[Doc(
    Id = "table",
    Summary = "Tabular renderer with a header row, alternating row backgrounds, and column separators.",
    WhenToUse = "Display structured records that benefit from named columns and row alignment.",
    SourcePath = "Lightweave/Lightweave/Data/Table.cs",
    PreferredVariantHeight = 220f
)]
public static class Table {
    private static readonly Rem DefaultRowHeight = new Rem(2.25f);

    public static LightweaveNode Create<T>(
        [DocParam("Records rendered as table rows.")]
        IReadOnlyList<T> rows,
        [DocParam("Column descriptors that produce header text and cell nodes for each row.")]
        IReadOnlyList<TableColumn<T>> columns,
        [DocParam("Row height. Defaults to 2.25rem.")]
        Rem? rowHeight = null,
        [DocParam("Stable key extractor used to preserve cell identity across renders.")]
        Func<T, object>? keyFn = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<LightweaveScrollStatus> statusRef =
            Hooks.Hooks.UseRef(new LightweaveScrollStatus(), line, file);

        LightweaveNode node = NodeBuilder.New($"Table<{typeof(T).Name}>", line, file);

        float resolvedRowHeight = (rowHeight ?? DefaultRowHeight).ToPixels();
        int rowCountForHeight = rows?.Count ?? 0;
        node.PreferredHeight = resolvedRowHeight * (rowCountForHeight + 1);

        node.Paint = (rect, _) => {
            if (columns == null || columns.Count == 0) {
                return;
            }

            float rh = (rowHeight ?? DefaultRowHeight).ToPixels();
            int rowCount = rows?.Count ?? 0;
            int colCount = columns.Count;

            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, rh);
            Rect bodyRect = new Rect(rect.x, rect.y + rh, rect.width, Mathf.Max(0f, rect.height - rh));

            int[] order = new int[colCount];
            for (int i = 0; i < colCount; i++) {
                order[i] = rtl ? colCount - 1 - i : i;
            }

            float[] widths = ResolveColumnWidths(columns, rect.width);

            PaintHeader(headerRect, columns, order, widths);

            statusRef.Current.Height = rowCount * rh;
            using (new LightweaveScrollView(bodyRect, statusRef.Current)) {
                float scrollbarGutter = LightweaveScrollView.GutterPixels(statusRef.Current.VerticalVisible);
                float innerWidth = bodyRect.width - scrollbarGutter;

                if (innerWidth != rect.width) {
                    widths = ResolveColumnWidths(columns, innerWidth);
                }

                if (rowCount == 0) {
                    return;
                }

                float scrollY = statusRef.Current.Position.y;
                int startIdx = Math.Max(0, (int)Math.Floor(scrollY / rh) - 2);
                int endIdx = Math.Min(rowCount, (int)Math.Ceiling((scrollY + bodyRect.height) / rh) + 2);

                for (int i = startIdx; i < endIdx; i++) {
                    Rect rowRect = new Rect(0f, i * rh, innerWidth, rh);
                    PaintRow(rowRect, rows![i], i, columns, order, widths, keyFn);
                }
            }
        };

        return node;
    }

    private static float[] ResolveColumnWidths<T>(
        IReadOnlyList<TableColumn<T>> columns,
        float totalWidth
    ) {
        int colCount = columns.Count;
        float[] widths = new float[colCount];

        float fixedTotal = 0f;
        int flexCount = 0;
        for (int i = 0; i < colCount; i++) {
            Rem? w = columns[i].Width;
            if (w.HasValue) {
                widths[i] = w.Value.ToPixels();
                fixedTotal += widths[i];
            }
            else {
                flexCount++;
            }
        }

        if (flexCount > 0) {
            float remaining = Mathf.Max(0f, totalWidth - fixedTotal);
            float flexWidth = remaining / flexCount;
            for (int i = 0; i < colCount; i++) {
                if (!columns[i].Width.HasValue) {
                    widths[i] = flexWidth;
                }
            }
        }
        else if (fixedTotal > 0f && fixedTotal != totalWidth) {
            float scale = totalWidth / fixedTotal;
            for (int i = 0; i < colCount; i++) {
                widths[i] *= scale;
            }
        }

        return widths;
    }

    private static void PaintHeader<T>(
        Rect headerRect,
        IReadOnlyList<TableColumn<T>> columns,
        int[] order,
        float[] widths
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;

        Color savedBg = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
        GUI.DrawTexture(RectSnap.Snap(headerRect), Texture2D.whiteTexture);
        GUI.color = savedBg;

        Rect borderRect = new Rect(headerRect.x, headerRect.yMax - 1f, headerRect.width, 1f);
        Color savedBorder = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.BorderDefault);
        GUI.DrawTexture(RectSnap.Snap(borderRect), Texture2D.whiteTexture);
        GUI.color = savedBorder;

        float padPx = SpacingScale.Sm.ToPixels();
        Font font = theme.GetFont(FontRole.BodyBold);
        int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
        style.alignment = TextAnchor.MiddleLeft;

        float cursor = headerRect.x;
        for (int visual = 0; visual < order.Length; visual++) {
            int logical = order[visual];
            float w = widths[logical];
            Rect cellRect = new Rect(cursor, headerRect.y, w, headerRect.height);

            if (visual < order.Length - 1) {
                PaintSeparator(cellRect);
            }

            Rect labelRect = new Rect(
                cellRect.x + padPx,
                cellRect.y,
                Mathf.Max(0f, cellRect.width - padPx * 2f),
                cellRect.height
            );

            Color savedLabel = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(labelRect), columns[logical].Header ?? string.Empty, style);
            GUI.color = savedLabel;

            cursor += w;
        }
    }

    private static void PaintRow<T>(
        Rect rowRect,
        T row,
        int rowIndex,
        IReadOnlyList<TableColumn<T>> columns,
        int[] order,
        float[] widths,
        Func<T, object>? keyFn
    ) {
        Theme.Theme theme = RenderContext.Current.Theme;

        ThemeSlot bgSlot = rowIndex % 2 == 0 ? ThemeSlot.SurfacePrimary : ThemeSlot.SurfaceRaised;
        Color savedBg = GUI.color;
        GUI.color = theme.GetColor(bgSlot);
        GUI.DrawTexture(RectSnap.Snap(rowRect), Texture2D.whiteTexture);
        GUI.color = savedBg;

        float padPx = SpacingScale.Sm.ToPixels();
        float cursor = rowRect.x;
        for (int visual = 0; visual < order.Length; visual++) {
            int logical = order[visual];
            float w = widths[logical];
            Rect cellRect = new Rect(cursor, rowRect.y, w, rowRect.height);

            if (visual < order.Length - 1) {
                PaintSeparator(cellRect);
            }

            Rect contentRect = new Rect(
                cellRect.x + padPx,
                cellRect.y + padPx,
                Mathf.Max(0f, cellRect.width - padPx * 2f),
                Mathf.Max(0f, cellRect.height - padPx * 2f)
            );

            LightweaveNode cellNode = columns[logical].CellRenderer(row);
            if (keyFn != null) {
                cellNode.ExplicitKey = (keyFn(row), logical);
            }

            LightweaveRoot.PaintSubtree(cellNode, contentRect);

            cursor += w;
        }
    }

    private static void PaintSeparator(Rect cellRect) {
        Theme.Theme theme = RenderContext.Current.Theme;
        Rect sepRect = new Rect(cellRect.xMax - 1f, cellRect.y + 2f, 1f, Mathf.Max(0f, cellRect.height - 4f));
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
        GUI.DrawTexture(RectSnap.Snap(sepRect), Texture2D.whiteTexture);
        GUI.color = saved;
    }

    private static LightweaveNode BuildSampleTable() {
        (string World, string Shards, string Population)[] rows = new[] {
            ("Roshar", "Honor + Cultivation", "Billions"),
            ("Scadrial", "Preservation + Ruin", "Millions"),
            ("Nalthis", "Endowment", "Millions"),
            ("Taldain", "Autonomy", "Few"),
        };

        List<TableColumn<(string World, string Shards, string Population)>> columns =
            new List<TableColumn<(string, string, string)>> {
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_World".Translate(),
                    r => Text.Create(r.Item1, FontRole.Body, new Rem(0.875f), ThemeSlot.TextPrimary),
                    new Rem(7f)
                ),
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_Shards".Translate(),
                    r => Text.Create(r.Item2, FontRole.Body, new Rem(0.8125f), ThemeSlot.TextSecondary)
                ),
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_Population".Translate(),
                    r => Text.Create(r.Item3, FontRole.Body, new Rem(0.8125f), ThemeSlot.TextMuted),
                    new Rem(6f)
                ),
            };

        return Table.Create<(string, string, string)>(rows, columns);
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(BuildSampleTable());
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSampleTable());
    }
}