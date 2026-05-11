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
        Id = "richtext",
        Summary = "Wrapped text that honors Unity rich-text tags (<b>, <i>, <color=...>).",
        WhenToUse = "Body copy that needs inline emphasis or color without composing multiple Text nodes.",
        SourcePath = "Lightweave/Lightweave/Typography/RichText.cs",
        ShowRtl = true
    )]
    public static class RichText {
        public static LightweaveNode Create(
            [DocParam("Tagged text. Resolved at paint time. Supports rich-text markup.")]
            TaggedString content,
            [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
            Style? style = null,
            [DocParam("Additional class names merged after the base 'rich-text' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
            string[]? classes = null,
            [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
            string? id = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("RichText", line, file);
            node.ApplyStyling("rich-text", style, classes, id);

            GUIStyle ResolveStyle() {
                Theme.Theme theme = RenderContext.Current.Theme;
                Style s = node.GetResolvedStyle();
                FontRef? fr = s.FontFamily;
                Font font = fr switch {
                    FontRef.Literal lit => lit.Value,
                    FontRef.Role role => theme.GetFont(role.RoleValue),
                    _ => theme.GetFont(FontRole.Body),
                };
                Rem fontSize = s.FontSize ?? new Rem(1f);
                int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
                GUIStyle gs = GuiStyleCache.GetOrCreate(font, pixelSize);
                gs.richText = true;
                gs.wordWrap = true;
                return gs;
            }

            node.Measure = availableWidth => {
                string resolved = content.Resolve();
                if (string.IsNullOrEmpty(resolved)) {
                    return 0f;
                }

                GUIStyle gs = ResolveStyle();
                return gs.CalcHeight(new GUIContent(resolved), availableWidth);
            };

            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                Style s = node.GetResolvedStyle();
                GUIStyle gs = ResolveStyle();
                gs.clipping = TextClipping.Clip;
                TextAlign align = s.TextAlign ?? TextAlign.Start;
                gs.alignment = ResolveAnchor(align, RenderContext.Current.Direction);
                ColorRef? cr = s.TextColor;
                Color c = cr switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => theme.GetColor(ThemeSlot.TextPrimary),
                };
                Color saved = GUI.color;
                GUI.color = c;
                GUI.Label(RectSnap.Snap(rect), content.Resolve(), gs);
                GUI.color = saved;
            };
            return node;
        }

        [DocVariant("CL_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(() => RichText.Create(new TaggedString((string)"CL_Playground_RichText_Sample".Translate())));
        }

        [DocVariant("CL_Playground_Label_Bold")]
        public static DocSample DocsBold() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("Honor lies in <b>keeping</b> your word, even when it costs you everything.")
                )
            );
        }

        [DocVariant("CL_Playground_Label_Italic")]
        public static DocSample DocsItalic() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("<i>Life before death. Strength before weakness. Journey before destination.</i>")
                )
            );
        }

        [DocVariant("CL_Playground_Label_Accent")]
        public static DocSample DocsAccent() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("Stormlight burns <color=#bba36a>brilliant</color> in his veins.")
                )
            );
        }

        [DocVariant("CL_Playground_Label_Mixed")]
        public static DocSample DocsMixed() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString(
                        "<b><color=#bba36a>Adolin</color></b> raised <i>Mayalaran</i>, the dead Blade he had sworn to honor."
                    )
                )
            );
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(() => 
                RichText.Create(new TaggedString("Honor lies in <b>keeping</b> your word."))
            );
        }
    }
}
