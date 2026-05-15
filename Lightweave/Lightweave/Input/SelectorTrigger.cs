using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Input;

internal static class SelectorTrigger {
    public static readonly Rem Height = new Rem(2.875f);
    public static readonly Rem PaddingX = new Rem(1f);
    public static readonly Rem ChevronWidth = new Rem(1.25f);
    public static readonly Rem ChevronFontSize = new Rem(1.25f);

    public static Rect ComputeTriggerRect(Rect allocatedRect) {
        float h = Mathf.Min(Height.ToPixels(), allocatedRect.height);
        float y = allocatedRect.y + (allocatedRect.height - h) / 2f;
        return new Rect(allocatedRect.x, y, allocatedRect.width, h);
    }

    public readonly struct Layout {
        public readonly Rect LabelRect;
        public readonly Rect ChevronRect;

        public Layout(Rect labelRect, Rect chevronRect) {
            LabelRect = labelRect;
            ChevronRect = chevronRect;
        }
    }

    public static Layout ComputeLayout(Rect rect, Direction dir) {
        float padPx = PaddingX.ToPixels();
        float chevronPx = ChevronWidth.ToPixels();
        bool rtl = dir == Direction.Rtl;

        float chevronX = rtl ? rect.x + padPx : rect.xMax - padPx - chevronPx;
        Rect chevronRect = new Rect(chevronX, rect.y, chevronPx, rect.height);

        float labelStartX = rtl ? chevronX + chevronPx + padPx : rect.x + padPx;
        float labelEndX = rtl ? rect.xMax - padPx : chevronX - padPx;
        Rect labelRect = new Rect(labelStartX, rect.y, labelEndX - labelStartX, rect.height);

        return new Layout(labelRect, chevronRect);
    }

    public static void DrawChevron(Rect chevronRect, ThemeSlot colorSlot, Theme.Theme theme) {
        Font chevronFont = theme.GetFont(FontRole.Body);
        int chevronPixelSize = Mathf.RoundToInt(ChevronFontSize.ToFontPx());
        GUIStyle chevronStyle = GuiStyleCache.GetOrCreate(chevronFont, chevronPixelSize);
        chevronStyle.alignment = TextAnchor.MiddleCenter;
        Color saved = GUI.color;
        GUI.color = theme.GetColor(colorSlot);
        GUI.Label(RectSnap.Snap(chevronRect), "▾", chevronStyle);
        GUI.color = saved;
    }
}
