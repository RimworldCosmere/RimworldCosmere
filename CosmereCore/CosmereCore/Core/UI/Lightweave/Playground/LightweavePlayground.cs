using System.Collections.Generic;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Theme;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Typography;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed class LightweavePlayground : LightweaveWindow
{
    private static readonly IReadOnlyList<PlaygroundCategory> Categories = new PlaygroundCategory[]
    {
        new PlaygroundCategory("layout",     "CC_Playground_Category_Layout",     "CC_Playground_Category_Layout_Desc",
            new[] { "stack", "column", "row", "hstack", "grid", "wrap", "scrollarea", "divider", "spacer", "each", "conditional" }),
        new PlaygroundCategory("surface",    "CC_Playground_Category_Surface",    "CC_Playground_Category_Surface_Desc",
            new[] { "card", "box", "panel", "surface" }),
        new PlaygroundCategory("typography", "CC_Playground_Category_Typography", "CC_Playground_Category_Typography_Desc",
            new[] { "heading", "text", "label", "caption", "richtext", "code", "icon" }),
        new PlaygroundCategory("buttons",    "CC_Playground_Category_Buttons",    "CC_Playground_Category_Buttons_Desc",
            new[] { "button", "iconbutton", "togglebutton" }),
        new PlaygroundCategory("inputs",     "CC_Playground_Category_Inputs",     "CC_Playground_Category_Inputs_Desc",
            new[] { "textfield", "checkbox", "switch", "radio", "slider", "textarea", "numberfield", "searchfield", "dropdown", "colorpicker", "keybinding" }),
        new PlaygroundCategory("feedback",   "CC_Playground_Category_Feedback",   "CC_Playground_Category_Feedback_Desc",
            new[] { "spinner", "progressbar", "ringgauge", "sparkline", "badge", "tag", "tooltip" }),
        new PlaygroundCategory("navigation", "CC_Playground_Category_Navigation", "CC_Playground_Category_Navigation_Desc",
            new[] { "tabs", "segmented", "breadcrumbs", "menu", "contextmenu" }),
        new PlaygroundCategory("overlay",    "CC_Playground_Category_Overlay",    "CC_Playground_Category_Overlay_Desc",
            new[] { "dialog", "popover", "drawer", "toast" }),
        new PlaygroundCategory("data",       "CC_Playground_Category_Data",       "CC_Playground_Category_Data_Desc",
            new[] { "list", "table", "tree", "keyvalue" }),
        new PlaygroundCategory("hooks",      "CC_Playground_Category_Hooks",      "CC_Playground_Category_Hooks_Desc",
            new[] { "usestate", "useanim", "usefocus", "usehotkey" }),
    };

    private const float PanelHeight = 260f;
    private const float CategoryHeaderHeight = 64f;
    private const float CategoryTitleHeight = 32f;
    private const float CategoryDescriptionHeight = 22f;
    private const float DividerHeight = 8f;

    private PlaygroundTheme currentTheme = PlaygroundTheme.Default;
    private PlaygroundDirectionMode currentDirectionMode = PlaygroundDirectionMode.Auto;

    public override Vector2 InitialSize => new Vector2(1100f, 720f);

    protected override Theme.Theme? ThemeOverride => currentTheme switch
    {
        PlaygroundTheme.Cosmere => ThemeRegistry.Cosmere,
        _ => ThemeRegistry.Default,
    };

    public override void DoWindowContents(Rect inRect)
    {
        Direction? dirOverride = currentDirectionMode switch
        {
            PlaygroundDirectionMode.Ltr => Direction.Ltr,
            PlaygroundDirectionMode.Rtl => Direction.Rtl,
            _ => null,
        };
        LightweaveRoot.Render(inRect, RootId, Build, dirOverride, ThemeOverride);
    }

    protected override LightweaveNode Build()
    {
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

    private LightweaveNode BuildBody(string selectedCategoryId, bool forceDisabled)
    {
        PlaygroundCategory category = Categories[0];
        for (int i = 0; i < Categories.Count; i++)
        {
            if (Categories[i].Id == selectedCategoryId)
            {
                category = Categories[i];
                break;
            }
        }

        PlaygroundCategory selectedCategory = category;
        LightweaveNode stack = Layout.Layout.Stack(
            gap: SpacingScale.Md,
            children: s =>
            {
                s.Add(CategoryHeaderNode(selectedCategory), CategoryHeaderHeight);

                IReadOnlyList<string> ids = selectedCategory.PrimitiveIds;
                for (int i = 0; i < ids.Count; i++)
                {
                    string id = ids[i];
                    string titleKey = "CC_Playground_" + id + "_Title";
                    string whatKey = "CC_Playground_" + id + "_What";
                    string whenKey = "CC_Playground_" + id + "_When";

                    (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) demo =
                        PlaygroundDemos.Build(id, forceDisabled);

                    LightweaveNode panel = PlaygroundPanel.Create(
                        titleKey: titleKey,
                        whatKey: whatKey,
                        whenKey: whenKey,
                        variants: demo.variants,
                        states: demo.states,
                        sourcePath: "Lightweave/" + id + ".cs",
                        height: PanelHeight);
                    s.Add(panel, PanelHeight);

                    if (i < ids.Count - 1)
                    {
                        s.Add(BrassRailDivider.Create(), DividerHeight);
                    }
                }
            });
        return Layout.Layout.ScrollArea(stack);
    }

    private static LightweaveNode CategoryHeaderNode(PlaygroundCategory category)
    {
        LightweaveNode title = Typography.Typography.Heading(2, (string)category.LabelKey.Translate());
        LightweaveNode description = Typography.Typography.Caption((string)category.DescriptionKey.Translate());

        return Layout.Layout.Stack(
            gap: SpacingScale.Xxs,
            children: s =>
            {
                s.Add(title, CategoryTitleHeight);
                s.Add(description, CategoryDescriptionHeight);
            });
    }
}
