using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;

namespace Cosmere.Lightweave.Playground;

[Doc(
    Id = "sidenav",
    Summary = "Vertical category rail with hover-expand and pinned selection.",
    WhenToUse = "Browse a flat list of categorized primitives or sections in a wide window.",
    SourcePath = "Lightweave/Lightweave/Playground/PlaygroundRail.cs",
    PreferredVariantHeight = 340f
)]
public static class SideNavDoc {
    private static IReadOnlyList<PlaygroundCategory> BuildDemoCategories() {
        return new List<PlaygroundCategory> {
            new PlaygroundCategory(
                "layout",
                "CC_Playground_Category_Layout",
                "CC_Playground_Category_Layout_Desc",
                new[] { "stack", "column", "row", "grid" }
            ),
            new PlaygroundCategory(
                "typography",
                "CC_Playground_Category_Typography",
                "CC_Playground_Category_Typography_Desc",
                new[] { "heading", "text", "caption" }
            ),
            new PlaygroundCategory(
                "buttons",
                "CC_Playground_Category_Buttons",
                "CC_Playground_Category_Buttons_Desc",
                new[] { "button", "iconbutton", "togglebutton" }
            ),
            new PlaygroundCategory(
                "inputs",
                "CC_Playground_Category_Inputs",
                "CC_Playground_Category_Inputs_Desc",
                new[] { "textfield", "checkbox", "slider", "switch" }
            ),
            new PlaygroundCategory(
                "feedback",
                "CC_Playground_Category_Feedback",
                "CC_Playground_Category_Feedback_Desc",
                new[] { "spinner", "progressbar", "badge" }
            ),
            new PlaygroundCategory(
                "overlay",
                "CC_Playground_Category_Overlay",
                "CC_Playground_Category_Overlay_Desc",
                new[] { "dialog", "popover", "drawer" }
            ),
        };
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        Hooks.Hooks.StateHandle<string> selectedHandle = Hooks.Hooks.UseState<string>("stack");

        LightweaveNode rail = PlaygroundRail.Create(BuildDemoCategories(), selectedHandle);
        LightweaveNode scroller = ScrollArea.Create(rail);
        LightweaveNode framed = Box.Create(
            null,
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
            RadiusSpec.All(new Rem(0.375f)),
            c => c.Add(scroller)
        );
        LightweaveNode constrained = Container.Create(
            framed,
            new Rem(14f),
            align: ContainerAlign.Start
        );

        return new DocSample(constrained);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("stack");
        LightweaveNode rail = PlaygroundRail.Create(BuildDemoCategories(), selected);
        return new DocSample(
            Container.Create(
                Box.Create(
                    null,
                    new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
                    BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                    RadiusSpec.All(new Rem(0.375f)),
                    c => c.Add(ScrollArea.Create(rail))
                ),
                new Rem(14f),
                align: ContainerAlign.Start
            )
        );
    }
}
