using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Navigation;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using Verse;

namespace Cosmere.Lightweave.Playground;

public enum PlaygroundTheme {
    Default,
    Cosmere,
    Scadrial,
    Roshar,
}

public static class PlaygroundHeader {
    private static readonly IReadOnlyList<PlaygroundTheme> ThemeOptions = new List<PlaygroundTheme> {
        PlaygroundTheme.Default,
        PlaygroundTheme.Cosmere,
        PlaygroundTheme.Scadrial,
        PlaygroundTheme.Roshar,
    };

    public static LightweaveNode Create(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<bool> forceDisabled,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode brand = BuildBrand();
        LightweaveNode controls = BuildControls(theme, forceDisabled);

        LightweaveNode row = Layout.HStack.Create(
            SpacingScale.Md,
            r => {
                r.AddFlex(brand);
                r.Add(controls, 480f);
            }
        );

        LightweaveNode surface = Layout.Box.Create(
            new EdgeInsets(
                SpacingScale.Xs,
                Bottom: SpacingScale.Xs,
                Left: SpacingScale.Md,
                Right: SpacingScale.Xl
            ),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            null,
            RadiusSpec.Top(new Rem(0.75f)),
            children: s => s.Add(row),
            line: line,
            file: file
        );

        return surface;
    }

    private static LightweaveNode BuildBrand() {
        LightweaveNode title = Typography.Typography.Heading.Create(
            2,
            (string)"CC_Playground_Header_Brand".Translate(),
            ThemeSlot.BorderFocus
        );

        LightweaveNode subtitle = Typography.Typography.Caption.Create(
            (string)"CC_Playground_Header_Subtitle".Translate()
        );

        return Layout.Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(title);
                s.Add(subtitle);
            }
        );
    }

    private static LightweaveNode BuildControls(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<bool> forceDisabled
    ) {
        LightweaveNode themeSegmented = Segmented.Create(
            theme.Value,
            ThemeOptions,
            ThemeLabel,
            next => theme.Set(next)
        );

        LightweaveNode disabledToggle = Checkbox.Create(
            (string)"CC_Playground_Header_ForceDisabled".Translate(),
            forceDisabled.Value,
            next => forceDisabled.Set(next),
            tooltipKey: "CC_Playground_Header_ForceDisabled_Tooltip"
        );

        return Layout.HStack.Create(
            SpacingScale.Sm,
            r => {
                r.Add(themeSegmented, 280f);
                r.Add(disabledToggle, 180f);
            }
        );
    }

    private static string ThemeLabel(PlaygroundTheme value) {
        return value switch {
            PlaygroundTheme.Cosmere => (string)"CC_Playground_Header_Theme_Cosmere".Translate(),
            PlaygroundTheme.Scadrial => (string)"CC_Playground_Header_Theme_Scadrial".Translate(),
            PlaygroundTheme.Roshar => (string)"CC_Playground_Header_Theme_Roshar".Translate(),
            _ => (string)"CC_Playground_Header_Theme_Default".Translate(),
        };
    }
}
