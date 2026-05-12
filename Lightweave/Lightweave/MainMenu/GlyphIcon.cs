using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.MainMenu;

public static class GlyphIcon {
    public static LightweaveNode Create(string glyph, Rem? sizeOverride = null) {
        LightweaveNode node = NodeBuilder.New($"GlyphIcon:{glyph}");
        Rem size = sizeOverride ?? new Rem(1f);
        float pxSize = size.ToPixels();
        node.PreferredHeight = pxSize;
        node.MeasureWidth = () => pxSize;

        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Body);
            int px = Mathf.RoundToInt(new Rem(0.8125f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleCenter;
            style.clipping = TextClipping.Overflow;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(rect), glyph, style);
            GUI.color = saved;
        };
        return node;
    }
}
