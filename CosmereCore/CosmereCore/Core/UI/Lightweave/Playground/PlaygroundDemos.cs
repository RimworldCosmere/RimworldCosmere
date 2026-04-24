using System;
using Cosmere.Core.UI.Lightweave.Data;
using Cosmere.Core.UI.Lightweave.Feedback;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Navigation;
using Cosmere.Core.UI.Lightweave.Overlay;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;
using ContextMenu = Cosmere.Core.UI.Lightweave.Navigation.ContextMenu;
using Tree = Cosmere.Core.UI.Lightweave.Data.Tree;
using TreeNode = Cosmere.Core.UI.Lightweave.Data.TreeNode;

namespace Cosmere.Core.UI.Lightweave.Playground;

internal static class PlaygroundDemos {
    private static readonly IReadOnlyList<PlaygroundVariant> EmptyVariants = Array.Empty<PlaygroundVariant>();
    private static readonly IReadOnlyList<PlaygroundState> EmptyStates = Array.Empty<PlaygroundState>();

    internal static (IReadOnlyList<PlaygroundVariant> variants, IReadOnlyList<PlaygroundState> states) Build(
        string id,
        bool forceDisabled
    ) {
        switch (id) {
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
            case "carousel": return CarouselDemo();
            case "container": return ContainerDemo();

            case "card": return CardDemo();
            case "box": return BoxDemo();

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
            case "buttongroup": return ButtonGroupDemo(forceDisabled);

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
            case "tooltip": return TooltipDemo();

            case "tabs": return TabsDemo();
            case "segmented": return SegmentedDemo();
            case "breadcrumbs": return BreadcrumbsDemo();
            case "menu": return MenuDemo();
            case "contextmenu": return ContextMenuDemo();
            case "accordion": return AccordionDemo();

            case "window": return WindowDemo();
            case "dialog": return DialogDemo();
            case "popover": return PopoverDemo();
            case "drawer": return DrawerDemo();
            case "toast": return ToastDemo();

            case "list": return ListDemo();
            case "table": return TableDemo();
            case "tree": return TreeDemo();
            case "keyvalue": return KeyValueDemo();

            case "usestate": return UseStateDemo();
            case "useanim": return UseAnimDemo();
            case "usefocus": return UseFocusDemo();
            case "usehotkey": return UseHotkeyDemo();

            default:
                return (EmptyVariants, EmptyStates);
        }
    }

    private static LightweaveNode Chip(string text, ThemeSlot bg, ThemeSlot fg) {
        return Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Xs),
            new BackgroundSpec.Solid(bg),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Typography.Typography.Text(text, FontRole.Body, new Rem(0.8125f), fg, TextAlign.Center))
        );
    }

    private static LightweaveNode SampleChip(string text) {
        return Chip(text, ThemeSlot.SurfaceSunken, ThemeSlot.TextPrimary);
    }

    private static LightweaveNode AccentChip(string text) {
        return Chip(text, ThemeSlot.SurfaceAccent, ThemeSlot.TextOnAccent);
    }

    private static LightweaveNode MutedChip(string text) {
        return Chip(text, ThemeSlot.SurfaceSunken, ThemeSlot.TextMuted);
    }

    private static LightweaveNode CenterFixed(LightweaveNode child, float width, float height) {
        LightweaveNode node = NodeBuilder.New("CenterFixed", 0, nameof(PlaygroundDemos));
        node.Children.Add(child);
        node.Paint = (rect, _) => {
            float w = Mathf.Min(width, rect.width);
            float h = Mathf.Min(height, rect.height);
            float x = rect.x + (rect.width - w) * 0.5f;
            float y = rect.y + (rect.height - h) * 0.5f;
            Rect inner = new Rect(x, y, w, h);
            child.MeasuredRect = inner;
            LightweaveRoot.PaintSubtree(child, inner);
        };
        return node;
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) StackDemo() {
        PlaygroundVariant tight = new PlaygroundVariant(
            "CC_Playground_Label_Tight",
            Layout.Layout.Stack(
                SpacingScale.Xxs,
                s => {
                    s.Add(SampleChip("A"), new Rem(1.75f).ToPixels());
                    s.Add(SampleChip("B"), new Rem(1.75f).ToPixels());
                    s.Add(SampleChip("C"), new Rem(1.75f).ToPixels());
                }
            )
        );

        PlaygroundVariant loose = new PlaygroundVariant(
            "CC_Playground_Label_Loose",
            Layout.Layout.Stack(
                SpacingScale.Sm,
                s => {
                    s.Add(SampleChip("A"), new Rem(2f).ToPixels());
                    s.Add(SampleChip("B"), new Rem(2f).ToPixels());
                    s.Add(SampleChip("C"), new Rem(2f).ToPixels());
                }
            )
        );

        return (new[] { tight, loose }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ColumnDemo() {
        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Column(
                SpacingScale.Xxs,
                children: k => {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                }
            )
        );

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RowDemo() {
        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Row(
                SpacingScale.Xs,
                children: k => {
                    k.Add(SampleChip("A"));
                    k.Add(SampleChip("B"));
                    k.Add(SampleChip("C"));
                }
            )
        );

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) HStackDemo() {
        PlaygroundVariant mix = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.HStack(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("48"), 48f);
                    r.AddFlex(AccentChip("flex"));
                    r.Add(SampleChip("32"), 32f);
                }
            )
        );

        return (new[] { mix }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) GridDemo() {
        List<GridTrack> columns = new List<GridTrack> {
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
            new GridTrack.Fr(1f),
        };

        PlaygroundVariant three = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Grid(
                columns,
                SpacingScale.Xs,
                k => {
                    k.Add(SampleChip("1"));
                    k.Add(SampleChip("2"));
                    k.Add(SampleChip("3"));
                }
            )
        );

        return (new[] { three }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) WrapDemo() {
        PlaygroundVariant wrap = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Wrap(
                SpacingScale.Xs,
                new Rem(3f),
                k => {
                    k.Add(SampleChip("one"));
                    k.Add(SampleChip("two"));
                    k.Add(SampleChip("three"));
                    k.Add(SampleChip("four"));
                }
            )
        );

        return (new[] { wrap }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ScrollAreaDemo() {
        PlaygroundVariant withBar = new PlaygroundVariant(
            "CC_Playground_ScrollArea_WithBar",
            Layout.Layout.ScrollArea(
                Layout.Layout.Stack(
                    SpacingScale.Xxs,
                    s => {
                        for (int i = 0; i < 20; i++) {
                            s.Add(SampleChip("row " + (i + 1)), new Rem(1.75f).ToPixels());
                        }
                    }
                )
            )
        );

        PlaygroundVariant noBar = new PlaygroundVariant(
            "CC_Playground_ScrollArea_NoBar",
            Layout.Layout.ScrollArea(
                Layout.Layout.Stack(
                    SpacingScale.Xxs,
                    s => {
                        for (int i = 0; i < 20; i++) {
                            s.Add(SampleChip("row " + (i + 1)), new Rem(1.75f).ToPixels());
                        }
                    }
                ),
                showScrollbar: false
            )
        );

        return (new[] { withBar, noBar }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DividerDemo() {
        PlaygroundVariant horizontal = new PlaygroundVariant(
            "CC_Playground_Label_Horizontal",
            Layout.Layout.Stack(
                SpacingScale.Xxs,
                s => {
                    s.Add(Typography.Typography.Caption("above"), 14f);
                    s.Add(Layout.Layout.Divider.Horizontal(), 2f);
                    s.Add(Typography.Typography.Caption("below"), 14f);
                }
            )
        );

        PlaygroundVariant vertical = new PlaygroundVariant(
            "CC_Playground_Label_Vertical",
            Layout.Layout.Row(
                SpacingScale.Xs,
                children: r => {
                    r.Add(Typography.Typography.Caption("left"));
                    r.Add(Layout.Layout.Divider.Vertical());
                    r.Add(Typography.Typography.Caption("right"));
                }
            )
        );

        return (new[] { horizontal, vertical }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SpacerDemo() {
        PlaygroundVariant flex = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.HStack(
                SpacingScale.Xs,
                r => {
                    r.Add(SampleChip("start"), 48f);
                    r.AddFlex(Layout.Layout.Spacer.Flex());
                    r.Add(SampleChip("end"), 48f);
                }
            )
        );

        return (new[] { flex }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) EachDemo() {
        string[] items = new[] { "A", "B", "C" };
        PlaygroundVariant each = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Layout.Layout.Row(
                SpacingScale.Xs,
                children: r => {
                    r.Add(
                        Layout.Layout.Each(
                            items,
                            (item, _) => SampleChip(item),
                            item => item
                        )
                    );
                }
            )
        );

        return (new[] { each }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ConditionalDemo() {
        PlaygroundVariant on = new PlaygroundVariant(
            "CC_Playground_Label_True",
            Layout.Layout.Conditional(
                true,
                () => AccentChip((string)"CC_Playground_Conditional_On".Translate())
            )
        );

        PlaygroundVariant off = new PlaygroundVariant(
            "CC_Playground_Label_False",
            Layout.Layout.Box(
                EdgeInsets.All(SpacingScale.Xs),
                new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
                null,
                RadiusSpec.All(new Rem(0.25f)),
                k => {
                    k.Add(MutedChip("subtree skipped — no draw"));
                    k.Add(
                        Layout.Layout.Conditional(
                            false,
                            () => AccentChip("hidden")
                        )
                    );
                }
            )
        );

        return (new[] { on, off }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CardDemo() {
        PlaygroundVariant plain = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Surface.Surface.Card.Create(
                Typography.Typography.Text("card content", FontRole.Body, new Rem(0.875f), ThemeSlot.TextPrimary)
            )
        );

        PlaygroundVariant composed = new PlaygroundVariant(
            "CC_Playground_Label_Primary",
            Surface.Surface.Card.Create(
                Surface.Surface.Card.Header(
                    Surface.Surface.Card.Title("Surgebinding"),
                    Surface.Surface.Card.Description("Bonded Radiant powers.")
                ),
                Surface.Surface.Card.Content(
                    Typography.Typography.Text(
                        "Progression unlocks with oaths.",
                        FontRole.Body,
                        new Rem(0.875f),
                        ThemeSlot.TextPrimary
                    )
                ),
                Surface.Surface.Card.Footer(
                    Button.Create(
                        (string)"CC_Playground_Label_Confirm".Translate(),
                        () => { }
                    )
                )
            )
        );

        PlaygroundVariant tight = new PlaygroundVariant(
            "CC_Playground_Label_Tight",
            Surface.Surface.Card.WithPadding(
                SpacingScale.Xs,
                Surface.Surface.Card.Title("Compact"),
                Typography.Typography.Text(
                    "Minimal padding for dense layouts.",
                    FontRole.Body,
                    new Rem(0.75f),
                    ThemeSlot.TextMuted
                )
            )
        );

        PlaygroundVariant loose = new PlaygroundVariant(
            "CC_Playground_Label_Loose",
            Surface.Surface.Card.WithPadding(
                SpacingScale.Lg,
                Surface.Surface.Card.Header(
                    Surface.Surface.Card.Title("Confirm action"),
                    Surface.Surface.Card.Description("Generous padding suits modal content.")
                ),
                Surface.Surface.Card.Footer(
                    Button.Create(
                        (string)"CC_Playground_Label_Cancel".Translate(),
                        () => { }
                    ),
                    Button.Create(
                        (string)"CC_Playground_Label_Confirm".Translate(),
                        () => { }
                    )
                )
            )
        );

        return (new[] { plain, composed, tight, loose }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BoxDemo() {
        LightweaveNode raised = Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Typography.Typography.Caption("raised"))
        );

        LightweaveNode sunken = Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Typography.Typography.Caption("sunken"))
        );

        LightweaveNode accent = Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
            null,
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(Typography.Typography.Text("accent", FontRole.Body, new Rem(0.8125f), ThemeSlot.TextOnAccent))
        );

        return (new[] {
            new PlaygroundVariant("CC_Playground_Label_Raised", raised),
            new PlaygroundVariant("CC_Playground_Label_Sunken", sunken),
            new PlaygroundVariant("CC_Playground_Label_Accent", accent),
        }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) HeadingDemo() {
        PlaygroundVariant h1 = new PlaygroundVariant(
            "CC_Playground_Label_Large",
            Typography.Typography.Heading(1, "Heading 1")
        );
        PlaygroundVariant h2 = new PlaygroundVariant(
            "CC_Playground_Label_Medium",
            Typography.Typography.Heading(2, "Heading 2")
        );
        PlaygroundVariant h3 = new PlaygroundVariant(
            "CC_Playground_Label_Small",
            Typography.Typography.Heading(3, "Heading 3")
        );

        return (new[] { h1, h2, h3 }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TextDemo() {
        string sample = "CC_Playground_Text_Sample".Translate();

        PlaygroundVariant normal = new PlaygroundVariant(
            "CC_Playground_Label_Normal",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary)
        );
        PlaygroundVariant accented = new PlaygroundVariant(
            "CC_Playground_Label_Accented",
            Typography.Typography.Text(
                sample,
                FontRole.Body,
                new Rem(0.9375f),
                ThemeSlot.SurfaceAccent,
                TextAlign.Start,
                FontStyle.Bold
            )
        );
        PlaygroundVariant muted = new PlaygroundVariant(
            "CC_Playground_Label_Muted",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted)
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextPrimary)
        );
        PlaygroundState mutedState = new PlaygroundState(
            "CC_Playground_Label_Muted",
            Typography.Typography.Text(sample, FontRole.Body, new Rem(0.9375f), ThemeSlot.TextMuted)
        );

        return (new[] { normal, accented, muted }, new[] { defaultState, mutedState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) LabelDemo() {
        PlaygroundVariant plain = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Typography.Typography.Label("Storm warnings")
        );
        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CaptionDemo() {
        PlaygroundVariant plain = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Typography.Typography.Caption("Updated moments ago")
        );
        return (new[] { plain }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RichTextDemo() {
        PlaygroundVariant defaultVariant = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Typography.Typography.RichText(new TaggedString((string)"CC_Playground_RichText_Sample".Translate()))
        );
        PlaygroundVariant boldVariant = new PlaygroundVariant(
            "CC_Playground_Label_Bold",
            Typography.Typography.RichText(
                new TaggedString("Honor lies in <b>keeping</b> your word, even when it costs you everything.")
            )
        );
        PlaygroundVariant italicVariant = new PlaygroundVariant(
            "CC_Playground_Label_Italic",
            Typography.Typography.RichText(
                new TaggedString("<i>Life before death. Strength before weakness. Journey before destination.</i>")
            )
        );
        PlaygroundVariant colorVariant = new PlaygroundVariant(
            "CC_Playground_Label_Accent",
            Typography.Typography.RichText(
                new TaggedString("Stormlight burns <color=#bba36a>brilliant</color> in his veins.")
            )
        );
        PlaygroundVariant mixedVariant = new PlaygroundVariant(
            "CC_Playground_Label_Mixed",
            Typography.Typography.RichText(
                new TaggedString(
                    "<b><color=#bba36a>Adolin</color></b> raised <i>Mayalaran</i>, the dead Blade he had sworn to honor."
                )
            )
        );
        return (new[] { defaultVariant, boldVariant, italicVariant, colorVariant, mixedVariant }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CodeDemo() {
        PlaygroundVariant inlineVariant = new PlaygroundVariant(
            "CC_Playground_Label_Inline",
            Typography.Typography.Code("Pawn.health.AddHediff(HediffDef);")
        );
        PlaygroundVariant xmlVariant = new PlaygroundVariant(
            "CC_Playground_Label_Xml",
            Typography.Typography.Code("<defName>CC_AbilityWindrunner</defName>")
        );
        PlaygroundVariant pathVariant = new PlaygroundVariant(
            "CC_Playground_Label_Path",
            Typography.Typography.Code("Things/Item/Equipment/Weapon/Shardblade.png")
        );
        PlaygroundVariant blockVariant = new PlaygroundVariant(
            "CC_Playground_Label_Block",
            Typography.Typography.Code((string)"CC_Playground_Code_Sample".Translate())
        );
        return (new[] { inlineVariant, xmlVariant, pathVariant, blockVariant }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) IconDemo() {
        Texture tex = Texture2D.whiteTexture;

        LightweaveNode defaultIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.TextPrimary);
        LightweaveNode accentIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.SurfaceAccent);
        LightweaveNode mutedIcon = Typography.Typography.Icon(tex, new Rem(1.5f), ThemeSlot.TextMuted);

        return (new[] {
            new PlaygroundVariant("CC_Playground_Label_Default", defaultIcon),
            new PlaygroundVariant("CC_Playground_Label_Accent", accentIcon),
            new PlaygroundVariant("CC_Playground_Label_Muted", mutedIcon),
        }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ButtonDemo(bool forceDisabled) {
        PlaygroundVariant primary = new PlaygroundVariant(
            "CC_Playground_Label_Primary",
            Button.Create("Primary", () => { }, disabled: forceDisabled)
        );
        PlaygroundVariant secondary = new PlaygroundVariant(
            "CC_Playground_Label_Secondary",
            Button.Create("Secondary", () => { }, ButtonVariant.Secondary, disabled: forceDisabled)
        );
        PlaygroundVariant ghost = new PlaygroundVariant(
            "CC_Playground_Label_Ghost",
            Button.Create("Ghost", () => { }, ButtonVariant.Ghost, disabled: forceDisabled)
        );
        PlaygroundVariant danger = new PlaygroundVariant(
            "CC_Playground_Label_Danger",
            Button.Create("Danger", () => { }, ButtonVariant.Danger, disabled: forceDisabled)
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Button.Create("Default", () => { }, disabled: forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            Button.Create("Hover me", () => { }, disabled: forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            Button.Create("Disabled", () => { }, disabled: true)
        );

        return (new[] { primary, secondary, ghost, danger }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>)
        IconButtonDemo(bool forceDisabled) {
        LightweaveNode MakeIcon() {
            return Typography.Typography.Icon(Texture2D.whiteTexture, new Rem(1f), ThemeSlot.TextPrimary);
        }

        PlaygroundVariant ghost = new PlaygroundVariant(
            "CC_Playground_Label_Ghost",
            IconButton.Create(MakeIcon(), () => { }, disabled: forceDisabled)
        );
        PlaygroundVariant primary = new PlaygroundVariant(
            "CC_Playground_Label_Primary",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Primary, disabled: forceDisabled)
        );
        PlaygroundVariant secondary = new PlaygroundVariant(
            "CC_Playground_Label_Secondary",
            IconButton.Create(MakeIcon(), () => { }, ButtonVariant.Secondary, disabled: forceDisabled)
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            IconButton.Create(MakeIcon(), () => { }, disabled: forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            IconButton.Create(MakeIcon(), () => { }, disabled: forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            IconButton.Create(MakeIcon(), () => { }, disabled: true)
        );

        return (new[] { ghost, primary, secondary }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ToggleButtonDemo(
        bool forceDisabled
    ) {
        _ = forceDisabled;
        Hooks.Hooks.StateHandle<bool> onValue = Hooks.Hooks.UseState(true);
        Hooks.Hooks.StateHandle<bool> offValue = Hooks.Hooks.UseState(false);

        PlaygroundVariant on = new PlaygroundVariant(
            "CC_Playground_Label_On",
            ToggleButton.Create("On", onValue.Value, v => onValue.Set(v))
        );
        PlaygroundVariant off = new PlaygroundVariant(
            "CC_Playground_Label_Off",
            ToggleButton.Create("Off", offValue.Value, v => offValue.Set(v))
        );

        return (new[] { on, off }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CheckboxDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<bool> checkedState = Hooks.Hooks.UseState(true);
        Hooks.Hooks.StateHandle<bool> uncheckedState = Hooks.Hooks.UseState(false);

        PlaygroundVariant onVariant = new PlaygroundVariant(
            "CC_Playground_Label_True",
            Checkbox.Create("Enabled", checkedState.Value, v => checkedState.Set(v), forceDisabled)
        );
        PlaygroundVariant offVariant = new PlaygroundVariant(
            "CC_Playground_Label_False",
            Checkbox.Create("Disabled", uncheckedState.Value, v => uncheckedState.Set(v), forceDisabled)
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Checkbox.Create("Default", true, _ => { }, forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            Checkbox.Create("Hover", false, _ => { }, forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            Checkbox.Create("Disabled", true, _ => { }, true)
        );

        return (new[] { onVariant, offVariant }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SwitchDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<bool> onState = Hooks.Hooks.UseState(true);
        Hooks.Hooks.StateHandle<bool> offState = Hooks.Hooks.UseState(false);

        PlaygroundVariant onVariant = new PlaygroundVariant(
            "CC_Playground_Label_On",
            Switch.Create(
                (string)"CC_Playground_Controls_Switch_Label".Translate(),
                onState.Value,
                v => onState.Set(v),
                forceDisabled
            )
        );
        PlaygroundVariant offVariant = new PlaygroundVariant(
            "CC_Playground_Label_Off",
            Switch.Create("Off", offState.Value, v => offState.Set(v), forceDisabled)
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Switch.Create("Default", true, _ => { }, forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            Switch.Create("Hover", false, _ => { }, forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            Switch.Create("Disabled", true, _ => { }, true)
        );

        return (new[] { onVariant, offVariant }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RadioDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<int> selection = Hooks.Hooks.UseState(1);

        LightweaveNode group = Radio.Group(
            selection.Value,
            v => selection.Set(v),
            k => {
                k.Add(Radio.Item((string)"CC_Playground_Controls_Radio_OptionA".Translate(), 0, forceDisabled));
                k.Add(Radio.Item((string)"CC_Playground_Controls_Radio_OptionB".Translate(), 1, forceDisabled));
                k.Add(Radio.Item((string)"CC_Playground_Controls_Radio_OptionC".Translate(), 2, forceDisabled));
            }
        );

        PlaygroundVariant groupVariant = new PlaygroundVariant("CC_Playground_Label_Default", group);

        return (new[] { groupVariant }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SliderDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<float> sliderValue = Hooks.Hooks.UseState(0.4f);

        PlaygroundVariant smooth = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Slider.Create(sliderValue.Value, v => sliderValue.Set(v), disabled: forceDisabled)
        );
        PlaygroundVariant stepped = new PlaygroundVariant(
            "CC_Playground_Label_Accented",
            Slider.Create(
                sliderValue.Value,
                v => sliderValue.Set(v),
                0f,
                1f,
                0.25f,
                new[] { 0f, 0.25f, 0.5f, 0.75f, 1f },
                disabled: forceDisabled
            )
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Slider.Create(0.4f, _ => { }, disabled: forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            Slider.Create(0.7f, _ => { }, disabled: forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            Slider.Create(0.4f, _ => { }, disabled: true)
        );

        return (new[] { smooth, stepped }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>)
        TextFieldDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<string> text = Hooks.Hooks.UseState<string>("Stormlight");

        PlaygroundVariant filled = new PlaygroundVariant(
            "CC_Playground_Label_Filled",
            TextField.Create(
                text.Value,
                v => text.Set(v),
                (string)"CC_Playground_Controls_TextField_Placeholder".Translate(),
                disabled: forceDisabled
            )
        );
        PlaygroundVariant empty = new PlaygroundVariant(
            "CC_Playground_Label_Empty",
            TextField.Create(
                string.Empty,
                _ => { },
                (string)"CC_Playground_Controls_TextField_Placeholder".Translate(),
                disabled: forceDisabled
            )
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            TextField.Create("Default", _ => { }, disabled: forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            TextField.Create("Hover", _ => { }, disabled: forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            TextField.Create("Disabled", _ => { }, disabled: true)
        );

        return (new[] { filled, empty }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TextAreaDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<string> text = Hooks.Hooks.UseState<string>("Multi-line sample.");

        PlaygroundVariant filled = new PlaygroundVariant(
            "CC_Playground_Label_Filled",
            TextArea.Create(
                text.Value,
                v => text.Set(v),
                (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
                2,
                3,
                disabled: forceDisabled
            )
        );
        PlaygroundVariant empty = new PlaygroundVariant(
            "CC_Playground_Label_Empty",
            TextArea.Create(
                string.Empty,
                _ => { },
                (string)"CC_Playground_Controls_TextArea_Placeholder".Translate(),
                2,
                3,
                disabled: forceDisabled
            )
        );

        return (new[] { filled, empty }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) NumberFieldDemo(
        bool forceDisabled
    ) {
        Hooks.Hooks.StateHandle<float> number = Hooks.Hooks.UseState(42f);

        PlaygroundVariant bounded = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            NumberField.Create(
                number.Value,
                v => number.Set(v),
                0f,
                100f,
                placeholder: (string)"CC_Playground_Controls_NumberField_Label".Translate(),
                disabled: forceDisabled
            )
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            NumberField.Create(42f, _ => { }, 0f, 100f, disabled: forceDisabled)
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            NumberField.Create(7f, _ => { }, 0f, 100f, disabled: forceDisabled)
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            NumberField.Create(13f, _ => { }, 0f, 100f, disabled: true)
        );

        return (new[] { bounded }, new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SearchFieldDemo(
        bool forceDisabled
    ) {
        Hooks.Hooks.StateHandle<string> query = Hooks.Hooks.UseState(string.Empty);

        PlaygroundVariant empty = new PlaygroundVariant(
            "CC_Playground_Label_Empty",
            SearchField.Create(
                query.Value,
                v => query.Set(v),
                (string)"CC_Playground_SearchField_Placeholder".Translate(),
                forceDisabled
            )
        );
        PlaygroundVariant filled = new PlaygroundVariant(
            "CC_Playground_Label_Filled",
            SearchField.Create("highstorm", _ => { }, disabled: forceDisabled)
        );

        return (new[] { empty, filled }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DropdownDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<string> inputSel = Hooks.Hooks.UseState<string>("Scadrial");
        Hooks.Hooks.StateHandle<string> buttonSel = Hooks.Hooks.UseState<string>("Scadrial");
        Hooks.Hooks.StateHandle<string> primarySel = Hooks.Hooks.UseState<string>("Scadrial");
        Hooks.Hooks.StateHandle<string> defaultSel = Hooks.Hooks.UseState<string>("Scadrial");
        Hooks.Hooks.StateHandle<string> hoverSel = Hooks.Hooks.UseState<string>("Roshar");
        Hooks.Hooks.StateHandle<string> disabledSel = Hooks.Hooks.UseState<string>("Nalthis");

        string[] options = new[] { "Roshar", "Scadrial", "Nalthis", "Taldain", "Ashyn" };

        PlaygroundVariant inputVariant = new PlaygroundVariant(
            "CC_Playground_Label_Input",
            Dropdown.Create<string>(inputSel.Value, options, v => v, v => inputSel.Set(v), disabled: forceDisabled)
        );
        PlaygroundVariant buttonVariant = new PlaygroundVariant(
            "CC_Playground_Label_Button",
            Dropdown.Create(
                buttonSel.Value,
                options,
                v => v,
                v => buttonSel.Set(v),
                DropdownVariant.Button,
                ButtonVariant.Secondary,
                forceDisabled,
                "btn-secondary"
            )
        );
        PlaygroundVariant primaryVariant = new PlaygroundVariant(
            "CC_Playground_Label_Primary",
            Dropdown.Create(
                primarySel.Value,
                options,
                v => v,
                v => primarySel.Set(v),
                DropdownVariant.Button,
                ButtonVariant.Primary,
                forceDisabled,
                "btn-primary"
            )
        );

        PlaygroundState defaultState = new PlaygroundState(
            "CC_Playground_Label_Default",
            Dropdown.Create(
                defaultSel.Value,
                options,
                v => v,
                v => defaultSel.Set(v),
                disabled: forceDisabled,
                instanceKey: "st-default"
            )
        );
        PlaygroundState hoverState = new PlaygroundState(
            "CC_Playground_Label_Hover",
            Dropdown.Create(
                hoverSel.Value,
                options,
                v => v,
                v => hoverSel.Set(v),
                disabled: forceDisabled,
                instanceKey: "st-hover"
            )
        );
        PlaygroundState disabledState = new PlaygroundState(
            "CC_Playground_Label_Disabled",
            Dropdown.Create(
                disabledSel.Value,
                options,
                v => v,
                v => disabledSel.Set(v),
                disabled: true,
                instanceKey: "st-disabled"
            )
        );

        return (new[] { inputVariant, buttonVariant, primaryVariant },
            new[] { defaultState, hoverState, disabledState });
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ColorPickerDemo(
        bool forceDisabled
    ) {
        Hooks.Hooks.StateHandle<Color> chosen = Hooks.Hooks.UseState(new Color(0.25f, 0.42f, 0.30f));

        PlaygroundVariant picker = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            ColorPicker.Create(chosen.Value, v => chosen.Set(v), disabled: forceDisabled)
        );

        return (new[] { picker }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>)
        KeyBindingDemo(bool forceDisabled) {
        Hooks.Hooks.StateHandle<KeyBinding> binding = Hooks.Hooks.UseState(
            new KeyBinding(KeyCode.F, KeyModifiers.Control)
        );

        PlaygroundVariant cast = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            KeyBindingField.Create(binding.Value, v => binding.Set(v), forceDisabled)
        );

        return (new[] { cast }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SpinnerDemo() {
        PlaygroundVariant small = new PlaygroundVariant(
            "CC_Playground_Label_Small",
            CenterFixed(Spinner.Create(new Rem(1f)), 24f, 24f)
        );
        PlaygroundVariant medium = new PlaygroundVariant(
            "CC_Playground_Label_Medium",
            CenterFixed(Spinner.Create(new Rem(1.5f)), 32f, 32f)
        );
        PlaygroundVariant large = new PlaygroundVariant(
            "CC_Playground_Label_Large",
            CenterFixed(Spinner.Create(new Rem(2f)), 44f, 44f)
        );

        return (new[] { small, medium, large }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ProgressBarDemo() {
        PlaygroundVariant accent = new PlaygroundVariant(
            "CC_Playground_Label_Accent",
            ProgressBar.Create(0.65f, 0f, 1f, "65%")
        );
        PlaygroundVariant success = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            ProgressBar.Create(0.35f, 0f, 1f, "35%", BadgeVariant.Success)
        );
        PlaygroundVariant danger = new PlaygroundVariant(
            "CC_Playground_Label_Danger",
            ProgressBar.Create(0.9f, 0f, 1f, "90%", BadgeVariant.Danger)
        );

        return (new[] { accent, success, danger }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) RingGaugeDemo() {
        PlaygroundVariant low = new PlaygroundVariant(
            "CC_Playground_Label_Small",
            CenterFixed(RingGauge.Create(0.25f), 72f, 72f)
        );
        PlaygroundVariant mid = new PlaygroundVariant(
            "CC_Playground_Label_Medium",
            CenterFixed(RingGauge.Create(0.6f), 72f, 72f)
        );
        PlaygroundVariant high = new PlaygroundVariant(
            "CC_Playground_Label_Large",
            CenterFixed(RingGauge.Create(0.95f), 72f, 72f)
        );

        return (new[] { low, mid, high }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SparklineDemo() {
        float[] rising = new[] { 1f, 2f, 3f, 5f, 8f, 13f };
        float[] wavy = new[] { 3f, 5f, 2f, 7f, 4f, 6f, 2f };
        float[] flat = new[] { 4f, 4f, 4f, 4f, 4f };

        PlaygroundVariant risingSpark = new PlaygroundVariant(
            "CC_Playground_Label_Accent",
            Sparkline.Create(rising)
        );
        PlaygroundVariant wavySpark = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Sparkline.Create(wavy)
        );
        PlaygroundVariant flatSpark = new PlaygroundVariant(
            "CC_Playground_Label_Muted",
            Sparkline.Create(flat, ThemeSlot.TextMuted)
        );

        return (new[] { risingSpark, wavySpark, flatSpark }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BadgeDemo() {
        LightweaveNode MakeBadge(string key, BadgeVariant variant) {
            return CenterFixed(Badge.Create((string)key.Translate(), variant), 96f, 24f);
        }

        PlaygroundVariant neutral = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Neutral",
            MakeBadge("CC_Playground_Feedback_Badge_Neutral", BadgeVariant.Neutral)
        );
        PlaygroundVariant accent = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Accent",
            MakeBadge("CC_Playground_Feedback_Badge_Accent", BadgeVariant.Accent)
        );
        PlaygroundVariant warning = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Warning",
            MakeBadge("CC_Playground_Feedback_Badge_Warning", BadgeVariant.Warning)
        );
        PlaygroundVariant danger = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Danger",
            MakeBadge("CC_Playground_Feedback_Badge_Danger", BadgeVariant.Danger)
        );
        PlaygroundVariant success = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Success",
            MakeBadge("CC_Playground_Feedback_Badge_Success", BadgeVariant.Success)
        );
        PlaygroundVariant clickable = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Clickable",
            CenterFixed(
                Badge.Create(
                    (string)"CC_Playground_Feedback_Badge_Clickable".Translate(),
                    BadgeVariant.Accent,
                    onClick: () => { }
                ),
                120f,
                24f
            )
        );
        PlaygroundVariant dismissible = new PlaygroundVariant(
            "CC_Playground_Feedback_Badge_Dismissible",
            CenterFixed(
                Badge.Create(
                    (string)"CC_Playground_Feedback_Badge_Dismissible".Translate(),
                    BadgeVariant.Neutral,
                    trailing: Badge.CloseGlyph(BadgeVariant.Neutral),
                    onTrailingClick: () => { }
                ),
                150f,
                24f
            )
        );

        return (new[] { neutral, accent, warning, danger, success, clickable, dismissible }, EmptyStates);
    }


    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TooltipDemo() {
        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Feedback_Tooltip_Button_Label".Translate(),
            () => { },
            ButtonVariant.Secondary
        );
        string body = "CC_Playground_Feedback_Tooltip_Button_Body".Translate();

        PlaygroundVariant wrapped = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Tooltip.Wrap(trigger, body)
        );

        return (new[] { wrapped }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TabsDemo() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("general");

        string[] tabs = new[] { "general", "combat", "storage" };
        LightweaveNode tabsNode = Tabs.Create(
            selected.Value,
            tabs,
            v => v switch {
                "combat" => (string)"CC_Playground_Navigation_Tabs_Combat".Translate(),
                "storage" => (string)"CC_Playground_Navigation_Tabs_Storage".Translate(),
                _ => (string)"CC_Playground_Navigation_Tabs_General".Translate(),
            },
            v => selected.Set(v),
            v => Typography.Typography.Caption(
                v switch {
                    "combat" => (string)"CC_Playground_Navigation_Tabs_Body_Combat".Translate(),
                    "storage" => (string)"CC_Playground_Navigation_Tabs_Body_Storage".Translate(),
                    _ => (string)"CC_Playground_Navigation_Tabs_Body_General".Translate(),
                }
            )
        );

        PlaygroundVariant threeTabs = new PlaygroundVariant("CC_Playground_Label_Default", tabsNode);

        return (new[] { threeTabs }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) SegmentedDemo() {
        Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("all");

        string[] segments = new[] { "all", "armor", "weapons", "tools" };
        LightweaveNode segNode = Segmented.Create(
            selected.Value,
            segments,
            v => v switch {
                "armor" => (string)"CC_Playground_Navigation_Segmented_Armor".Translate(),
                "weapons" => (string)"CC_Playground_Navigation_Segmented_Weapons".Translate(),
                "tools" => (string)"CC_Playground_Navigation_Segmented_Tools".Translate(),
                _ => (string)"CC_Playground_Navigation_Segmented_All".Translate(),
            },
            v => selected.Set(v)
        );

        PlaygroundVariant four = new PlaygroundVariant("CC_Playground_Label_Default", segNode);

        return (new[] { four }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) BreadcrumbsDemo() {
        string[] path = new[] {
            (string)"CC_Playground_Breadcrumbs_Crumb_Worlds".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate(),
            (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate(),
        };

        PlaygroundVariant crumbs = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            Breadcrumbs.Create(path)
        );

        return (new[] { crumbs }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) MenuDemo() {
        Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
        Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

        List<MenuItem> exportChildren = new List<MenuItem> {
            Menu.Item((string)"CC_Playground_Navigation_Menu_ExportPng".Translate(), () => open.Set(false)),
            Menu.Item((string)"CC_Playground_Navigation_Menu_ExportSvg".Translate(), () => open.Set(false)),
        };

        List<MenuItem> items = new List<MenuItem> {
            Menu.Item((string)"CC_Playground_Navigation_Menu_Open".Translate(), () => open.Set(false)),
            Menu.Item((string)"CC_Playground_Navigation_Menu_Save".Translate(), () => open.Set(false)),
            Menu.Item((string)"CC_Playground_Navigation_Menu_SaveAs".Translate(), () => open.Set(false)),
            Menu.Divider(),
            Menu.Submenu((string)"CC_Playground_Navigation_Menu_Export".Translate(), exportChildren),
            Menu.Divider(),
            Menu.Item((string)"CC_Playground_Navigation_Menu_Close".Translate(), () => open.Set(false)),
        };

        LightweaveNode trigger = NodeBuilder.New("MenuTrigger", 0, nameof(PlaygroundDemos));
        LightweaveNode button = Button.Create(
            (string)"CC_Playground_Menu_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );
        trigger.Children.Add(button);
        trigger.Paint = (rect, _) => {
            anchor.Current = rect;
            button.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(button, rect);
        };

        LightweaveNode menu = Menu.Create(
            open.Value,
            anchor.Current,
            items,
            () => open.Set(false),
            MenuAnchor.Left,
            MenuDirection.Down,
            "playground-menu"
        );

        LightweaveNode composed = NodeBuilder.New("MenuHost", 0, nameof(PlaygroundDemos));
        composed.Children.Add(trigger);
        composed.Children.Add(menu);
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            menu.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(menu, rect);
        };

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", composed) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ContextMenuDemo() {
        List<MenuItem> items = new List<MenuItem> {
            Menu.Item((string)"CC_Playground_ContextMenu_Inspect".Translate(), () => { }),
            Menu.Item((string)"CC_Playground_ContextMenu_Rename".Translate(), () => { }),
            Menu.Item((string)"CC_Playground_ContextMenu_Duplicate".Translate(), () => { }),
            Menu.Divider(),
            Menu.Item((string)"CC_Playground_ContextMenu_Delete".Translate(), () => { }),
        };

        LightweaveNode target = Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Sm),
            new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised),
            BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
            RadiusSpec.All(new Rem(0.25f)),
            c => c.Add(
                Typography.Typography.Caption(
                    (string)"CC_Playground_ContextMenu_RightClick".Translate()
                )
            )
        );

        PlaygroundVariant wrapped = new PlaygroundVariant(
            "CC_Playground_Label_Default",
            ContextMenu.Create(target, items)
        );

        return (new[] { wrapped }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) WindowDemo() {
        PlaygroundVariant bordered = new PlaygroundVariant(
            "CC_Playground_Window_Bordered",
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Bordered_Body",
                    new Vector2(480f, 320f)
                )),
                ButtonVariant.Secondary
            )
        );

        PlaygroundVariant borderless = new PlaygroundVariant(
            "CC_Playground_Window_Borderless",
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Borderless_Body",
                    new Vector2(480f, 320f),
                    drawBorder: false
                )),
                ButtonVariant.Secondary
            )
        );

        PlaygroundVariant fixedSize = new PlaygroundVariant(
            "CC_Playground_Window_FixedSize",
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Fixed_Body",
                    new Vector2(420f, 280f),
                    edgeResizable: false
                )),
                ButtonVariant.Secondary
            )
        );

        PlaygroundVariant large = new PlaygroundVariant(
            "CC_Playground_Window_Large",
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Large_Body",
                    new Vector2(720f, 520f),
                    minSize: new Vector2(520f, 360f)
                )),
                ButtonVariant.Secondary
            )
        );

        return (new[] { bordered, borderless, fixedSize, large }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DialogDemo() {
        Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);

        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Dialog_TriggerOpen".Translate(),
            () => open.Set(true)
        );

        LightweaveNode dialog = Dialog.Create(
            open.Value,
            () => open.Set(false),
            () => Dialog.Root(
                Dialog.Header(
                    Dialog.Title((string)"CC_Playground_Overlay_Dialog_Header".Translate()),
                    Dialog.Description((string)"CC_Playground_Overlay_Dialog_Body".Translate())
                ),
                Dialog.Content(
                    Typography.Typography.Text(
                        (string)"CC_Playground_Overlay_Dialog_Body".Translate(),
                        FontRole.Body,
                        new Rem(0.9375f),
                        ThemeSlot.TextPrimary
                    )
                ),
                Dialog.Footer(
                    Button.Create(
                        (string)"CC_Playground_Label_Confirm".Translate(),
                        () => open.Set(false)
                    )
                )
            )
        );

        LightweaveNode composed = NodeBuilder.New("DialogHost", 0, nameof(PlaygroundDemos));
        composed.Children.Add(trigger);
        composed.Children.Add(dialog);
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            dialog.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(dialog, rect);
        };

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", composed) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) PopoverDemo() {
        Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);
        Hooks.Hooks.RefHandle<Rect> anchor = Hooks.Hooks.UseRef(default(Rect));

        LightweaveNode button = Button.Create(
            (string)"CC_Playground_Popover_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );

        LightweaveNode trigger = NodeBuilder.New("PopoverTrigger", 0, nameof(PlaygroundDemos));
        trigger.Children.Add(button);
        trigger.Paint = (rect, _) => {
            anchor.Current = rect;
            button.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(button, rect);
        };

        LightweaveNode body = Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Md),
            null,
            null,
            null,
            k => k.Add(
                Typography.Typography.Text(
                    (string)"CC_Playground_Overlay_Popover_Body".Translate(),
                    FontRole.Body,
                    new Rem(0.875f),
                    ThemeSlot.TextPrimary,
                    wrap: true
                )
            )
        );

        LightweaveNode popover = Popover.Create(
            open.Value,
            anchor.Current,
            PopoverPlacement.Bottom,
            body,
            () => open.Set(false),
            new Vector2(new Rem(15f).ToPixels(), -1f)
        );

        LightweaveNode composed = NodeBuilder.New("PopoverHost", 0, nameof(PlaygroundDemos));
        composed.Children.Add(trigger);
        composed.Children.Add(popover);
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            popover.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(popover, rect);
        };

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", composed) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) DrawerDemo() {
        Hooks.Hooks.StateHandle<bool> open = Hooks.Hooks.UseState(false);

        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Drawer_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );

        LightweaveNode drawer = Drawer.Create(
            open.Value,
            DrawerSide.Right,
            () => Layout.Layout.Stack(
                SpacingScale.Sm,
                s => {
                    s.Add(
                        Typography.Typography.Heading(
                            3,
                            (string)"CC_Playground_Drawer_ContentTitle".Translate()
                        ),
                        28f
                    );
                    s.Add(
                        Typography.Typography.Text(
                            (string)"CC_Playground_Drawer_ContentBody".Translate(),
                            FontRole.Body,
                            new Rem(0.875f),
                            ThemeSlot.TextPrimary
                        ),
                        80f
                    );
                    LightweaveNode closeBtn = Button.Create(
                        (string)"CC_Playground_Drawer_Close".Translate(),
                        () => open.Set(false),
                        ButtonVariant.Secondary
                    );
                    LightweaveNode closeRow = Layout.Layout.HStack(
                        SpacingScale.Xs,
                        r => {
                            r.AddFlex(NodeBuilder.New("Spacer", 0, nameof(PlaygroundDemos)));
                            r.Add(closeBtn, 96f);
                        }
                    );
                    s.Add(closeRow, 32f);
                }
            ),
            () => open.Set(false)
        );

        LightweaveNode composed = NodeBuilder.New("DrawerHost", 0, nameof(PlaygroundDemos));
        composed.Children.Add(trigger);
        composed.Children.Add(drawer);
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            drawer.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(drawer, rect);
        };

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", composed) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ToastDemo() {
        Hooks.Hooks.StateHandle<List<ToastMessage>> toasts =
            Hooks.Hooks.UseState(new List<ToastMessage>());
        Hooks.Hooks.RefHandle<int> counter = Hooks.Hooks.UseRef(0);

        void PushToast(ToastKind kind, string textKey) {
            counter.Current = counter.Current + 1;
            List<ToastMessage> next = new List<ToastMessage>(toasts.Value) {
                new ToastMessage(
                    "playground-toast-" + counter.Current,
                    (string)textKey.Translate(),
                    kind,
                    3f
                ),
            };
            toasts.Set(next);
        }

        void DismissToast(string id) {
            List<ToastMessage> next = new List<ToastMessage>();
            for (int i = 0; i < toasts.Value.Count; i++) {
                if (toasts.Value[i].Id != id) {
                    next.Add(toasts.Value[i]);
                }
            }

            toasts.Set(next);
        }

        LightweaveNode toastLayer = Toast.Create(
            toasts.Value,
            DismissToast
        );
        toastLayer.PreferredHeight = 0f;

        LightweaveNode infoButton = Layout.Layout.Stack(
            new Rem(0f),
            s => {
                s.Add(
                    Button.Create(
                        (string)"CC_Playground_Toast_Info".Translate(),
                        () => PushToast(ToastKind.Info, "CC_Playground_Toast_Msg_Info"),
                        ButtonVariant.Secondary
                    )
                );
                s.Add(toastLayer);
            }
        );

        PlaygroundVariant info = new PlaygroundVariant("CC_Playground_Toast_Info", infoButton);
        PlaygroundVariant success = new PlaygroundVariant(
            "CC_Playground_Toast_Success",
            Button.Create(
                (string)"CC_Playground_Toast_Success".Translate(),
                () => PushToast(ToastKind.Success, "CC_Playground_Toast_Msg_Success"),
                ButtonVariant.Secondary
            )
        );
        PlaygroundVariant warning = new PlaygroundVariant(
            "CC_Playground_Toast_Warning",
            Button.Create(
                (string)"CC_Playground_Toast_Warning".Translate(),
                () => PushToast(ToastKind.Warning, "CC_Playground_Toast_Msg_Warning"),
                ButtonVariant.Secondary
            )
        );
        PlaygroundVariant danger = new PlaygroundVariant(
            "CC_Playground_Toast_Danger",
            Button.Create(
                (string)"CC_Playground_Toast_Danger".Translate(),
                () => PushToast(ToastKind.Danger, "CC_Playground_Toast_Msg_Danger"),
                ButtonVariant.Danger
            )
        );

        return (new[] { info, success, warning, danger }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ListDemo() {
        string[] items = new[] {
            (string)"CC_Playground_DemoItem_Highstorm".Translate(),
            (string)"CC_Playground_DemoItem_Stormlight".Translate(),
            (string)"CC_Playground_DemoItem_Radiant".Translate(),
            (string)"CC_Playground_DemoItem_Emotion".Translate(),
            (string)"CC_Playground_DemoItem_Alpha".Translate(),
            (string)"CC_Playground_DemoItem_Beta".Translate(),
            (string)"CC_Playground_DemoItem_Gamma".Translate(),
            (string)"CC_Playground_DemoItem_Delta".Translate(),
        };

        LightweaveNode list = List.Create(
            items,
            (item, _) => Layout.Layout.Box(
                new EdgeInsets(SpacingScale.Sm, Bottom: SpacingScale.Sm, Left: SpacingScale.Md, Right: SpacingScale.Md),
                null,
                null,
                null,
                k => k.Add(
                    Typography.Typography.Text(
                        item,
                        FontRole.Body,
                        new Rem(0.9375f),
                        ThemeSlot.TextPrimary
                    )
                )
            ),
            new Rem(2.25f).ToPixels()
        );

        PlaygroundVariant variant = new PlaygroundVariant("CC_Playground_Label_Default", list);

        return (new[] { variant }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TableDemo() {
        (string World, string Shards, string Population)[] rows = new[] {
            ("Roshar", "Honor + Cultivation", "Billions"),
            ("Scadrial", "Preservation + Ruin", "Millions"),
            ("Nalthis", "Endowment", "Millions"),
            ("Taldain", "Autonomy", "Few"),
        };

        List<TableColumn<(string World, string Shards, string Population)>> columns =
            new List<TableColumn<(string, string, string)>> {
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_World".Translate(),
                    r => Typography.Typography.Text(r.Item1, FontRole.Body, new Rem(0.875f), ThemeSlot.TextPrimary),
                    new Rem(7f)
                ),
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_Shards".Translate(),
                    r => Typography.Typography.Text(r.Item2, FontRole.Body, new Rem(0.8125f), ThemeSlot.TextSecondary)
                ),
                new TableColumn<(string, string, string)>(
                    (string)"CC_Playground_Table_Col_Population".Translate(),
                    r => Typography.Typography.Text(r.Item3, FontRole.Body, new Rem(0.8125f), ThemeSlot.TextMuted),
                    new Rem(6f)
                ),
            };

        LightweaveNode table = Table.Create<(string, string, string)>(rows, columns);

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", table) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) TreeDemo() {
        TreeNode[] roots = Hooks.Hooks.UseMemo(
            () => {
                TreeNode shatteredPlains = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate()
                );
                TreeNode luthadel = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Luthadel".Translate(),
                    new[] {
                        new TreeNode((string)"CC_Playground_Breadcrumbs_Crumb_CentralDistrict".Translate()),
                        new TreeNode((string)"CC_Playground_Breadcrumbs_Crumb_VentureKeep".Translate()),
                    }
                );
                TreeNode roshar = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate(),
                    new[] { shatteredPlains }
                );
                TreeNode scadrial = new TreeNode(
                    (string)"CC_Playground_Breadcrumbs_Crumb_Scadrial".Translate(),
                    new[] { luthadel }
                );
                return new[] { roshar, scadrial };
            },
            Array.Empty<object>()
        );

        LightweaveNode tree = Tree.Create(roots);

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", tree) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) KeyValueDemo() {
        LightweaveNode stormlight = Typography.Typography.Text(
            "1200",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.SurfaceAccent
        );
        LightweaveNode investiture = Typography.Typography.Text(
            "Honor",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary
        );
        LightweaveNode sprenBond = Typography.Typography.Text(
            "Honorspren",
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary
        );

        PlaygroundVariant stormlightKv = new PlaygroundVariant(
            "CC_Playground_Data_KV_Stormlight",
            KeyValue.Create((string)"CC_Playground_Data_KV_Stormlight".Translate(), stormlight)
        );
        PlaygroundVariant investitureKv = new PlaygroundVariant(
            "CC_Playground_Data_KV_Investiture",
            KeyValue.Create((string)"CC_Playground_Data_KV_Investiture".Translate(), investiture)
        );
        PlaygroundVariant bondKv = new PlaygroundVariant(
            "CC_Playground_Data_KV_SprenBond",
            KeyValue.Create((string)"CC_Playground_Data_KV_SprenBond".Translate(), sprenBond)
        );

        return (new[] { stormlightKv, investitureKv, bondKv }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) UseStateDemo() {
        Hooks.Hooks.StateHandle<int> count = Hooks.Hooks.UseState(0);

        LightweaveNode countLabel = Typography.Typography.Text(
            count.Value.ToString(),
            FontRole.BodyBold,
            new Rem(1f),
            ThemeSlot.TextPrimary,
            TextAlign.Center,
            FontStyle.Bold
        );

        LightweaveNode row = Layout.Layout.HStack(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseState_Decrement".Translate(),
                        () => count.Set(count.Value - 1),
                        ButtonVariant.Secondary
                    ),
                    48f
                );
                r.AddFlex(countLabel);
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseState_Increment".Translate(),
                        () => count.Set(count.Value + 1)
                    ),
                    48f
                );
            }
        );

        return (new[] { new PlaygroundVariant("CC_Playground_UseState_Label", row) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) UseAnimDemo() {
        Hooks.Hooks.StateHandle<bool> target = Hooks.Hooks.UseState(false);

        LightweaveNode fadeNode = NodeBuilder.New("UseAnimFade", 0, nameof(PlaygroundDemos));
        fadeNode.Paint = (rect, _) => {
            float t = UseAnim.Animate(target.Value ? 1f : 0f, 0.35f);
            Color saved = GUI.color;
            Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
            GUI.color = new Color(accent.r, accent.g, accent.b, accent.a * t);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        };

        LightweaveNode row = Layout.Layout.HStack(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseAnim_Toggle".Translate(),
                        () => target.Set(!target.Value),
                        ButtonVariant.Secondary
                    ),
                    120f
                );
                r.AddFlex(fadeNode);
            }
        );

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", row) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) UseFocusDemo() {
        Hooks.Hooks.StateHandle<string> text = Hooks.Hooks.UseState(string.Empty);
        UseFocus.FocusHandle focus = UseFocus.Use();

        LightweaveNode row = Layout.Layout.HStack(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseFocus_Focus".Translate(),
                        () => focus.Request(),
                        ButtonVariant.Secondary
                    ),
                    120f
                );
                r.AddFlex(
                    TextField.Create(
                        text.Value,
                        v => text.Set(v),
                        (string)"CC_Playground_UseFocus_Placeholder".Translate(),
                        focus: focus
                    )
                );
            }
        );

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", row) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) UseHotkeyDemo() {
        Hooks.Hooks.StateHandle<string> status = Hooks.Hooks.UseState<string>(
            (string)"CC_Playground_UseHotkey_Idle".Translate()
        );

        LightweaveNode hotkeyHost = NodeBuilder.New("UseHotkeyHost", 0, nameof(PlaygroundDemos));
        hotkeyHost.Paint = (rect, _) => {
            UseHotkey.Use(
                KeyCode.Escape,
                () => status.Set("CC_Playground_UseHotkey_Escape".Translate())
            );
            UseHotkey.Use(
                KeyCode.S,
                () => status.Set((string)"CC_Playground_UseHotkey_Saved".Translate()),
                KeyModifiers.Control
            );

            Theme.Theme theme = RenderContext.Current.Theme;
            GUIStyle style = GuiStyleCache.Get(
                theme.GetFont(FontRole.Body),
                Mathf.RoundToInt(new Rem(0.875f).ToFontPx())
            );
            style.alignment = TextAnchor.MiddleLeft;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(rect, status.Value, style);
            GUI.color = saved;
        };

        LightweaveNode hint = Typography.Typography.Caption(
            (string)"CC_Playground_UseHotkey_Hint".Translate()
        );

        LightweaveNode stack = Layout.Layout.Stack(
            SpacingScale.Xxs,
            s => {
                s.Add(hint, 14f);
                s.Add(hotkeyHost, 18f);
            }
        );

        return (new[] { new PlaygroundVariant("CC_Playground_Label_Default", stack) }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) AccordionDemo() {
        Hooks.Hooks.RefHandle<HashSet<string>> singleSet =
            Hooks.Hooks.UseRef(new HashSet<string> { "overview" });
        Hooks.Hooks.RefHandle<HashSet<string>> multiSet =
            Hooks.Hooks.UseRef(new HashSet<string> { "stormlight", "spren" });

        LightweaveNode overviewBody = Typography.Typography.Text(
            (string)"CC_Playground_accordion_Body_Overview".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
        LightweaveNode stormlightBody = Typography.Typography.Text(
            (string)"CC_Playground_accordion_Body_Stormlight".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );
        LightweaveNode sprenBody = Typography.Typography.Text(
            (string)"CC_Playground_accordion_Body_Spren".Translate(),
            FontRole.Body,
            new Rem(0.875f),
            ThemeSlot.TextPrimary,
            wrap: true
        );

        List<AccordionItem> items = new List<AccordionItem> {
            new AccordionItem(
                "overview",
                (string)"CC_Playground_accordion_Header_Overview".Translate(),
                overviewBody,
                56f
            ),
            new AccordionItem(
                "stormlight",
                (string)"CC_Playground_accordion_Header_Stormlight".Translate(),
                stormlightBody,
                64f
            ),
            new AccordionItem("spren", (string)"CC_Playground_accordion_Header_Spren".Translate(), sprenBody, 64f),
        };

        LightweaveNode singleAccordion = Accordion.Create(
            items,
            singleSet.Current,
            _ => { }
        );

        LightweaveNode multiAccordion = Accordion.Create(
            items,
            multiSet.Current,
            _ => { },
            AccordionMode.Multi
        );

        PlaygroundVariant single = new PlaygroundVariant("CC_Playground_accordion_Mode_Single", singleAccordion);
        PlaygroundVariant multi = new PlaygroundVariant("CC_Playground_accordion_Mode_Multi", multiAccordion);

        return (new[] { single, multi }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ButtonGroupDemo(
        bool forceDisabled
    ) {
        Hooks.Hooks.StateHandle<string> lastPick = Hooks.Hooks.UseState<string>(
            (string)"CC_Playground_buttongroup_None".Translate()
        );

        List<ButtonGroupItem> primaryItems = new List<ButtonGroupItem> {
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Day".Translate(),
                () => lastPick.Set("Day"),
                forceDisabled
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Week".Translate(),
                () => lastPick.Set("Week"),
                forceDisabled
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Month".Translate(),
                () => lastPick.Set("Month"),
                forceDisabled
            ),
        };

        List<ButtonGroupItem> secondaryItems = new List<ButtonGroupItem> {
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Refresh".Translate(),
                () => lastPick.Set("Refresh"),
                forceDisabled
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Export".Translate(),
                () => lastPick.Set("Export"),
                forceDisabled
            ),
            new ButtonGroupItem(
                (string)"CC_Playground_buttongroup_Delete".Translate(),
                () => lastPick.Set("Delete"),
                forceDisabled
            ),
            new ButtonGroupItem((string)"CC_Playground_buttongroup_More".Translate(), () => lastPick.Set("More"), true),
        };

        LightweaveNode primaryGroup = ButtonGroup.Create(primaryItems, ButtonVariant.Primary);
        LightweaveNode secondaryGroup = ButtonGroup.Create(secondaryItems);

        PlaygroundVariant primary = new PlaygroundVariant("CC_Playground_Label_Primary", primaryGroup);
        PlaygroundVariant secondary = new PlaygroundVariant("CC_Playground_Label_Secondary", secondaryGroup);

        return (new[] { primary, secondary }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) CarouselDemo() {
        Hooks.Hooks.StateHandle<int> index = Hooks.Hooks.UseState(0);

        LightweaveNode Slide(ThemeSlot bg, string labelKey) {
            return Layout.Layout.Box(
                EdgeInsets.All(SpacingScale.Md),
                new BackgroundSpec.Solid(bg),
                null,
                null,
                c => c.Add(
                    Typography.Typography.Text(
                        (string)labelKey.Translate(),
                        FontRole.BodyBold,
                        new Rem(1f),
                        ThemeSlot.TextPrimary,
                        TextAlign.Center
                    )
                )
            );
        }

        List<LightweaveNode> slides = new List<LightweaveNode> {
            Slide(ThemeSlot.SurfaceRaised, "CC_Playground_carousel_Slide_First"),
            Slide(ThemeSlot.SurfaceAccent, "CC_Playground_carousel_Slide_Second"),
            Slide(ThemeSlot.SurfacePrimary, "CC_Playground_carousel_Slide_Third"),
            Slide(ThemeSlot.SurfaceSunken, "CC_Playground_carousel_Slide_Fourth"),
        };

        LightweaveNode carousel = Carousel.Create(
            slides,
            index.Value,
            i => index.Set(i)
        );

        PlaygroundVariant only = new PlaygroundVariant("CC_Playground_Label_Default", carousel);
        return (new[] { only }, EmptyStates);
    }

    private static (IReadOnlyList<PlaygroundVariant>, IReadOnlyList<PlaygroundState>) ContainerDemo() {
        LightweaveNode Viewport(string labelKey, Rem maxWidth, ContainerAlign align) {
            LightweaveNode block = Layout.Layout.Box(
                EdgeInsets.Vertical(SpacingScale.Sm),
                new BackgroundSpec.Solid(ThemeSlot.SurfaceAccent),
                null,
                RadiusSpec.All(new Rem(0.25f)),
                c => c.Add(
                    Typography.Typography.Text(
                        (string)labelKey.Translate(),
                        FontRole.BodyBold,
                        new Rem(0.8125f),
                        ThemeSlot.TextOnAccent,
                        TextAlign.Center
                    )
                )
            );
            LightweaveNode contained = Layout.Layout.Container(
                block,
                maxWidth,
                EdgeInsets.Horizontal(SpacingScale.Xs),
                align
            );
            return Layout.Layout.Box(
                EdgeInsets.All(new Rem(0.25f)),
                new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken),
                BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                RadiusSpec.All(new Rem(0.25f)),
                c => c.Add(contained)
            );
        }

        PlaygroundVariant centered = new PlaygroundVariant(
            "CC_Playground_Container_Centered",
            Viewport("CC_Playground_Container_Centered_Body", new Rem(8f), ContainerAlign.Center)
        );

        PlaygroundVariant start = new PlaygroundVariant(
            "CC_Playground_Container_Start",
            Viewport("CC_Playground_Container_Start_Body", new Rem(8f), ContainerAlign.Start)
        );

        PlaygroundVariant end = new PlaygroundVariant(
            "CC_Playground_Container_End",
            Viewport("CC_Playground_Container_End_Body", new Rem(8f), ContainerAlign.End)
        );

        PlaygroundVariant unconstrained = new PlaygroundVariant(
            "CC_Playground_Container_Unconstrained",
            Viewport("CC_Playground_Container_Unconstrained_Body", default, ContainerAlign.Center)
        );

        return (new[] { centered, start, end, unconstrained }, EmptyStates);
    }
}