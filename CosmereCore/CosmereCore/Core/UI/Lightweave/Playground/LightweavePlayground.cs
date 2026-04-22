using UnityEngine;
using Verse;
using Cosmere.Core.UI.Lightweave.Hooks;
using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Rendering;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Surface;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Typography;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed class LightweavePlayground : LightweaveWindow
{
    public static Direction? DirectionOverride;

    public override Vector2 InitialSize => new Vector2(900f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        LightweaveRoot.Render(inRect, RootId, Build, DirectionOverride);
    }

    protected override LightweaveNode Build()
    {
        return Layout.Layout.Column(gap: SpacingScale.Md, children: col =>
        {
            col.Add(Typography.Typography.Heading(1, "Lightweave Playground"));
            col.Add(Typography.Typography.Caption($"Direction: {DirectionOverride?.ToString() ?? "auto"}"));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "Layout"));
                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Surface.Surface.ByRole(SurfaceRole.Raised, padding: EdgeInsets.All(SpacingScale.Sm),
                        children: rb => rb.Add(Typography.Typography.Text("A"))));
                    r.Add(Surface.Surface.ByRole(SurfaceRole.Raised, padding: EdgeInsets.All(SpacingScale.Sm),
                        children: rb => rb.Add(Typography.Typography.Text("B"))));
                    r.Add(Surface.Surface.ByRole(SurfaceRole.Raised, padding: EdgeInsets.All(SpacingScale.Sm),
                        children: rb => rb.Add(Typography.Typography.Text("C"))));
                }));
                c.Add(Layout.Layout.Divider.Horizontal());
                c.Add(Layout.Layout.Grid(
                    columns: new GridTrack[] { GridTrack.Of(SpacingScale.Xxxl), new GridTrack.Fr(1), new GridTrack.Fr(2) },
                    gap: SpacingScale.Xs,
                    children: g =>
                    {
                        g.Add(Typography.Typography.Text("48px"));
                        g.Add(Typography.Typography.Text("1fr"));
                        g.Add(Typography.Typography.Text("2fr"));
                    }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "Typography stress"));
                float[] sizes = { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 2.5f, 3f, 4f };
                foreach (float s in sizes)
                {
                    c.Add(Typography.Typography.Text($"{s}rem - The quick brown fox jumps over the lazy dog.",
                        size: new Rem(s)));
                }
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Controls_Buttons_Title".Translate()));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonPrimary".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Primary));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonSecondary".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Secondary));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonGhost".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Ghost));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonDanger".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Danger));
                }));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonPrimary".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Primary,
                        disabled: true));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonSecondary".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Secondary,
                        disabled: true));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonGhost".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Ghost,
                        disabled: true));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonDanger".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Danger,
                        disabled: true));
                }));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonLeading".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Primary,
                        leading: IconPlaceholder()));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonTrailing".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Secondary,
                        trailing: IconPlaceholder()));
                    r.Add(Button.Create(
                        label: "CC_Playground_Controls_ButtonBothIcons".Translate(),
                        onClick: () => { },
                        variant: ButtonVariant.Ghost,
                        leading: IconPlaceholder(),
                        trailing: IconPlaceholder()));
                }));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(IconButton.Create(
                        icon: IconPlaceholder(),
                        onClick: () => { },
                        variant: ButtonVariant.Ghost,
                        tooltipKey: "CC_Playground_Controls_IconButtonTooltip"));
                    r.Add(IconButton.Create(
                        icon: IconPlaceholder(),
                        onClick: () => { },
                        variant: ButtonVariant.Primary));
                    r.Add(IconButton.Create(
                        icon: IconPlaceholder(),
                        onClick: () => { },
                        variant: ButtonVariant.Danger));
                    r.Add(IconButton.Create(
                        icon: IconPlaceholder(),
                        onClick: () => { },
                        variant: ButtonVariant.Secondary,
                        disabled: true));
                }));

                Hooks.Hooks.StateHandle<bool> toggleState = Hooks.Hooks.UseState<bool>(false);
                c.Add(ToggleButton.Create(
                    label: toggleState.Value
                        ? "CC_Playground_Controls_ToggleOn".Translate()
                        : "CC_Playground_Controls_ToggleOff".Translate(),
                    value: toggleState.Value,
                    onChange: next => toggleState.Set(next)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Controls_Toggles_Title".Translate()));

                Hooks.Hooks.StateHandle<bool> checkA = Hooks.Hooks.UseState<bool>(false);
                Hooks.Hooks.StateHandle<bool> checkB = Hooks.Hooks.UseState<bool>(true);
                Hooks.Hooks.StateHandle<bool> checkC = Hooks.Hooks.UseState<bool>(false);

                c.Add(Checkbox.Create(
                    label: "CC_Playground_Controls_Checkbox_OptionA".Translate(),
                    value: checkA.Value,
                    onChange: next => checkA.Set(next)));
                c.Add(Checkbox.Create(
                    label: "CC_Playground_Controls_Checkbox_OptionB".Translate(),
                    value: checkB.Value,
                    onChange: next => checkB.Set(next)));
                c.Add(Checkbox.Create(
                    label: "CC_Playground_Controls_Checkbox_OptionC".Translate(),
                    value: checkC.Value,
                    onChange: next => checkC.Set(next),
                    disabled: true));

                Hooks.Hooks.StateHandle<string> radioState = Hooks.Hooks.UseState<string>("A");
                c.Add(Radio.Group<string>(
                    value: radioState.Value,
                    onChange: next => radioState.Set(next),
                    children: g =>
                    {
                        g.Add(Radio.Item<string>(
                            label: "CC_Playground_Controls_Radio_OptionA".Translate(),
                            value: "A"));
                        g.Add(Radio.Item<string>(
                            label: "CC_Playground_Controls_Radio_OptionB".Translate(),
                            value: "B"));
                        g.Add(Radio.Item<string>(
                            label: "CC_Playground_Controls_Radio_OptionC".Translate(),
                            value: "C"));
                    }));

                Hooks.Hooks.StateHandle<bool> switchState = Hooks.Hooks.UseState<bool>(false);
                c.Add(Switch.Create(
                    label: "CC_Playground_Controls_Switch_Label".Translate(),
                    value: switchState.Value,
                    onChange: next => switchState.Set(next)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Controls_Sliders_Title".Translate()));

                Hooks.Hooks.StateHandle<float> sliderVal = Hooks.Hooks.UseState<float>(0.5f);
                float[] marks = { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
                c.Add(Slider.Create(
                    value: sliderVal.Value,
                    onChange: v => sliderVal.Set(v),
                    min: 0f, max: 1f, step: 0.05f,
                    marks: marks,
                    format: v => $"{v:P0}"));

                Hooks.Hooks.StateHandle<float> sliderSmooth = Hooks.Hooks.UseState<float>(0.25f);
                c.Add(Slider.Create(
                    value: sliderSmooth.Value,
                    onChange: v => sliderSmooth.Set(v),
                    min: 0f, max: 1f,
                    format: v => $"{v:0.00}"));

                Hooks.Hooks.StateHandle<float> sliderDisabled = Hooks.Hooks.UseState<float>(0.7f);
                c.Add(Slider.Create(
                    value: sliderDisabled.Value,
                    onChange: v => sliderDisabled.Set(v),
                    min: 0f, max: 1f, step: 0.05f,
                    marks: marks,
                    format: v => $"{v:P0}",
                    disabled: true));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Controls_TextInputs_Title".Translate()));

                Hooks.Hooks.StateHandle<string> textValue = Hooks.Hooks.UseState<string>(string.Empty);
                c.Add(TextField.Create(
                    value: textValue.Value,
                    onChange: next => textValue.Set(next),
                    placeholder: "CC_Playground_Controls_TextField_Placeholder".Translate(),
                    validator: s => !string.IsNullOrWhiteSpace(s)));

                Hooks.Hooks.StateHandle<string> areaValue = Hooks.Hooks.UseState<string>(string.Empty);
                c.Add(TextArea.Create(
                    value: areaValue.Value,
                    onChange: next => areaValue.Set(next),
                    placeholder: "CC_Playground_Controls_TextArea_Placeholder".Translate(),
                    minRows: 3,
                    maxRows: 8));

                Hooks.Hooks.StateHandle<float> numberValue = Hooks.Hooks.UseState<float>(50f);
                c.Add(Typography.Typography.Caption("CC_Playground_Controls_NumberField_Label".Translate()));
                c.Add(NumberField.Create(
                    value: numberValue.Value,
                    onChange: v => numberValue.Set(v),
                    min: 0f,
                    max: 100f,
                    format: v => v.ToString("0")));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "500-row virtualized list"));
                c.Add(Layout.Layout.ScrollArea(contentHeight: 500 * 32f, children: sa =>
                {
                    for (int i = 0; i < 500; i++)
                    {
                        sa.Add(Typography.Typography.Text($"Row {i}"));
                    }
                }));
            }));
        });
    }

    private static LightweaveNode IconPlaceholder(
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("IconPlaceholder", line, file);
        node.Paint = (rect, _) =>
        {
            BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.TextMuted);
            RadiusSpec radius = RadiusSpec.All(new Rem(0.125f));
            PaintBox.Draw(rect, bg, null, radius);
        };
        return node;
    }
}
