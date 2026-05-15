using System;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Fonts;
using Cosmere.Lightweave.Icons;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Display = Cosmere.Lightweave.Typography.Display;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Glyph = Cosmere.Lightweave.Typography.Glyph;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "dialog-header",
    Summary =
        "Dialog title bar with optional eyebrow breadcrumb, optional trailing text action, and optional close button.",
    WhenToUse =
        "Top of any redesigned dialog (Load Colony, Mods Config, Options). Pairs Display title + Eyebrow breadcrumb + IconButton close in a single primitive so headers stay consistent across the suite.",
    SourcePath = "Lightweave/Lightweave/Layout/DialogHeader.cs"
)]
public static class DialogHeader {
    public static LightweaveNode Create(
        [DocParam("Display title text. Rendered uppercase, tracked.")]
        string title,
        [DocParam("Optional eyebrow text rendered before the title in muted color.")]
        string? breadcrumb = null,
        [DocParam("Optional trailing text action (e.g. APPLY). Hidden when null.")]
        string? trailingActionLabel = null,
        [DocParam("Click handler for the trailing action.")]
        Action? onTrailingAction = null,
        [DocParam("Close button click handler. Close button is hidden when null.")]
        Action? onClose = null,
        [DocParam("Whether to draw the bottom divider line.")]
        bool drawDivider = true,
        [DocParam("Optional tab/filter pills rendered between title and close button. Each tab is (label, isActive, onClick).")]
        IReadOnlyList<DialogHeaderTab>? tabs = null,
        [DocParam("Optional mono status line rendered to the right of the title (e.g. '12 active · 14 installed').")]
        string? statusLine = null,
        [DocParam("Optional warning segment appended to the status line in warn color (e.g. ' · 1 conflict').")]
        string? statusWarn = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode root = Stack.Create(default, s => {
            s.Add(HStack.Create(SpacingScale.Md, row => {
                row.AddHug(BuildLeftZone(breadcrumb, title, statusLine, statusWarn));
                row.AddFlex(Spacer.Flex());
                row.AddHug(BuildRightZone(tabs, trailingActionLabel, onTrailingAction, onClose));
            }, style: new Style {
                Padding = new EdgeInsets(
                    Top: new Rem(1.25f),
                    Right: new Rem(1.5f),
                    Bottom: new Rem(1.25f),
                    Left: new Rem(1.5f)
                ),
            }));
            if (drawDivider) {
                s.Add(Divider.Horizontal());
            }
        }, style: style, classes: classes, id: id, line: line, file: file);
        return root;
    }

    private static LightweaveNode BuildLeftZone(string? breadcrumb, string title, string? statusLine, string? statusWarn) {
        return HStack.Create(SpacingScale.Lg, h => {
            if (!string.IsNullOrEmpty(breadcrumb)) {
                h.AddHug(Eyebrow.Create(breadcrumb!, style: new Style {
                    FontFamily = FontRole.Mono,
                    FontSize = new Rem(0.7f),
                    LetterSpacing = Tracking.Of(0.15f),
                    TextColor = ThemeSlot.TextMuted,
                }));
            }
            h.AddHug(Display.Create(title, level: 2, style: new Style {
                FontWeight = FontStyle.Bold,
                FontSize = new Rem(2.25f),
                LetterSpacing = Tracking.Of(0.12f),
                TextColor = ThemeSlot.TextPrimary,
            }));
            if (!string.IsNullOrEmpty(statusLine)) {
                h.AddHug(HStack.Create(SpacingScale.None, sl => {
                    sl.AddHug(Text.Create(statusLine!, style: new Style {
                        FontFamily = FontRole.Mono,
                        FontSize = new Rem(0.85f),
                        TextColor = ThemeSlot.TextMuted,
                    }));
                    if (!string.IsNullOrEmpty(statusWarn)) {
                        sl.AddHug(Text.Create(statusWarn!, style: new Style {
                            FontFamily = FontRole.Mono,
                            FontSize = new Rem(0.85f),
                            TextColor = ThemeSlot.StatusWarning,
                        }));
                    }
                }));
            }
        });
    }

    private static LightweaveNode BuildRightZone(
        IReadOnlyList<DialogHeaderTab>? tabs,
        string? trailingActionLabel,
        Action? onTrailingAction,
        Action? onClose
    ) {
        return HStack.Create(SpacingScale.Xxs, h => {
            if (tabs != null && tabs.Count > 0) {
                IReadOnlyList<DialogHeaderTab> tabList = tabs;
                h.AddHug(HStack.Create(SpacingScale.Xs, t => {
                    for (int i = 0; i < tabList.Count; i++) {
                        t.AddHug(BuildTabPill(tabList[i]));
                    }
                }));
            }
            if (!string.IsNullOrEmpty(trailingActionLabel)) {
                h.AddHug(BuildTrailingAction(trailingActionLabel!, onTrailingAction));
            }
            if (onClose != null) {
                h.AddHug(BuildCloseButton(onClose));
            }
        });
    }

    private static LightweaveNode BuildTrailingAction(string label, Action? onClick) {
        LightweaveNode node = NodeBuilder.New($"DialogHeader.TrailingAction:{label}", 0, "");
        node.ApplyStyling("dialog-header-trailing", null, null, null);
        node.PreferredHeight = new Rem(2.25f).ToPixels();

        Color restColor = new Color(0.722f, 0.678f, 0.584f, 1f);
        Color hoverColor = new Color(0.929f, 0.894f, 0.816f, 1f);
        Color hoverBg = new Color(0.157f, 0.125f, 0.086f, 0.4f);
        Color hoverBorder = new Color(0.769f, 0.667f, 0.510f, 0.28f);
        float hPadPx = new Rem(1.125f).ToPixels();

        Font ResolveFont() {
            return LightweaveFonts.CarlitoRegular ?? RenderContext.Current.Theme.GetFont(FontRole.Body);
        }

        LightweaveNode BuildEyebrow(Color color) {
            return Eyebrow.Create(label, style: new Style {
                FontFamily = ResolveFont(),
                FontSize = new Rem(0.875f),
                LetterSpacing = Tracking.Of(0.04f),
                TextAlign = TextAlign.Center,
                TextColor = color,
            });
        }

        node.MeasureWidth = () => {
            LightweaveNode probe = BuildEyebrow(restColor);
            float textW = probe.MeasureWidth?.Invoke() ?? 0f;
            return textW + hPadPx * 2f;
        };

        node.Paint = (rect, _) => {
            LightweaveHitTracker.Track(rect);
            InteractionState state = InteractionState.Resolve(rect, null, false);
            if (state.Hovered) {
                PaintBox.Draw(
                    rect,
                    BackgroundSpec.Of(hoverBg),
                    BorderSpec.All(new Rem(1f / 16f), hoverBorder),
                    RadiusSpec.All(RadiusScale.Sm)
                );
            }
            LightweaveNode painted = BuildEyebrow(state.Hovered ? hoverColor : restColor);
            LightweaveRoot.PaintSubtree(painted, rect);

            InteractionFeedback.Apply(rect, true, true);
            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick?.Invoke();
                SoundDefOf.Click.PlayOneShotOnCamera();
                e.Use();
            }
            MouseoverSounds.DoRegion(rect);
        };
        return node;
    }

    private static LightweaveNode BuildCloseButton(Action onClose) {
        LightweaveNode node = NodeBuilder.New("DialogHeader.Close", 0, "");
        node.ApplyStyling("dialog-header-close", null, null, null);
        float sizePx = new Rem(2.25f).ToPixels();
        node.PreferredHeight = sizePx;

        Color restColor = new Color(0.722f, 0.678f, 0.584f, 1f);
        Color hoverColor = new Color(0.929f, 0.894f, 0.816f, 1f);
        Color restBorder = new Color(0.769f, 0.667f, 0.510f, 0.16f);
        Color hoverBorder = new Color(0.769f, 0.667f, 0.510f, 0.28f);
        Color hoverBg = new Color(0.157f, 0.125f, 0.086f, 0.5f);

        LightweaveNode BuildGlyph(Color color) {
            return Glyph.Create(Icons.Phosphor.X, style: new Style {
                FontSize = new Rem(1.125f),
                TextColor = color,
            });
        }

        node.MeasureWidth = () => sizePx;

        node.Paint = (rect, _) => {
            float size = Mathf.Min(sizePx, Mathf.Min(rect.width, rect.height));
            Rect square = new Rect(
                rect.x + (rect.width - size) / 2f,
                rect.y + (rect.height - size) / 2f,
                size,
                size
            );

            LightweaveHitTracker.Track(square);
            InteractionState state = InteractionState.Resolve(square, null, false);

            PaintBox.Draw(
                square,
                state.Hovered ? BackgroundSpec.Of(hoverBg) : null,
                BorderSpec.All(new Rem(1f / 16f), state.Hovered ? hoverBorder : restBorder),
                RadiusSpec.All(RadiusScale.Sm)
            );

            LightweaveNode glyph = BuildGlyph(state.Hovered ? hoverColor : restColor);
            LightweaveRoot.PaintSubtree(glyph, square);

            InteractionFeedback.Apply(square, true, true);
            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && square.Contains(e.mousePosition)) {
                onClose.Invoke();
                SoundDefOf.Click.PlayOneShotOnCamera();
                e.Use();
            }
            MouseoverSounds.DoRegion(square);
        };
        return node;
    }

    private static LightweaveNode BuildTabPill(DialogHeaderTab tab) {
        LightweaveNode node = NodeBuilder.New($"DialogHeader.Tab:{tab.Label}", 0, "");
        node.ApplyStyling("dialog-header-tab", null, null, null);
        node.PreferredHeight = new Rem(2.0f).ToPixels();
        float tabPadPx = SpacingScale.Xxs.ToPixels();

        LightweaveNode BuildLabelNode(Color color) {
            return Eyebrow.Create(tab.Label, style: new Style {
                FontFamily = FontRole.BodyBold,
                FontSize = new Rem(0.85f),
                TextAlign = TextAlign.Center,
                TextColor = color,
            });
        }

        node.MeasureWidth = () => {
            LightweaveNode probe = BuildLabelNode(Color.white);
            float textW = probe.MeasureWidth?.Invoke() ?? 0f;
            return textW + tabPadPx * 2f;
        };

        node.Paint = (rect, _) => {
            LightweaveHitTracker.Track(rect);
            InteractionState state = InteractionState.Resolve(rect, null, false);
            Theme.Theme theme = RenderContext.Current.Theme;
            ThemeSlot fgSlot = tab.IsActive
                ? ThemeSlot.SurfaceAccent
                : (state.Hovered ? ThemeSlot.TextPrimary : ThemeSlot.TextMuted);
            LightweaveNode painted = BuildLabelNode(theme.GetColor(fgSlot));
            LightweaveRoot.PaintSubtree(painted, rect);
            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                tab.OnClick?.Invoke();
                SoundDefOf.Click.PlayOneShotOnCamera();
                e.Use();
            }
            MouseoverSounds.DoRegion(rect);
        };
        return node;
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_TitleOnly")]
    public static DocSample DocsTitleOnly() {
        return new DocSample(() => Create("Settings"));
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_WithBreadcrumb", Order = 1)]
    public static DocSample DocsBreadcrumb() {
        return new DocSample(() => Create("Audio", "Options", onClose: () => { }));
    }

    [DocVariant("CL_Playground_Layout_DialogHeader_FullChrome", Order = 2)]
    public static DocSample DocsFullChrome() {
        return new DocSample(() => Create(
                "Mods",
                "Configuration",
                "Apply",
                () => { },
                () => { }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Create("Load colony", "Saves", onClose: () => { }));
    }
}

public readonly struct DialogHeaderTab {
    public string Label { get; }
    public bool IsActive { get; }
    public Action OnClick { get; }

    public DialogHeaderTab(string label, bool isActive, Action onClick) {
        Label = label;
        IsActive = isActive;
        OnClick = onClick;
    }
}