using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Typography;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

internal static class PlaygroundDemos
{
    private static readonly IReadOnlyList<PlaygroundVariant> EmptyVariants = Array.Empty<PlaygroundVariant>();
    private static readonly IReadOnlyList<PlaygroundState> EmptyStates = Array.Empty<PlaygroundState>();

    internal static (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) Build(
        string id,
        bool forceDisabled)
    {
        switch (id)
        {
            case "stack": return StackDemo();
            case "column": return ColumnDemo();
            case "row": return RowDemo();
            case "hstack": return HStackDemo();
            case "grid": return GridDemo();
            case "wrap": return WrapDemo();
            case "scrollarea": return ScrollAreaDemo();
            case "divider": return DividerDemo();
            case "spacer": return SpacerDemo();
            case "each": return EachDemo();
            case "conditional": return ConditionalDemo();

            case "card": return CardDemo();
            case "box": return BoxDemo();
            case "panel": return PanelDemo();
            case "surface": return SurfaceDemo();

            case "heading": return HeadingDemo();
            case "text": return TextDemo();
            case "label": return LabelDemo();
            case "caption": return CaptionDemo();
            case "richtext": return RichTextDemo();
            case "code": return CodeDemo();
            case "icon": return IconDemo();

            default:
                return (EmptyVariants, EmptyStates);
        }
    }

    private static LightweaveNode Chip(string text, ThemeSlot bg, ThemeSlot fg)
    {
        return Surface.Surface.Box(
            padding: EdgeInsets.All(SpacingScale.Xs),
            background: new BackgroundSpec.Solid(bg),
            border: null,
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Text(text, FontRole.Body, new Rem(0.8125f), fg, TextAlign.Center)));
    }

    private static LightweaveNode SampleChip(string text) => Chip(text, ThemeSlot.SurfaceRaised, ThemeSlot.TextPrimary);

    private static LightweaveNode AccentChip(string text) => Chip(text, ThemeSlot.SurfaceAccent, ThemeSlot.TextOnAccent);

    private static LightweaveNode MutedChip(string text) => Chip(text, ThemeSlot.SurfaceSunken, ThemeSlot.TextMuted);

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) StackDemo()
    {
        PlaygroundVariant tight = new PlaygroundVariant(
            "CC_Playground_Label_Tight",
            Layout.Layout.Stack(
                gap: SpacingScale.Xxs,
                children: s =>
                {
                    s.Add(SampleChip("A"), 14f);
                    s.Add(SampleChip("B"), 14f);
                    s.Add(SampleChip("C"), 14f);
                }));

        PlaygroundVariant loose = new PlaygroundVariant(
            "CC_Playground_Label_Loose",
            Layout.Layout.Stack(
                gap: SpacingScale.Sm,
                children: s =>
                {
                    s.Add(SampleChip("A"), 10f);
                    s.Add(SampleChip("B"), 10f);
                    s.Add(SampleChip("C"), 10f);
                }));

        return (new[] { tight, loose }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ColumnDemo()
    {
        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Column(
                gap: SpacingScale.Xxs,
                children: k =>
                {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                }));

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RowDemo()
    {
        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Row(
                gap: SpacingScale.Xs,
                children: k =>
                {
                    k.Add(SampleChip("A"));
                    k.Add(SampleChip("B"));
                    k.Add(SampleChip("C"));
                }));

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) HStackDemo()
    {
        PlaygroundVariant mix = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.HStack(
                gap: SpacingScale.Xs,
                children: r =>
                {
                    r.Add(SampleChip("48"), 48f);
                    r.AddFlex(AccentChip("flex"));
                    r.Add(SampleChip("32"), 32f);
                }));

        return (new[] { mix }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) GridDemo()
    {
        List<GridTrack> columns = new List<GridTrack>
        {
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
        };

        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Grid(
                columns: columns,
                gap: SpacingScale.Xs,
                children: k =>
                {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                }));

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) WrapDemo()
    {
        PlaygroundVariant wrap = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Wrap(
                gap: SpacingScale.Xs,
                minChildWidth: new Rem(3f),
                children: k =>
                {
                    k.Add(SampleChip("one"));
                    k.Add(SampleChip("two"));
                    k.Add(SampleChip("three"));
                    k.Add(SampleChip("four"));
                }));

        return (new[] { wrap }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ScrollAreaDemo()
    {
        PlaygroundVariant scroll = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.ScrollArea(
                Layout.Layout.Stack(
                    gap: SpacingScale.Xxs,
                    children: s =>
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            s.Add(SampleChip("row " + (i + 1)), 18f);
                        }
                    })));

        return (new[] { scroll }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DividerDemo()
    {
        PlaygroundVariant horizontal = new PlaygroundVariant(
            "CC_Playground_Label_Horizontal",
            Layout.Layout.Stack(
                gap: SpacingScale.Xxs,
                children: s =>
                {
                    s.Add(Typography.Typography.Caption("above"), 14f);
                    s.Add(Layout.Layout.Divider.Horizontal(), 2f);
                    s.Add(Typography.Typography.Caption("below"), 14f);
                }));

        PlaygroundVariant vertical = new PlaygroundVariant(
            "CC_Playground_Label_Vertical",
            Layout.Layout.Row(
                gap: SpacingScale.Xs,
                children: r =>
                {
                    r.Add(Typography.Typography.Caption("left"));
                    r.Add(Layout.Layout.Divider.Vertical());
                    r.Add(Typography.Typography.Caption("right"));
                }));

        return (new[] { horizontal, vertical }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SpacerDemo()
    {
        PlaygroundVariant flex = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.HStack(
                gap: SpacingScale.Xs,
                children: r =>
                {
                    r.Add(SampleChip("start"), 48f);
                    r.AddFlex(Layout.Layout.Spacer.Flex());
                    r.Add(SampleChip("end"), 48f);
                }));

        return (new[] { flex }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) EachDemo()
    {
        string[] items = new[] { "A", "B", "C" };
        PlaygroundVariant each = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Row(
                gap: SpacingScale.Xs,
                children: r =>
                {
                    r.Add(Layout.Layout.Each(
                        items,
                        (item, _) => SampleChip(item),
                        item => item));
                }));

        return (new[] { each }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ConditionalDemo()
    {
        PlaygroundVariant on = new PlaygroundVariant(
            "CC_Playground_Label_True",
            Layout.Layout.Conditional(
                when: true,
                children: () => AccentChip((string)"CC_Playground_Conditional_On".Translate())));

        PlaygroundVariant off = new PlaygroundVariant(
            "CC_Playground_Label_False",
            Surface.Surface.Box(
                padding: EdgeInsets.All(SpacingScale.Xs),
                background: new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
                border: null,
                radius: RadiusSpec.All(new Rem(0.25f)),
                children: k => k.Add(
                    Layout.Layout.Conditional(
                        when: false,
                        children: () => AccentChip("hidden")))));

        return (new[] { on, off }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CardDemo()
    {
        PlaygroundVariant plain = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Surface.Surface.Card(c =>
                c.Add(Typography.Typography.Text("card content", FontRole.Body, new Rem(0.875f), ThemeSlot.TextPrimary))));

        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BoxDemo()
    {
        LightweaveNode raised = Surface.Surface.Box(
            padding: EdgeInsets.All(SpacingScale.Sm),
            background: new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            border: BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption("raised")));

        LightweaveNode sunken = Surface.Surface.Box(
            padding: EdgeInsets.All(SpacingScale.Sm),
            background: new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
            border: null,
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption("sunken")));

        LightweaveNode accent = Surface.Surface.Box(
            padding: EdgeInsets.All(SpacingScale.Sm),
            background: new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
            border: null,
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Text("accent", FontRole.Body, new Rem(0.8125f), ThemeSlot.TextOnAccent)));

        return (new[]
        {
            new PlaygroundVariant("CC_Playground_Label_Raised", raised),
            new PlaygroundVariant("CC_Playground_Label_Sunken", sunken),
            new PlaygroundVariant("CC_Playground_Label_Accent", accent),
        }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) PanelDemo()
    {
        LightweaveNode panel = Surface.Surface.Panel(
            title: Typography.Typography.Heading(3, "Panel"),
            body: Typography.Typography.Caption("body copy"));

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", panel) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SurfaceDemo()
    {
        LightweaveNode primary = Surface.Surface.ByRole(SurfaceRole.Primary,
            padding: EdgeInsets.All(SpacingScale.Sm),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption("primary")));

        LightweaveNode raised = Surface.Surface.ByRole(SurfaceRole.Raised,
            padding: EdgeInsets.All(SpacingScale.Sm),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption("raised")));

        LightweaveNode sunken = Surface.Surface.ByRole(SurfaceRole.Sunken,
            padding: EdgeInsets.All(SpacingScale.Sm),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption("sunken")));

        LightweaveNode accent = Surface.Surface.ByRole(SurfaceRole.Accent,
            padding: EdgeInsets.All(SpacingScale.Sm),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Text("accent", FontRole.Body, new Rem(0.8125f), ThemeSlot.TextOnAccent)));

        return (new[]
        {
            new PlaygroundVariant("CC_Playground_Label_Primary", primary),
            new PlaygroundVariant("CC_Playground_Label_Raised", raised),
            new PlaygroundVariant("CC_Playground_Label_Sunken", sunken),
            new PlaygroundVariant("CC_Playground_Label_Accent", accent),
        }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) HeadingDemo()
    {
        PlaygroundVariant h1 = new PlaygroundVariant("CC_Playground_Label_Large",
            Typography.Typography.Heading(1, "Heading 1"));
        PlaygroundVariant h2 = new PlaygroundVariant("CC_Playground_Label_Medium",
            Typography.Typography.Heading(2, "Heading 2"));
        PlaygroundVariant h3 = new PlaygroundVariant("CC_Playground_Label_Small",
            Typography.Typography.Heading(3, "Heading 3"));

        return (new[] { h1, h2, h3 }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TextDemo()
    {
        string sample = (string)"CC_Playground_Text_Sample".Translate();

        PlaygroundVariant normal = new PlaygroundVariant("CC_Playground_Label_Normal",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary));
        PlaygroundVariant accented = new PlaygroundVariant("CC_Playground_Label_Accented",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.SurfaceAccent, TextAlign.Start, FontStyle.Bold));
        PlaygroundVariant muted = new PlaygroundVariant("CC_Playground_Label_Muted",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary));
        PlaygroundState mutedState = new PlaygroundState("CC_Playground_Label_Muted",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted));

        return (new[] { normal, accented, muted }, new[] { defaultState, mutedState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) LabelDemo()
    {
        PlaygroundVariant plain = new PlaygroundVariant("CC_Playground_Label_Default",
            Typography.Typography.Label("Storm warnings"));
        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CaptionDemo()
    {
        PlaygroundVariant plain = new PlaygroundVariant("CC_Playground_Label_Default",
            Typography.Typography.Caption("Updated moments ago"));
        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RichTextDemo()
    {
        string raw = (string)"CC_Playground_RichText_Sample".Translate();
        TaggedString tagged = new TaggedString(raw);
        PlaygroundVariant rich = new PlaygroundVariant("CC_Playground_Label_Default",
            Typography.Typography.RichText(tagged));
        return (new[] { rich }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CodeDemo()
    {
        PlaygroundVariant plain = new PlaygroundVariant("CC_Playground_Label_Default",
            Typography.Typography.Code((string)"CC_Playground_Code_Sample".Translate()));
        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) IconDemo()
    {
        Texture tex = Texture2D.whiteTexture;

        LightweaveNode defaultIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.TextPrimary);
        LightweaveNode accentIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.SurfaceAccent);
        LightweaveNode mutedIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.TextMuted);

        return (new[]
        {
            new PlaygroundVariant("CC_Playground_Label_Default", defaultIcon),
            new PlaygroundVariant("CC_Playground_Label_Accent", accentIcon),
            new PlaygroundVariant("CC_Playground_Label_Muted", mutedIcon),
        }, EmptyStates);
    }
}
