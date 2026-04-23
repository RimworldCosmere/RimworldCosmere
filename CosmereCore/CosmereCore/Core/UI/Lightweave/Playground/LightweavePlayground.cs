using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
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
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Controls_Dropdown_Title".Translate()));

                Hooks.Hooks.StateHandle<string> selected = Hooks.Hooks.UseState<string>("Option 1");
                string[] dropdownOptions = new string[]
                {
                    "Option 1",
                    "Option 2",
                    "Option 3",
                    "Option 4 - A very long label to demonstrate truncation in the collapsed view",
                    "Option 5",
                    "Option 6",
                    "Option 7",
                    "Option 8",
                    "Option 9",
                    "Option 10",
                };
                c.Add(Dropdown.Create<string>(
                    value: selected.Value,
                    options: dropdownOptions,
                    labelFn: s => s,
                    onChange: v => selected.Set(v)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Feedback_Title".Translate()));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(ProgressBar.Create(value: 0f, label: "0%", variant: BadgeVariant.Accent));
                    r.Add(ProgressBar.Create(value: 0.3f, label: "30%", variant: BadgeVariant.Accent));
                    r.Add(ProgressBar.Create(value: 0.7f, label: "70%", variant: BadgeVariant.Accent));
                    r.Add(ProgressBar.Create(value: 1f, label: "100%", variant: BadgeVariant.Accent));
                }));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Badge.Create(
                        text: "CC_Playground_Feedback_Badge_Neutral".Translate(),
                        variant: BadgeVariant.Neutral));
                    r.Add(Badge.Create(
                        text: "CC_Playground_Feedback_Badge_Accent".Translate(),
                        variant: BadgeVariant.Accent));
                    r.Add(Badge.Create(
                        text: "CC_Playground_Feedback_Badge_Warning".Translate(),
                        variant: BadgeVariant.Warning));
                    r.Add(Badge.Create(
                        text: "CC_Playground_Feedback_Badge_Danger".Translate(),
                        variant: BadgeVariant.Danger));
                    r.Add(Badge.Create(
                        text: "CC_Playground_Feedback_Badge_Success".Translate(),
                        variant: BadgeVariant.Success));
                }));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Tag.Create(
                        text: "CC_Playground_Feedback_Tag_Dismissible".Translate(),
                        onDismiss: () => { },
                        variant: BadgeVariant.Accent));
                    r.Add(Tag.Create(
                        text: "CC_Playground_Feedback_Tag_Static".Translate(),
                        onDismiss: null,
                        variant: BadgeVariant.Neutral));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Feedback_Tooltip_Title".Translate()));

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    LightweaveNode tooltipButtonBody = Typography.Typography.Text(
                        "CC_Playground_Feedback_Tooltip_Button_Body".Translate());
                    r.Add(Tooltip.Wrap(
                        children: Button.Create(
                            label: "CC_Playground_Feedback_Tooltip_Button_Label".Translate(),
                            onClick: () => { },
                            variant: ButtonVariant.Primary),
                        content: tooltipButtonBody));

                    LightweaveNode tooltipIconBody = Layout.Layout.Column(gap: SpacingScale.Xxs, children: col2 =>
                    {
                        col2.Add(Typography.Typography.Heading(3,
                            "CC_Playground_Feedback_Tooltip_Icon_Title".Translate()));
                        col2.Add(Typography.Typography.Text(
                            "CC_Playground_Feedback_Tooltip_Icon_Body".Translate()));
                    });
                    r.Add(Tooltip.Wrap(
                        children: IconButton.Create(
                            icon: IconPlaceholder(),
                            onClick: () => { },
                            variant: ButtonVariant.Secondary),
                        content: tooltipIconBody));

                    LightweaveNode tooltipLabelBody = Layout.Layout.Column(gap: SpacingScale.Xs, children: col2 =>
                    {
                        col2.Add(Typography.Typography.Text(
                            "CC_Playground_Feedback_Tooltip_Label_Para1".Translate()));
                        col2.Add(Typography.Typography.Text(
                            "CC_Playground_Feedback_Tooltip_Label_Para2".Translate()));
                    });
                    r.Add(Tooltip.Wrap(
                        children: Typography.Typography.Text(
                            "CC_Playground_Feedback_Tooltip_Label_Text".Translate()),
                        content: tooltipLabelBody));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Navigation_Nav_Title".Translate()));

                Hooks.Hooks.StateHandle<string> tabValue = Hooks.Hooks.UseState<string>("General");
                string[] tabItems = new string[]
                {
                    "General",
                    "Combat",
                    "Storage",
                };

                c.Add(Typography.Typography.Heading(3, "CC_Playground_Navigation_Tabs_Label".Translate()));
                c.Add(Tabs.Create<string>(
                    value: tabValue.Value,
                    items: tabItems,
                    labelFn: s => s switch
                    {
                        "General" => (string)"CC_Playground_Navigation_Tabs_General".Translate(),
                        "Combat" => (string)"CC_Playground_Navigation_Tabs_Combat".Translate(),
                        "Storage" => (string)"CC_Playground_Navigation_Tabs_Storage".Translate(),
                        _ => s,
                    },
                    onChange: v => tabValue.Set(v),
                    bodyFn: v => Typography.Typography.Text(v switch
                    {
                        "General" => (string)"CC_Playground_Navigation_Tabs_Body_General".Translate(),
                        "Combat" => (string)"CC_Playground_Navigation_Tabs_Body_Combat".Translate(),
                        "Storage" => (string)"CC_Playground_Navigation_Tabs_Body_Storage".Translate(),
                        _ => string.Empty,
                    })));

                Hooks.Hooks.StateHandle<string> segmentedValue = Hooks.Hooks.UseState<string>("All");
                string[] segmentedItems = new string[]
                {
                    "All",
                    "Armor",
                    "Weapons",
                    "Tools",
                };

                c.Add(Typography.Typography.Heading(3, "CC_Playground_Navigation_Segmented_Label".Translate()));
                c.Add(Segmented.Create<string>(
                    value: segmentedValue.Value,
                    items: segmentedItems,
                    labelFn: s => s switch
                    {
                        "All" => (string)"CC_Playground_Navigation_Segmented_All".Translate(),
                        "Armor" => (string)"CC_Playground_Navigation_Segmented_Armor".Translate(),
                        "Weapons" => (string)"CC_Playground_Navigation_Segmented_Weapons".Translate(),
                        "Tools" => (string)"CC_Playground_Navigation_Segmented_Tools".Translate(),
                        _ => s,
                    },
                    onChange: v => segmentedValue.Set(v)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Overlay_Popover_Title".Translate()));

                Hooks.Hooks.StateHandle<bool> popoverOpen = Hooks.Hooks.UseState<bool>(false);
                Hooks.Hooks.RefHandle<Rect> anchorRef = Hooks.Hooks.UseRef<Rect>(default(Rect));

                LightweaveNode popoverBody = Layout.Layout.Column(gap: SpacingScale.Sm, children: pc =>
                {
                    pc.Add(Typography.Typography.Text("CC_Playground_Overlay_Popover_Body".Translate()));
                    pc.Add(Button.Create(
                        label: "CC_Playground_Overlay_Popover_Close".Translate(),
                        onClick: () => popoverOpen.Set(false),
                        variant: ButtonVariant.Secondary));
                });

                LightweaveNode anchorButton = Button.Create(
                    label: "CC_Playground_Overlay_Popover_Open".Translate(),
                    onClick: () => popoverOpen.Set(!popoverOpen.Value),
                    variant: ButtonVariant.Primary);

                c.Add(AnchorTracker(anchorButton, anchorRef));
                c.Add(Popover.Create(
                    isOpen: popoverOpen.Value,
                    anchorRect: anchorRef.Current,
                    placement: PopoverPlacement.Bottom,
                    content: popoverBody,
                    onDismiss: () => popoverOpen.Set(false)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Navigation_Menu_Title".Translate()));
                c.Add(Layout.Layout.Row(gap: SpacingScale.Md, children: r =>
                {
                    r.Add(MenuDemo(Direction.Ltr));
                    r.Add(MenuDemo(Direction.Rtl));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Overlay_Dialog_Title".Translate()));

                Hooks.Hooks.StateHandle<bool> dialogOpen = Hooks.Hooks.UseState<bool>(false);
                Hooks.Hooks.StateHandle<string> dialogText = Hooks.Hooks.UseState<string>(string.Empty);

                c.Add(Button.Create(
                    label: "CC_Playground_Overlay_Dialog_Open".Translate(),
                    onClick: () => dialogOpen.Set(true),
                    variant: ButtonVariant.Primary));

                c.Add(Dialog.Create(
                    isOpen: dialogOpen.Value,
                    onClose: () => dialogOpen.Set(false),
                    title: () => Typography.Typography.Heading(2,
                        "CC_Playground_Overlay_Dialog_Header".Translate()),
                    body: () => Layout.Layout.Column(gap: SpacingScale.Sm, children: bc =>
                    {
                        bc.Add(Typography.Typography.Text(
                            "CC_Playground_Overlay_Dialog_Body".Translate()));
                        bc.Add(TextField.Create(
                            value: dialogText.Value,
                            onChange: s => dialogText.Set(s),
                            placeholder: "CC_Playground_Overlay_Dialog_Placeholder".Translate()));
                    }),
                    footer: () => Layout.Layout.Row(gap: SpacingScale.Sm, children: fr =>
                    {
                        fr.Add(Button.Create(
                            label: "CC_Playground_Overlay_Dialog_Cancel".Translate(),
                            onClick: () => dialogOpen.Set(false),
                            variant: ButtonVariant.Secondary));
                        fr.Add(Button.Create(
                            label: "CC_Playground_Overlay_Dialog_Confirm".Translate(),
                            onClick: () => dialogOpen.Set(false),
                            variant: ButtonVariant.Primary));
                    })));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Data_Title".Translate()));

                c.Add(Typography.Typography.Heading(3, "CC_Playground_Data_List_Title".Translate()));

                IReadOnlyList<DataRow> rows = Hooks.Hooks.UseMemo(() =>
                {
                    DataRow[] buf = new DataRow[500];
                    for (int i = 0; i < 500; i++)
                    {
                        buf[i] = new DataRow(i, $"Spren #{i:000}");
                    }
                    return (IReadOnlyList<DataRow>)buf;
                }, Array.Empty<object>());

                LightweaveNode listNode = List.Create<DataRow>(
                    items: rows,
                    rowBuilder: (row, _) => KeyValue.Create(
                        label: row.Index.ToString(),
                        value: Typography.Typography.Text(row.Name)),
                    rowHeight: 28f,
                    keyFn: row => row.Index,
                    virtualize: true);

                LightweaveNode listBounds = ListBoundsNode();
                listBounds.Children.Add(listNode);
                listBounds.Paint = (rect, _) =>
                {
                    Rect bounded = new Rect(rect.x, rect.y, rect.width, 320f);
                    LightweaveRoot.PaintSubtree(listNode, bounded);
                };
                c.Add(listBounds);

                c.Add(Typography.Typography.Heading(3, "CC_Playground_Data_KV_Title".Translate()));

                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_Stormlight".Translate(),
                    value: Typography.Typography.Text("94 / 100"),
                    labelWidth: new Rem(8f)));
                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_Investiture".Translate(),
                    value: Typography.Typography.Text("High"),
                    labelWidth: new Rem(8f)));
                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_SprenBond".Translate(),
                    value: Typography.Typography.Text("Fourth Ideal"),
                    labelWidth: new Rem(10f)));
                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_Highstorm".Translate(),
                    value: Typography.Typography.Text("3 days"),
                    labelWidth: new Rem(10f)));
                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_Shardplate".Translate(),
                    value: Typography.Typography.Text("Full"),
                    labelWidth: new Rem(8f)));
                c.Add(KeyValue.Create(
                    label: "CC_Playground_Data_KV_Honorblade".Translate(),
                    value: Typography.Typography.Text("None"),
                    labelWidth: new Rem(8f)));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_UseAnim_Title".Translate()));

                Hooks.Hooks.StateHandle<bool> animToggle = Hooks.Hooks.UseState<bool>(true);
                float alpha = UseAnim.Animate(animToggle.Value ? 1f : 0.3f, 0.3f);

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Button.Create(
                        label: "CC_Playground_UseAnim_Toggle".Translate(),
                        onClick: () => animToggle.Set(!animToggle.Value),
                        variant: ButtonVariant.Primary));
                    LightweaveNode swatch = NodeBuilder.New("AnimSwatch");
                    swatch.Paint = (rect, _) =>
                    {
                        Rect swatchRect = new Rect(rect.x, rect.y + (rect.height - 32f) * 0.5f, 64f, 32f);
                        Color saved = GUI.color;
                        GUI.color = new Color(0.42f, 0.72f, 0.88f, alpha);
                        GUI.DrawTexture(swatchRect, Texture2D.whiteTexture);
                        GUI.color = saved;
                    };
                    r.Add(swatch);
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_UseFocus_Title".Translate()));

                UseFocus.FocusHandle focusHandle = UseFocus.Use();
                Hooks.Hooks.StateHandle<string> focusText = Hooks.Hooks.UseState<string>(string.Empty);

                c.Add(Layout.Layout.Row(gap: SpacingScale.Sm, children: r =>
                {
                    r.Add(Button.Create(
                        label: "CC_Playground_UseFocus_Focus".Translate(),
                        onClick: () => focusHandle.Request(),
                        variant: ButtonVariant.Secondary));

                    LightweaveNode field = NodeBuilder.New("FocusDemoField");
                    string capturedName = focusHandle.Name;
                    Hooks.Hooks.StateHandle<string> capturedText = focusText;
                    field.Paint = (rect, _) =>
                    {
                        float height = new Rem(2f).ToPixels();
                        Rect fieldRect = new Rect(rect.x, rect.y + (rect.height - height) * 0.5f, 200f, height);
                        GUI.SetNextControlName(capturedName);
                        string next = Verse.Widgets.TextField(fieldRect, capturedText.Value ?? string.Empty);
                        if (next != capturedText.Value)
                        {
                            capturedText.Set(next);
                        }
                    };
                    r.Add(field);
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_UseHotkey_Title".Translate()));

                Hooks.Hooks.StateHandle<string> hotkeyState = Hooks.Hooks.UseState<string>("idle");

                UseHotkey.Use(KeyCode.Escape,
                    () => hotkeyState.Set(hotkeyState.Value == "escape" ? "idle" : "escape"));
                UseHotkey.Use(KeyCode.S,
                    () => hotkeyState.Set(hotkeyState.Value == "saved" ? "idle" : "saved"),
                    modifiers: KeyModifiers.Control);

                c.Add(Typography.Typography.Text((string)"CC_Playground_UseHotkey_Hint".Translate()));
                string hotkeyDisplay = hotkeyState.Value switch
                {
                    "escape" => (string)"CC_Playground_UseHotkey_Escape".Translate(),
                    "saved" => (string)"CC_Playground_UseHotkey_Saved".Translate(),
                    _ => (string)"CC_Playground_UseHotkey_Idle".Translate(),
                };
                c.Add(Typography.Typography.Text(hotkeyDisplay));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Spinner_Title".Translate()));
                c.Add(Layout.Layout.Row(gap: SpacingScale.Lg, children: r =>
                {
                    r.Add(Spinner.Create(size: new Rem(1f)));
                    r.Add(Spinner.Create(size: new Rem(1.5f)));
                    r.Add(Spinner.Create(size: new Rem(2f)));
                    r.Add(Spinner.Create(size: new Rem(1.5f), color: ThemeSlot.StatusSuccess));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_RingGauge_Title".Translate()));
                c.Add(Layout.Layout.Row(gap: SpacingScale.Lg, children: r =>
                {
                    r.Add(RingGauge.Create(value: 0.25f, centerLabel: "25%"));
                    r.Add(RingGauge.Create(value: 0.5f, centerLabel: "50%"));
                    r.Add(RingGauge.Create(value: 0.75f, centerLabel: "75%"));
                    r.Add(RingGauge.Create(value: 1f, centerLabel: "100%"));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Sparkline_Title".Translate()));

                IReadOnlyList<float> sparkSamples = Hooks.Hooks.UseMemo(() =>
                {
                    float[] buf = new float[32];
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = Mathf.Sin(i * 0.4f) * 0.5f + 0.5f + Mathf.Sin(i * 0.9f) * 0.2f;
                    }
                    return (IReadOnlyList<float>)buf;
                }, Array.Empty<object>());

                c.Add(Layout.Layout.Column(gap: SpacingScale.Md, children: col2 =>
                {
                    col2.Add(Sparkline.Create(samples: sparkSamples));
                    col2.Add(Sparkline.Create(samples: sparkSamples, fillColor: ThemeSlot.SurfaceAccent));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Breadcrumbs_Title".Translate()));

                Hooks.Hooks.StateHandle<string> lastClicked = Hooks.Hooks.UseState<string>(string.Empty);

                string homeLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Home".Translate();
                string worldbuildingLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Worldbuilding".Translate();
                string rosharLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Roshar".Translate();
                string shatteredPlainsLabel = (string)"CC_Playground_Breadcrumbs_Crumb_ShatteredPlains".Translate();

                string[] basicCrumbs = new string[]
                {
                    homeLabel,
                    worldbuildingLabel,
                    rosharLabel,
                    shatteredPlainsLabel,
                };

                c.Add(Layout.Layout.Column(gap: SpacingScale.Md, children: col2 =>
                {
                    col2.Add(Breadcrumbs.Create(
                        crumbs: basicCrumbs,
                        onNavigate: idx => lastClicked.Set(basicCrumbs[idx])));

                    string statusText = string.IsNullOrEmpty(lastClicked.Value)
                        ? (string)"CC_Playground_Breadcrumbs_NoneClicked".Translate()
                        : (string)"CC_Playground_Breadcrumbs_LastClicked".Translate(lastClicked.Value.Named("CRUMB"));
                    col2.Add(Typography.Typography.Caption(statusText));

                    col2.Add(Typography.Typography.Heading(3, "CC_Playground_Breadcrumbs_LongDemoTitle".Translate()));

                    string worldsLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Worlds".Translate();
                    string scadrialLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Scadrial".Translate();
                    string finalEmpireLabel = (string)"CC_Playground_Breadcrumbs_Crumb_FinalEmpire".Translate();
                    string luthadelLabel = (string)"CC_Playground_Breadcrumbs_Crumb_Luthadel".Translate();
                    string centralDistrictLabel = (string)"CC_Playground_Breadcrumbs_Crumb_CentralDistrict".Translate();
                    string ventureKeepLabel = (string)"CC_Playground_Breadcrumbs_Crumb_VentureKeep".Translate();
                    string greatHallLabel = (string)"CC_Playground_Breadcrumbs_Crumb_GreatHall".Translate();

                    string[] longCrumbs = new string[]
                    {
                        homeLabel,
                        worldsLabel,
                        scadrialLabel,
                        finalEmpireLabel,
                        luthadelLabel,
                        centralDistrictLabel,
                        ventureKeepLabel,
                        greatHallLabel,
                    };

                    col2.Add(BreadcrumbsBounds(
                        Breadcrumbs.Create(
                            crumbs: longCrumbs,
                            onNavigate: idx => lastClicked.Set(longCrumbs[idx]))));
                }));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_ContextMenu_Title".Translate()));

                Hooks.Hooks.StateHandle<string> lastAction = Hooks.Hooks.UseState<string>(string.Empty);

                IReadOnlyList<MenuItem> contextItems = new MenuItem[]
                {
                    new MenuItem(
                        "CC_Playground_ContextMenu_Inspect".Translate(),
                        OnInvoke: () => lastAction.Set((string)"CC_Playground_ContextMenu_Inspect".Translate())),
                    new MenuItem(
                        "CC_Playground_ContextMenu_Rename".Translate(),
                        OnInvoke: () => lastAction.Set((string)"CC_Playground_ContextMenu_Rename".Translate())),
                    new MenuItem(
                        "CC_Playground_ContextMenu_Duplicate".Translate(),
                        OnInvoke: () => lastAction.Set((string)"CC_Playground_ContextMenu_Duplicate".Translate())),
                    new MenuItem(
                        "CC_Playground_ContextMenu_Delete".Translate(),
                        OnInvoke: () => lastAction.Set((string)"CC_Playground_ContextMenu_Delete".Translate())),
                };

                LightweaveNode hintArea = ContextMenuHintArea();
                c.Add(Navigation.ContextMenu.Create(child: hintArea, items: contextItems));

                string actionText = string.IsNullOrEmpty(lastAction.Value)
                    ? (string)"CC_Playground_ContextMenu_Idle".Translate()
                    : (string)"CC_Playground_ContextMenu_LastAction".Translate(lastAction.Value.Named("ACTION"));
                c.Add(Typography.Typography.Caption(actionText));
            }));

            col.Add(Surface.Surface.Card(c =>
            {
                c.Add(Typography.Typography.Heading(2, "CC_Playground_Table_Title".Translate()));

                IReadOnlyList<WorldEntry> worldRows = new WorldEntry[]
                {
                    new WorldEntry("Roshar", "Honor, Cultivation", "Unknown"),
                    new WorldEntry("Scadrial", "Ruin, Preservation", "~1M"),
                    new WorldEntry("Nalthis", "Endowment", "~5M"),
                    new WorldEntry("Sel", "Dominion, Devotion", "~15M"),
                    new WorldEntry("Taldain", "Autonomy", "~500K"),
                };

                IReadOnlyList<TableColumn<WorldEntry>> worldColumns = new TableColumn<WorldEntry>[]
                {
                    new TableColumn<WorldEntry>(
                        Header: (string)"CC_Playground_Table_Col_World".Translate(),
                        CellRenderer: entry => Typography.Typography.Text(entry.World),
                        Width: new Rem(8f)),
                    new TableColumn<WorldEntry>(
                        Header: (string)"CC_Playground_Table_Col_Shards".Translate(),
                        CellRenderer: entry => Typography.Typography.Text(entry.Shard)),
                    new TableColumn<WorldEntry>(
                        Header: (string)"CC_Playground_Table_Col_Population".Translate(),
                        CellRenderer: entry => Typography.Typography.Text(entry.Population),
                        Width: new Rem(7f)),
                };

                LightweaveNode tableNode = Table.Create<WorldEntry>(
                    rows: worldRows,
                    columns: worldColumns,
                    rowHeight: new Rem(2f),
                    keyFn: entry => entry.World);

                c.Add(TablePlaygroundBounds(tableNode));
            }));

        });
    }

    private readonly record struct WorldEntry(string World, string Shard, string Population);

    private static LightweaveNode TablePlaygroundBounds(
        LightweaveNode inner,
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("TablePlaygroundBounds", line, file);
        node.Children.Add(inner);
        node.Paint = (rect, _) =>
        {
            float height = new Rem(14f).ToPixels();
            Rect bounded = new Rect(rect.x, rect.y, rect.width, height);
            inner.MeasuredRect = bounded;
            LightweaveRoot.PaintSubtree(inner, bounded);
        };
        return node;
    }

    private static LightweaveNode ContextMenuHintArea(
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("ContextMenuHintArea", line, file);
        node.Paint = (rect, _) =>
        {
            float height = new Rem(6f).ToPixels();
            Rect boxRect = new Rect(rect.x, rect.y, rect.width, height);

            BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
            BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
            PaintBox.Draw(boxRect, bg, border, radius);

            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Body);
            int pixelSize = Mathf.RoundToInt(new Rem(0.875f).ToPixels());
            GUIStyle style = GuiStyleCache.Get(font, pixelSize, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleCenter;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(boxRect), (string)"CC_Playground_ContextMenu_RightClickHint".Translate(), style);
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BreadcrumbsBounds(
        LightweaveNode inner,
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("BreadcrumbsBounds", line, file);
        node.Children.Add(inner);
        node.Paint = (rect, _) =>
        {
            float width = new Rem(20f).ToPixels();
            float height = new Rem(1.5f).ToPixels();
            Rect bounded = new Rect(rect.x, rect.y, Mathf.Min(width, rect.width), height);
            inner.MeasuredRect = bounded;
            LightweaveRoot.PaintSubtree(inner, bounded);
        };
        return node;
    }

    private readonly record struct DataRow(int Index, string Name);

    private static LightweaveNode ListBoundsNode(
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        return NodeBuilder.New("ListBounds", line, file);
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

    private static LightweaveNode MenuDemo(
        Direction direction,
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        Hooks.Hooks.StateHandle<bool> menuOpen = Hooks.Hooks.UseState<bool>(false, line, file + "#open");
        Hooks.Hooks.RefHandle<Rect> anchorRef = Hooks.Hooks.UseRef<Rect>(default(Rect), line, file + "#anchor");

        IReadOnlyList<MenuItem> saveAsChildren = new MenuItem[]
        {
            new MenuItem("CC_Playground_Navigation_Menu_Txt".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_Json".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_Xml".Translate()),
        };
        IReadOnlyList<MenuItem> exportChildren = new MenuItem[]
        {
            new MenuItem("CC_Playground_Navigation_Menu_Pdf".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_Png".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_Svg".Translate()),
        };
        IReadOnlyList<MenuItem> items = new MenuItem[]
        {
            new MenuItem("CC_Playground_Navigation_Menu_Open".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_Save".Translate()),
            new MenuItem("CC_Playground_Navigation_Menu_SaveAs".Translate(), Children: saveAsChildren),
            new MenuItem("CC_Playground_Navigation_Menu_Export".Translate(), Children: exportChildren),
            new MenuItem("CC_Playground_Navigation_Menu_Reload".Translate(), Disabled: true),
            new MenuItem("CC_Playground_Navigation_Menu_Close".Translate(),
                OnInvoke: () => menuOpen.Set(false)),
        };

        LightweaveNode anchorButton = Button.Create(
            label: direction == Direction.Ltr
                ? "CC_Playground_Navigation_Menu_OpenLtr".Translate()
                : "CC_Playground_Navigation_Menu_OpenRtl".Translate(),
            onClick: () => menuOpen.Set(!menuOpen.Value),
            variant: ButtonVariant.Primary);

        LightweaveNode body = Layout.Layout.Column(gap: SpacingScale.Xs, children: col =>
        {
            col.Add(AnchorTracker(anchorButton, anchorRef));
            col.Add(Menu.Create(
                isOpen: menuOpen.Value,
                anchorRect: anchorRef.Current,
                items: items,
                onDismiss: () => menuOpen.Set(false),
                instanceKey: direction));
        });

        return DirectionScope(direction, body, line, file);
    }

    private static LightweaveNode DirectionScope(
        Direction direction,
        LightweaveNode inner,
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New($"DirectionScope:{direction}", line, file);
        node.Children.Add(inner);
        node.Paint = (rect, _) =>
        {
            RenderContext ctx = RenderContext.Current;
            ctx.DirectionStack.Push(direction);
            try
            {
                inner.MeasuredRect = rect;
                LightweaveRoot.PaintSubtree(inner, rect);
            }
            finally
            {
                ctx.DirectionStack.Pop();
            }
        };
        return node;
    }

    private static LightweaveNode AnchorTracker(
        LightweaveNode inner,
        Hooks.Hooks.RefHandle<Rect> anchorRef,
        [global::System.Runtime.CompilerServices.CallerLineNumber] int line = 0,
        [global::System.Runtime.CompilerServices.CallerFilePath] string file = "")
    {
        LightweaveNode node = NodeBuilder.New("AnchorTracker", line, file);
        node.Children.Add(inner);
        node.Paint = (rect, _) =>
        {
            anchorRef.Current = rect;
            inner.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(inner, rect);
        };
        return node;
    }
}
