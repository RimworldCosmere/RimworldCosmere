using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

[Doc(
    Id = "display",
    Summary = "Oversized display text rendered with the theme display font. Supports letter-spacing for wordmarks.",
    WhenToUse = "Wordmarks, hero titles, and other top-of-hierarchy moments. Use sparingly — one per surface.",
    SourcePath = "Lightweave/Lightweave/Typography/Display.cs",
    ShowRtl = false
)]
public static class Display {
    public static LightweaveNode Create(
        [DocParam("Display text content.")]
        string content,
        [DocParam("Display level. 1 is largest; higher levels step down.")]
        int level = 1,
        [DocParam("Letter spacing in pixels. Positive tracks wider, negative tighter. Zero (default) uses native rendering.")]
        float letterSpacing = 0f,
        [DocParam("Override color. Defaults to TextPrimary.")]
        ColorRef? color = null,
        [DocParam("Horizontal alignment within the rect.")]
        TextAlign align = TextAlign.Start,
        [DocParam("Font style. Defaults to Bold.")]
        FontStyle weight = FontStyle.Bold,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem size = level switch {
            1 => new Rem(4.0f),
            2 => new Rem(3.0f),
            3 => new Rem(2.25f),
            _ => new Rem(1.75f),
        };

        if (Mathf.Approximately(letterSpacing, 0f)) {
            return Text.Create(
                content,
                FontRole.Display,
                size,
                color,
                align,
                weight,
                line: line,
                file: file
            );
        }

        LightweaveNode node = NodeBuilder.New($"Display:{content}", line, file);
        int pixelSize = Mathf.RoundToInt(size.ToFontPx());
        float descenderPad = Mathf.Max(2f, pixelSize * 0.25f);

        GUIStyle ResolveStyle() {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Display);
            return GuiStyleCache.GetOrCreate(font, pixelSize, weight);
        }

        float MeasureTotalWidth(GUIStyle style) {
            float total = 0f;
            for (int i = 0; i < content.Length; i++) {
                GUIContent gc = new GUIContent(content[i].ToString());
                total += style.CalcSize(gc).x;
                if (i < content.Length - 1) {
                    total += letterSpacing;
                }
            }
            return total;
        }

        node.Measure = _ => {
            if (string.IsNullOrEmpty(content)) {
                return 0f;
            }
            GUIStyle style = ResolveStyle();
            GUIContent gc = new GUIContent(content);
            return Mathf.Ceil(style.CalcHeight(gc, float.MaxValue) + descenderPad);
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(content)) {
                return;
            }
            Theme.Theme theme = RenderContext.Current.Theme;
            GUIStyle style = ResolveStyle();
            float totalW = MeasureTotalWidth(style);
            TextAnchor anchor = ResolveAnchor(align, RenderContext.Current.Direction);
            float startX = anchor switch {
                TextAnchor.MiddleCenter or TextAnchor.UpperCenter or TextAnchor.LowerCenter
                    => rect.x + (rect.width - totalW) * 0.5f,
                TextAnchor.MiddleRight or TextAnchor.UpperRight or TextAnchor.LowerRight
                    => rect.xMax - totalW,
                _ => rect.x,
            };
            Color c = color switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextPrimary),
            };
            Color saved = GUI.color;
            GUI.color = c;
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;

            float cursor = startX;
            for (int i = 0; i < content.Length; i++) {
                string ch = content[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float charW = style.CalcSize(gc).x;
                Rect charRect = new Rect(cursor, rect.y, charW, rect.height);
                GUI.Label(RectSnap.Snap(charRect), ch, style);
                cursor += charW + letterSpacing;
            }
            GUI.color = saved;
        };
        return node;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => Display.Create("RIMWORLD"));
    }

    [DocVariant("CL_Playground_Label_Tracked")]
    public static DocSample DocsTracked() {
        return new DocSample(() => Display.Create("RIMW•RLD", level: 1, letterSpacing: 12f, align: TextAlign.Center));
    }

    [DocVariant("CL_Playground_Label_Small")]
    public static DocSample DocsSmall() {
        return new DocSample(() => Display.Create("Stormlight", level: 3));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Display.Create("RIMW•RLD", letterSpacing: 12f, align: TextAlign.Center));
    }
}
