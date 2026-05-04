using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Cosmere.Lightweave.Layout;
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Navigation;

[Doc(
    Id = "contextmenu",
    Summary = "Right-click target that opens a Menu at the cursor.",
    WhenToUse = "Expose secondary actions on a region without consuming bar space.",
    SourcePath = "Lightweave/Lightweave/Navigation/ContextMenu.cs"
)]
public static class ContextMenu {
    public static LightweaveNode Create(
        [DocParam("Region that captures the right-click.")]
        LightweaveNode child,
        [DocParam("Menu rows to display when triggered.")]
        IReadOnlyList<MenuItem> items,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.StateHandle<bool> isOpen = Hooks.Hooks.UseState(false, line, file + "#open");
        Hooks.Hooks.StateHandle<Vector2> anchorPos = Hooks.Hooks.UseState(Vector2.zero, line, file + "#pos");

        LightweaveNode node = NodeBuilder.New("ContextMenu", line, file);
        node.Children.Add(child);
        if (child.Measure != null) {
            node.Measure = child.Measure;
        }
        else if (child.PreferredHeight.HasValue) {
            node.PreferredHeight = child.PreferredHeight.Value;
        }

        node.Paint = (rect, _) => {
            child.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(child, rect);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 1 && rect.Contains(e.mousePosition)) {
                anchorPos.Set(e.mousePosition);
                isOpen.Set(true);
                e.Use();
            }

            if (isOpen.Value) {
                Vector2 pos = anchorPos.Value;
                Rect anchorRect = new Rect(pos.x, pos.y, 0f, 0f);
                LightweaveNode menu = Menu.Create(
                    true,
                    anchorRect,
                    items,
                    () => isOpen.Set(false),
                    MenuAnchor.Left,
                    MenuDirection.Down,
                    "context"
                );
                menu.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(menu, rect);
            }
        };
        return node;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        List<MenuItem> items = new List<MenuItem> {
            Menu.Item((string)"CC_Playground_ContextMenu_Inspect".Translate(), () => { }),
            Menu.Item((string)"CC_Playground_ContextMenu_Rename".Translate(), () => { }),
            Menu.Item((string)"CC_Playground_ContextMenu_Duplicate".Translate(), () => { }),
            Menu.Divider(),
            Menu.Item((string)"CC_Playground_ContextMenu_Delete".Translate(), () => { }),
        };

        LightweaveNode target = Box.Create(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(
                Caption.Create(
                    (string)"CC_Playground_ContextMenu_RightClick".Translate()
                )
            )
        );

        return new DocSample(ContextMenu.Create(target, items));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        List<MenuItem> items = new List<MenuItem> {
            Menu.Item("Inspect", () => { }),
            Menu.Item("Rename", () => { }),
            Menu.Divider(),
            Menu.Item("Delete", () => { }),
        };

        LightweaveNode target = Box.Create(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Caption.Create("Right-click here"))
        );

        return new DocSample(ContextMenu.Create(target, items));
    }
}