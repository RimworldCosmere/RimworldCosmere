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
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'eyebrow' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string upper = content?.ToUpperInvariant() ?? string.Empty;

        Tracking? styleTracking = style?.LetterSpacing;
        if (!styleTracking.HasValue || Mathf.Approximately(styleTracking.Value.Em, 0f)) {
            return Text.Create(
                upper,
                style: style,
                classes: StyleExtensions.PrependClass("eyebrow", classes),
                id: id,
                line: line,
                file: file
            );
        }

        LightweaveNode node = NodeBuilder.New($"Eyebrow:{upper}", line, file);
        node.ApplyStyling("eyebrow", style, classes, id);

        int ResolveLetterSpacing() {
            Style s = node.GetResolvedStyle();
            Tracking? t = s.LetterSpacing;
            if (!t.HasValue) {
                return 0;
            }
            Rem fontSize = s.FontSize ?? new Rem(0.75f);
            return Mathf.Max(0, Mathf.RoundToInt(t.Value.ToPixels(fontSize.ToFontPx())));
        }

        GUIStyle ResolveGuiStyle() {
            Theme.Theme theme = RenderContext.Current.Theme;
            Style s = node.GetResolvedStyle();
            FontRef? fr = s.FontFamily;
            Font font = fr switch {
                FontRef.Literal lit => lit.Value,
                FontRef.Role role => theme.GetFont(role.RoleValue),
                _ => theme.GetFont(FontRole.Body),
            };
            Rem fontSize = s.FontSize ?? new Rem(0.75f);
            FontStyle weight = s.FontWeight ?? FontStyle.Normal;
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            return GuiStyleCache.GetOrCreate(font, pixelSize, weight);
        }

        int[] MeasureCharWidths(GUIStyle gs) {
            int[] widths = new int[upper.Length];
            for (int i = 0; i < upper.Length; i++) {
                GUIContent gc = new GUIContent(upper[i].ToString());
                widths[i] = Mathf.CeilToInt(gs.CalcSize(gc).x);
            }
            return widths;
        }

        int MeasureTotalWidth(int[] widths, int letterSpacing) {
            int total = 0;
            for (int i = 0; i < widths.Length; i++) {
                total += widths[i];
                if (i < widths.Length - 1) {
                    total += letterSpacing;
                }
            }
            return total;
        }

        node.Measure = _ => {
            if (string.IsNullOrEmpty(upper)) {
                return 0f;
            }
            GUIStyle gs = ResolveGuiStyle();
            Style s = node.GetResolvedStyle();
            Rem fontSize = s.FontSize ?? new Rem(0.75f);
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            float descenderPad = Mathf.Max(2f, pixelSize * 0.25f);
            GUIContent gc = new GUIContent(upper);
            return Mathf.Ceil(gs.CalcHeight(gc, float.MaxValue) + descenderPad);
        };

        node.MeasureWidth = () => {
            if (string.IsNullOrEmpty(upper)) {
                return 0f;
            }
            GUIStyle gs = ResolveGuiStyle();
            int letterSpacing = ResolveLetterSpacing();
            int[] widths = MeasureCharWidths(gs);
            return MeasureTotalWidth(widths, letterSpacing);
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(upper)) {
                return;
            }
            Theme.Theme theme = RenderContext.Current.Theme;
            Style s = node.GetResolvedStyle();
            GUIStyle gs = ResolveGuiStyle();
            int letterSpacing = ResolveLetterSpacing();
            int[] widths = MeasureCharWidths(gs);
            int totalW = MeasureTotalWidth(widths, letterSpacing);
            TextAlign align = s.TextAlign ?? TextAlign.Start;
            TextAnchor anchor = ResolveAnchor(align, RenderContext.Current.Direction);
            int startX = anchor switch {
                TextAnchor.MiddleCenter or TextAnchor.UpperCenter or TextAnchor.LowerCenter
                    => Mathf.FloorToInt(rect.x + (rect.width - totalW) * 0.5f),
                TextAnchor.MiddleRight or TextAnchor.UpperRight or TextAnchor.LowerRight
                    => Mathf.FloorToInt(rect.xMax - totalW),
                _ => Mathf.FloorToInt(rect.x),
            };
            int y = Mathf.FloorToInt(rect.y);
            int h = Mathf.CeilToInt(rect.height);
            ColorRef? cr = s.TextColor;
            Color c = cr switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextMuted),
            };
            Color saved = GUI.color;
            GUI.color = c;
            gs.alignment = TextAnchor.MiddleLeft;
            gs.clipping = TextClipping.Overflow;

            int cursor = startX;
            for (int i = 0; i < upper.Length; i++) {
                string ch = upper[i].ToString();
                GUI.Label(new Rect(cursor, y, widths[i], h), ch, gs);
                cursor += widths[i] + letterSpacing;
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
        return new DocSample(() => Eyebrow.Create("framerate", style: new Style { TextColor = (ColorRef)ThemeSlot.SurfaceAccent }));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Eyebrow.Create("display"));
    }
}
