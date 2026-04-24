using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed record PlaygroundVariant(string LabelKey, LightweaveNode Demo);

public sealed record PlaygroundState(string LabelKey, LightweaveNode Demo);

public static class PlaygroundPanel {
    public static LightweaveNode Create(
        string titleKey,
        string whatKey,
        string whenKey,
        IReadOnlyList<PlaygroundVariant>? variants,
        IReadOnlyList<PlaygroundState>? states,
        string sourcePath,
        float? demoRowHeight = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        bool hasVariants = variants != null && variants.Count > 0;
        bool hasStates = states != null && states.Count > 0;

        LightweaveNode title = Typography.Typography.Heading(
            3,
            (string)titleKey.Translate(),
            ThemeSlot.SurfaceAccent
        );

        LightweaveNode whatText = Typography.Typography.Text(
            (string)whatKey.Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );

        LightweaveNode whenText = Typography.Typography.Text(
            (string)whenKey.Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextMuted,
            wrap: true
        );

        LightweaveNode sourceText = SourceLink.Create(sourcePath);

        LightweaveNode headerBundle = Layout.Layout.Stack(
            SpacingScale.Xxs,
            s => {
                s.Add(title);
                s.Add(whatText);
                s.Add(whenText);
            }
        );

        LightweaveNode sourceSpacer = NodeBuilder.New("SourceSpacer", 0, nameof(PlaygroundPanel));
        sourceSpacer.PreferredHeight = new Rem(0.25f).ToPixels();

        LightweaveNode body = Layout.Layout.Stack(
            SpacingScale.Xs,
            stack => {
                stack.Add(headerBundle);

                if (hasVariants) {
                    stack.Add(BuildSectionLabel("CC_Playground_Panel_Variants"));
                    stack.Add(BuildDemoRow(variants!, demoRowHeight));
                }

                if (hasStates) {
                    stack.Add(BuildSectionLabel("CC_Playground_Panel_States"));
                    stack.Add(BuildStateRow(states!, demoRowHeight));
                }

                stack.Add(sourceSpacer);
                stack.Add(sourceText);
            }
        );

        LightweaveNode card = Surface.Surface.Card.Create(body);
        return card;
    }

    private static LightweaveNode BuildSectionLabel(string key) {
        return Typography.Typography.Label((string)key.Translate());
    }

    private static LightweaveNode BuildDemoRow(IReadOnlyList<PlaygroundVariant> variants, float? demoRowHeight) {
        return Layout.Layout.Row(
            SpacingScale.Sm,
            children: r => {
                for (int i = 0; i < variants.Count; i++) {
                    PlaygroundVariant variant = variants[i];
                    r.Add(BuildDemoCell(variant.LabelKey, variant.Demo, demoRowHeight));
                }
            }
        );
    }

    private static LightweaveNode BuildStateRow(IReadOnlyList<PlaygroundState> states, float? demoRowHeight) {
        return Layout.Layout.Row(
            SpacingScale.Sm,
            children: r => {
                for (int i = 0; i < states.Count; i++) {
                    PlaygroundState state = states[i];
                    r.Add(BuildDemoCell(state.LabelKey, state.Demo, demoRowHeight));
                }
            }
        );
    }

    private static LightweaveNode BuildDemoCell(string labelKey, LightweaveNode demo, float? demoRowHeight) {
        LightweaveNode cellLabel = Typography.Typography.Label((string)labelKey.Translate());
        bool demoCanMeasure = demo.Measure != null || demo.PreferredHeight.HasValue;
        return Layout.Layout.Stack(
            new Rem(0.25f),
            s => {
                s.Add(cellLabel);
                if (demoCanMeasure && !demoRowHeight.HasValue) {
                    s.Add(demo);
                } else {
                    s.Add(demo, demoRowHeight ?? new Rem(3f).ToPixels());
                }
            }
        );
    }
}