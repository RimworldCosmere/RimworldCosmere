using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Navigation;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Playground;

public enum PlaygroundTheme {
    Default,
    Cosmere,
    Scadrial,
    Roshar,
}

public enum PlaygroundDirectionMode {
    Auto,
    Ltr,
    Rtl,
}

public static class PlaygroundHeader {
    private static readonly IReadOnlyList<PlaygroundTheme> ThemeOptions = new List<PlaygroundTheme> {
        PlaygroundTheme.Default,
        PlaygroundTheme.Cosmere,
        PlaygroundTheme.Scadrial,
        PlaygroundTheme.Roshar,
    };

    private static readonly IReadOnlyList<PlaygroundDirectionMode> DirectionOptions = new List<PlaygroundDirectionMode> {
        PlaygroundDirectionMode.Auto,
        PlaygroundDirectionMode.Ltr,
        PlaygroundDirectionMode.Rtl,
    };

    public static LightweaveNode Create(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<PlaygroundDirectionMode> direction,
        Hooks.Hooks.StateHandle<bool> forceDisabled,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode brand = BuildBrand();
        LightweaveNode controls = BuildControls(theme, direction, forceDisabled);

        LightweaveNode row = Layout.Layout.HStack(
            SpacingScale.Md,
            r => {
                r.AddFlex(brand);
                r.Add(controls, 640f);
            }
        );

        LightweaveNode surface = Layout.Layout.Box(
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
        LightweaveNode title = Typography.Typography.Heading(
            2,
            (string)"CC_Playground_Header_Brand".Translate(),
            ThemeSlot.BorderFocus
        );

        LightweaveNode subtitle = Typography.Typography.Caption(
            (string)"CC_Playground_Header_Subtitle".Translate()
        );

        return Layout.Layout.Stack(
            SpacingScale.Xxs,
            s => {
                s.Add(title);
                s.Add(subtitle);
            }
        );
    }

    private static LightweaveNode BuildControls(
        Hooks.Hooks.StateHandle<PlaygroundTheme> theme,
        Hooks.Hooks.StateHandle<PlaygroundDirectionMode> direction,
        Hooks.Hooks.StateHandle<bool> forceDisabled
    ) {
        LightweaveNode themeSegmented = Segmented.Create(
            theme.Value,
            ThemeOptions,
            ThemeLabel,
            next => theme.Set(next)
        );

        LightweaveNode directionSegmented = Segmented.Create(
            direction.Value,
            DirectionOptions,
            DirectionLabel,
            next => direction.Set(next)
        );

        LightweaveNode disabledToggle = Checkbox.Create(
            (string)"CC_Playground_Header_ForceDisabled".Translate(),
            forceDisabled.Value,
            next => forceDisabled.Set(next),
            tooltipKey: "CC_Playground_Header_ForceDisabled_Tooltip"
        );

        return Layout.Layout.HStack(
            SpacingScale.Sm,
            r => {
                r.Add(themeSegmented, 280f);
                r.Add(directionSegmented, 160f);
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

    private static string DirectionLabel(PlaygroundDirectionMode value) {
        return value switch {
            PlaygroundDirectionMode.Ltr => (string)"CC_Playground_Header_Direction_Ltr".Translate(),
            PlaygroundDirectionMode.Rtl => (string)"CC_Playground_Header_Direction_Rtl".Translate(),
            _ => (string)"CC_Playground_Header_Direction_Auto".Translate(),
        };
    }
}