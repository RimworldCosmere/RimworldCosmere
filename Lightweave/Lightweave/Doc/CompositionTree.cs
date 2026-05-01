using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Doc;

public static partial class Doc {
    public static LightweaveNode CompositionTree(
        IReadOnlyList<CompositionLine> lines,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Doc.CompositionTree", line, file);

        float fontPx = new Rem(0.8125f).ToFontPx();
        float rowHeightPx = new Rem(1.5f).ToPixels();
        float indentUnitPx = new Rem(1f).ToPixels();
        float padPx = new Rem(0.75f).ToPixels();

        node.Measure = _ => {
            int n = lines?.Count ?? 0;
            return padPx * 2f + n * rowHeightPx;
        };

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            BackgroundSpec.Solid bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle);
            RadiusSpec rad = RadiusSpec.All(new Rem(0.375f));
            PaintBox.Draw(rect, bg, border, rad);

            if (lines == null || lines.Count == 0) {
                return;
            }

            Font mono = theme.GetFont(FontRole.Mono);
            GUIStyle style = GuiStyleCache.Get(mono, Mathf.RoundToInt(fontPx));
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;

            Color saved = GUI.color;
            float y = rect.y + padPx;
            for (int i = 0; i < lines.Count; i++) {
                CompositionLine entry = lines[i];
                float indent = entry.Indent * indentUnitPx;
                Rect row = new Rect(
                    rect.x + padPx + indent,
                    y,
                    rect.width - padPx * 2f - indent,
                    rowHeightPx
                );

                ThemeSlot slot = entry.Indent == 0 ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted;
                GUI.color = theme.GetColor(slot);
                GUI.Label(RectSnap.Snap(row), entry.Text, style);
                y += rowHeightPx;
            }

            GUI.color = saved;
        };

        return node;
    }
}
