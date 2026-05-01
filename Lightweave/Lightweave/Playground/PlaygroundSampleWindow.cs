using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

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

        return Layout.Layout.Box.Create(
            EdgeInsets.All(SpacingScale.Md),
            bg,
            null,
            null,
            c => c.Add(
                Layout.Layout.Stack.Create(
                    SpacingScale.Sm,
                    stack => {
                        stack.Add(
                            Typography.Typography.Heading.Create(
                                2,
                                (string)titleKey.Translate()
                            )
                        );
                        stack.Add(
                            Typography.Typography.Text.Create(
                                (string)bodyKey.Translate(),
                                FontRole.Body,
                                new Rem(0.9375f),
                                ThemeSlot.TextSecondary,
                                wrap: true
                            )
                        );
                        stack.AddFlex(Layout.Layout.Spacer.Flex());
                        stack.Add(
                            Layout.Layout.HStack.Create(
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
