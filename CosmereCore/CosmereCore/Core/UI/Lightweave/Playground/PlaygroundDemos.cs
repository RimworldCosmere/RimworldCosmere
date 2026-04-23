using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Feedback;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Navigation;
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

            case "button": return ButtonDemo(forceDisabled);
            case "iconbutton": return IconButtonDemo(forceDisabled);
            case "togglebutton": return ToggleButtonDemo(forceDisabled);

            case "checkbox": return CheckboxDemo(forceDisabled);
            case "switch": return SwitchDemo(forceDisabled);
            case "radio": return RadioDemo(forceDisabled);
            case "slider": return SliderDemo(forceDisabled);
            case "textfield": return TextFieldDemo(forceDisabled);
            case "textarea": return TextAreaDemo(forceDisabled);
            case "numberfield": return NumberFieldDemo(forceDisabled);
            case "searchfield": return SearchFieldDemo(forceDisabled);
            case "dropdown": return DropdownDemo(forceDisabled);
            case "colorpicker": return ColorPickerDemo(forceDisabled);
            case "keybinding": return KeyBindingDemo(forceDisabled);

            case "spinner": return SpinnerDemo();
            case "progressbar": return ProgressBarDemo();
            case "ringgauge": return RingGaugeDemo();
            case "sparkline": return SparklineDemo();
            case "badge": return BadgeDemo();
            case "tag": return TagDemo();
            case "tooltip": return TooltipDemo();

            case "tabs": return TabsDemo();
            case "segmented": return SegmentedDemo();
            case "breadcrumbs": return BreadcrumbsDemo();
            case "menu": return MenuDemo();
            case "contextmenu": return ContextMenuDemo();

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

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ButtonDemo(bool forceDisabled)
    {
        PlaygroundVariant primary = new PlaygroundVariant("CC_Playground_Label_Primary",
            Button.Create("Primary", () => { }, ButtonVariant.Primary, disabled: forceDisabled));
        PlaygroundVariant secondary = new PlaygroundVariant("CC_Playground_Label_Secondary",
            Button.Create("Secondary", () => { }, ButtonVariant.Secondary, disabled: forceDisabled));
        PlaygroundVariant ghost = new PlaygroundVariant("CC_Playground_Label_Ghost",
            Button.Create("Ghost", () => { }, ButtonVariant.Ghost, disabled: forceDisabled));
        PlaygroundVariant danger = new PlaygroundVariant("CC_Playground_Label_Danger",
            Button.Create("Danger", () => { }, ButtonVariant.Danger, disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Button.Create("Default", () => { }, ButtonVariant.Primary, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            Button.Create("Hover me", () => { }, ButtonVariant.Primary, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            Button.Create("Disabled", () => { }, ButtonVariant.Primary, disabled: true));

        return (new[] { primary, secondary, ghost, danger }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) IconButtonDemo(bool forceDisabled)
    {
        LightweaveNode MakeIcon() => Typography.Typography.Icon(Texture2D.whiteTexture, new Rem(1f), ThemeSlot.TextPrimary);

        PlaygroundVariant ghost = new PlaygroundVariant("CC_Playground_Label_Ghost",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Ghost, disabled: forceDisabled));
        PlaygroundVariant primary = new PlaygroundVariant("CC_Playground_Label_Primary",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Primary, disabled: forceDisabled));
        PlaygroundVariant secondary = new PlaygroundVariant("CC_Playground_Label_Secondary",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Secondary, disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Ghost, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Ghost, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Ghost, disabled: true));

        return (new[] { ghost, primary, secondary }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ToggleButtonDemo(bool forceDisabled)
    {
        _ = forceDisabled;
        Hooks.Hooks.StateHandle<bool> onValue = Hooks.Hooks.UseState<bool>(true);
        Hooks.Hooks.StateHandle<bool> offValue = Hooks.Hooks.UseState<bool>(false);

        PlaygroundVariant on = new PlaygroundVariant("CC_Playground_Label_On",
            ToggleButton.Create("On", onValue.Value, v => onValue.Set(v)));
        PlaygroundVariant off = new PlaygroundVariant("CC_Playground_Label_Off",
            ToggleButton.Create("Off", offValue.Value, v => offValue.Set(v)));

        return (new[] { on, off }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CheckboxDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<bool> checkedState = Hooks.Hooks.UseState<bool>(true);
        Hooks.Hooks.StateHandle<bool> uncheckedState = Hooks.Hooks.UseState<bool>(false);

        PlaygroundVariant onVariant = new PlaygroundVariant("CC_Playground_Label_True",
            Checkbox.Create("Enabled", checkedState.Value, v => checkedState.Set(v), disabled: forceDisabled));
        PlaygroundVariant offVariant = new PlaygroundVariant("CC_Playground_Label_False",
            Checkbox.Create("Disabled", uncheckedState.Value, v => uncheckedState.Set(v), disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Checkbox.Create("Default", true, _ => { }, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            Checkbox.Create("Hover", false, _ => { }, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            Checkbox.Create("Disabled", true, _ => { }, disabled: true));

        return (new[] { onVariant, offVariant }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SwitchDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<bool> onState = Hooks.Hooks.UseState<bool>(true);
        Hooks.Hooks.StateHandle<bool> offState = Hooks.Hooks.UseState<bool>(false);

        PlaygroundVariant onVariant = new PlaygroundVariant("CC_Playground_Label_On",
            Switch.Create((string)"CC_Playground_Controls_Switch_Label".Translate(), onState.Value, v => onState.Set(v), disabled: forceDisabled));
        PlaygroundVariant offVariant = new PlaygroundVariant("CC_Playground_Label_Off",
            Switch.Create("Off", offState.Value, v => offState.Set(v), disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Switch.Create("Default", true, _ => { }, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            Switch.Create("Hover", false, _ => { }, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            Switch.Create("Disabled", true, _ => { }, disabled: true));

        return (new[] { onVariant, offVariant }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RadioDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<int> selection = Hooks.Hooks.UseState<int>(1);

        LightweaveNode group = Radio.Group<int>(
            value: selection.Value,
            onChange: v => selection.Set(v),
            children: k =>
            {
                k.Add(Radio.Item<int>((string)"CC_Playground_Controls_Radio_OptionA".Translate(), 0, disabled: forceDisabled));
                k.Add(Radio.Item<int>((string)"CC_Playground_Controls_Radio_OptionB".Translate(), 1, disabled: forceDisabled));
                k.Add(Radio.Item<int>((string)"CC_Playground_Controls_Radio_OptionC".Translate(), 2, disabled: forceDisabled));
            });

        PlaygroundVariant groupVariant = new PlaygroundVariant("CC_Playground_Label_Default", group);

        return (new[] { groupVariant }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SliderDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<float> sliderValue = Hooks.Hooks.UseState<float>(0.4f);

        PlaygroundVariant smooth = new PlaygroundVariant("CC_Playground_Label_Default",
            Slider.Create(sliderValue.Value, v => sliderValue.Set(v), 0f, 1f, disabled: forceDisabled));
        PlaygroundVariant stepped = new PlaygroundVariant("CC_Playground_Label_Accented",
            Slider.Create(sliderValue.Value, v => sliderValue.Set(v), 0f, 1f, step: 0.25f,
                marks: new float[] { 0f, 0.25f, 0.5f, 0.75f, 1f }, disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Slider.Create(0.4f, _ => { }, 0f, 1f, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            Slider.Create(0.7f, _ => { }, 0f, 1f, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            Slider.Create(0.4f, _ => { }, 0f, 1f, disabled: true));

        return (new[] { smooth, stepped }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TextFieldDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<string> text = Hooks.Hooks.UseState<string>("Stormlight");

        PlaygroundVariant filled = new PlaygroundVariant("CC_Playground_Label_Filled",
            TextField.Create(text.Value, v => text.Set(v),
                placeholder: (string)"CC_Playground_Controls_TextField_Placeholder".Translate(),
                disabled: forceDisabled));
        PlaygroundVariant empty = new PlaygroundVariant("CC_Playground_Label_Empty",
            TextField.Create(string.Empty, _ => { },
                placeholder: (string)"CC_Playground_Controls_TextField_Placeholder".Translate(),
                disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            TextField.Create("Default", _ => { }, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            TextField.Create("Hover", _ => { }, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            TextField.Create("Disabled", _ => { }, disabled: true));

        return (new[] { filled, empty }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TextAreaDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<string> text = Hooks.Hooks.UseState<string>("Multi-line sample.");

        PlaygroundVariant filled = new PlaygroundVariant("CC_Playground_Label_Filled",
            TextArea.Create(text.Value, v => text.Set(v),
                placeholder: (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
                minRows: 2, maxRows: 3, disabled: forceDisabled));
        PlaygroundVariant empty = new PlaygroundVariant("CC_Playground_Label_Empty",
            TextArea.Create(string.Empty, _ => { },
                placeholder: (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
                minRows: 2, maxRows: 3, disabled: forceDisabled));

        return (new[] { filled, empty }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) NumberFieldDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<float> number = Hooks.Hooks.UseState<float>(42f);

        PlaygroundVariant bounded = new PlaygroundVariant("CC_Playground_Label_Default",
            NumberField.Create(number.Value, v => number.Set(v), 0f, 100f,
                placeholder: (string)"CC_Playground_Controls_NumberField_Label".Translate(),
                disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            NumberField.Create(42f, _ => { }, 0f, 100f, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            NumberField.Create(7f, _ => { }, 0f, 100f, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            NumberField.Create(13f, _ => { }, 0f, 100f, disabled: true));

        return (new[] { bounded }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SearchFieldDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<string> query = Hooks.Hooks.UseState<string>(string.Empty);

        PlaygroundVariant empty = new PlaygroundVariant("CC_Playground_Label_Empty",
            SearchField.Create(query.Value, v => query.Set(v),
                placeholder: (string)"CC_Playground_SearchField_Placeholder".Translate(),
                disabled: forceDisabled));
        PlaygroundVariant filled = new PlaygroundVariant("CC_Playground_Label_Filled",
            SearchField.Create("highstorm", _ => { }, disabled: forceDisabled));

        return (new[] { empty, filled }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DropdownDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("Scadrial");

        string[] options = new[] { "Roshar", "Scadrial", "Nalthis", "Taldain", "Ashyn" };

        PlaygroundVariant choice = new PlaygroundVariant("CC_Playground_Label_Default",
            Dropdown.Create<string>(selected.Value, options, v => v, v => selected.Set(v), disabled: forceDisabled));

        PlaygroundState defaultState = new PlaygroundState("CC_Playground_Label_Default",
            Dropdown.Create<string>("Scadrial", options, v => v, _ => { }, disabled: forceDisabled));
        PlaygroundState hoverState = new PlaygroundState("CC_Playground_Label_Hover",
            Dropdown.Create<string>("Roshar", options, v => v, _ => { }, disabled: forceDisabled));
        PlaygroundState disabledState = new PlaygroundState("CC_Playground_Label_Disabled",
            Dropdown.Create<string>("Nalthis", options, v => v, _ => { }, disabled: true));

        return (new[] { choice }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ColorPickerDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<Color> chosen = Hooks.Hooks.UseState<Color>(new Color(0.25f, 0.42f, 0.30f));

        PlaygroundVariant picker = new PlaygroundVariant("CC_Playground_Label_Default",
            ColorPicker.Create(chosen.Value, v => chosen.Set(v), disabled: forceDisabled));

        return (new[] { picker }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) KeyBindingDemo(bool forceDisabled)
    {
        Hooks.Hooks.StateHandle<KeyBinding> binding = Hooks.Hooks.UseState<KeyBinding>(
            new KeyBinding(KeyCode.F, KeyModifiers.Control));

        PlaygroundVariant cast = new PlaygroundVariant("CC_Playground_Label_Default",
            KeyBindingField.Create(binding.Value, v => binding.Set(v), disabled: forceDisabled));

        return (new[] { cast }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SpinnerDemo()
    {
        PlaygroundVariant small = new PlaygroundVariant("CC_Playground_Label_Small",
            Spinner.Create(new Rem(1f)));
        PlaygroundVariant medium = new PlaygroundVariant("CC_Playground_Label_Medium",
            Spinner.Create(new Rem(1.5f)));
        PlaygroundVariant large = new PlaygroundVariant("CC_Playground_Label_Large",
            Spinner.Create(new Rem(2f)));

        return (new[] { small, medium, large }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ProgressBarDemo()
    {
        PlaygroundVariant accent = new PlaygroundVariant("CC_Playground_Label_Accent",
            ProgressBar.Create(0.65f, 0f, 1f, "65%", BadgeVariant.Accent));
        PlaygroundVariant success = new PlaygroundVariant("CC_Playground_Label_Default",
            ProgressBar.Create(0.35f, 0f, 1f, "35%", BadgeVariant.Success));
        PlaygroundVariant danger = new PlaygroundVariant("CC_Playground_Label_Danger",
            ProgressBar.Create(0.9f, 0f, 1f, "90%", BadgeVariant.Danger));

        return (new[] { accent, success, danger }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RingGaugeDemo()
    {
        PlaygroundVariant low = new PlaygroundVariant("CC_Playground_Label_Small",
            RingGauge.Create(0.25f));
        PlaygroundVariant mid = new PlaygroundVariant("CC_Playground_Label_Medium",
            RingGauge.Create(0.6f));
        PlaygroundVariant high = new PlaygroundVariant("CC_Playground_Label_Large",
            RingGauge.Create(0.95f));

        return (new[] { low, mid, high }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SparklineDemo()
    {
        float[] rising = new float[] { 1f, 2f, 3f, 5f, 8f, 13f };
        float[] wavy = new float[] { 3f, 5f, 2f, 7f, 4f, 6f, 2f };
        float[] flat = new float[] { 4f, 4f, 4f, 4f, 4f };

        PlaygroundVariant risingSpark = new PlaygroundVariant("CC_Playground_Label_Accent",
            Sparkline.Create(rising));
        PlaygroundVariant wavySpark = new PlaygroundVariant("CC_Playground_Label_Default",
            Sparkline.Create(wavy));
        PlaygroundVariant flatSpark = new PlaygroundVariant("CC_Playground_Label_Muted",
            Sparkline.Create(flat, ThemeSlot.TextMuted));

        return (new[] { risingSpark, wavySpark, flatSpark }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BadgeDemo()
    {
        PlaygroundVariant neutral = new PlaygroundVariant("CC_Playground_Feedback_Badge_Neutral",
            Badge.Create((string)"CC_Playground_Feedback_Badge_Neutral".Translate(), BadgeVariant.Neutral));
        PlaygroundVariant accent = new PlaygroundVariant("CC_Playground_Feedback_Badge_Accent",
            Badge.Create((string)"CC_Playground_Feedback_Badge_Accent".Translate(), BadgeVariant.Accent));
        PlaygroundVariant warning = new PlaygroundVariant("CC_Playground_Feedback_Badge_Warning",
            Badge.Create((string)"CC_Playground_Feedback_Badge_Warning".Translate(), BadgeVariant.Warning));
        PlaygroundVariant danger = new PlaygroundVariant("CC_Playground_Feedback_Badge_Danger",
            Badge.Create((string)"CC_Playground_Feedback_Badge_Danger".Translate(), BadgeVariant.Danger));
        PlaygroundVariant success = new PlaygroundVariant("CC_Playground_Feedback_Badge_Success",
            Badge.Create((string)"CC_Playground_Feedback_Badge_Success".Translate(), BadgeVariant.Success));

        return (new[] { neutral, accent, warning, danger, success }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TagDemo()
    {
        PlaygroundVariant staticTag = new PlaygroundVariant("CC_Playground_Feedback_Tag_Static",
            Tag.Create((string)"CC_Playground_Feedback_Tag_Static".Translate(), onDismiss: null, BadgeVariant.Neutral));
        PlaygroundVariant dismissTag = new PlaygroundVariant("CC_Playground_Feedback_Tag_Dismissible",
            Tag.Create((string)"CC_Playground_Feedback_Tag_Dismissible".Translate(), onDismiss: () => { }, BadgeVariant.Accent));

        return (new[] { staticTag, dismissTag }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TooltipDemo()
    {
        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Feedback_Tooltip_Button_Label".Translate(),
            () => { },
            ButtonVariant.Secondary);
        LightweaveNode body = Typography.Typography.Text(
            (string)"CC_Playground_Feedback_Tooltip_Button_Body".Translate(),
            FontRole.Body,
            new Rem(0.8125f),
            ThemeSlot.TextPrimary);

        PlaygroundVariant wrapped = new PlaygroundVariant("CC_Playground_Label_Default",
            Tooltip.Wrap(trigger, body));

        return (new[] { wrapped }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TabsDemo()
    {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("general");

        string[] tabs = new[] { "general", "combat", "storage" };
        LightweaveNode tabsNode = Tabs.Create<string>(
            value: selected.Value,
            items: tabs,
            labelFn: v => v switch
            {
                "combat" => (string)"CC_Playground_Navigation_Tabs_Combat".Translate(),
                "storage" => (string)"CC_Playground_Navigation_Tabs_Storage".Translate(),
                _ => (string)"CC_Playground_Navigation_Tabs_General".Translate(),
            },
            onChange: v => selected.Set(v),
            bodyFn: v => Typography.Typography.Caption(v switch
            {
                "combat" => (string)"CC_Playground_Navigation_Tabs_Body_Combat".Translate(),
                "storage" => (string)"CC_Playground_Navigation_Tabs_Body_Storage".Translate(),
                _ => (string)"CC_Playground_Navigation_Tabs_Body_General".Translate(),
            }));

        PlaygroundVariant threeTabs = new PlaygroundVariant("CC_Playground_Label_Default", tabsNode);

        return (new[] { threeTabs }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SegmentedDemo()
    {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("all");

        string[] segments = new[] { "all", "armor", "weapons", "tools" };
        LightweaveNode segNode = Segmented.Create<string>(
            value: selected.Value,
            items: segments,
            labelFn: v => v switch
            {
                "armor" => (string)"CC_Playground_Navigation_Segmented_Armor".Translate(),
                "weapons" => (string)"CC_Playground_Navigation_Segmented_Weapons".Translate(),
                "tools" => (string)"CC_Playground_Navigation_Segmented_Tools".Translate(),
                _ => (string)"CC_Playground_Navigation_Segmented_All".Translate(),
            },
            onChange: v => selected.Set(v));

        PlaygroundVariant four = new PlaygroundVariant("CC_Playground_Label_Default", segNode);

        return (new[] { four }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BreadcrumbsDemo()
    {
        string[] path = new[]
        {
            (string)"CC_Playground_Breadcrumbs_Crumb_Worlds".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate(),
        };

        PlaygroundVariant crumbs = new PlaygroundVariant("CC_Playground_Label_Default",
            Breadcrumbs.Create(path));

        return (new[] { crumbs }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) MenuDemo()
    {
        Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState<bool>(false);
        Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef<Rect>(default(Rect));

        List<MenuItem> items = new List<MenuItem>
        {
            new MenuItem((string)"CC_Playground_Navigation_Menu_Open".Translate(), () => open.Set(false)),
            new MenuItem((string)"CC_Playground_Navigation_Menu_Save".Translate(), () => open.Set(false)),
            new MenuItem((string)"CC_Playground_Navigation_Menu_SaveAs".Translate(), () => open.Set(false)),
            new MenuItem((string)"CC_Playground_Navigation_Menu_Close".Translate(), () => open.Set(false)),
        };

        LightweaveNode trigger = NodeBuilder.New("MenuTrigger", 0, nameof(PlaygroundDemos));
        LightweaveNode button = Button.Create(
            (string)"CC_Playground_Menu_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary);
        trigger.Children.Add(button);
        trigger.Paint = (rect, _) =>
        {
            anchor.Current = rect;
            button.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(button, rect);
        };

        LightweaveNode menu = Menu.Create(
            isOpen: open.Value,
            anchorRect: anchor.Current,
            items: items,
            onDismiss: () => open.Set(false),
            instanceKey: "playground-menu");

        LightweaveNode composed = NodeBuilder.New("MenuHost", 0, nameof(PlaygroundDemos));
        composed.Children.Add(trigger);
        composed.Children.Add(menu);
        composed.Paint = (rect, _) =>
        {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            menu.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(menu, rect);
        };

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", composed) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ContextMenuDemo()
    {
        List<MenuItem> items = new List<MenuItem>
        {
            new MenuItem((string)"CC_Playground_ContextMenu_Inspect".Translate(), () => { }),
            new MenuItem((string)"CC_Playground_ContextMenu_Rename".Translate(), () => { }),
            new MenuItem((string)"CC_Playground_ContextMenu_Duplicate".Translate(), () => { }),
            new MenuItem((string)"CC_Playground_ContextMenu_Delete".Translate(), () => { }),
        };

        LightweaveNode target = Surface.Surface.Box(
            padding: EdgeInsets.All(SpacingScale.Sm),
            background: new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            border: BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            radius: RadiusSpec.All(new Rem(0.25f)),
            children: c => c.Add(Typography.Typography.Caption(
                (string)"CC_Playground_ContextMenu_RightClick".Translate())));

        PlaygroundVariant wrapped = new PlaygroundVariant("CC_Playground_Label_Default",
            Cosmere.Core.UI.Lightweave.Navigation.ContextMenu.Create(target, items));

        return (new[] { wrapped }, EmptyStates);
    }
}
