using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Dock;

/// <summary>
///     A row that reads as a value and opens a menu when clicked.
/// </summary>
/// <remarks>
///     Built out of the feruchemy dock's metalmind selector, which was the only one of these
///     until duralumin needed a second. It knows a label, a tooltip and how to build a menu -
///     nothing about what is being selected.
/// </remarks>
public static class DockDropdownRow {
    public const float Height = 20f;

    private static readonly Color Backing = new Color(0.055f, 0.063f, 0.071f);
    private static readonly Color LabelTint = new Color(0.604f, 0.659f, 0.678f);
    private static readonly Color ChevronTint = new Color(0.435f, 0.478f, 0.498f);

    public static float Draw(Rect row, string label, string tooltip, Func<List<FloatMenuOption>> options) {
        Widgets.DrawBoxSolid(row, Backing);
        Widgets.DrawHighlightIfMouseover(row);
        if (!tooltip.NullOrEmpty()) TooltipHandler.TipRegion(row, tooltip);

        UIText.EllipsisLabel(
            new Rect(row.x + 6f, row.y, row.width - 26f, row.height),
            label,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            LabelTint
        );
        UIText.EllipsisLabel(
            new Rect(row.xMax - 20f, row.y, 14f, row.height),
            "v",
            GameFont.Tiny,
            TextAnchor.MiddleCenter,
            ChevronTint
        );

        if (Widgets.ButtonInvisible(row)) {
            Find.WindowStack.Add(new FloatMenu(options()));
            Event.current?.Use();
        }

        return row.yMax;
    }
}
