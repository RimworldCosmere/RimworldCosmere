using System;
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
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'display'/'display-{level}' classes.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        string sizeClass = level switch {
            1 => "display-1",
            2 => "display-2",
            3 => "display-3",
            _ => "display-4",
        };
        string[] basedClasses = classes == null
            ? new[] { "display", sizeClass }
            : ConcatClasses(new[] { "display", sizeClass }, classes);

        Tracking? styleTracking = style?.LetterSpacing;
        if (!styleTracking.HasValue || Mathf.Approximately(styleTracking.Value.Em, 0f)) {
            return Text.Create(content, style: style, classes: basedClasses, id: id, line: line, file: file);
        }

        LightweaveNode node = NodeBuilder.New($"Display:{content}", line, file);
        node.Classes = basedClasses;
        if (style.HasValue) {
            node.Style = style.Value;
        }
        if (id != null) {
            node.Id = id;
        }

        int ResolveLetterSpacing() {
            Style s = node.GetResolvedStyle();
            Tracking? t = s.LetterSpacing;
            if (!t.HasValue) {
                return 0;
            }
            Rem fontSize = s.FontSize ?? new Rem(2f);
            return Mathf.Max(0, Mathf.RoundToInt(t.Value.ToPixels(fontSize.ToFontPx())));
        }

        GUIStyle ResolveGuiStyle() {
            Theme.Theme theme = RenderContext.Current.Theme;
            Style s = node.GetResolvedStyle();
            FontRef? fr = s.FontFamily;
            Font font = fr switch {
                FontRef.Literal lit => lit.Value,
                FontRef.Role role => theme.GetFont(role.RoleValue),
                _ => theme.GetFont(FontRole.Display),
            };
            Rem fontSize = s.FontSize ?? new Rem(2f);
            FontStyle weight = s.FontWeight ?? FontStyle.Bold;
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            return GuiStyleCache.GetOrCreate(font, pixelSize, weight);
        }

        int[] MeasureCharWidths(GUIStyle gs) {
            int[] widths = new int[content.Length];
            for (int i = 0; i < content.Length; i++) {
                GUIContent gc = new GUIContent(content[i].ToString());
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
            if (string.IsNullOrEmpty(content)) {
                return 0f;
            }
            GUIStyle gs = ResolveGuiStyle();
            Style s = node.GetResolvedStyle();
            Rem fontSize = s.FontSize ?? new Rem(2f);
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            float descenderPad = Mathf.Max(2f, pixelSize * 0.25f);
            GUIContent gc = new GUIContent(content);
            return Mathf.Ceil(gs.CalcHeight(gc, float.MaxValue) + descenderPad);
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(content)) {
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
                _ => theme.GetColor(ThemeSlot.TextPrimary),
            };
            Color saved = GUI.color;
            GUI.color = c;
            gs.alignment = TextAnchor.MiddleLeft;
            gs.clipping = TextClipping.Overflow;

            int cursor = startX;
            for (int i = 0; i < content.Length; i++) {
                string ch = content[i].ToString();
                GUI.Label(new Rect(cursor, y, widths[i], h), ch, gs);
                cursor += widths[i] + letterSpacing;
            }
            GUI.color = saved;
        };
        return node;
    }

    private static string[] ConcatClasses(string[] head, string[] tail) {
        string[] result = new string[head.Length + tail.Length];
        Array.Copy(head, 0, result, 0, head.Length);
        Array.Copy(tail, 0, result, head.Length, tail.Length);
        return result;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => Display.Create("RIMWORLD"));
    }

    [DocVariant("CL_Playground_Label_Tracked")]
    public static DocSample DocsTracked() {
        return new DocSample(() => Display.Create("RIMW•RLD", style: new Style { TextAlign = TextAlign.Center, LetterSpacing = Tracking.Of(0.2f) }, level: 1));
    }

    [DocVariant("CL_Playground_Label_Small")]
    public static DocSample DocsSmall() {
        return new DocSample(() => Display.Create("Stormlight", level: 3));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Display.Create("RIMW•RLD", style: new Style { TextAlign = TextAlign.Center, LetterSpacing = Tracking.Of(0.2f) }));
    }
}
