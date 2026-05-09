using System;
using System.Runtime.CompilerServices;
using UnityEngine;
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
                r.Add(controls, 610f);
            }
        );

        LightweaveNode surface = Layout.Box.Create(
            new EdgeInsets(
                SpacingScale.Xs,
                Bottom: SpacingScale.Xs,
                Left: SpacingScale.Md,
                Right: SpacingScale.Xl
            ),
            BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
            null,
            RadiusSpec.Top(RadiusScale.Xl),
            children: s => s.Add(row),
            line: line,
            file: file
        );

        Action<Rect, Action>? innerPaint = surface.Paint;
        surface.Paint = (rect, paintChildren) => {
            Runtime.LightweaveWindowContext.PublishHeader(rect, draggable: true, ownsClose: false);
            innerPaint?.Invoke(rect, paintChildren);
        };

        return surface;
    }

    private static LightweaveNode BuildBrand() {
        LightweaveNode title = Typography.Typography.Heading.Create(
            2,
            (string)"CL_Playground_Header_Brand".Translate(),
            ThemeSlot.BorderFocus
        );

        LightweaveNode subtitle = Typography.Typography.Caption.Create(
            (string)"CL_Playground_Header_Subtitle".Translate()
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
            (string)"CL_Playground_Header_ForceDisabled".Translate(),
            forceDisabled.Value,
            next => forceDisabled.Set(next),
            tooltipKey: "CL_Playground_Header_ForceDisabled_Tooltip"
        );

        bool tourActive = PlaygroundTour.IsActive;
        LightweaveNode tourButton = Button.Create(
            tourActive
                ? (string)"CL_Playground_Header_Tour_Stop".Translate()
                : (string)"CL_Playground_Header_Tour_Start".Translate(),
            () => {
                if (PlaygroundTour.IsActive) {
                    PlaygroundTour.Stop();
                } else {
                    PlaygroundTour.Start();
                }
            },
            tourActive ? ButtonVariant.Danger : ButtonVariant.Secondary
        );

        return Layout.HStack.Create(
            SpacingScale.Sm,
            r => {
                r.Add(themeSegmented, 280f);
                r.Add(disabledToggle, 180f);
                r.Add(tourButton, 110f);
            }
        );
    }

    private static string ThemeLabel(PlaygroundTheme value) {
        return value switch {
            PlaygroundTheme.Cosmere => (string)"CL_Playground_Header_Theme_Cosmere".Translate(),
            PlaygroundTheme.Scadrial => (string)"CL_Playground_Header_Theme_Scadrial".Translate(),
            PlaygroundTheme.Roshar => (string)"CL_Playground_Header_Theme_Roshar".Translate(),
            _ => (string)"CL_Playground_Header_Theme_Default".Translate(),
        };
    }
}
