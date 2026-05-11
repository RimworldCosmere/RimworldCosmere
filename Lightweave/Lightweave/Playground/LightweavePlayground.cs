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
    internal static readonly IReadOnlyList<PlaygroundCategory> Categories = new[] {
        new PlaygroundCategory(
            "layout",
            "CL_Playground_Category_Layout",
            "CL_Playground_Category_Layout_Desc",
            new[] {
                "stack", "column", "row", "hstack", "grid", "wrap", "scrollarea", "divider", "spacer", "each",
                "conditional", "carousel", "container", "card", "box",
            }
        ),
        new PlaygroundCategory(
            "typography",
            "CL_Playground_Category_Typography",
            "CL_Playground_Category_Typography_Desc",
            new[] { "heading", "text", "label", "caption", "richtext", "code", "icon" }
        ),
        new PlaygroundCategory(
            "buttons",
            "CL_Playground_Category_Buttons",
            "CL_Playground_Category_Buttons_Desc",
            new[] { "button", "iconbutton", "togglebutton", "buttongroup" }
        ),
        new PlaygroundCategory(
            "inputs",
            "CL_Playground_Category_Inputs",
            "CL_Playground_Category_Inputs_Desc",
            new[] {
                "textfield", "checkbox", "switch", "radio", "slider", "slider-readout", "textarea", "numberfield", "searchfield",
                "dropdown", "colorpicker", "keybinding",
            }
        ),
        new PlaygroundCategory(
            "feedback",
            "CL_Playground_Category_Feedback",
            "CL_Playground_Category_Feedback_Desc",
            new[] { "spinner", "progressbar", "ringgauge", "badge", "pill", "tooltip" }
        ),
        new PlaygroundCategory(
            "navigation",
            "CL_Playground_Category_Navigation",
            "CL_Playground_Category_Navigation_Desc",
            new[] { "tabs", "segmented", "breadcrumbs", "menu", "menu-row", "contextmenu", "accordion", "sidenav" }
        ),
        new PlaygroundCategory(
            "overlay",
            "CL_Playground_Category_Overlay",
            "CL_Playground_Category_Overlay_Desc",
            new[] { "window", "dialog", "dialog-header", "popover", "drawer", "toast" }
        ),
        new PlaygroundCategory(
            "data",
            "CL_Playground_Category_Data",
            "CL_Playground_Category_Data_Desc",
            new[] { "list", "table", "tree", "keyvalue", "chart" }
        ),
        new PlaygroundCategory(
            "hooks",
            "CL_Playground_Category_Hooks",
            "CL_Playground_Category_Hooks_Desc",
            new[] { "usestate", "useanim", "usefocus", "usehotkey" }
        ),
        new PlaygroundCategory(
            "tokens",
            "CL_Playground_Category_Tokens",
            "CL_Playground_Category_Tokens_Desc",
            new[] { "rem", "breakpoints" }
        ),
        new PlaygroundCategory(
            "blocks",
            "CL_Playground_Category_Blocks",
            "CL_Playground_Category_Blocks_Desc",
            new[] {
                "block-main-menu-root", "block-title-hero", "block-continue-card", "block-expansion-row",
                "block-metadata-table", "block-menu-buttons", "block-dock-tile", "block-more-button",
                "block-more-popover", "block-accent-stripe", "block-lightweave-wordmark",
            }
        ),
    };

    private static readonly Dictionary<string, string> SourcePaths = new Dictionary<string, string> {
        { "stack", "Lightweave/Lightweave/Layout/Stack.cs" },
        { "column", "Lightweave/Lightweave/Layout/Column.cs" },
        { "row", "Lightweave/Lightweave/Layout/Row.cs" },
        { "hstack", "Lightweave/Lightweave/Layout/HStack.cs" },
        { "grid", "Lightweave/Lightweave/Layout/Grid.cs" },
        { "wrap", "Lightweave/Lightweave/Layout/Wrap.cs" },
        { "scrollarea", "Lightweave/Lightweave/Layout/ScrollArea.cs" },
        { "divider", "Lightweave/Lightweave/Layout/Divider.cs" },
        { "spacer", "Lightweave/Lightweave/Layout/Spacer.cs" },
        { "each", "Lightweave/Lightweave/Layout/Each.cs" },
        { "conditional", "Lightweave/Lightweave/Layout/Conditional.cs" },
        { "carousel", "Lightweave/Lightweave/Layout/Carousel.cs" },
        { "container", "Lightweave/Lightweave/Layout/Container.cs" },
        { "card", "Lightweave/Lightweave/Layout/Card.cs" },
        { "box", "Lightweave/Lightweave/Layout/Box.cs" },
        { "heading", "Lightweave/Lightweave/Typography/Heading.cs" },
        { "text", "Lightweave/Lightweave/Typography/Text.cs" },
        { "label", "Lightweave/Lightweave/Typography/Label.cs" },
        { "caption", "Lightweave/Lightweave/Typography/Caption.cs" },
        { "richtext", "Lightweave/Lightweave/Typography/RichText.cs" },
        { "code", "Lightweave/Lightweave/Typography/Code.cs" },
        { "icon", "Lightweave/Lightweave/Typography/Icon.cs" },
        { "button", "Lightweave/Lightweave/Input/Button.cs" },
        { "iconbutton", "Lightweave/Lightweave/Input/IconButton.cs" },
        { "togglebutton", "Lightweave/Lightweave/Input/ToggleButton.cs" },
        { "buttongroup", "Lightweave/Lightweave/Input/ButtonGroup.cs" },
        { "textfield", "Lightweave/Lightweave/Input/TextField.cs" },
        { "checkbox", "Lightweave/Lightweave/Input/Checkbox.cs" },
        { "switch", "Lightweave/Lightweave/Input/Switch.cs" },
        { "radio", "Lightweave/Lightweave/Input/Radio.cs" },
        { "slider", "Lightweave/Lightweave/Input/Slider.cs" },
        { "slider-readout", "Lightweave/Lightweave/Input/SliderWithReadout.cs" },
        { "textarea", "Lightweave/Lightweave/Input/TextArea.cs" },
        { "numberfield", "Lightweave/Lightweave/Input/NumberField.cs" },
        { "searchfield", "Lightweave/Lightweave/Input/SearchField.cs" },
        { "dropdown", "Lightweave/Lightweave/Input/Dropdown.cs" },
        { "colorpicker", "Lightweave/Lightweave/Input/ColorPicker.cs" },
        { "keybinding", "Lightweave/Lightweave/Input/KeyBindingField.cs" },
        { "spinner", "Lightweave/Lightweave/Feedback/Spinner.cs" },
        { "progressbar", "Lightweave/Lightweave/Feedback/ProgressBar.cs" },
        { "ringgauge", "Lightweave/Lightweave/Feedback/RingGauge.cs" },
        { "chart", "Lightweave/Lightweave/Feedback/Chart.cs" },
        { "badge", "Lightweave/Lightweave/Feedback/Badge.cs" },
        { "pill", "Lightweave/Lightweave/Feedback/Pill.cs" },
        { "tooltip", "Lightweave/Lightweave/Feedback/Tooltip.cs" },
        { "tabs", "Lightweave/Lightweave/Navigation/Tabs.cs" },
        { "segmented", "Lightweave/Lightweave/Navigation/Segmented.cs" },
        { "breadcrumbs", "Lightweave/Lightweave/Navigation/Breadcrumbs.cs" },
        { "menu", "Lightweave/Lightweave/Navigation/Menu.cs" },
        { "menu-row", "Lightweave/Lightweave/Navigation/MenuRow.cs" },
        { "contextmenu", "Lightweave/Lightweave/Navigation/ContextMenu.cs" },
        { "accordion", "Lightweave/Lightweave/Navigation/Accordion.cs" },
        { "sidenav", "Lightweave/Lightweave/Playground/PlaygroundRail.cs" },
        { "window", "Lightweave/Lightweave/Runtime/LightweaveWindow.cs" },
        { "dialog", "Lightweave/Lightweave/Overlay/Dialog.cs" },
        { "dialog-header", "Lightweave/Lightweave/Layout/DialogHeader.cs" },
        { "popover", "Lightweave/Lightweave/Overlay/Popover.cs" },
        { "drawer", "Lightweave/Lightweave/Overlay/Drawer.cs" },
        { "toast", "Lightweave/Lightweave/Overlay/Toast.cs" },
        { "list", "Lightweave/Lightweave/Data/List.cs" },
        { "table", "Lightweave/Lightweave/Data/Table.cs" },
        { "tree", "Lightweave/Lightweave/Data/Tree.cs" },
        { "keyvalue", "Lightweave/Lightweave/Data/KeyValue.cs" },
        { "usestate", "Lightweave/Lightweave/Hooks/Hooks.cs" },
        { "useanim", "Lightweave/Lightweave/Hooks/Hooks.cs" },
        { "usefocus", "Lightweave/Lightweave/Hooks/Hooks.cs" },
        { "usehotkey", "Lightweave/Lightweave/Hooks/Hooks.cs" },
        { "breakpoints", "Lightweave/Lightweave/Tokens/Breakpoint.cs" },
        { "block-main-menu-root", "Lightweave/Lightweave/MainMenu/MainMenuRoot.cs" },
        { "block-title-hero", "Lightweave/Lightweave/MainMenu/TitleHero.cs" },
        { "block-continue-card", "Lightweave/Lightweave/MainMenu/ContinueCard.cs" },
        { "block-expansion-row", "Lightweave/Lightweave/MainMenu/ExpansionRow.cs" },
        { "block-metadata-table", "Lightweave/Lightweave/MainMenu/MetadataTable.cs" },
        { "block-menu-buttons", "Lightweave/Lightweave/MainMenu/MenuButtons.cs" },
        { "block-dock-tile", "Lightweave/Lightweave/MainMenu/DockTile.cs" },
        { "block-more-button", "Lightweave/Lightweave/MainMenu/MoreButton.cs" },
        { "block-accent-stripe", "Lightweave/Lightweave/MainMenu/LightweaveBranding.cs" },
        { "block-lightweave-wordmark", "Lightweave/Lightweave/MainMenu/LightweaveBranding.cs" },
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

    public static string? OverrideSelectedPrimitive;

    public LightweavePlayground() {
        doCloseX = false;
        drawOwnCloseX = true;
        draggable = true;
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
        return base.DragRegion(inRect);
    }

    protected override LightweaveNode Body() {
        Hooks.Hooks.StateHandle<PlaygroundTheme> themeHandle = Hooks.Hooks.UseState(currentTheme);
        Hooks.Hooks.StateHandle<bool> forceDisabledHandle = Hooks.Hooks.UseState(false);
        Hooks.Hooks.StateHandle<string> selectedPrimitiveHandle = Hooks.Hooks.UseState(DefaultPrimitiveId());

        if (OverrideSelectedPrimitive != null && selectedPrimitiveHandle.Value != OverrideSelectedPrimitive) {
            selectedPrimitiveHandle.Set(OverrideSelectedPrimitive);
        }

        if (PlaygroundTour.IsActive) {
            PlaygroundTour.TryAdvance();
            string? tourPrim = PlaygroundTour.CurrentPrimitiveId();
            if (tourPrim != null && selectedPrimitiveHandle.Value != tourPrim) {
                selectedPrimitiveHandle.Set(tourPrim);
            }
        }

        Hooks.Hooks.RefHandle<DocContext> docCtxRef = Hooks.Hooks.UseRef(new DocContext());
        Hooks.Hooks.RefHandle<string?> lastPrimitiveRef = Hooks.Hooks.UseRef<string?>(null);
        if (lastPrimitiveRef.Current != selectedPrimitiveHandle.Value) {
            docCtxRef.Current.Reset();
            lastPrimitiveRef.Current = selectedPrimitiveHandle.Value;
            if (PlaygroundTour.IsActive) {
                PlaygroundTour.NotifyPrimitiveSwitched();
            }
        }

        if (PlaygroundTour.IsActive) {
            LightweaveScrollStatus scroll = docCtxRef.Current.Scroll;
            PlaygroundTour.NotifyMetrics(scroll.LastContentHeight, scroll.LastViewportHeight);
            float t = PlaygroundTour.ScrollProgress();
            float maxScroll = Mathf.Max(0f, scroll.LastContentHeight - scroll.LastViewportHeight);
            scroll.Position = new Vector2(0f, t * maxScroll);
        }

        currentTheme = themeHandle.Value;

        LightweaveNode header = PlaygroundHeader.Create(themeHandle, forceDisabledHandle);
        LightweaveNode rail = Layout.ScrollArea.Create(PlaygroundRail.Create(Categories, selectedPrimitiveHandle));
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

        string titleKey = "CL_Playground_" + selectedPrimitiveId + "_Title";
        string whatKey = "CL_Playground_" + selectedPrimitiveId + "_What";
        string whenKey = "CL_Playground_" + selectedPrimitiveId + "_When";

        (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) demo =
            DocReflection.BuildSamplesById(selectedPrimitiveId, forceDisabled);

        float? rowOverride = DocReflection.GetPreferredVariantHeight(selectedPrimitiveId);
        if (!rowOverride.HasValue && DemoRowHeights.TryGetValue(selectedPrimitiveId, out float rh)) {
            rowOverride = rh;
        }

        string sourcePath = SourcePaths.TryGetValue(selectedPrimitiveId, out string sp)
            ? sp
            : "Lightweave/Lightweave/" + selectedPrimitiveId + ".cs";

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

        LightweaveNode docStack = Layout.Stack.Create(
            SpacingScale.None,
            s => {
                s.Add(Layout.Spacer.Fixed(SpacingScale.Lg));
                s.Add(panel.Body);
                s.Add(Layout.Spacer.Fixed(SpacingScale.Xxl));
            }
        );

        LightweaveNode contained = Layout.Container.Create(
            docStack,
            style: new Style {
                MaxWidth = new Rem(64f),
                Padding = new EdgeInsets(Top: SpacingScale.None, Right: SpacingScale.Xl, Bottom: SpacingScale.None, Left: SpacingScale.Xl),
            }
        );
        LightweaveNode mainScroll = Layout.ScrollArea.External(contained, ctx.Scroll);

        LightweaveNode tocNode = BuildTocColumn(panel.TocEntries, ctx);

        LightweaveNode bodyHStack = Layout.HStack.Create(
            SpacingScale.None,
            r => {
                r.AddFlex(mainScroll);
                if (RenderContext.Current.Breakpoint >= Breakpoint.Lg) {
                    r.Add(tocNode, new Rem(15f).ToPixels());
                }
            }
        );

        return bodyHStack;
    }

    private static LightweaveNode BuildTocColumn(IReadOnlyList<TocEntry> entries, DocContext ctx) {
        if (entries.Count == 0) {
            return Layout.Spacer.Fixed(new Rem(0f));
        }

        LightweaveNode toc = Doc.Doc.TableOfContents(
            (string)"CL_Playground_Panel_OnThisPage".Translate(),
            entries,
            ctx
        );

        return Layout.Box.Create(
            c => c.Add(toc),
            style: new Style {
                Padding = new EdgeInsets(Top: SpacingScale.Xl, Right: SpacingScale.Lg, Bottom: SpacingScale.Xl, Left: SpacingScale.Lg),
            }
        );
    }

    private static LightweaveNode CategoryBreadcrumbNode(PlaygroundCategory category, string primitiveId) {
        string categoryLabel = (string)category.LabelKey.Translate();
        string primitiveLabel = (string)("CL_Playground_" + primitiveId + "_Title").Translate();
        return Typography.Typography.Caption.Create(categoryLabel + " / " + primitiveLabel);
    }
}