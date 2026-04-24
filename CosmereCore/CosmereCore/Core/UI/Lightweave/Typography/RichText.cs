using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Typography;

public static partial class Typography {
    public static LightweaveNode RichText(
        TaggedString content,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("RichText", line, file);

        GUIStyle ResolveStyle() {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font f = theme.GetFont(FontRole.Body);
            GUIStyle style = GuiStyleCache.Get(f, 16);
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
}