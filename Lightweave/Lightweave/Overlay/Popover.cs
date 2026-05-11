using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Feedback;
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

[Doc(
    Id = "popover",
    Summary = "Floating panel anchored to a trigger, dismissed on outside click.",
    WhenToUse = "Reveal contextual options or details next to the element that opened them.",
    SourcePath = "Lightweave/Lightweave/Overlay/Popover.cs"
)]
public static class Popover {
    public static LightweaveNode Create(
        [DocParam("Whether the popover is currently visible.")]
        bool isOpen,
        [DocParam("Trigger Rect in current GUI space; used to position the popover.")]
        Rect anchorRect,
        [DocParam("Side of the anchor on which the popover should appear.")]
        PopoverPlacement placement,
        [DocParam("Content node painted inside the popover.")]
        LightweaveNode content,
        [DocParam("Invoked when the user clicks outside the popover.")]
        Action onDismiss,
        [DocParam("Preferred size in pixels. Height of -1 auto-sizes to content.")]
        Vector2? preferredSize = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        if (!isOpen) {
            LightweaveNode empty = NodeBuilder.New("Popover:closed", line, file);
            empty.Paint = (_, _) => { };
            return empty;
        }

        LightweaveNode node = NodeBuilder.New($"Popover:{placement}", line, file);
        node.ApplyStyling("popover", style, classes, id);
        node.Paint = (rect, paintChildren) => {
            Vector2 size = preferredSize ?? new Vector2(new Rem(15f).ToPixels(), -1f);
            if (size.x <= 0f) {
                size.x = new Rem(15f).ToPixels();
            }

            if (size.y <= 0f) {
                float measuredHeight = content.Measure?.Invoke(size.x) ??
                                       content.PreferredHeight ?? new Rem(10f).ToPixels();
                size.y = measuredHeight;
            }

            Rect screen = RenderContext.Current.RootRect;
            Direction dir = RenderContext.Current.Direction;

            Rect anchorAbsolute = OverlayAnchor.CaptureAbsolute(anchorRect);

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                Rect anchorHere = OverlayAnchor.ResolveLocal(anchorAbsolute);
                Rect popoverRect = PopoverLayout.Resolve(anchorHere, placement, dir, size, screen);

                Color savedColor = GUI.color;
                GUI.color = Color.white;

                Rect shadowRect = new Rect(
                    popoverRect.x + 2f,
                    popoverRect.y + 3f,
                    popoverRect.width,
                    popoverRect.height
                );
                BackgroundSpec shadowBg = BackgroundSpec.Of(ThemeSlot.SurfaceShadow);
                PaintBox.Draw(shadowRect, shadowBg, null, RadiusSpec.All(RadiusScale.Lg));

                BackgroundSpec bg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
                BorderSpec border = BorderSpec.All(new Rem(2f / 16f), ThemeSlot.BorderDefault);
                RadiusSpec radius = RadiusSpec.All(RadiusScale.Lg);
                PaintBox.Draw(popoverRect, bg, border, radius);

                LightweaveRoot.PaintSubtree(content, popoverRect);

                GUI.color = savedColor;

                Event e = Event.current;
                if (e.rawType == EventType.MouseDown &&
                    !popoverRect.Contains(e.mousePosition) &&
                    !anchorHere.Contains(e.mousePosition)) {
                    onDismiss?.Invoke();
                    if (e.type == EventType.MouseDown) {
                        e.Use();
                    }
                }
            }
            );
        };
        return node;
    }

    private static LightweaveNode BuildHostDemo() {
        StateHandle<bool> open = UseState(false);
        RefHandle<Rect> anchor = UseRef(default(Rect));

        LightweaveNode button = Button.Create(
            (string)"CL_Playground_Popover_TriggerOpen".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );

        LightweaveNode trigger = NodeBuilder.New("PopoverTrigger", 0, nameof(Popover));
        trigger.Children.Add(button);
        trigger.Paint = (rect, _) => {
            anchor.Current = rect;
            button.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(button, rect);
        };

        LightweaveNode body = Box.Create(
            k => k.Add(
                Text.Create(
                    (string)"CL_Playground_Overlay_Popover_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.875f), TextColor = ThemeSlot.TextPrimary }
                )
            ),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Md),
            }
        );

        // ReSharper disable once ArrangeStaticMemberQualifier
        LightweaveNode popover = Popover.Create(
            open.Value,
            anchor.Current,
            PopoverPlacement.Bottom,
            body,
            () => open.Set(false),
            new Vector2(new Rem(15f).ToPixels(), -1f)
        );

        LightweaveNode composed = NodeBuilder.New("PopoverHost", 0, nameof(Popover));
        composed.Children.Add(trigger);
        composed.Children.Add(popover);
        composed.Measure = w => button.Measure?.Invoke(w) ?? button.PreferredHeight ?? 32f;
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            popover.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(popover, rect);
        };
        return composed;
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => BuildHostDemo(), useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => BuildHostDemo(), useFullSource: true);
    }

    private static LightweaveNode BuildPawnCardDemo() {
        StateHandle<bool> open = UseState(false);
        RefHandle<Rect> anchor = UseRef(default(Rect));

        LightweaveNode button = Button.Create(
            (string)"CL_Playground_Overlay_Popover_PawnCard_Trigger".Translate(),
            () => open.Set(!open.Value),
            ButtonVariant.Secondary
        );

        LightweaveNode trigger = NodeBuilder.New("PopoverTrigger:PawnCard", 0, nameof(Popover));
        trigger.Children.Add(button);
        trigger.Paint = (rect, _) => {
            anchor.Current = rect;
            button.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(button, rect);
        };

        LightweaveNode avatar = Box.Create(
            style: new Style {
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault),
                Radius = RadiusSpec.All(RadiusScale.Full),
            }
        );
        avatar.PreferredHeight = new Rem(2.75f).ToPixels();

        LightweaveNode identity = Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Heading.Create(3, (string)"CL_Playground_Overlay_Popover_PawnCard_Name".Translate()));
                s.Add(
                    Badge.Create(
                        (string)"CL_Playground_Overlay_Popover_PawnCard_Order".Translate(),
                        BadgeVariant.Accent
                    )
                );
            }
        );

        LightweaveNode header = HStack.Create(
            SpacingScale.Md,
            h => {
                h.Add(avatar, new Rem(2.75f).ToPixels());
                h.AddFlex(identity);
            }
        );

        LightweaveNode stormlight = Stack.Create(
            SpacingScale.Xs,
            s => {
                s.Add(
                    HStack.Create(
                        SpacingScale.Sm,
                        h => {
                            h.AddFlex(Label.Create((string)"CL_Playground_Overlay_Popover_PawnCard_Stormlight".Translate()));
                            h.Add(
                                Text.Create(
                                    (string)"CL_Playground_Overlay_Popover_PawnCard_StormlightValue".Translate(),
                                    style: new Style { FontFamily = FontRole.BodyBold, FontSize = new Rem(0.875f), TextColor = ThemeSlot.TextPrimary, TextAlign = TextAlign.End, FontWeight = FontStyle.Bold }
                                ),
                                new Rem(5f).ToPixels()
                            );
                        }
                    )
                );
                s.Add(ProgressBar.Create(412f, 0f, 1000f, variant: BadgeVariant.Accent));
            }
        );

        LightweaveNode actions = HStack.Create(
            SpacingScale.Sm,
            h => {
                h.AddFlex(
                    Button.Create(
                        (string)"CL_Playground_Overlay_Popover_PawnCard_AbilityLashing".Translate(),
                        () => { },
                        ButtonVariant.Primary,
                        style: new Style { Width = Length.Stretch }
                    )
                );
                h.AddFlex(
                    Button.Create(
                        (string)"CL_Playground_Overlay_Popover_PawnCard_AbilityAdhesion".Translate(),
                        () => { },
                        ButtonVariant.Secondary,
                        style: new Style { Width = Length.Stretch }
                    )
                );
            }
        );

        LightweaveNode body = Stack.Create(
            SpacingScale.Md,
            s => {
                s.Add(header);
                s.Add(stormlight);
                s.Add(actions);
            }
        );

        LightweaveNode card = Box.Create(
            k => k.Add(body),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Md),
            }
        );

        // ReSharper disable once ArrangeStaticMemberQualifier
        LightweaveNode popover = Popover.Create(
            open.Value,
            anchor.Current,
            PopoverPlacement.Bottom,
            card,
            () => open.Set(false),
            new Vector2(new Rem(18f).ToPixels(), -1f)
        );

        LightweaveNode composed = NodeBuilder.New("PopoverHost:PawnCard", 0, nameof(Popover));
        composed.Children.Add(trigger);
        composed.Children.Add(popover);
        composed.Measure = w => button.Measure?.Invoke(w) ?? button.PreferredHeight ?? 32f;
        composed.Paint = (rect, _) => {
            trigger.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(trigger, rect);
            popover.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(popover, rect);
        };
        return composed;
    }

    [DocVariant("CL_Playground_Overlay_Popover_PawnCard", Order = 1)]
    public static DocSample DocsPawnCard() {
        return new DocSample(() => BuildPawnCardDemo(), useFullSource: true);
    }
}