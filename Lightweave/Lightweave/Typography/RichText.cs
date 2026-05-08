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
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("RichText", line, file);

            GUIStyle ResolveStyle() {
                Theme.Theme theme = RenderContext.Current.Theme;
                Font f = theme.GetFont(FontRole.Body);
                GUIStyle style = GuiStyleCache.GetOrCreate(f, 16);
                style.richText = true;
                style.wordWrap = true;
                return style;
            }

            node.Measure = availableWidth => {
                string resolved = content.Resolve();
                if (string.IsNullOrEmpty(resolved)) {
                    return 0f;
                }

                GUIStyle style = ResolveStyle();
                return style.CalcHeight(new GUIContent(resolved), availableWidth);
            };

            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                GUIStyle style = ResolveStyle();
                style.clipping = TextClipping.Clip;
                style.alignment = ResolveAnchor(TextAlign.Start, RenderContext.Current.Direction);
                Color saved = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.Label(RectSnap.Snap(rect), content.Resolve(), style);
                GUI.color = saved;
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(() => RichText.Create(new TaggedString((string)"CC_Playground_RichText_Sample".Translate())));
        }

        [DocVariant("CC_Playground_Label_Bold")]
        public static DocSample DocsBold() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("Honor lies in <b>keeping</b> your word, even when it costs you everything.")
                )
            );
        }

        [DocVariant("CC_Playground_Label_Italic")]
        public static DocSample DocsItalic() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("<i>Life before death. Strength before weakness. Journey before destination.</i>")
                )
            );
        }

        [DocVariant("CC_Playground_Label_Accent")]
        public static DocSample DocsAccent() {
            return new DocSample(() => 
                RichText.Create(
                    new TaggedString("Stormlight burns <color=#bba36a>brilliant</color> in his veins.")
                )
            );
        }

        [DocVariant("CC_Playground_Label_Mixed")]
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
                RichText.Create(
                    new TaggedString("Honor lies in <b>keeping</b> your word.")
                )
            );
        }
    }
}
