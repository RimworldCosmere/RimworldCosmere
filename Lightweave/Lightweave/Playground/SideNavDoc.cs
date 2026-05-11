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
                "CL_Playground_Category_Layout",
                "CL_Playground_Category_Layout_Desc",
                new[] { "stack", "column", "row", "grid" }
            ),
            new PlaygroundCategory(
                "typography",
                "CL_Playground_Category_Typography",
                "CL_Playground_Category_Typography_Desc",
                new[] { "heading", "text", "caption" }
            ),
            new PlaygroundCategory(
                "buttons",
                "CL_Playground_Category_Buttons",
                "CL_Playground_Category_Buttons_Desc",
                new[] { "button", "iconbutton", "togglebutton" }
            ),
            new PlaygroundCategory(
                "inputs",
                "CL_Playground_Category_Inputs",
                "CL_Playground_Category_Inputs_Desc",
                new[] { "textfield", "checkbox", "slider", "switch" }
            ),
            new PlaygroundCategory(
                "feedback",
                "CL_Playground_Category_Feedback",
                "CL_Playground_Category_Feedback_Desc",
                new[] { "spinner", "progressbar", "badge" }
            ),
            new PlaygroundCategory(
                "overlay",
                "CL_Playground_Category_Overlay",
                "CL_Playground_Category_Overlay_Desc",
                new[] { "dialog", "popover", "drawer" }
            ),
        };
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<string> selectedHandle = Hooks.Hooks.UseState<string>("stack");

            LightweaveNode rail = PlaygroundRail.Create(BuildDemoCategories(), selectedHandle);
            LightweaveNode scroller = ScrollArea.Create(rail);
            LightweaveNode framed = Box.Create(
                c => c.Add(scroller),
                style: new Style {
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                    Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                    Radius = RadiusSpec.All(RadiusScale.Md),
                }
            );
            LightweaveNode constrained = Container.Create(
                framed,
                align: ContainerAlign.Start,
                style: new Style { MaxWidth = new Rem(14f) }
            );

            return constrained;
        });
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => {
            Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("stack");
            LightweaveNode rail = PlaygroundRail.Create(BuildDemoCategories(), selected);
            return Container.Create(
                Box.Create(
                    c => c.Add(ScrollArea.Create(rail)),
                    style: new Style {
                        Background = BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                        Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                        Radius = RadiusSpec.All(RadiusScale.Md),
                    }
                ),
                align: ContainerAlign.Start,
                style: new Style { MaxWidth = new Rem(14f) }
            );
        });
    }
}
