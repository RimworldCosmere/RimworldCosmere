using Cosmere.Core.UI.Lightweave.Input;
using Cosmere.Core.UI.Lightweave.Layout;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Tokens;
using Cosmere.Core.UI.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Lightweave.Playground;

public sealed class PlaygroundSampleWindow : LightweaveWindow {
    private readonly string titleKey;
    private readonly string bodyKey;
    private readonly bool drawBorder;
    private readonly bool edgeResizable;
    private readonly Vector2 initialSize;
    private readonly Vector2 minSize;

    public PlaygroundSampleWindow(
        string titleKey,
        string bodyKey,
        Vector2 initialSize,
        bool drawBorder = true,
        bool edgeResizable = true,
        Vector2? minSize = null
    ) {
        this.titleKey = titleKey;
        this.bodyKey = bodyKey;
        this.drawBorder = drawBorder;
        this.edgeResizable = edgeResizable;
        this.initialSize = initialSize;
        this.minSize = minSize ?? new Vector2(320f, 200f);

        doCloseX = true;
        forcePause = false;
        closeOnClickedOutside = false;
    }

    public override Vector2 InitialSize => initialSize;

    protected override bool DrawBorder => drawBorder;

    protected override bool EdgeResizable => edgeResizable;

    protected override Vector2 MinWindowSize => minSize;

    protected override Rect? DragRegion(Rect inRect) {
        return new Rect(inRect.x, inRect.y, inRect.width, 12f);
    }

    protected override LightweaveNode Build() {
        BackgroundSpec? bg = drawBorder
            ? null
            : new BackgroundSpec.Solid(ThemeSlot.SurfacePrimary);

        return Layout.Layout.Box(
            EdgeInsets.All(SpacingScale.Md),
            bg,
            null,
            null,
            c => c.Add(
                Layout.Layout.Stack(
                    SpacingScale.Sm,
                    stack => {
                        stack.Add(
                            Typography.Typography.Heading(
                                2,
                                (string)titleKey.Translate()
                            )
                        );
                        stack.Add(
                            Typography.Typography.Text(
                                (string)bodyKey.Translate(),
                                FontRole.Body,
                                new Rem(0.9375f),
                                ThemeSlot.TextSecondary,
                                wrap: true
                            )
                        );
                        stack.AddFlex(Layout.Layout.Spacer.Flex());
                        stack.Add(
                            Layout.Layout.HStack(
                                SpacingScale.Sm,
                                r => {
                                    r.AddFlex(Layout.Layout.Spacer.Flex());
                                    r.Add(
                                        Button.Create(
                                            (string)"CC_Playground_Window_Close".Translate(),
                                            () => Close(),
                                            ButtonVariant.Secondary
                                        ),
                                        120f
                                    );
                                }
                            ),
                            new Rem(2.5f).ToPixels()
                        );
                    }
                )
            )
        );
    }
}
