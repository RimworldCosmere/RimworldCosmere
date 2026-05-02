using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Typography;

public static partial class Typography {
    [Doc(
        Id = "icon",
        Summary = "Themed bitmap icon scaled to fit and tinted by a theme color.",
        WhenToUse = "Inline glyphs in buttons, list rows, and toolbars.",
        SourcePath = "Lightweave/Lightweave/Typography/Icon.cs"
    )]
    public static class Icon {
        public static LightweaveNode Create(
            [DocParam("Source texture. Use a stuff-tintable grayscale asset for theme-driven coloring.")]
            Texture texture,
            [DocParam("Square size in Rem. Defaults to 1.5rem.")]
            Rem? size = null,
            [DocParam("Color reference. Defaults to white.")]
            ColorRef? color = null,
            [DocParam("Mirror horizontally when the layout direction is RTL.")]
            bool mirrorInRtl = false,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = ""
        ) {
            LightweaveNode node = NodeBuilder.New("Icon", line, file);
            node.PreferredHeight = (size ?? new Rem(1.5f)).ToPixels();
            node.Paint = (rect, _) => {
                Theme.Theme theme = RenderContext.Current.Theme;
                float px = (size ?? new Rem(1.5f)).ToPixels();
                float drawPx = Mathf.Min(px, Mathf.Min(rect.width, rect.height));
                Rect r = new Rect(
                    rect.x + (rect.width - drawPx) / 2f,
                    rect.y + (rect.height - drawPx) / 2f,
                    drawPx,
                    drawPx
                );
                Color c = color switch {
                    ColorRef.Literal lit => lit.Value,
                    ColorRef.Token tok => theme.GetColor(tok.Slot),
                    _ => Color.white,
                };
                Matrix4x4 saved = default;
                bool pushed = false;
                if (mirrorInRtl) {
                    saved = IconMirror.PushIfRtl(r, RenderContext.Current.Direction);
                    pushed = true;
                }

                Color savedColor = GUI.color;
                GUI.color = c;
                GUI.DrawTexture(RectSnap.Snap(r), texture, ScaleMode.ScaleToFit);
                GUI.color = savedColor;
                if (pushed) {
                    IconMirror.Pop(saved);
                }
            };
            return node;
        }

        [DocVariant("CC_Playground_Label_Default")]
        public static DocSample DocsDefault() {
            return new DocSample(Icon.Create(Texture2D.whiteTexture, new Rem(1.5f), ThemeSlot.TextPrimary));
        }

        [DocVariant("CC_Playground_Label_Accent")]
        public static DocSample DocsAccent() {
            return new DocSample(Icon.Create(Texture2D.whiteTexture, new Rem(1.5f), ThemeSlot.SurfaceAccent));
        }

        [DocVariant("CC_Playground_Label_Muted")]
        public static DocSample DocsMuted() {
            return new DocSample(Icon.Create(Texture2D.whiteTexture, new Rem(1.5f), ThemeSlot.TextMuted));
        }

        [DocUsage]
        public static DocSample DocsUsage() {
            return new DocSample(Icon.Create(Texture2D.whiteTexture, new Rem(1.5f), ThemeSlot.TextPrimary));
        }
    }
}
