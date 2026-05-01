using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Playground.PlaygroundChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "text",
        Summary = "Single-line or wrapped text rendered with theme font and color.",
        WhenToUse = "Body copy, inline labels, or any text content. The foundation for Heading, Label, Caption, and Code.",
        SourcePath = "CosmereCore/CosmereCore/Lightweave/Typography/Text.cs"
    )]
    public static class Text {
        public static LightweaveNode Create(
            [DocParam("Text content to display.")]
            string content,
            [DocParam("Font role or literal font reference. Defaults to Body.")]
            FontRef? font = null,
            [DocParam("Font size in Rem units. Defaults to 1rem.")]
            Rem? size = null,
            [DocParam("Color reference. Defaults to TextPrimary.")]
            ColorRef? color = null,
            [DocParam("Horizontal alignment within the rect.")]
            TextAlign align = TextAlign.Start,
            [DocParam("Font style: Normal, Bold, Italic, BoldAndItalic.")]
            FontStyle weight = FontStyle.Normal,
            [DocParam("Wrap to multiple lines when content exceeds available width.")]
            bool wrap = false,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New($"Text:{content}", line, file);

            GUIStyle ResolveStyle() {
                Theme.Theme theme = RenderContext.Current.Theme;
                Font f = font switch {
                    FontRef.Literal lit => lit.Value,
                    FontRef.Role role => theme.GetFont(role.RoleValue),
                    _ => theme.GetFont(FontRole.Body),
                };
                int pixelSize = Mathf.RoundToInt((size ?? new Rem(1f)).ToFontPx());
                GUIStyle style = GuiStyleCache.Get(f, pixelSize, weight);
                style.wordWrap = wrap;
                return style;
            }

            node.Measure = availableWidth => {
                if (string.IsNullOrEmpty(content)) {
                    return 0f;
                }

                GUIStyle style = ResolveStyle();
                GUIContent guiContent = new GUIContent(content);
                if (wrap) {
                    return style.CalcHeight(guiContent, availableWidth);
                }

                return style.CalcSize(guiContent).y;
            };

            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                GUIStyle style = ResolveStyle();
                TextAnchor anchor = ResolveAnchor(align, RenderContext.Current.Direction);
                if (wrap) {
                    anchor = anchor switch {
                        TextAnchor.MiddleLeft => TextAnchor.UpperLeft,
                        TextAnchor.MiddleRight => TextAnchor.UpperRight,
                        TextAnchor.MiddleCenter => TextAnchor.UpperCenter,
                        _ => anchor,
                    };
                }

                style.alignment = anchor;
                style.clipping = TextClipping.Clip;
                Color c = color switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(ThemeSlot.TextPrimary),
                };
                Color saved = GUI.color;
                GUI.color = c;
                GUI.Label(RectSnap.Snap(rect), content, style);
                GUI.color = saved;
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Normal")]
        public static DocSample DocsNormal() {
            string sample = (string)"CC_Playground_Text_Sample".Translate();
            return new DocSample(Text.Create(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary));
        }

        [DocVariant("CC_Playground_Label_Accented")]
        public static DocSample DocsAccented() {
            string sample = (string)"CC_Playground_Text_Sample".Translate();
            return new DocSample(
                Text.Create(
                    sample,
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.SurfaceAccent,
                    TextAlign.Start,
                    FontStyle.Bold
                )
            );
        }

        [DocVariant("CC_Playground_Label_Muted")]
        public static DocSample DocsMuted() {
            string sample = (string)"CC_Playground_Text_Sample".Translate();
            return new DocSample(Text.Create(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted));
        }

        [DocState("CC_Playground_Label_Default")]
        public static DocSample DocsDefaultState() {
            string sample = (string)"CC_Playground_Text_Sample".Translate();
            return new DocSample(Text.Create(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary));
        }

        [DocState("CC_Playground_Label_Muted")]
        public static DocSample DocsMutedState() {
            string sample = (string)"CC_Playground_Text_Sample".Translate();
            return new DocSample(Text.Create(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(
                Text.Create("Stormlight burns within him.", FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary)
            );
        }
    }
}
