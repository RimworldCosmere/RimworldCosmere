using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Cosmere.Lightweave.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "text",
        Summary = "Single-line or wrapped text rendered with theme font and color.",
        WhenToUse = "Body copy, inline labels, or any text content. The foundation for Heading, Label, Caption, and Code.",
        SourcePath = "Lightweave/Lightweave/Typography/Text.cs",
        ShowRtl = true
    )]
    public static class Text {
        public static LightweaveNode Create(
            [DocParam("Text content to display.")]
            string content,
            [DocParam("Wrap to multiple lines when content exceeds available width.")]
            bool wrap = false,
            [DocParam("Style applied to the text (FontFamily/FontSize/TextColor/TextAlign/FontWeight/etc).", TypeOverride = "Style?", DefaultOverride = "null")]
            Style? style = null,
            [DocParam("Additional class names merged after the base 'text' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
            string[]? classes = null,
            [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
            string? id = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New($"Text:{content}", line, file);
            node.ApplyStyling("text", style, classes, id);

            GUIStyle ResolveGuiStyle() {
                Style s = node.GetResolvedStyle();
                Theme.Theme theme = RenderContext.Current.Theme;
                FontRef? fr = s.FontFamily;
                Font f = fr switch {
                    FontRef.Literal lit => lit.Value,
                    FontRef.Role role => theme.GetFont(role.RoleValue),
                    _ => theme.GetFont(FontRole.Body),
                };
                Rem fontSize = s.FontSize ?? new Rem(1f);
                FontStyle weight = s.FontWeight ?? FontStyle.Normal;
                int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
                GUIStyle guiStyle = GuiStyleCache.GetOrCreate(f, pixelSize, weight);
                guiStyle.wordWrap = wrap;
                return guiStyle;
            }

            node.Measure = availableWidth => {
                if (string.IsNullOrEmpty(content)) {
                    return 0f;
                }

                GUIStyle gs = ResolveGuiStyle();
                GUIContent guiContent = new GUIContent(content);
                float h = wrap
                    ? gs.CalcHeight(guiContent, availableWidth)
                    : gs.CalcHeight(guiContent, float.MaxValue);
                Style s = node.GetResolvedStyle();
                Rem fontSize = s.FontSize ?? new Rem(1f);
                int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
                float descenderPad = Mathf.Max(2f, pixelSize * 0.25f);
                return Mathf.Ceil(h + descenderPad);
            };

            node.MeasureWidth = () => {
                if (string.IsNullOrEmpty(content)) {
                    return 0f;
                }

                GUIStyle gs = ResolveGuiStyle();
                return Mathf.Ceil(gs.CalcSize(new GUIContent(content)).x);
            };

            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                Style s = node.GetResolvedStyle();
                GUIStyle gs = ResolveGuiStyle();
                TextAlign align = s.TextAlign ?? TextAlign.Start;
                TextAnchor anchor = ResolveAnchor(align, RenderContext.Current.Direction);
                if (wrap) {
                    anchor = anchor switch {
                        TextAnchor.MiddleLeft => TextAnchor.UpperLeft,
                        TextAnchor.MiddleRight => TextAnchor.UpperRight,
                        TextAnchor.MiddleCenter => TextAnchor.UpperCenter,
                        _ => anchor,
                    };
                }

                gs.alignment = anchor;
                gs.clipping = TextClipping.Clip;
                ColorRef? cr = s.TextColor;
                Color c = cr switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(ThemeSlot.TextPrimary),
                };
                Color saved = GUI.color;
                GUI.color = c;
                GUI.Label(RectSnap.Snap(rect), content, gs);
                GUI.color = saved;
            };
            return node;
        }

        [DocVariant("CL_Playground_Label_Normal")]
        public static DocSample DocsNormal() {
            string sample = (string)"CL_Playground_Text_Sample".Translate();
            return new DocSample(() => Text.Create(sample, style: new Style { FontSize = new Rem(0.9375f) }));
        }

        [DocVariant("CL_Playground_Label_Accented")]
        public static DocSample DocsAccented() {
            string sample = (string)"CL_Playground_Text_Sample".Translate();
            return new DocSample(() =>
                Text.Create(
                    sample,
                    style: new Style {
                        FontSize = new Rem(0.9375f),
                        TextColor = ThemeSlot.SurfaceAccent,
                        FontWeight = FontStyle.Bold,
                    }
                )
            );
        }

        [DocVariant("CL_Playground_Label_Muted")]
        public static DocSample DocsMuted() {
            string sample = (string)"CL_Playground_Text_Sample".Translate();
            return new DocSample(() => Text.Create(
                sample,
                style: new Style { FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextMuted }
            ));
        }

        [DocState("CL_Playground_Label_Default")]
        public static DocSample DocsDefaultState() {
            string sample = (string)"CL_Playground_Text_Sample".Translate();
            return new DocSample(() => Text.Create(sample, style: new Style { FontSize = new Rem(0.9375f) }));
        }

        [DocState("CL_Playground_Label_Muted")]
        public static DocSample DocsMutedState() {
            string sample = (string)"CL_Playground_Text_Sample".Translate();
            return new DocSample(() => Text.Create(
                sample,
                style: new Style { FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextMuted }
            ));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() =>
                Text.Create("Stormlight burns within him.", style: new Style { FontSize = new Rem(0.9375f) })
            );
        }
    }
}
