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
    Id = "eyebrow",
    Summary = "Small uppercase tracked label used above titles or to mark sections.",
    WhenToUse = "Section eyebrows above headings, metadata-row labels, status group prefixes. Always uppercase.",
    SourcePath = "Lightweave/Lightweave/Typography/Eyebrow.cs",
    ShowRtl = false
)]
public static class Eyebrow {
    public static LightweaveNode Create(
        [DocParam("Eyebrow text. Will be rendered upper-cased.")]
        string content,
        [DocParam("Tracking (letter spacing) in pixels.")]
        float letterSpacing = 2f,
        [DocParam("Override color. Defaults to TextMuted.")]
        ColorRef? color = null,
        [DocParam("Horizontal alignment within the rect.")]
        TextAlign align = TextAlign.Start,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string upper = content?.ToUpperInvariant() ?? string.Empty;
        ColorRef resolvedColor = color ?? (ColorRef)ThemeSlot.TextMuted;
        Rem size = new Rem(0.75f);

        if (Mathf.Approximately(letterSpacing, 0f)) {
            return Text.Create(
                upper,
                FontRole.Body,
                size,
                resolvedColor,
                align,
                FontStyle.Bold,
                line: line,
                file: file
            );
        }

        LightweaveNode node = NodeBuilder.New($"Eyebrow:{upper}", line, file);
        int pixelSize = Mathf.RoundToInt(size.ToFontPx());
        float descenderPad = Mathf.Max(2f, pixelSize * 0.25f);

        GUIStyle ResolveStyle() {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Body);
            return GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
        }

        float MeasureTotalWidth(GUIStyle style) {
            float total = 0f;
            for (int i = 0; i < upper.Length; i++) {
                GUIContent gc = new GUIContent(upper[i].ToString());
                total += style.CalcSize(gc).x;
                if (i < upper.Length - 1) {
                    total += letterSpacing;
                }
            }
            return total;
        }

        node.Measure = _ => {
            if (string.IsNullOrEmpty(upper)) {
                return 0f;
            }
            GUIStyle style = ResolveStyle();
            GUIContent gc = new GUIContent(upper);
            return Mathf.Ceil(style.CalcHeight(gc, float.MaxValue) + descenderPad);
        };

        node.MeasureWidth = () => {
            if (string.IsNullOrEmpty(upper)) {
                return 0f;
            }
            GUIStyle style = ResolveStyle();
            return Mathf.Ceil(MeasureTotalWidth(style));
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(upper)) {
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
            Color c = resolvedColor switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextMuted),
            };
            Color saved = GUI.color;
            GUI.color = c;
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;

            float cursor = startX;
            for (int i = 0; i < upper.Length; i++) {
                string ch = upper[i].ToString();
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
        return new DocSample(() => Eyebrow.Create("section header"));
    }

    [DocVariant("CL_Playground_Label_Accented")]
    public static DocSample DocsAccented() {
        return new DocSample(() => Eyebrow.Create("framerate", color: (ColorRef)ThemeSlot.SurfaceAccent));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Eyebrow.Create("display"));
    }
}
