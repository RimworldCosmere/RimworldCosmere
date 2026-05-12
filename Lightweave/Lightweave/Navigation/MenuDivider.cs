using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "menu-divider",
    Summary = "Standalone horizontal divider for use inside custom menu-style stacks.",
    WhenToUse = "Group MenuItems in a custom popover, drawer, or sidebar.",
    SourcePath = "Lightweave/Lightweave/Navigation/MenuDivider.cs"
)]
public static class MenuDivider {
    public static LightweaveNode Create(
        [DocParam("Vertical padding (in rems) above and below the rule.")]
        float padRem = 0.25f,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("MenuDivider", line, file);
        node.ApplyStyling("menu-divider", style, classes, id);
        float padPx = new Rem(padRem).ToPixels();
        float thickness = Mathf.Max(1f, new Rem(1f / 16f).ToPixels());
        node.PreferredHeight = padPx * 2f + thickness;

        node.Paint = (rect, paintChildren) => {
            if (Event.current.type != EventType.Repaint) {
                paintChildren();
                return;
            }
            Theme.Theme theme = RenderContext.Current.Theme;
            float midY = rect.y + rect.height / 2f - thickness / 2f;
            Rect line = new Rect(rect.x, midY, rect.width, thickness);
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
            GUI.DrawTexture(RectSnap.Snap(line), Texture2D.whiteTexture);
            GUI.color = saved;
            paintChildren();
        };
        return node;
    }

    [DocVariant("CL_Playground_Navigation_MenuDivider_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => Stack.Create(
            SpacingScale.None,
            s => {
                s.Add(MenuItem.Create("Open settings", onClick: () => { }));
                s.Add(MenuDivider.Create());
                s.Add(MenuItem.Create("Quit to desktop", onClick: () => { }, danger: true));
            }
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Stack.Create(
            SpacingScale.None,
            s => {
                s.Add(MenuItem.Create("Cut", onClick: () => { }));
                s.Add(MenuItem.Create("Copy", onClick: () => { }));
                s.Add(MenuDivider.Create());
                s.Add(MenuItem.Create("Delete", onClick: () => { }, danger: true));
            }
        ));
    }
}
