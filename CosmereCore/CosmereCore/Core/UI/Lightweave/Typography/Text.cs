using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography {
    public static LightweaveNode Text(
        string content,
        FontRef? font = null,
        Rem? size = null,
        ColorRef? color = null,
        TextAlign align = TextAlign.Start,
        FontStyle weight = FontStyle.Normal,
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
}