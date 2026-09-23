using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI;

/// <summary>One tab: what it says, what colour it carries, and what it is.</summary>
public readonly struct TabStripItem<T> {
    public TabStripItem(T value, string label, Color accent, string? tooltip = null) {
        Value = value;
        Label = label;
        Accent = accent;
        Tooltip = tooltip;
    }

    public T Value { get; }

    public string Label { get; }

    public Color Accent { get; }

    public string? Tooltip { get; }
}

/// <summary>
///     A row of tabs across the top of a rect.
/// </summary>
/// <remarks>
///     Deliberately knows nothing about worlds, pawns or the Codex - it takes labels and colours
///     and hands back which one was clicked. The Codex's own subtab bar is welded to a closed
///     enum and to investiture providers, so it could not be reused here; this one is generic so
///     the next surface that wants tabs does not write a third.
/// </remarks>
public static class TabStrip {
    public const float Height = 32f;

    private const float Gap = 4f;
    private const float UnderlineHeight = 3f;
    private const float MinTabWidth = 90f;

    /// <summary>Draws the strip and returns the item clicked this frame, if any.</summary>
    public static bool Draw<T>(Rect rect, IReadOnlyList<TabStripItem<T>> items, T selected, out T clicked)
        where T : class {
        clicked = selected;
        if (items.Count == 0) return false;

        float width = Mathf.Max(MinTabWidth, (rect.width - Gap * (items.Count - 1)) / items.Count);
        bool changed = false;
        float x = rect.x;

        for (int i = 0; i < items.Count; i++) {
            TabStripItem<T> item = items[i];
            bool isSelected = ReferenceEquals(item.Value, selected);
            Rect tab = new Rect(x, rect.y, width, Height);

            DrawTab(tab, item, isSelected);

            if (Widgets.ButtonInvisible(tab, false) && !isSelected) {
                RimWorld.SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                clicked = item.Value;
                changed = true;
            }

            x += width + Gap;
        }

        return changed;
    }

    private static void DrawTab<T>(Rect tab, TabStripItem<T> item, bool isSelected) {
        Widgets.DrawBoxSolid(
            tab,
            isSelected
                ? new Color(item.Accent.r, item.Accent.g, item.Accent.b, 0.22f)
                : new Color(0.14f, 0.14f, 0.17f, 0.55f)
        );

        if (!isSelected) Widgets.DrawHighlightIfMouseover(tab);

        // the accent reads as an underline, not a fill, so a bright shard colour does not fight the label on top.
        Widgets.DrawBoxSolid(
            new Rect(tab.x, tab.yMax - UnderlineHeight, tab.width, UnderlineHeight),
            isSelected ? item.Accent : new Color(item.Accent.r, item.Accent.g, item.Accent.b, 0.25f)
        );

        using (new TextBlock(
                   GameFont.Small,
                   TextAnchor.MiddleCenter,
                   isSelected ? Color.white : ColoredText.SubtleGrayColor
               )) {
            Widgets.Label(tab, item.Label);
        }

        if (!string.IsNullOrEmpty(item.Tooltip)) TooltipHandler.TipRegion(tab, item.Tooltip);

        MouseoverSounds.DoRegion(tab);
    }
}
