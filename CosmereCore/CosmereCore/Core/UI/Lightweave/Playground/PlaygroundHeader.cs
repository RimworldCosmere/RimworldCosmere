using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Typography;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public enum PlaygroundTheme
{
    Default,
    Cosmere,
}

public enum PlaygroundDirectionMode
{
    Auto,
    Ltr,
    Rtl,
}

public static class PlaygroundHeader
{
    private static readonly IReadOnlyList<PlaygroundTheme> ThemeOptions = new List<PlaygroundTheme>
    {
        PlaygroundTheme.Default,
        PlaygroundTheme.Cosmere,
    };

    private static readonly IReadOnlyList<PlaygroundDirectionMode> DirectionOptions = new List<PlaygroundDirectionMode>
    {
        PlaygroundDirectionMode.Auto,
        PlaygroundDirectionMode.Ltr,
        PlaygroundDirectionMode.Rtl,
    };

    public static LightweaveNode Create(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<PlaygroundDirectionMode> direction,
        Hooks.Hooks.StateHandle<bool> forceDisabled,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        LightweaveNode brand = BuildBrand();
        LightweaveNode controls = BuildControls(theme, direction, forceDisabled);

        LightweaveNode row = Layout.Layout.Row(
            gap: SpacingScale.Md,
            children: r =>
            {
                r.Add(brand);
                r.Add(controls);
            });

        LightweaveNode surface = Surface.Surface.ByRole(
            SurfaceRole.Raised,
            padding: new EdgeInsets(
                Top: SpacingScale.Xs,
                Bottom: SpacingScale.Xs,
                Left: SpacingScale.Md,
                Right: SpacingScale.Md),
            children: s => s.Add(row),
            line: line,
            file: file);

        return surface;
    }

    private static LightweaveNode BuildBrand()
    {
        LightweaveNode title = Typography.Typography.Heading(
            2,
            (string)"CC_Playground_Header_Brand".Translate(),
            ThemeSlot.SurfaceAccent);

        LightweaveNode subtitle = Typography.Typography.Caption(
            (string)"CC_Playground_Header_Subtitle".Translate());

        return Layout.Layout.Stack(
            gap: SpacingScale.Xxs,
            children: s =>
            {
                s.Add(title, new Rem(1.5f).ToPixels());
                s.Add(subtitle, new Rem(0.9f).ToPixels());
            });
    }

    private static LightweaveNode BuildControls(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<PlaygroundDirectionMode> direction,
        Hooks.Hooks.StateHandle<bool> forceDisabled)
    {
        LightweaveNode themeDropdown = Dropdown.Create<PlaygroundTheme>(
            value: theme.Value,
            options: ThemeOptions,
            labelFn: ThemeLabel,
            onChange: next => theme.Set(next));

        LightweaveNode directionDropdown = Dropdown.Create<PlaygroundDirectionMode>(
            value: direction.Value,
            options: DirectionOptions,
            labelFn: DirectionLabel,
            onChange: next => direction.Set(next));

        LightweaveNode disabledToggle = Checkbox.Create(
            label: (string)"CC_Playground_Header_ForceDisabled".Translate(),
            value: forceDisabled.Value,
            onChange: next => forceDisabled.Set(next));

        return Layout.Layout.Row(
            gap: SpacingScale.Sm,
            children: r =>
            {
                r.Add(themeDropdown);
                r.Add(directionDropdown);
                r.Add(disabledToggle);
            });
    }

    private static string ThemeLabel(PlaygroundTheme value) => value switch
    {
        PlaygroundTheme.Cosmere => (string)"CC_Playground_Header_Theme_Cosmere".Translate(),
        _ => (string)"CC_Playground_Header_Theme_Default".Translate(),
    };

    private static string DirectionLabel(PlaygroundDirectionMode value) => value switch
    {
        PlaygroundDirectionMode.Ltr => (string)"CC_Playground_Header_Direction_Ltr".Translate(),
        PlaygroundDirectionMode.Rtl => (string)"CC_Playground_Header_Direction_Rtl".Translate(),
        _ => (string)"CC_Playground_Header_Direction_Auto".Translate(),
    };
}
