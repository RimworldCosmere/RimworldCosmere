using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed class LightweavePlayground : LightweaveWindow {
    private static readonly IReadOnlyList<PlaygroundCategory> Categories = new[] {
        new PlaygroundCategory(
            "layout",
            "CC_Playground_Category_Layout",
            "CC_Playground_Category_Layout_Desc",
            new[] {
                "stack", "column", "row", "hstack", "grid", "wrap", "scrollarea", "divider", "spacer", "each",
                "conditional", "carousel", "container", "card", "box",
            }
        ),
        new PlaygroundCategory(
            "typography",
            "CC_Playground_Category_Typography",
            "CC_Playground_Category_Typography_Desc",
            new[] { "heading", "text", "label", "caption", "richtext", "code", "icon" }
        ),
        new PlaygroundCategory(
            "buttons",
            "CC_Playground_Category_Buttons",
            "CC_Playground_Category_Buttons_Desc",
            new[] { "button", "iconbutton", "togglebutton", "buttongroup" }
        ),
        new PlaygroundCategory(
            "inputs",
            "CC_Playground_Category_Inputs",
            "CC_Playground_Category_Inputs_Desc",
            new[] {
                "textfield", "checkbox", "switch", "radio", "slider", "textarea", "numberfield", "searchfield",
                "dropdown", "colorpicker", "keybinding",
            }
        ),
        new PlaygroundCategory(
            "feedback",
            "CC_Playground_Category_Feedback",
            "CC_Playground_Category_Feedback_Desc",
            new[] { "spinner", "progressbar", "ringgauge", "sparkline", "badge", "tooltip" }
        ),
        new PlaygroundCategory(
            "navigation",
            "CC_Playground_Category_Navigation",
            "CC_Playground_Category_Navigation_Desc",
            new[] { "tabs", "segmented", "breadcrumbs", "menu", "contextmenu", "accordion" }
        ),
        new PlaygroundCategory(
            "overlay",
            "CC_Playground_Category_Overlay",
            "CC_Playground_Category_Overlay_Desc",
            new[] { "window", "dialog", "popover", "drawer", "toast" }
        ),
        new PlaygroundCategory(
            "data",
            "CC_Playground_Category_Data",
            "CC_Playground_Category_Data_Desc",
            new[] { "list", "table", "tree", "keyvalue" }
        ),
        new PlaygroundCategory(
            "hooks",
            "CC_Playground_Category_Hooks",
            "CC_Playground_Category_Hooks_Desc",
            new[] { "usestate", "useanim", "usefocus", "usehotkey" }
        ),
    };

    private static readonly Dictionary<string, string> SourcePaths = new Dictionary<string, string> {
        { "stack", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Stack.cs" },
        { "column", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Column.cs" },
        { "row", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Row.cs" },
        { "hstack", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/HStack.cs" },
        { "grid", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Grid.cs" },
        { "wrap", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Wrap.cs" },
        { "scrollarea", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/ScrollArea.cs" },
        { "divider", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Divider.cs" },
        { "spacer", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Spacer.cs" },
        { "each", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Each.cs" },
        { "conditional", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Conditional.cs" },
        { "carousel", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Carousel.cs" },
        { "container", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Container.cs" },
        { "card", "CosmereCore/CosmereCore/Core/UI/Lightweave/Surface/Card.cs" },
        { "box", "CosmereCore/CosmereCore/Core/UI/Lightweave/Layout/Box.cs" },
        { "heading", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Heading.cs" },
        { "text", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Text.cs" },
        { "label", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Label.cs" },
        { "caption", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Caption.cs" },
        { "richtext", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/RichText.cs" },
        { "code", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Code.cs" },
        { "icon", "CosmereCore/CosmereCore/Core/UI/Lightweave/Typography/Icon.cs" },
        { "button", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Button.cs" },
        { "iconbutton", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/IconButton.cs" },
        { "togglebutton", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/ToggleButton.cs" },
        { "buttongroup", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/ButtonGroup.cs" },
        { "textfield", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/TextField.cs" },
        { "checkbox", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Checkbox.cs" },
        { "switch", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Switch.cs" },
        { "radio", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Radio.cs" },
        { "slider", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Slider.cs" },
        { "textarea", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/TextArea.cs" },
        { "numberfield", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/NumberField.cs" },
        { "searchfield", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/SearchField.cs" },
        { "dropdown", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/Dropdown.cs" },
        { "colorpicker", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/ColorPicker.cs" },
        { "keybinding", "CosmereCore/CosmereCore/Core/UI/Lightweave/Input/KeyBindingField.cs" },
        { "spinner", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/Spinner.cs" },
        { "progressbar", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/ProgressBar.cs" },
        { "ringgauge", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/RingGauge.cs" },
        { "sparkline", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/Sparkline.cs" },
        { "badge", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/Badge.cs" },
        { "tooltip", "CosmereCore/CosmereCore/Core/UI/Lightweave/Feedback/Tooltip.cs" },
        { "tabs", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/Tabs.cs" },
        { "segmented", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/Segmented.cs" },
        { "breadcrumbs", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/Breadcrumbs.cs" },
        { "menu", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/Menu.cs" },
        { "contextmenu", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/ContextMenu.cs" },
        { "accordion", "CosmereCore/CosmereCore/Core/UI/Lightweave/Navigation/Accordion.cs" },
        { "window", "CosmereCore/CosmereCore/Core/UI/Lightweave/Runtime/LightweaveWindow.cs" },
        { "dialog", "CosmereCore/CosmereCore/Core/UI/Lightweave/Overlay/Dialog.cs" },
        { "popover", "CosmereCore/CosmereCore/Core/UI/Lightweave/Overlay/Popover.cs" },
        { "drawer", "CosmereCore/CosmereCore/Core/UI/Lightweave/Overlay/Drawer.cs" },
        { "toast", "CosmereCore/CosmereCore/Core/UI/Lightweave/Overlay/Toast.cs" },
        { "list", "CosmereCore/CosmereCore/Core/UI/Lightweave/Data/List.cs" },
        { "table", "CosmereCore/CosmereCore/Core/UI/Lightweave/Data/Table.cs" },
        { "tree", "CosmereCore/CosmereCore/Core/UI/Lightweave/Data/Tree.cs" },
        { "keyvalue", "CosmereCore/CosmereCore/Core/UI/Lightweave/Data/KeyValue.cs" },
        { "usestate", "CosmereCore/CosmereCore/Core/UI/Lightweave/Hooks/Hooks.cs" },
        { "useanim", "CosmereCore/CosmereCore/Core/UI/Lightweave/Hooks/Hooks.cs" },
        { "usefocus", "CosmereCore/CosmereCore/Core/UI/Lightweave/Hooks/Hooks.cs" },
        { "usehotkey", "CosmereCore/CosmereCore/Core/UI/Lightweave/Hooks/Hooks.cs" },
    };

    private static readonly Dictionary<string, float> DemoRowHeights = new Dictionary<string, float> {
        { "list", 200f },
        { "table", 220f },
        { "tree", 200f },
        { "keyvalue", 180f },
        { "accordion", 260f },
        { "carousel", 200f },
        { "container", 96f },
        { "card", 200f },
        { "stack", 160f },
        { "column", 160f },
        { "wrap", 120f },
        { "scrollarea", 160f },
    };

    private PlaygroundDirectionMode currentDirectionMode = PlaygroundDirectionMode.Auto;

    private PlaygroundTheme currentTheme = PlaygroundTheme.Default;

    public LightweavePlayground() {
        doCloseX = true;
        draggable = false;
        closeOnClickedOutside = false;
        closeOnAccept = false;
        closeOnCancel = true;
        preventCameraMotion = false;
        absorbInputAroundWindow = false;
    }

    public override Vector2 InitialSize => new Vector2(2200f, 1080f);

    protected override Theme.Theme? ThemeOverride => currentTheme switch {
        PlaygroundTheme.Cosmere => ThemeRegistry.Cosmere,
        PlaygroundTheme.Scadrial => ThemeRegistry.Scadrial,
        PlaygroundTheme.Roshar => ThemeRegistry.Roshar,
        _ => ThemeRegistry.Default,
    };

    protected override Direction? DirectionOverride => currentDirectionMode switch {
        PlaygroundDirectionMode.Ltr => Direction.Ltr,
        PlaygroundDirectionMode.Rtl => Direction.Rtl,
        _ => null,
    };

    protected override Rect? DragRegion(Rect inRect) {
        float headerHeight = 72f;
        return new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
    }

    protected override LightweaveNode Build() {
        Hooks.Hooks.StateHandle<PlaygroundTheme> themeHandle = Hooks.Hooks.UseState(currentTheme);
        Hooks.Hooks.StateHandle<PlaygroundDirectionMode> directionHandle = Hooks.Hooks.UseState(currentDirectionMode);
        Hooks.Hooks.StateHandle<bool> forceDisabledHandle = Hooks.Hooks.UseState(false);
        Hooks.Hooks.StateHandle<string> selectedCategoryHandle = Hooks.Hooks.UseState(Categories[0].Id);

        currentTheme = themeHandle.Value;
        currentDirectionMode = directionHandle.Value;

        LightweaveNode header = PlaygroundHeader.Create(themeHandle, directionHandle, forceDisabledHandle);
        LightweaveNode rail = PlaygroundRail.Create(Categories, selectedCategoryHandle);
        LightweaveNode body = BuildBody(selectedCategoryHandle.Value, forceDisabledHandle.Value);

        return PlaygroundShell.Create(header, rail, body);
    }

    private LightweaveNode BuildBody(string selectedCategoryId, bool forceDisabled) {
        PlaygroundCategory category = Categories[0];
        for (int i = 0; i < Categories.Count; i++) {
            if (Categories[i].Id == selectedCategoryId) {
                category = Categories[i];
                break;
            }
        }

        PlaygroundCategory selectedCategory = category;
        LightweaveNode stack = Layout.Layout.Stack(
            SpacingScale.Md,
            s => {
                s.Add(Layout.Layout.Spacer.Fixed(SpacingScale.Xxs));
                s.Add(CategoryHeaderNode(selectedCategory));

                IReadOnlyList<string> ids = selectedCategory.PrimitiveIds;
                for (int i = 0; i < ids.Count; i++) {
                    string id = ids[i];
                    string titleKey = "CC_Playground_" + id + "_Title";
                    string whatKey = "CC_Playground_" + id + "_What";
                    string whenKey = "CC_Playground_" + id + "_When";

                    (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) demo =
                        PlaygroundDemos.Build(id, forceDisabled);

                    float? rowOverride = null;
                    if (DemoRowHeights.TryGetValue(id, out float rh)) {
                        rowOverride = rh;
                    }

                    string sourcePath = SourcePaths.TryGetValue(id, out string sp)
                        ? sp
                        : "CosmereCore/CosmereCore/Core/UI/Lightweave/" + id + ".cs";

                    LightweaveNode panel = PlaygroundPanel.Create(
                        titleKey,
                        whatKey,
                        whenKey,
                        demo.variants,
                        demo.states,
                        sourcePath,
                        rowOverride
                    );
                    s.Add(panel);
                }

                s.Add(Layout.Layout.Spacer.Fixed(new Rem(1f)));
            }
        );
        LightweaveNode contained = Layout.Layout.Container(
            stack,
            new Rem(80f),
            EdgeInsets.Horizontal(SpacingScale.Lg)
        );
        return Layout.Layout.ScrollArea(contained, selectedCategoryId);
    }

    private static LightweaveNode CategoryHeaderNode(PlaygroundCategory category) {
        LightweaveNode title = Typography.Typography.Heading(2, (string)category.LabelKey.Translate());
        LightweaveNode description = Typography.Typography.Caption((string)category.DescriptionKey.Translate());

        return Layout.Layout.Stack(
            SpacingScale.Xxs,
            s => {
                s.Add(title);
                s.Add(description);
            }
        );
    }
}