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

        if (Mathf.Approximately(letterSpacing, 0f)) {
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
            FontStyle weight = s.FontWeight ?? FontStyle.Bold;
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            return GuiStyleCache.GetOrCreate(font, pixelSize, weight);
        }

        float MeasureTotalWidth(GUIStyle gs) {
            float total = 0f;
            for (int i = 0; i < upper.Length; i++) {
                GUIContent gc = new GUIContent(upper[i].ToString());
                total += gs.CalcSize(gc).x;
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
            return Mathf.Ceil(MeasureTotalWidth(gs));
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(upper)) {
                return;
            }
            Theme.Theme theme = RenderContext.Current.Theme;
            Style s = node.GetResolvedStyle();
            GUIStyle gs = ResolveGuiStyle();
            float totalW = MeasureTotalWidth(gs);
            TextAlign align = s.TextAlign ?? TextAlign.Start;
            TextAnchor anchor = ResolveAnchor(align, RenderContext.Current.Direction);
            float startX = anchor switch {
                TextAnchor.MiddleCenter or TextAnchor.UpperCenter or TextAnchor.LowerCenter
                    => rect.x + (rect.width - totalW) * 0.5f,
                TextAnchor.MiddleRight or TextAnchor.UpperRight or TextAnchor.LowerRight
                    => rect.xMax - totalW,
                _ => rect.x,
            };
            ColorRef? cr = s.TextColor;
            Color c = cr switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextMuted),
            };
            Color saved = GUI.color;
            GUI.color = c;
            gs.alignment = TextAnchor.MiddleLeft;
            gs.clipping = TextClipping.Clip;

            float cursor = startX;
            for (int i = 0; i < upper.Length; i++) {
                string ch = upper[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float charW = gs.CalcSize(gc).x;
                Rect charRect = new Rect(cursor, rect.y, charW, rect.height);
                GUI.Label(RectSnap.Snap(charRect), ch, gs);
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
        return new DocSample(() => Eyebrow.Create("framerate", style: new Style { TextColor = (ColorRef)ThemeSlot.SurfaceAccent }));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Eyebrow.Create("display"));
    }
}
