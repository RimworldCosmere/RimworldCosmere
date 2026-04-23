using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Typography;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed record PlaygroundVariant(string LabelKey, LightweaveNode Demo);

public sealed record PlaygroundState(string LabelKey, LightweaveNode Demo);

public static class PlaygroundPanel
{
    private const float TitleHeight = 28f;
    private const float BodyLineHeight = 20f;
    private const float SectionLabelHeight = 18f;
    private const float DemoRowHeight = 56f;
    private const float DividerHeight = 10f;
    private const float SourceRowHeight = 18f;
    private const float DemoCellGap = 4f;

    public static LightweaveNode Create(
        string titleKey,
        string whatKey,
        string whenKey,
        IReadOnlyList<PlaygroundVariant>? variants,
        IReadOnlyList<PlaygroundState>? states,
        string sourcePath,
        float height,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        bool hasVariants = variants != null && variants.Count > 0;
        bool hasStates = states != null && states.Count > 0;

        LightweaveNode title = Typography.Typography.Heading(
            3,
            (string)titleKey.Translate(),
            ThemeSlot.SurfaceAccent);

        LightweaveNode whatText = Typography.Typography.Text(
            (string)whatKey.Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary);

        LightweaveNode whenText = Typography.Typography.Text(
            (string)whenKey.Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextMuted);

        LightweaveNode sourceText = Typography.Typography.Text(
            string.Format("{0} {1}", (string)"CC_Playground_Panel_SourceLabel".Translate(), sourcePath),
            FontRole.Mono,
            new Rem(0.75f),
            ThemeSlot.TextMuted);

        LightweaveNode headerBundle = Layout.Layout.Stack(
            gap: SpacingScale.Xxs,
            children: s =>
            {
                s.Add(title, TitleHeight);
                s.Add(whatText, BodyLineHeight);
                s.Add(whenText, BodyLineHeight);
            });

        float headerBundleHeight = headerBundle.PreferredHeight ?? (TitleHeight + BodyLineHeight * 2f + SpacingScale.Xxs.ToPixels() * 2f);

        LightweaveNode body = Layout.Layout.Stack(
            gap: SpacingScale.Sm,
            children: stack =>
            {
                stack.Add(headerBundle, headerBundleHeight);

                if (hasVariants)
                {
                    stack.Add(BrassRailDivider.Create(), DividerHeight);
                    stack.Add(BuildSectionLabel("CC_Playground_Panel_Variants"), SectionLabelHeight);
                    stack.Add(BuildDemoRow(variants!), DemoRowHeight);
                }

                if (hasStates)
                {
                    stack.Add(BrassRailDivider.Create(), DividerHeight);
                    stack.Add(BuildSectionLabel("CC_Playground_Panel_States"), SectionLabelHeight);
                    stack.Add(BuildStateRow(states!), DemoRowHeight);
                }

                stack.Add(BrassRailDivider.Create(), DividerHeight);
                stack.Add(sourceText, SourceRowHeight);
            });

        LightweaveNode card = Surface.Surface.Card(c => c.Add(body), line, file);
        card.PreferredHeight = height;
        return card;
    }

    private static LightweaveNode BuildSectionLabel(string key)
    {
        return Typography.Typography.Caption((string)key.Translate());
    }

    private static LightweaveNode BuildDemoRow(IReadOnlyList<PlaygroundVariant> variants)
    {
        return Layout.Layout.Row(
            gap: SpacingScale.Sm,
            children: r =>
            {
                for (int i = 0; i < variants.Count; i++)
                {
                    PlaygroundVariant variant = variants[i];
                    r.Add(BuildDemoCell(variant.LabelKey, variant.Demo));
                }
            });
    }

    private static LightweaveNode BuildStateRow(IReadOnlyList<PlaygroundState> states)
    {
        return Layout.Layout.Row(
            gap: SpacingScale.Sm,
            children: r =>
            {
                for (int i = 0; i < states.Count; i++)
                {
                    PlaygroundState state = states[i];
                    r.Add(BuildDemoCell(state.LabelKey, state.Demo));
                }
            });
    }

    private static LightweaveNode BuildDemoCell(string labelKey, LightweaveNode demo)
    {
        LightweaveNode cellLabel = Typography.Typography.Caption((string)labelKey.Translate());
        float labelHeight = 16f;
        float demoHeight = DemoRowHeight - labelHeight - DemoCellGap;
        return Layout.Layout.Stack(
            gap: new Rem(0.25f),
            children: s =>
            {
                s.Add(cellLabel, labelHeight);
                s.Add(demo, demoHeight);
            });
    }
}
