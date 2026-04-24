using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed record PlaygroundCategory(
    string Id,
    string LabelKey,
    string DescriptionKey,
    IReadOnlyList<string> PrimitiveIds
);

public static class PlaygroundRail {
    private const float RowHeight = 38f;
    private const float HighlightBarWidth = 3f;
    private const float RowPaddingX = 10f;
    private const float CategoryGap = 2f;

    public static LightweaveNode Create(
        IReadOnlyList<PlaygroundCategory> categories,
        Hooks.Hooks.StateHandle<string> selectedCategoryId,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("PlaygroundRail", line, file);

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;
            Event e = Event.current;

            Color saved = GUI.color;

            float padX = SpacingScale.Sm.ToPixels();
            float padY = SpacingScale.Sm.ToPixels();
            float contentX = rect.x + padX;
            float contentWidth = rect.width - padX * 2f;
            float cursorY = rect.y + padY;

            for (int i = 0; i < categories.Count; i++) {
                PlaygroundCategory category = categories[i];
                Rect rowRect = new Rect(contentX, cursorY, contentWidth, RowHeight);

                bool selected = category.Id == selectedCategoryId.Value;
                bool hovering = rowRect.Contains(e.mousePosition);

                if (selected) {
                    Color selectedBg = theme.GetColor(ThemeSlot.SurfaceAccent);
                    selectedBg.a = 0.18f;
                    GUI.color = selectedBg;
                    GUI.DrawTexture(rowRect, Texture2D.whiteTexture);
                    GUI.color = saved;

                    Color focusBar = theme.GetColor(ThemeSlot.BorderFocus);
                    float barX = rtl ? rowRect.xMax - HighlightBarWidth : rowRect.x;
                    Rect bar = new Rect(barX, rowRect.y, HighlightBarWidth, rowRect.height);
                    GUI.color = focusBar;
                    GUI.DrawTexture(bar, Texture2D.whiteTexture);
                    GUI.color = saved;
                } else if (hovering) {
                    PaintBox.DrawHighlight(rowRect, RadiusSpec.All(new Rem(0.25f)), true);
                }

                float labelInsetStart = rtl ? padX : HighlightBarWidth + RowPaddingX;
                float labelInsetEnd = rtl ? HighlightBarWidth + RowPaddingX : padX;
                Rect labelRect = new Rect(
                    rowRect.x + labelInsetStart,
                    rowRect.y,
                    rowRect.width - labelInsetStart - labelInsetEnd,
                    rowRect.height
                );

                Font font = theme.GetFont(selected ? FontRole.BodyBold : FontRole.Body);
                int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
                GUIStyle style = GuiStyleCache.Get(font, pixelSize, selected ? FontStyle.Bold : FontStyle.Normal);
                style.alignment = rtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

                ThemeSlot textSlot = selected ? ThemeSlot.TextPrimary : ThemeSlot.TextSecondary;
                GUI.color = theme.GetColor(textSlot);
                string labelText = category.LabelKey.Translate();
                GUI.Label(RectSnap.Snap(labelRect), labelText, style);
                GUI.color = saved;

                TooltipHandler.TipRegion(rowRect, (string)category.DescriptionKey.Translate());

                if (!selected && e.type == EventType.MouseUp && e.button == 0 && rowRect.Contains(e.mousePosition)) {
                    selectedCategoryId.Set(category.Id);
                    e.Use();
                }

                cursorY += RowHeight + CategoryGap;
            }
        };

        return node;
    }
}