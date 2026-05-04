using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Layout;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Code = Cosmere.Lightweave.Typography.Typography.Code;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;
using Icon = Cosmere.Lightweave.Typography.Typography.Icon;
using Label = Cosmere.Lightweave.Typography.Typography.Label;
using RichText = Cosmere.Lightweave.Typography.Typography.RichText;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Overlay;

public enum DrawerSide {
    Left,
    Right,
    Top,
    Bottom,
}

[Doc(
    Id = "drawer",
    Summary = "Edge-anchored panel that slides in from a window side.",
    WhenToUse = "Reveal secondary navigation, filters, or detail content without leaving the page.",
    SourcePath = "Lightweave/Lightweave/Overlay/Drawer.cs"
)]
public static class Drawer {
    private static readonly Func<float, float> EaseOutCubic = t => 1f - Mathf.Pow(1f - t, 3f);

    public static LightweaveNode Create(
        [DocParam("Whether the drawer is currently visible.")]
        bool isOpen,
        [DocParam("Which window edge the drawer slides in from.")]
        DrawerSide side,
        [DocParam("Builds the drawer content node.")]
        Func<LightweaveNode> content,
        [DocParam("Invoked when the user clicks the scrim or presses Escape.")]
        Action onDismiss,
        [DocParam("Drawer thickness in Rem; width for Left/Right, height for Top/Bottom.")]
        Rem? size = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Drawer:{side}", line, file);
        node.Paint = (_, _) => {
            float target = isOpen ? 1f : 0f;
            float progress = UseAnim.Animate(target, 0.22f, EaseOutCubic, line, file);

            if (!isOpen && progress <= 0.001f) {
                return;
            }

            Rect host = RenderContext.Current.RootRect;

            float widthPx;
            float heightPx;
            if (side == DrawerSide.Left || side == DrawerSide.Right) {
                widthPx = (size ?? new Rem(20f)).ToPixels();
                heightPx = host.height;
            }
            else {
                widthPx = host.width;
                heightPx = (size ?? new Rem(12f)).ToPixels();
            }

            float restX;
            float restY;
            float offscreenX;
            float offscreenY;
            switch (side) {
                case DrawerSide.Left:
                    restX = host.x;
                    restY = host.y;
                    offscreenX = host.x - widthPx;
                    offscreenY = host.y;
                    break;
                case DrawerSide.Right:
                    restX = host.xMax - widthPx;
                    restY = host.y;
                    offscreenX = host.xMax;
                    offscreenY = host.y;
                    break;
                case DrawerSide.Top:
                    restX = host.x;
                    restY = host.y;
                    offscreenX = host.x;
                    offscreenY = host.y - heightPx;
                    break;
                default:
                    restX = host.x;
                    restY = host.yMax - heightPx;
                    offscreenX = host.x;
                    offscreenY = host.yMax;
                    break;
            }

            float drawerX = Mathf.Lerp(offscreenX, restX, progress);
            float drawerY = Mathf.Lerp(offscreenY, restY, progress);
            Rect drawerRect = new Rect(drawerX, drawerY, widthPx, heightPx);
            float scrimAlpha = progress * 0.35f;

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                Color savedColor = GUI.color;
                GUI.color = Color.white;

                Rect screenRect = host;
                BackgroundSpec scrimBg = new BackgroundSpec.Solid(new Color(0f, 0f, 0f, scrimAlpha));
                PaintBox.Draw(screenRect, scrimBg, null, null);

                Rect shadowRect = new Rect(
                    drawerRect.x + 3f,
                    drawerRect.y + 3f,
                    drawerRect.width,
                    drawerRect.height
                );
                BackgroundSpec shadowBg = new BackgroundSpec.Solid(new Color(0f, 0f, 0f, 0.35f));
                PaintBox.Draw(shadowRect, shadowBg, null, null);

                BackgroundSpec drawerBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                BorderSpec? drawerBorder = ResolveBorder(side);
                PaintBox.Draw(drawerRect, drawerBg, drawerBorder, null);

                float padPx = SpacingScale.Md.ToPixels();
                Rect innerRect = new Rect(
                    drawerRect.x + padPx,
                    drawerRect.y + padPx,
                    Mathf.Max(0f, drawerRect.width - padPx * 2f),
                    Mathf.Max(0f, drawerRect.height - padPx * 2f)
                );

                LightweaveNode inner = content();
                LightweaveRoot.PaintSubtree(inner, innerRect);

                GUI.color = savedColor;

                Event e = Event.current;
                if (e.type == EventType.MouseDown &&
                    !drawerRect.Contains(e.mousePosition) &&
                    screenRect.Contains(e.mousePosition)) {
                    onDismiss?.Invoke();
                    e.Use();
                }
                else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) {
                    onDismiss?.Invoke();
                    e.Use();
                }
            }
            );
        };
        return node;
    }

    private static LightweaveNode BuildHostDemo() {
        StateHandle<bool> open = UseState(false);

        LightweaveNode trigger = Button.Create(
            (string)"CC_Playground_Drawer_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );

        LightweaveNode drawer = Create(
            open.Value,
            DrawerSide.Right,
            () => Stack.Create(
                SpacingScale.Sm,
                s => {
                    s.Add(
                        Heading.Create(
                            3,
                            (string)"CC_Playground_Drawer_ContentTitle".Translate()
                        ),
                        28f
                    );
                    s.Add(
                        Text.Create(
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
                    LightweaveNode closeRow = HStack.Create(
                        SpacingScale.Xs,
                        r => {
                            r.AddFlex(NodeBuilder.New("Spacer", 0, nameof(Drawer)));
                            r.Add(closeBtn, 96f);
                        }
                    );
                    s.Add(closeRow, 32f);
                }
            ),
            () => open.Set(false)
        );

        LightweaveNode composed = NodeBuilder.New("DrawerHost", 0, nameof(Drawer));
        composed.Children.Add(trigger);
        composed.Children.Add(drawer);
        composed.Measure = w => trigger.Measure?.Invoke(w) ?? trigger.PreferredHeight ?? 32f;
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            drawer.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(drawer, rect);
        };
        return composed;
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(BuildHostDemo(), useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildHostDemo(), useFullSource: true);
    }

    private static BorderSpec ResolveBorder(DrawerSide side) {
        Rem thickness = new Rem(1f / 16f);
        switch (side) {
            case DrawerSide.Left:
                return new BorderSpec(Right: thickness, Color: ThemeSlot.BorderDefault);
            case DrawerSide.Right:
                return new BorderSpec(Left: thickness, Color: ThemeSlot.BorderDefault);
            case DrawerSide.Top:
                return new BorderSpec(Bottom: thickness, Color: ThemeSlot.BorderDefault);
            default:
                return new BorderSpec(thickness, Color: ThemeSlot.BorderDefault);
        }
    }
}