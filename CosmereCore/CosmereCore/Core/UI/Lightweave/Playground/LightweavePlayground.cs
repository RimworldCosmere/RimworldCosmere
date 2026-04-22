using UnityEngine;
using Cosmere.Core.UI.Lightweave.Layout;
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
}
