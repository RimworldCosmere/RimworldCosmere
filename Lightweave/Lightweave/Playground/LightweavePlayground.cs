using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

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
            new[] { "tabs", "segmented", "breadcrumbs", "menu", "contextmenu", "accordion", "sidenav" }
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
        { "stack", "CosmereCore/CosmereCore/Lightweave/Layout/Stack.cs" },
        { "column", "CosmereCore/CosmereCore/Lightweave/Layout/Column.cs" },
        { "row", "CosmereCore/CosmereCore/Lightweave/Layout/Row.cs" },
        { "hstack", "CosmereCore/CosmereCore/Lightweave/Layout/HStack.cs" },
        { "grid", "CosmereCore/CosmereCore/Lightweave/Layout/Grid.cs" },
        { "wrap", "CosmereCore/CosmereCore/Lightweave/Layout/Wrap.cs" },
        { "scrollarea", "CosmereCore/CosmereCore/Lightweave/Layout/ScrollArea.cs" },
        { "divider", "CosmereCore/CosmereCore/Lightweave/Layout/Divider.cs" },
        { "spacer", "CosmereCore/CosmereCore/Lightweave/Layout/Spacer.cs" },
        { "each", "CosmereCore/CosmereCore/Lightweave/Layout/Each.cs" },
        { "conditional", "CosmereCore/CosmereCore/Lightweave/Layout/Conditional.cs" },
        { "carousel", "CosmereCore/CosmereCore/Lightweave/Layout/Carousel.cs" },
        { "container", "CosmereCore/CosmereCore/Lightweave/Layout/Container.cs" },
        { "card", "CosmereCore/CosmereCore/Lightweave/Layout/Card.cs" },
        { "box", "CosmereCore/CosmereCore/Lightweave/Layout/Box.cs" },
        { "heading", "CosmereCore/CosmereCore/Lightweave/Typography/Heading.cs" },
        { "text", "CosmereCore/CosmereCore/Lightweave/Typography/Text.cs" },
        { "label", "CosmereCore/CosmereCore/Lightweave/Typography/Label.cs" },
        { "caption", "CosmereCore/CosmereCore/Lightweave/Typography/Caption.cs" },
        { "richtext", "CosmereCore/CosmereCore/Lightweave/Typography/RichText.cs" },
        { "code", "CosmereCore/CosmereCore/Lightweave/Typography/Code.cs" },
        { "icon", "CosmereCore/CosmereCore/Lightweave/Typography/Icon.cs" },
        { "button", "CosmereCore/CosmereCore/Lightweave/Input/Button.cs" },
        { "iconbutton", "CosmereCore/CosmereCore/Lightweave/Input/IconButton.cs" },
        { "togglebutton", "CosmereCore/CosmereCore/Lightweave/Input/ToggleButton.cs" },
        { "buttongroup", "CosmereCore/CosmereCore/Lightweave/Input/ButtonGroup.cs" },
        { "textfield", "CosmereCore/CosmereCore/Lightweave/Input/TextField.cs" },
        { "checkbox", "CosmereCore/CosmereCore/Lightweave/Input/Checkbox.cs" },
        { "switch", "CosmereCore/CosmereCore/Lightweave/Input/Switch.cs" },
        { "radio", "CosmereCore/CosmereCore/Lightweave/Input/Radio.cs" },
        { "slider", "CosmereCore/CosmereCore/Lightweave/Input/Slider.cs" },
        { "textarea", "CosmereCore/CosmereCore/Lightweave/Input/TextArea.cs" },
        { "numberfield", "CosmereCore/CosmereCore/Lightweave/Input/NumberField.cs" },
        { "searchfield", "CosmereCore/CosmereCore/Lightweave/Input/SearchField.cs" },
        { "dropdown", "CosmereCore/CosmereCore/Lightweave/Input/Dropdown.cs" },
        { "colorpicker", "CosmereCore/CosmereCore/Lightweave/Input/ColorPicker.cs" },
        { "keybinding", "CosmereCore/CosmereCore/Lightweave/Input/KeyBindingField.cs" },
        { "spinner", "CosmereCore/CosmereCore/Lightweave/Feedback/Spinner.cs" },
        { "progressbar", "CosmereCore/CosmereCore/Lightweave/Feedback/ProgressBar.cs" },
        { "ringgauge", "CosmereCore/CosmereCore/Lightweave/Feedback/RingGauge.cs" },
        { "sparkline", "CosmereCore/CosmereCore/Lightweave/Feedback/Sparkline.cs" },
        { "badge", "CosmereCore/CosmereCore/Lightweave/Feedback/Badge.cs" },
        { "tooltip", "CosmereCore/CosmereCore/Lightweave/Feedback/Tooltip.cs" },
        { "tabs", "CosmereCore/CosmereCore/Lightweave/Navigation/Tabs.cs" },
        { "segmented", "CosmereCore/CosmereCore/Lightweave/Navigation/Segmented.cs" },
        { "breadcrumbs", "CosmereCore/CosmereCore/Lightweave/Navigation/Breadcrumbs.cs" },
        { "menu", "CosmereCore/CosmereCore/Lightweave/Navigation/Menu.cs" },
        { "contextmenu", "CosmereCore/CosmereCore/Lightweave/Navigation/ContextMenu.cs" },
        { "accordion", "CosmereCore/CosmereCore/Lightweave/Navigation/Accordion.cs" },
        { "sidenav", "CosmereCore/CosmereCore/Lightweave/Playground/PlaygroundRail.cs" },
        { "window", "CosmereCore/CosmereCore/Lightweave/Runtime/LightweaveWindow.cs" },
        { "dialog", "CosmereCore/CosmereCore/Lightweave/Overlay/Dialog.cs" },
        { "popover", "CosmereCore/CosmereCore/Lightweave/Overlay/Popover.cs" },
        { "drawer", "CosmereCore/CosmereCore/Lightweave/Overlay/Drawer.cs" },
        { "toast", "CosmereCore/CosmereCore/Lightweave/Overlay/Toast.cs" },
        { "list", "CosmereCore/CosmereCore/Lightweave/Data/List.cs" },
        { "table", "CosmereCore/CosmereCore/Lightweave/Data/Table.cs" },
        { "tree", "CosmereCore/CosmereCore/Lightweave/Data/Tree.cs" },
        { "keyvalue", "CosmereCore/CosmereCore/Lightweave/Data/KeyValue.cs" },
        { "usestate", "CosmereCore/CosmereCore/Lightweave/Hooks/Hooks.cs" },
        { "useanim", "CosmereCore/CosmereCore/Lightweave/Hooks/Hooks.cs" },
        { "usefocus", "CosmereCore/CosmereCore/Lightweave/Hooks/Hooks.cs" },
        { "usehotkey", "CosmereCore/CosmereCore/Lightweave/Hooks/Hooks.cs" },
    };

    private static readonly Dictionary<string, float> DemoRowHeights = new Dictionary<string, float> {
        { "list", 200f },
        { "table", 220f },
        { "tree", 200f },
        { "keyvalue", 180f },
        { "accordion", 260f },
        { "sidenav", 340f },
        { "carousel", 200f },
        { "container", 96f },
        { "card", 280f },
        { "stack", 160f },
        { "column", 160f },
        { "wrap", 120f },
        { "scrollarea", 160f },
    };

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

    protected override Vector2 MinWindowSize => new Vector2(720f, 480f);

    protected override Theme.Theme? ThemeOverride => currentTheme switch {
        PlaygroundTheme.Cosmere => ThemeRegistry.Cosmere,
        PlaygroundTheme.Scadrial => ThemeRegistry.Scadrial,
        PlaygroundTheme.Roshar => ThemeRegistry.Roshar,
        _ => ThemeRegistry.Default,
    };

    protected override Rect? DragRegion(Rect inRect) {
        float headerHeight = 72f;
        return new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
    }

    protected override LightweaveNode Build() {
        Hooks.Hooks.StateHandle<PlaygroundTheme> themeHandle = Hooks.Hooks.UseState(currentTheme);
        Hooks.Hooks.StateHandle<bool> forceDisabledHandle = Hooks.Hooks.UseState(false);
        Hooks.Hooks.StateHandle<string> selectedPrimitiveHandle = Hooks.Hooks.UseState(DefaultPrimitiveId());

        Hooks.Hooks.RefHandle<DocContext> docCtxRef = Hooks.Hooks.UseRef(new DocContext());
        Hooks.Hooks.RefHandle<string?> lastPrimitiveRef = Hooks.Hooks.UseRef<string?>(null);
        if (lastPrimitiveRef.Current != selectedPrimitiveHandle.Value) {
            docCtxRef.Current.Reset();
            lastPrimitiveRef.Current = selectedPrimitiveHandle.Value;
        }

        currentTheme = themeHandle.Value;

        LightweaveNode header = PlaygroundHeader.Create(themeHandle, forceDisabledHandle);
        LightweaveNode rail = Layout.Layout.ScrollArea.Create(PlaygroundRail.Create(Categories, selectedPrimitiveHandle));
        LightweaveNode body = BuildBody(selectedPrimitiveHandle.Value, forceDisabledHandle.Value, docCtxRef.Current);

        return PlaygroundShell.Create(header, rail, body);
    }

    private static string DefaultPrimitiveId() {
        PlaygroundCategory first = Categories[0];
        List<string> sorted = new List<string>(first.PrimitiveIds);
        sorted.Sort(string.CompareOrdinal);
        return sorted.Count > 0 ? sorted[0] : first.PrimitiveIds[0];
    }

    private LightweaveNode BuildBody(string selectedPrimitiveId, bool forceDisabled, DocContext ctx) {
        PlaygroundCategory? owningCategory = null;
        for (int i = 0; i < Categories.Count; i++) {
            IReadOnlyList<string> ids = Categories[i].PrimitiveIds;
            for (int j = 0; j < ids.Count; j++) {
                if (ids[j] == selectedPrimitiveId) {
                    owningCategory = Categories[i];
                    break;
                }
            }

            if (owningCategory != null) {
                break;
            }
        }

        string titleKey = "CC_Playground_" + selectedPrimitiveId + "_Title";
        string whatKey = "CC_Playground_" + selectedPrimitiveId + "_What";
        string whenKey = "CC_Playground_" + selectedPrimitiveId + "_When";

        (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) demo =
            DocReflection.BuildSamplesById(selectedPrimitiveId, forceDisabled);

        float? rowOverride = null;
        if (DemoRowHeights.TryGetValue(selectedPrimitiveId, out float rh)) {
            rowOverride = rh;
        }

        string sourcePath = SourcePaths.TryGetValue(selectedPrimitiveId, out string sp)
            ? sp
            : "CosmereCore/CosmereCore/Lightweave/" + selectedPrimitiveId + ".cs";

        LightweaveNode? breadcrumb = owningCategory != null
            ? CategoryBreadcrumbNode(owningCategory, selectedPrimitiveId)
            : null;

        PlaygroundDocs? docs = DocReflection.BuildDocsById(selectedPrimitiveId);

        PlaygroundPanelResult panel = PlaygroundPanel.Create(
            titleKey,
            whatKey,
            whenKey,
            demo.variants,
            demo.states,
            sourcePath,
            ctx,
            breadcrumb,
            rowOverride,
            docs
        );

        LightweaveNode docStack = Layout.Layout.Stack.Create(
            SpacingScale.None,
            s => {
                s.Add(Layout.Layout.Spacer.Fixed(SpacingScale.Lg));
                s.Add(panel.Body);
                s.Add(Layout.Layout.Spacer.Fixed(SpacingScale.Xxl));
            }
        );

        LightweaveNode contained = Layout.Layout.Container.Create(
            docStack,
            new Rem(56f),
            new EdgeInsets(Top: SpacingScale.None, Right: SpacingScale.Xl, Bottom: SpacingScale.None, Left: SpacingScale.Xl)
        );
        LightweaveNode mainScroll = Layout.Layout.ScrollArea.External(contained, ctx.Scroll);

        LightweaveNode tocNode = BuildTocColumn(panel.TocEntries, ctx);

        LightweaveNode bodyHStack = Layout.Layout.HStack.Create(
            SpacingScale.None,
            r => {
                r.AddFlex(mainScroll);
                r.Add(tocNode, new Rem(15f).ToPixels());
            }
        );

        return bodyHStack;
    }

    private static LightweaveNode BuildTocColumn(IReadOnlyList<TocEntry> entries, DocContext ctx) {
        if (entries.Count == 0) {
            return Layout.Layout.Spacer.Fixed(new Rem(0f));
        }

        LightweaveNode toc = Doc.Doc.TableOfContents(
            (string)"CC_Playground_Panel_OnThisPage".Translate(),
            entries,
            ctx
        );

        return Layout.Layout.Box.Create(
            new EdgeInsets(Top: SpacingScale.Xl, Right: SpacingScale.Lg, Bottom: SpacingScale.Xl, Left: SpacingScale.Lg),
            null,
            null,
            null,
            c => c.Add(toc)
        );
    }

    private static LightweaveNode CategoryBreadcrumbNode(PlaygroundCategory category, string primitiveId) {
        string categoryLabel = (string)category.LabelKey.Translate();
        string primitiveLabel = (string)("CC_Playground_" + primitiveId + "_Title").Translate();
        return Typography.Typography.Caption.Create(categoryLabel + " / " + primitiveLabel);
    }
}