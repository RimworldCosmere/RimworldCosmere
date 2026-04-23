using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Data;

public sealed record TableColumn<T>(
    string Header,
    Func<T, LightweaveNode> CellRenderer,
    Rem? Width = null);

public static class Table
{
    private static readonly Rem DefaultRowHeight = new Rem(2f);

    public static LightweaveNode Create<T>(
        IReadOnlyList<T> rows,
        IReadOnlyList<TableColumn<T>> columns,
        Rem? rowHeight = null,
        Func<T, object>? keyFn = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        Hooks.Hooks.RefHandle<ScrollViewStatus> statusRef =
            Hooks.Hooks.UseRef(new ScrollViewStatus(), line, file);

        LightweaveNode node = NodeBuilder.New($"Table<{typeof(T).Name}>", line, file);

        node.Paint = (rect, _) =>
        {
            if (columns == null || columns.Count == 0)
            {
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
            for (int i = 0; i < colCount; i++)
            {
                order[i] = rtl ? colCount - 1 - i : i;
            }

            float[] widths = ResolveColumnWidths(columns, rect.width);

            PaintHeader(headerRect, columns, order, widths);

            statusRef.Current.height = rowCount * rh;
            using (new ScrollView(bodyRect, statusRef.Current))
            {
                float scrollbarGutter = statusRef.Current.scrollVisibile ? 20f : 0f;
                float innerWidth = bodyRect.width - scrollbarGutter;

                if (innerWidth != rect.width)
                {
                    widths = ResolveColumnWidths(columns, innerWidth);
                }

                if (rowCount == 0)
                {
                    return;
                }

                float scrollY = statusRef.Current.position.y;
                int startIdx = Math.Max(0, (int)Math.Floor(scrollY / rh) - 2);
                int endIdx = Math.Min(rowCount, (int)Math.Ceiling((scrollY + bodyRect.height) / rh) + 2);

                for (int i = startIdx; i < endIdx; i++)
                {
                    Rect rowRect = new Rect(0f, i * rh, innerWidth, rh);
                    PaintRow(rowRect, rows![i], i, columns, order, widths, keyFn);
                }
            }
        };

        return node;
    }

    private static float[] ResolveColumnWidths<T>(
        IReadOnlyList<TableColumn<T>> columns,
        float totalWidth)
    {
        int colCount = columns.Count;
        float[] widths = new float[colCount];

        float fixedTotal = 0f;
        int flexCount = 0;
        for (int i = 0; i < colCount; i++)
        {
            Rem? w = columns[i].Width;
            if (w.HasValue)
            {
                widths[i] = w.Value.ToPixels();
                fixedTotal += widths[i];
            }
            else
            {
                flexCount++;
            }
        }

        if (flexCount > 0)
        {
            float remaining = Mathf.Max(0f, totalWidth - fixedTotal);
            float flexWidth = remaining / flexCount;
            for (int i = 0; i < colCount; i++)
            {
                if (!columns[i].Width.HasValue)
                {
                    widths[i] = flexWidth;
                }
            }
        }
        else if (fixedTotal > 0f && fixedTotal != totalWidth)
        {
            float scale = totalWidth / fixedTotal;
            for (int i = 0; i < colCount; i++)
            {
                widths[i] *= scale;
            }
        }

        return widths;
    }

    private static void PaintHeader<T>(
        Rect headerRect,
        IReadOnlyList<TableColumn<T>> columns,
        int[] order,
        float[] widths)
    {
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
        int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
        GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Bold);
        style.alignment = TextAnchor.MiddleLeft;

        float cursor = headerRect.x;
        for (int visual = 0; visual < order.Length; visual++)
        {
            int logical = order[visual];
            float w = widths[logical];
            Rect cellRect = new Rect(cursor, headerRect.y, w, headerRect.height);

            if (visual < order.Length - 1)
            {
                PaintSeparator(cellRect);
            }

            Rect labelRect = new Rect(
                cellRect.x + padPx,
                cellRect.y,
                Mathf.Max(0f, cellRect.width - padPx * 2f),
                cellRect.height);

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
        Func<T, object>? keyFn)
    {
        Theme.Theme theme = RenderContext.Current.Theme;

        ThemeSlot bgSlot = rowIndex % 2 == 0 ? ThemeSlot.SurfacePrimary : ThemeSlot.SurfaceRaised;
        Color savedBg = GUI.color;
        GUI.color = theme.GetColor(bgSlot);
        GUI.DrawTexture(RectSnap.Snap(rowRect), Texture2D.whiteTexture);
        GUI.color = savedBg;

        float padPx = SpacingScale.Sm.ToPixels();
        float cursor = rowRect.x;
        for (int visual = 0; visual < order.Length; visual++)
        {
            int logical = order[visual];
            float w = widths[logical];
            Rect cellRect = new Rect(cursor, rowRect.y, w, rowRect.height);

            if (visual < order.Length - 1)
            {
                PaintSeparator(cellRect);
            }

            Rect contentRect = new Rect(
                cellRect.x + padPx,
                cellRect.y + padPx,
                Mathf.Max(0f, cellRect.width - padPx * 2f),
                Mathf.Max(0f, cellRect.height - padPx * 2f));

            LightweaveNode cellNode = columns[logical].CellRenderer(row);
            if (keyFn != null)
            {
                cellNode.ExplicitKey = (keyFn(row), logical);
            }
            LightweaveRoot.PaintSubtree(cellNode, contentRect);

            cursor += w;
        }
    }

    private static void PaintSeparator(Rect cellRect)
    {
        Theme.Theme theme = RenderContext.Current.Theme;
        Rect sepRect = new Rect(cellRect.xMax - 1f, cellRect.y + 2f, 1f, Mathf.Max(0f, cellRect.height - 4f));
        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
        GUI.DrawTexture(RectSnap.Snap(sepRect), Texture2D.whiteTexture);
        GUI.color = saved;
    }

}
