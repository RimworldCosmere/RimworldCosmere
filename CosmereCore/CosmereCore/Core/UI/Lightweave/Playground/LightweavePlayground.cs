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
